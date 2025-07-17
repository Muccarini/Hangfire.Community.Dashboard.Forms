using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Hangfire.Common;
using Hangfire.Community.Dashboard.Forms.Metadata;
using Hangfire.Community.Dashboard.Forms.Support;
using Hangfire.Server;
using Hangfire.States;
using Newtonsoft.Json;
using Hangfire.Dashboard;

namespace Hangfire.Community.Dashboard.Forms.Pages
{
	partial class ManagementBasePage
	{
		public readonly string menuName;
		public readonly IEnumerable<JobMetadata> jobs;
		public readonly Dictionary<string, string> jobSections;

		protected internal ManagementBasePage(string menuName) : base()
		{
			this.menuName = menuName;

			jobs = JobsHelper.Metadata.Where(j => j.MenuName.Contains(menuName)).OrderBy(x => x.SectionTitle).ThenBy(x => x.Name);
			jobSections = jobs.Select(j => j.SectionTitle).Distinct().ToDictionary(k => k, v => string.Empty);
		}

		public static void AddCommands(string menuName)
		{
			var jobs = JobsHelper.Metadata.Where(j => j.MenuName.Contains(menuName));
			
			foreach (var jobMetadata in jobs)
			{
				var route = $"{ManagementPage.UrlRoute}/{jobMetadata.JobId.ScrubURL()}";

				//POST
				DashboardRoutes.Routes.Add(route, new CommandWithResponseDispatcher(context => {
					string errorMessage = null;
					var par = new List<object>();
					string GetFormVariable(string key)
					{
						return Task.Run(() => context.Request.GetFormValuesAsync(key)).Result.FirstOrDefault();
					}

					var id = GetFormVariable("id");

					foreach (var parameterInfo in jobMetadata.MethodInfo.GetParameters())
					{
						if (parameterInfo.ParameterType == typeof(PerformContext) || parameterInfo.ParameterType == typeof(IJobCancellationToken))
						{
							par.Add(null);
							continue;
						}

						DisplayDataAttribute displayInfo = parameterInfo.GetCustomAttributes(true).OfType<DisplayDataAttribute>().Any() ?
							parameterInfo.GetCustomAttribute<DisplayDataAttribute>(true) :
							new DisplayDataAttribute();

						object item = CreateObject(parameterInfo.ParameterType, $"{id}_{parameterInfo.Name}", displayInfo, 0, GetFormVariable, out errorMessage);

						par.Add(item);
					}

					string jobLink = null;

					if (string.IsNullOrEmpty(errorMessage))
					{
						var type = GetFormVariable("type");
						var array = par.ToArray();
						var job = new Job(jobMetadata.Type, jobMetadata.MethodInfo, par.ToArray());
						var client = new BackgroundJobClient(context.Storage);

						switch (type)
						{
							case "CronExpression":
								{
									var manager = new RecurringJobManager(context.Storage);
									var cron = GetFormVariable($"{id}_sys_cron");
									var name = GetFormVariable($"{id}_sys_name");

									if (string.IsNullOrWhiteSpace(cron))
									{
										errorMessage = "No Cron Expression Defined";
										break;
									}
									if (jobMetadata.AllowMultiple && string.IsNullOrWhiteSpace(name))
									{
										errorMessage = "No Job Name Defined";
										break;
									}

									try
									{
										var jobId = jobMetadata.AllowMultiple ? name : jobMetadata.JobId;
										manager.AddOrUpdate(jobId, job, cron, TimeZoneInfo.Local, jobMetadata.Queue);
										jobLink = new UrlHelper(context).To("/recurring");
									}
									catch (Exception e)
									{
										errorMessage = e.Message;
									}
									break;
								}
							case "ScheduleDateTime":
								{
									var datetime = GetFormVariable($"{id}_sys_datetime");

									if (string.IsNullOrWhiteSpace(datetime))
									{
										errorMessage = "No Schedule Defined";
										break;
									}

									if (!DateTime.TryParse(datetime, null, DateTimeStyles.RoundtripKind, out DateTime dt))
									{
										errorMessage = "Unable to parse Schedule";
										break;
									}
									try
									{
										var jobId = client.Create(job, new ScheduledState(dt.ToUniversalTime()));//Queue
										jobLink = new UrlHelper(context).JobDetails(jobId);
									}
									catch (Exception e)
									{
										errorMessage = e.Message;
									}
									break;
								}
							case "ScheduleTimeSpan":
								{
									var timeSpan = GetFormVariable($"{id}_sys_timespan");

									if (string.IsNullOrWhiteSpace(timeSpan))
									{
										errorMessage = $"No Delay Defined '{id}'";
										break;
									}

									if (!TimeSpan.TryParse(timeSpan, out TimeSpan dt))
									{
										errorMessage = "Unable to parse Delay";
										break;
									}

									try
									{
										var jobId = client.Create(job, new ScheduledState(dt));//Queue
										jobLink = new UrlHelper(context).JobDetails(jobId);
									}
									catch (Exception e)
									{
										errorMessage = e.Message;
									}
									break;
								}
							case "Enqueue":
							default:
								{
									try
									{
										var jobId = client.Create(job, new EnqueuedState(jobMetadata.Queue));
										jobLink = new UrlHelper(context).JobDetails(jobId);
									}
									catch (Exception e)
									{
										errorMessage = e.Message;
									}
									break;
								}
						}
					}

					context.Response.ContentType = "application/json";
					if (!string.IsNullOrEmpty(jobLink))
					{
						context.Response.StatusCode = (int)HttpStatusCode.OK;
						context.Response.WriteAsync(JsonConvert.SerializeObject(new { jobLink }));
						return true;
					}
					context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
					context.Response.WriteAsync(JsonConvert.SerializeObject(new { errorMessage }));

					return false;
				}));
			}
		}

