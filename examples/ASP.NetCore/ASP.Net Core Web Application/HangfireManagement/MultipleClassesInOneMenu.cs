using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using Hangfire.Dashboard.Management.Dynamic.Metadata;
using Hangfire.Dashboard.Management.Dynamic.Support;
using Hangfire.Server;

namespace ASP.Net_Core_Web_Application.HangfireManagement
{
	[ManagementPage(MenuName = "Multiple Classes in One Menu", Title = nameof(Simple2))]
	public class Simple2 : IJob
	{
		[DisplayName("Configure a new Event for company")]
		public void Job0(PerformContext context, IJobCancellationToken token,

			[DisplayData(
				Label = "Event"
			)] CompanyEvent companyEvent,

			[DisplayData(
				Description = "Date from which the inviters can invite people",
				IsRequired = true
			)] DateTime openInviteDate,

			[DisplayData(
				Description = "Date until which the inviters can invite people"
			)] DateTime closeInviteDate,

			[DisplayData(
				Label = "Inviters",
				Description = "who can issue tickets for this event",
				IsRequired = true
			)] List<IInviter> inviter
		)
		{
		}

		public interface IInviter
		{
			void EnableToInvite();
		}

		public class EmployeeInviter : IInviter
		{
			[DisplayData]
			public int EmployeeID { get; set; }
			public void EnableToInvite()
			{
				Console.WriteLine($"Employee: {EmployeeID} is enabled to invite");
			}
		}

		public class ExternalInviter : IInviter
		{
			[DisplayData]
			public string Email { get; set; }

			public void EnableToInvite()
			{
				Console.WriteLine($"External user: {Email} is enabled to invite");
			}
		}

		public class ExcelColInviter : IInviter
		{
			[DisplayData(
				isMultiline: true
				)]
			public string Ids { get; set; }

			public void EnableToInvite()
			{
				Console.WriteLine("Excel column data is being processed for invites");
				var ids = ParseExcelData(Ids);
				foreach (var id in ids)
				{
					Console.WriteLine($"Enable employee: {id} to invite");
				}
			}

			public List<long> ParseExcelData(string multilineText)
			{
				if (string.IsNullOrWhiteSpace(multilineText))
					return new List<long>();

				return multilineText.Split('\n')
						   .Where(line => !string.IsNullOrWhiteSpace(line))
						   .Select(line => long.TryParse(line.Trim(), out long number) ? number : 0)
						   .Where(number => number != 0)
						   .ToList();
			}
		}

		public class CompanyEvent
		{
			[DisplayData(
				Label = "Event Name",
				Description = "Name of the company event"
			)]
			public string EventName { get; set; }

			[DisplayData(
				Label = "Max Invites per inviter",
				Description = "number of ticket per inviter"
			)]
			public int MaxInvites { get; set; }

			[DisplayData(
				Label = "Slots",
				Description = "An Event as multiple slots, each slot has a maximum number of invites"
			)]
			public List<Slot> Slots { get; set; }
		}

		public class Slot
		{
			[DisplayData(
				Label = "Max Invites",
				Description = "Maximum number of invites for this slot"
			)]
			public int MaxInvites { get; set; }

			[DisplayData(
				Label = "Create Slots",
				Description = "Slect the strategy to create slots for this event"
			)]
			public ISlotGeneratorStrategy SlotGenerators { get; set; }
		}

		public interface ISlotGeneratorStrategy
		{
			public void CreateSlot(string eventName);
		}

		public class SlotRange : ISlotGeneratorStrategy
		{
			[DisplayData(
				Label = "Start Date",
				Description = "Start date of the slot range"
			)]
			public DateTime StartDate { get; set; }

			[DisplayData(
				Label = "End Date",
				Description = "End date of the slot range"
			)]
			public DateTime EndDate { get; set; }

			public void CreateSlot(string eventName)
			{
				Console.WriteLine($"Created Slot from: {StartDate} to {EndDate} for event: {eventName}");
			}
		}

		public class SlotBlock : ISlotGeneratorStrategy
		{
			[DisplayData(
				Label = "Start Date",
				Description = "Start date of the slot block"
			)]
			public DateTime StartDate { get; set; }

			[DisplayData(
				Label = "Minutes",
				Description = "Length of each slot in minutes"
			)]
			public int SlotLengthInMinutes { get; set; }

			[DisplayData(
				Label = "Number of Slots",
				Description = "Name of the strategy"
			)]
			public int NumberOfSlots { get; set; }

			public void CreateSlot(string eventName)
			{
				Console.WriteLine($"Created Slot from: {StartDate} for event: {eventName} with {NumberOfSlots} slots of {SlotLengthInMinutes} minutes each");
			}
		}



	}

	[ManagementPage(MenuName = "Multiple Classes in One Menu", Title = nameof(Simple3))]
	public class Simple3 : IJob
	{
		[DisplayName("Simple 3 Job 0")]
		public void Job0(PerformContext context, IJobCancellationToken token)
		{
		}
	}
}
