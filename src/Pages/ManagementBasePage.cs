using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Hangfire.Common;
using Hangfire.Dashboard;
using Hangfire.Dashboard.Management.v3.Metadata;
using Hangfire.Dashboard.Management.v3.Support;
using Hangfire.Dashboard.Pages;
using Hangfire.Server;
using Hangfire.States;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hangfire.Dashboard.Management.v3.Pages
{
	partial class ManagementBasePage
	{
		public readonly string menuName;
		public readonly IEnumerable<JobMetadata> jobs;
		public readonly Dictionary<string, string> jobSections;

		protected internal ManagementBasePage(string menuName) : base()
		{
			//this.UrlHelper = new UrlHelper(this.Context);
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

				DashboardRoutes.Routes.Add(route, new CommandWithResponseDispatcher(context => {
					string errorMessage = null;
					string jobLink = null;
					var par = new List<object>();
					string GetFormVariable(string key)
					{
						return Task.Run(() => context.Request.GetFormValuesAsync(key)).Result.FirstOrDefault();
					}

					var id = GetFormVariable("id");
					var type = GetFormVariable("type");

					HashSet<Type> nestedTypes = new HashSet<Type>();

					foreach (var parameterInfo in jobMetadata.MethodInfo.GetParameters())
					{
						if (parameterInfo.ParameterType == typeof(PerformContext) || parameterInfo.ParameterType == typeof(IJobCancellationToken))
						{
							par.Add(null);
							continue;
						}

						DisplayDataAttribute displayInfo = null;

						if (parameterInfo.GetCustomAttributes(true).OfType<DisplayDataAttribute>().Any())
						{
							displayInfo = parameterInfo.GetCustomAttribute<DisplayDataAttribute>(true);
						}
						else
						{
							displayInfo = new DisplayDataAttribute();
						}

						Type rootType = parameterInfo.ParameterType;

						var variable = $"{id}_{parameterInfo.Name}";

						if (rootType == typeof(DateTime))
						{
							variable = $"{variable}_datetimepicker";
						}

						variable = variable.Trim('_');
						var formInput = GetFormVariable(variable);

						object item = null;

						if (rootType.IsGenericType && rootType.GetGenericTypeDefinition() == typeof(List<>))
						{
							var elementType = rootType.GetGenericArguments()[0];
							var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));

							if (int.TryParse(GetFormVariable($"{variable}"), out int count))
							{
								for (int i = 0; i < count; i++)
								{
									nestedTypes.Add(elementType);
									var nestedInstance = ProcessType($"{variable}_list_{i}", elementType, GetFormVariable, nestedTypes, out errorMessage);
									nestedTypes.Remove(elementType);
									list.GetType().GetMethod("Add").Invoke(list, new[] { nestedInstance });
								}
							}

							item = list;

						}
						else if (rootType.IsInterface)
						{
							if (!VT.Implementations.ContainsKey(rootType)) { errorMessage = $"{displayInfo.Label ?? parameterInfo.Name} is not a valid interface type or is not registered in VT."; break; }
							VT.Implementations.TryGetValue(rootType, out HashSet<Type> impls);
							var impl = impls.FirstOrDefault(concrete => concrete.FullName == GetFormVariable($"{id}_{parameterInfo.Name}"));

							if (impl == null)
							{
								errorMessage = $"{impl.FullName} is not a valid concrete type of {rootType} or is not registered in VT.";
								break;
							}

							nestedTypes.Add(impl);
							item = ProcessType($"{variable}_{impl.Name}", impl, GetFormVariable, nestedTypes, out errorMessage);
							nestedTypes.Remove(impl);
						}
						else if (rootType == typeof(string))
						{
							item = formInput;
							if (displayInfo.IsRequired && string.IsNullOrWhiteSpace((string)item))
							{
								errorMessage = $"{displayInfo.Label ?? parameterInfo.Name} is required.";
								break;
							}
						}
						else if (rootType == typeof(int))
						{
							int intNumber;
							if (int.TryParse(formInput, out intNumber) == false)
							{
								errorMessage = $"{displayInfo.Label ?? parameterInfo.Name} was not in a correct format.";
								break;
							}
							item = intNumber;
						}
						else if (rootType == typeof(DateTime))
						{
							item = formInput == null ? DateTime.MinValue : DateTime.Parse(formInput, null, DateTimeStyles.RoundtripKind);
							if (displayInfo.IsRequired && item.Equals(DateTime.MinValue))
							{
								errorMessage = $"{displayInfo.Label ?? parameterInfo.Name} is required.";
								break;
							}
						}
						else if (rootType == typeof(bool))
						{
							item = formInput == "on";
						}
						else if (rootType.IsClass)
						{
							nestedTypes.Add(rootType);
							item = ProcessType(variable, rootType, GetFormVariable, nestedTypes, out errorMessage);
							nestedTypes.Remove(rootType);
						}
						else if (!rootType.IsValueType)
						{
							if (formInput == null || formInput.Length == 0)
							{
								item = null;
								if (displayInfo.IsRequired)
								{
									errorMessage = $"{displayInfo.Label ?? parameterInfo.Name} is required.";
									break;
								}
							}
							else
							{
								item = JsonConvert.DeserializeObject(formInput, rootType);
							}
						}
						else
						{
							item = formInput;
						}

						par.Add(item);
					}

					if (errorMessage == null)
					{
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


		private static object ProcessType(string parentId, Type parentType, Func<string, string> getFormVariable, HashSet<Type> nestedTypes, out string errorMessage)
		{
			errorMessage = null;

			if (parentType == typeof(DateTime))
			{
				parentId = $"{parentId}_datetimepicker";
			}

			if (parentType == typeof(string))
				return getFormVariable(parentId);
			else if (parentType == typeof(int))
			{
				var res = int.TryParse(getFormVariable(parentId), out var n) ? n : (int?)null;
				if (res.HasValue) { return res; } else { return errorMessage = $"{parentId} was not in a correct format."; }
			}
			else if (parentType == typeof(DateTime))
				return getFormVariable(parentId) == null ? DateTime.MinValue : DateTime.Parse(getFormVariable(parentId), null, DateTimeStyles.RoundtripKind);
			else if (parentType == typeof(bool))
				return getFormVariable(parentId) == "on";

			if (!string.IsNullOrEmpty(errorMessage)) return null;

			// Check if parameter is a generic type and is a enumerable
			if (parentType.IsGenericType && (parentType.GetGenericTypeDefinition() == typeof(List<>)))
			{
				var elementType = parentType.GetGenericArguments()[0];
				var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));

				//example: Expedited_Expedited_Job1_listofString = "0"

				if (int.TryParse(getFormVariable($"{parentId}"), out int count))
				{
					for (int i = 0; i < count; i++)
					{
						nestedTypes.Add(elementType);
						var nestedInstance = ProcessType($"{parentId}_list_{i}", elementType, getFormVariable, nestedTypes, out errorMessage);
						nestedTypes.Remove(elementType);
						list.GetType().GetMethod("Add").Invoke(list, new[] { nestedInstance });
					}
				}

				return list;
			}

			if (parentType.IsInterface)
			{
				if (!VT.Implementations.ContainsKey(parentType))
				{
					errorMessage = $"{parentType.Name} is not a valid interface type or is not registered in VT.";
					return null;
				}

				VT.Implementations.TryGetValue(parentType, out HashSet<Type> impls);
				var filteredImpls = new HashSet<Type>(impls.Where(impl => !nestedTypes.Contains(impl)));

				var choosedImpl = impls.FirstOrDefault(concrete => concrete.FullName == getFormVariable($"{parentId}"));

				if (choosedImpl == null)
				{
					errorMessage = $"cannot find a valid concrete type of {parentType} or is not registered in VT.";
					return null;
				}

				nestedTypes.Add(choosedImpl);
				var nestedInstance = ProcessType($"{parentId}_{choosedImpl.Name}", choosedImpl, getFormVariable, nestedTypes, out errorMessage);
				nestedTypes.Remove(choosedImpl);

				return nestedInstance;
			}

			if (parentType.IsEnum)
			{
				try
				{
					var enumValue = Enum.Parse(parentType, getFormVariable(parentId));
					return enumValue;
				}
				catch (Exception e)
				{
					errorMessage = $"{parentId} was not in a correct format: {e.Message}";
					return null;
				}
			}

			if (parentType.IsClass)
			{
				var instance = Activator.CreateInstance(parentType);
				if (instance == null)
				{
					errorMessage = $"Unable to create instance of {parentType.Name}";
					return null;
				}

				foreach (var propertyInfo in parentType.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(DisplayDataAttribute))))
				{
					string propId = $"{parentId}_{propertyInfo.Name}";

					if (propertyInfo.PropertyType == typeof(DateTime))
					{
						propId = $"{propId}_datetimepicker";
					}

					var propDisplayInfo = propertyInfo.GetCustomAttribute<DisplayDataAttribute>();
					var propLabel = propDisplayInfo.Label ?? propertyInfo.Name;

					var formInput = getFormVariable(propId);

					if (parentType.IsGenericType && parentType.GetGenericTypeDefinition() == typeof(List<>))
					{
						var elementType = parentType.GetGenericArguments()[0];
						var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));

						if (int.TryParse(getFormVariable($"{propId}"), out int count))
						{
							for (int i = 0; i < count; i++)
							{
								nestedTypes.Add(elementType);
								var nestedInstance = ProcessType($"{propId}_list_{i}", elementType, getFormVariable, nestedTypes, out errorMessage);
								nestedTypes.Remove(elementType);
								list.GetType().GetMethod("Add").Invoke(list, new[] { nestedInstance });
							}
						}

						propertyInfo.SetValue(instance, list);
					}
					else if (propertyInfo.PropertyType.IsInterface)
					{
						if (!VT.Implementations.ContainsKey(propertyInfo.PropertyType)) { errorMessage = $"{propDisplayInfo.Label ?? propertyInfo.Name} is not a valid interface type or is not 	registered in VT."; break; }
						VT.Implementations.TryGetValue(propertyInfo.PropertyType, out HashSet<Type> impls);
						var filteredImpls = new HashSet<Type>(impls.Where(impl => !nestedTypes.Contains(impl)));

						var choosedImpl = impls.FirstOrDefault(concrete => concrete.FullName == getFormVariable($"{propId}"));

						if (choosedImpl == null)
						{
							errorMessage = $"cannot find a valid concrete type of {propertyInfo.PropertyType} or is not registered in VT.";
							break;
						}

						nestedTypes.Add(choosedImpl);
						var nestedInstance = ProcessType($"{propId}_{choosedImpl.Name}", choosedImpl, getFormVariable, nestedTypes, out errorMessage);
						nestedTypes.Remove(choosedImpl);

						propertyInfo.SetValue(instance, nestedInstance);
					}
					else if (propertyInfo.PropertyType == typeof(string))
					{
						propertyInfo.SetValue(instance, formInput);
						if (propDisplayInfo.IsRequired && string.IsNullOrWhiteSpace((string)formInput))
						{
							errorMessage = $"{propLabel} is required.";
							break;
						}
					}
					else if (propertyInfo.PropertyType == typeof(int))
					{
						if (int.TryParse(formInput, out int intValue))
						{
							propertyInfo.SetValue(instance, intValue);
						}
						else
						{
							errorMessage = $"{propLabel} was not in a correct format.";
							break;
						}
					}
					else if (propertyInfo.PropertyType == typeof(DateTime))
					{
						var dateTimeValue = formInput == null ? DateTime.MinValue : DateTime.Parse(formInput, null, DateTimeStyles.RoundtripKind);
						propertyInfo.SetValue(instance, dateTimeValue);
						if (propDisplayInfo.IsRequired && dateTimeValue.Equals(DateTime.MinValue))
						{
							errorMessage = $"{propLabel} is required.";
							break;
						}
					}
					else if (propertyInfo.PropertyType == typeof(bool))
					{
						propertyInfo.SetValue(instance, formInput == "on");
					}
					else if (propertyInfo.PropertyType.IsEnum)
					{
						try
						{
							var enumValue = Enum.Parse(propertyInfo.PropertyType, formInput);
							propertyInfo.SetValue(instance, enumValue);
						}
						catch (Exception e)
						{
							errorMessage = $"{propLabel} was not in a correct format: {e.Message}";
							break;
						}
					}
					else if (propertyInfo.PropertyType.IsClass)
					{
						if (!nestedTypes.Add(propertyInfo.PropertyType)) { continue; } //Circular reference, not allowed
						var nestedInstance = ProcessType(propId, propertyInfo.PropertyType, getFormVariable, nestedTypes, out errorMessage);
						nestedTypes.Remove(propertyInfo.PropertyType);

						propertyInfo.SetValue(instance, nestedInstance);
					}
					else if (!propertyInfo.PropertyType.IsValueType)
					{
						if (formInput == null || formInput.Length == 0)
						{
							propertyInfo.SetValue(instance, null);
							if (propDisplayInfo.IsRequired)
							{
								errorMessage = $"{propLabel} is required.";
								break;
							}
						}
						else
						{
							propertyInfo.SetValue(instance, JsonConvert.DeserializeObject(formInput, propertyInfo.PropertyType));
						}
					}
					else
					{
						propertyInfo.SetValue(instance, formInput);
					}
				}

				return instance;
			}

			errorMessage = $"Unable to process type {parentType.Name} for {parentId}";
			return null;
		}
	}
}
