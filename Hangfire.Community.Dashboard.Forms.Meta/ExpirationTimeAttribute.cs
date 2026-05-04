using System;
using Hangfire.States;
using Hangfire.Common;
using Hangfire.Storage;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class ExpirationTimeAttribute : JobFilterAttribute, IApplyStateFilter
{
    public TimeSpan ExpirationTime { get; }

    public ExpirationTimeAttribute(int days = 0, int hours = 0, int minutes = 0, int seconds = 0)
    {
        ExpirationTime = new TimeSpan(days, hours, minutes, seconds);
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        context.JobExpirationTimeout = ExpirationTime;
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        context.JobExpirationTimeout = ExpirationTime;
    }
}