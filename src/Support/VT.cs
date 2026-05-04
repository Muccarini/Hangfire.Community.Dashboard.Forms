using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

using Hangfire.Community.Dashboard.Forms.Metadata;

namespace Hangfire.Community.Dashboard.Forms.Support
{
	// TODO: rewrite cleaner.
	public static class VT
	{
		public static HashSet<Type> AllTypes { get; private set; } = new HashSet<Type>();

		public static Dictionary<Type, HashSet<Type>> Implementations { get; private set; } = new Dictionary<Type, HashSet<Type>>();

		internal static void SetAllImplementations(Assembly assembly)
		{
			JobsHelper.Metadata.ForEach(job => job.MethodInfo.GetParameters().ToList().ForEach(param => RegisterInterfaceImpls(assembly, param.ParameterType)));
		}

		// it get all interfaces from a type, including generic parameters interfaces.
		private static List<Type> GetGenericParamInterface(Type parameterType)
		{
			if(!AllTypes.Add(parameterType)) // avoid stack overflow on circular references
				return new List<Type>();

			List<Type> interfaces = new List<Type>();

			if (parameterType.IsInterface)
			{
				interfaces.Add(parameterType);
			}
			if (parameterType.IsGenericType)
			{
				parameterType.GetGenericArguments().ToList().ForEach(arg => {
					GetGenericParamInterface(arg)
					.Where(i => !interfaces.Contains(i)).ToList()
					.ForEach(i => interfaces.Add(i));
				});
			}
			if (parameterType.IsClass)
			{
				parameterType.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(DisplayDataAttribute))).ToList()
					.ForEach(prop => GetGenericParamInterface(prop.PropertyType)
						.Where(i => !interfaces.Contains(i)).ToList()
						.ForEach(i => interfaces.Add(i)));
			}

			return interfaces;
		}
		
		private static void RegisterInterfaceImpls(Assembly assembly, Type parameterType)
		{
			GetGenericParamInterface(parameterType).ForEach(i =>
			{
				if (Implementations.ContainsKey(i))
				{
					RegisterImplementations(assembly, i);
				}
				else
				{
					Implementations[i] = new HashSet<Type>();
					RegisterImplementations(assembly, i);
				}
			});
		}

		private static void RegisterImplementations(Assembly assembly, Type interfaceType)
		{
			var dictList = Implementations[interfaceType];
			var implementations = GetInterfaceImplementations(assembly, interfaceType).ToList();

			foreach (var impl in implementations)
			{
				if (!dictList.Contains(impl))
					dictList.Add(impl);

				// get nested interfaces
				//non direct circular reference are avoided during serialization, not needed here?
				var nestedInterfaces = GetInterfacePropsFromType(impl)
					.Where(i => i != interfaceType) // avoids direct circular references.
					.ToList();

				foreach (var nestedInterface in nestedInterfaces)
					RegisterInterfaceImpls(assembly, nestedInterface);
			}
		}

		// gets all properties which are interfaces inside a type
		private static IEnumerable<Type> GetInterfacePropsFromType(Type classType)
		{
			var types = classType.GetProperties().Select(p => p.PropertyType).ToList();

			return types.Where(t => t.IsInterface);
		}

		// gets all concrete impls of given interface
		private static IEnumerable<Type> GetInterfaceImplementations(Assembly assembly, Type interfaceType) =>
			assembly.GetTypes().Where(t => interfaceType.IsAssignableFrom(t) && !t.IsInterface);

		public static string GetDisplayName(Type type)
		{
			if (type == null) return string.Empty;

			var displayNameAttr = type.GetCustomAttribute<DisplayNameAttribute>();
			return displayNameAttr?.DisplayName ?? type.Name;
		}
	}
}