        private static object CreateObject(Type type, string id, DisplayDataAttribute displayInfo, int depthList, Func<string, string> getFormValue, out string errorMessage, HashSet<Type> nAllowedTypes = null)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (displayInfo == null) throw new ArgumentNullException(nameof(displayInfo));
            if (nAllowedTypes == null) nAllowedTypes = new HashSet<Type>();

            bool isGeneric = type.IsGenericType;
            bool isList = isGeneric && type.GetGenericTypeDefinition() == typeof(List<>);
            bool isNullable = isGeneric && type.GetGenericTypeDefinition() == typeof(Nullable<>);

            Type genericArgument = type.IsGenericType ? type.GetGenericArguments()[0] : null; //multiple generic arguments are not supported

            string labelText = displayInfo.Label ?? genericArgument?.Name ?? type.Name;
            string placeholderText = displayInfo.Placeholder ?? labelText;

            errorMessage = string.Empty;

            if (type == typeof(string))
            {
				var value = getFormValue(id);
				if (displayInfo.IsRequired && string.IsNullOrEmpty(value))
				{
					errorMessage = $"{labelText}: is required";
					return null;
				}
				return value;
            }
            else if (type == typeof(int) || type == typeof(int?))
            {
                int intNumber;
                if (int.TryParse(getFormValue(id), out intNumber) == false)
                {
					if (displayInfo.IsRequired)
					{
						errorMessage = $"{labelText}: is required";
						return null;
					}
                    if (type == typeof(int?))
                    {
                        return null;
                    }
                    errorMessage = $"{labelText}: was not in a correct format.";
                    return null;
                }
                return intNumber;
            }
            else if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                //jquery appends _datetimepicker to the id
                id += "_datetimepicker";
                var val = getFormValue(id) == null ? DateTime.MinValue : DateTime.Parse(getFormValue(id), null, DateTimeStyles.RoundtripKind);
                if (val.Equals(DateTime.MinValue))
                {
					if (displayInfo.IsRequired)
					{
						errorMessage = $"{labelText}: is required";
						return null;
					}
                    if (type == typeof(DateTime?))
                    {
                        return null;
                    }
                }

