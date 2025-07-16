﻿using System;
using System.ComponentModel;
using Hangfire;
using Hangfire.Dashboard.Management.Dynamic.Metadata;
using Hangfire.Dashboard.Management.Dynamic.Support;
using Hangfire.Server;

namespace ASP.Net_Web_Application.HangfireManagement
{
	[ManagementPage(MenuName = "Expedited Jobs", Title = nameof(Expedited))]
	/*              A                            B                        */
	public class Expedited : IJob
	{
		[DisplayName("Job Number 1")] //C
		[Description("This is the description for Job Number 1")] //D
		[Queue("expedited")]
		public void Job1(PerformContext context, IJobCancellationToken token,
			[DisplayData(
				Label = "String Input 1",
				Description = "This is the description text for the string input with a default value and the control is disabled",
				DefaultValue = "This is the Default Value",
				IsDisabled = true
			)] string strInput1,

			[DisplayData(
				Placeholder = "This is the placeholder text",
				Description = "This is the description text for the string input without a default value and the control is enabled"
			)] string strInput2,

			[DisplayData(
				Label = "Multiline Input",
				IsMultiLine = true,
				Placeholder = "This is the multiline\nplaceholder text",
				Description = "This is the description text for the multiline input without a default value where the control is enabled and not required"
			)] string strInput3,

			[DisplayData(
				Label = "DateTime Input",
				Placeholder = "What is the date and time?",
				DefaultValue = "1/20/2020 1:02 AM",
				Description = "This is a date time input control"
			)] DateTime dtInput,

			[DisplayData(
				Label = "Boolean Input",
				DefaultValue = true,
				Description = "This is a boolean input"
			)] bool blInput,

			[DisplayData(
				Label = "Select Input",
				DefaultValue = TestEnum.Test5,
				Description = "Based on an enum object"
			)] TestEnum enumTest
		)
		{
			//Do awesome things here
		}

		public enum TestEnum
		{
			Test1,
			Test2,
			Test3,
			Test4 = 44,
			Test5
		}
	}
}
