using System.Collections.Generic;

namespace Hangfire.Dashboard.Management.Dynamic.Metadata
{
	public interface IInputDataList
	{
		Dictionary<string, string> GetData();
		string GetDefaultValue();
	}
}
