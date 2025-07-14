using System;
using System.Collections.Generic;

namespace Hangfire.Dashboard.Management.v3.Metadata
{
    public class JobHistoryMetadata
    {
        public string Id;
        public DateTime Time { get; set; }
        public string Type { get; set; }
        public IReadOnlyList<object> Args { get; set; }
    }   
}