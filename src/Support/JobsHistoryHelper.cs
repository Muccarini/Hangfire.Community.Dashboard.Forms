using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Storage;
using Hangfire.Community.Dashboard.Forms.Metadata;
using Hangfire.Dashboard;

namespace Hangfire.Community.Dashboard.Forms.Support
{
    public static class JobsHistoryHelper
    {
        public static List<JobHistoryMetadata> GetJobHistoryByName(string jobMethodName, int count = 10, string queue = "default", DashboardContext context = null)
        {
            IMonitoringApi monitoringApi = context?.Storage.GetMonitoringApi() ?? JobStorage.Current.GetMonitoringApi();

            var jobsHistory = new List<JobHistoryMetadata>();

            monitoringApi.ScheduledJobs(0, count)
                .Where(j => j.Value.Job != null && $"{j.Value.Job.Type.Name}_{j.Value.Job.Method.Name}" == jobMethodName)
                .ToList()
                .ForEach(j => jobsHistory.Add(new JobHistoryMetadata {
                    Id = j.Key,
                    Time = j.Value.ScheduledAt ?? j.Value.EnqueueAt,
                    Args = j.Value.Job.Args,
                    Type = "Scheduled",
                }));

            monitoringApi.SucceededJobs(0, count)
                .Where(j => j.Value.Job != null && $"{j.Value.Job.Type.Name}_{j.Value.Job.Method.Name}" == jobMethodName)
                .ToList()
                .ForEach(j => jobsHistory.Add(new JobHistoryMetadata {
                    Id = j.Key,
                    Time = j.Value.SucceededAt ?? DateTime.MinValue,
                    Args = j.Value.Job.Args,
                    Type = "Succeeded",
                }));

            monitoringApi.FailedJobs(0, count)
                .Where(j => j.Value.Job != null && $"{j.Value.Job.Type.Name}_{j.Value.Job.Method.Name}" == jobMethodName)
                .ToList()
                .ForEach(j => jobsHistory.Add(new JobHistoryMetadata {
                    Id = j.Key,
                    Time = j.Value.FailedAt ?? DateTime.MinValue,
                    Args = j.Value.Job.Args,
                    Type = "Failed",
                }));

            monitoringApi.DeletedJobs(0, count)
                .Where(j => j.Value.Job != null && $"{j.Value.Job.Type.Name}_{j.Value.Job.Method.Name}" == jobMethodName)
                .ToList()
                .ForEach(j => jobsHistory.Add(new JobHistoryMetadata {
                    Id = j.Key,
                    Time = j.Value.DeletedAt ?? DateTime.MinValue,
                    Args = j.Value.Job.Args,
                    Type = "Enqueued",
                }));

            return jobsHistory;
        }
        
        public static IReadOnlyList<object> GetJobArguments(string jobId, DashboardContext context = null)
        {
            IMonitoringApi monitoringApi = context?.Storage.GetMonitoringApi() ?? JobStorage.Current.GetMonitoringApi();

            var scheduled = monitoringApi.ScheduledJobs(0, int.MaxValue).FirstOrDefault(j => j.Key == jobId);
            if (!scheduled.Equals(default(KeyValuePair<string, Hangfire.Storage.Monitoring.ScheduledJobDto>)) && scheduled.Value.Job != null)
                return scheduled.Value.Job.Args;

            var succeeded = monitoringApi.SucceededJobs(0, int.MaxValue).FirstOrDefault(j => j.Key == jobId);
            if (!succeeded.Equals(default(KeyValuePair<string, Hangfire.Storage.Monitoring.SucceededJobDto>)) && succeeded.Value.Job != null)
                return succeeded.Value.Job.Args;

            var failed = monitoringApi.FailedJobs(0, int.MaxValue).FirstOrDefault(j => j.Key == jobId);
            if (!failed.Equals(default(KeyValuePair<string, Hangfire.Storage.Monitoring.FailedJobDto>)) && failed.Value.Job != null)
                return failed.Value.Job.Args;

            var deleted = monitoringApi.DeletedJobs(0, int.MaxValue).FirstOrDefault(j => j.Key == jobId);
            if (!deleted.Equals(default(KeyValuePair<string, Hangfire.Storage.Monitoring.DeletedJobDto>)) && deleted.Value.Job != null)
                return deleted.Value.Job.Args;

            return null;
        }
    }
}
