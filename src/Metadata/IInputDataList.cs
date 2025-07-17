using System.Collections.Generic;

namespace Hangfire.Community.Dashboard.Forms.Metadata
{
	public interface IInputDataList
	{
		Dictionary<string, string> GetData();
		string GetDefaultValue();
	}
}
