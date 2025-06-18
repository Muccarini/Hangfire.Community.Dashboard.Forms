using System.Collections.Generic;

namespace Hangfire.Dashboard.Management.v3.Metadata
{
	public interface IInputDataList
	{
		Dictionary<string, string> GetData();
		string GetDefaultValue();
	}
}