                return val;
            }
            else if (type == typeof(bool))
            {
                return getFormValue(id) == "on";
            }
            else if (type.IsEnum || (isNullable && genericArgument.IsEnum))
            {
                Type el = null;

                try
                {
                    el = type;

                    if (isNullable && genericArgument.IsEnum)
                    {
                        el = genericArgument;
                    }

                    return Enum.Parse(el, getFormValue(id));
                }
                catch
                {
					if (displayInfo.IsRequired)
					{
						errorMessage = $"{labelText}: is required";
						return null;
					}

                    return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && type.GetGenericArguments()[0].IsEnum) ?
                        null : GetDefaultEnumValue(el);
                }
            }

            if (type.IsClass && !isGeneric)
            {
                if (!nAllowedTypes.Add(type)) { errorMessage = $"Circular reference detected for type {type.Name}."; return null; }

                var instance = Activator.CreateInstance(type);
                if (instance == null)
                {
                    errorMessage = $"Unable to create instance of {type.Name}";
                    return null;
                }

                foreach (var propertyInfo in type.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(DisplayDataAttribute))))
                {
                    var propDisplayInfo = propertyInfo.GetCustomAttribute<DisplayDataAttribute>();
                    propDisplayInfo.Label = propDisplayInfo.Label ?? propertyInfo.Name;
                    string propId = $"{id}_{propertyInfo.Name}";

					var propObj = CreateObject(propertyInfo.PropertyType, propId, propDisplayInfo, depthList, getFormValue, out errorMessage, nAllowedTypes);

					//propagating error message from nested props to upper layers
					if (!string.IsNullOrEmpty(errorMessage))
					{
						return null;
					}

					instance.GetType().GetProperty(propertyInfo.Name).SetValue(instance, propObj);
                }

                nAllowedTypes.Remove(type);

                return instance;
            }

            if (type.IsInterface)
            {
                if (!VT.Implementations.ContainsKey(type)) { errorMessage = $"No concrete implementation of \"{type.Name}\" found in the current assembly."; return null; }

                VT.Implementations.TryGetValue(type, out HashSet<Type> impls);
                var filteredImpls = new HashSet<Type>(impls.Where(impl => nAllowedTypes.Add(impl)));

                var choosedImpl = impls.FirstOrDefault(concrete => concrete.FullName == getFormValue(id));

                if (choosedImpl == null)
                {
                    errorMessage = $"{displayInfo.Label ?? type.Name}: \" {getFormValue(id)} \" not found in VT";
                    return null;
                }

                var instance = Activator.CreateInstance(choosedImpl);
                if (instance == null)
                {
                    errorMessage = $"Unable to create instance of {choosedImpl.Name}";
                    return null;
                }

                foreach (var propertyInfo in choosedImpl.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(DisplayDataAttribute))))
                {
                    var propDisplayInfo = propertyInfo.GetCustomAttribute<DisplayDataAttribute>();
                    propDisplayInfo.Label = propDisplayInfo.Label ?? propertyInfo.Name;
                    string propId = $"{id}_{choosedImpl.Name}_{propertyInfo.Name}";

                    var propObj = CreateObject(propertyInfo.PropertyType, propId, propDisplayInfo, depthList, getFormValue, out errorMessage, nAllowedTypes);

					if (!string.IsNullOrEmpty(errorMessage))
					{
						return null;
					}

					instance.GetType().GetProperty(propertyInfo.Name).SetValue(instance, propObj);
                }

                nAllowedTypes.Remove(type);

                return instance;
            }

            if (isList)
            {
                var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(genericArgument));
                int listCount = 0;

                if (int.TryParse(getFormValue(id), out listCount))
                {
					var labelTMP = displayInfo.Label;

					for (int i = 0; i < listCount; i++)
                    {
						displayInfo.Label = $"{labelTMP} element number {i + 1}";
						
						var nestedInstance = CreateObject(genericArgument, $"{id}_list_{i}", displayInfo, depthList + 1, getFormValue, out errorMessage, nAllowedTypes);

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            return null;
                        }

                        list.GetType().GetMethod("Add").Invoke(list, new[] { nestedInstance });
                    }

                }

                return list;
            }

            errorMessage = $"Unable to process type {type} for {id}";
            return null;
        }


		/// <summary>
		/// Enums doesn't have a default value, this method it returns the first value of the enum.
		/// The first value of an enum is the lowest positive integer (or the negative integer with the greatest absolute value less than zero), which is usually 0.
		/// so if you have overridden the values of the enum it may not return the expected value.
		/// if enum has duplicated values, is not guaranteed to return the first defined value.
		/// https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1069
		/// </summary>
		/// <param name="enumType"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static object GetDefaultEnumValue(Type enumType)
		{
			if (!enumType.IsEnum)
				throw new ArgumentException("Type must be an enum.");

			//rare case where the enum has no defined values.
			var names = Enum.GetNames(enumType);
			if (names.Length == 0)
				throw new InvalidOperationException("Enum type has no defined keys.");

			return Enum.GetValues(enumType).GetValue(0);
		}
	}
}
