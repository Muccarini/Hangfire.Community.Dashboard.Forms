using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Hangfire.Dashboard.Management.v3.Metadata;

namespace Hangfire.Dashboard.Management.v3.Support
{
	public static class VT
	{
		public static Dictionary<Type, HashSet<Type>> Implementations { get; private set; } = new Dictionary<Type, HashSet<Type>>();

		internal static void SetAllImplementations(Assembly assembly)
		{
			JobsHelper.Metadata.ForEach(job => job.MethodInfo.GetParameters().ToList().ForEach(param => RegisterInterfaceImpls(assembly, param.ParameterType)));
		}

		// it get all interfaces from a type, including generic parameters interfaces.
		private static List<Type> GetGenericParamInterface(Type parameterType)
		{
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
	}
}
