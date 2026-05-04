using System;
using System.Collections.Generic;
using System.ComponentModel;
using Hangfire;
using Hangfire.Community.Dashboard.Forms.Metadata;
using Hangfire.Community.Dashboard.Forms.Support;
using Hangfire.Server;

namespace ASP.Net_Core_Web_Application.HangfireManagement
{
	[ManagementPage(MenuName = "Feature Matrix", Title = "01 Basic Inputs")]
	public class FeatureMatrixBasics : IJob
	{
		[DisplayName("No input job")]
		[Description("Verifies the empty-form state and all execution buttons.")]
		[Queue("default")]
		public void NoInputs(PerformContext context, IJobCancellationToken token)
		{
			Console.WriteLine("Feature Matrix: no input job executed.");
		}

		[DisplayName("Primitive inputs")]
		[Description("Covers string, multiline text, integers, nullable integers, Uri, bool, enum, defaults, disabled, and required validation.")]
		[Queue("default")]
		[ShowMetaData(true)]
		public void PrimitiveInputs(
			PerformContext context,
			IJobCancellationToken token,
			[DisplayData(Label = "Required title", Placeholder = "Quarterly import", Description = "Required single-line string.", IsRequired = true)]
			string title,
			[DisplayData(Label = "Notes", Placeholder = "Paste notes here", Description = "Multiline string.", IsMultiLine = true)]
			string notes,
			[DisplayData(Label = "Retry count", Description = "Required integer.", DefaultValue = 3, IsRequired = true)]
			int retryCount,
			[DisplayData(Label = "Optional batch size", Description = "Nullable integer can be empty.")]
			int? batchSize,
			[DisplayData(Label = "Callback URL", Placeholder = "https://example.test/callback", Description = "Plain string URL field, used to avoid Uri serialization quirks while still testing URL-like input.")]
			string callbackUrl,
			[DisplayData(Label = "Dry run", Description = "Boolean checkbox with a default value.", DefaultValue = true)]
			bool dryRun,
			[DisplayData(Label = "Priority", Description = "Enum dropdown with a non-zero default.", DefaultValue = MatrixPriority.High)]
			MatrixPriority priority)
		{
			Console.WriteLine($"Feature Matrix primitives: {title}, retry {retryCount}, batch {batchSize}, dry run {dryRun}, priority {priority}, url {callbackUrl}");
		}

		[IgnoreMethod]
		public void HiddenMethod()
		{
			throw new NotSupportedException("This method should not be shown in the Forms dashboard.");
		}
	}

	[ManagementPage(MenuName = "Feature Matrix", Title = "02 Date and Schedule")]
	public class FeatureMatrixDates : IJob
	{
		private const string DateTimeOptions = @"
		{
			""display"": {
				""components"": {
					""calendar"": true,
					""date"": true,
					""month"": true,
					""year"": true,
					""decades"": false,
					""clock"": true,
					""hours"": true,
					""minutes"": true,
					""seconds"": false
				},
				""buttons"": {
					""clear"": true,
					""today"": true,
					""close"": true
				}
			},
			""localization"": {
				""locale"": ""en"",
				""format"": ""L LT"",
				""hourCycle"": ""h24""
			}
		}";

		[DisplayName("DateTime inputs")]
		[Description("Covers required DateTime, nullable DateTime, default DateTime strings, and custom Tempus Dominus configuration.")]
		[Queue("default")]
		public void DateTimeInputs(
			PerformContext context,
			IJobCancellationToken token,
			[DisplayData(Label = "Starts at", Description = "Required DateTime with custom control configuration.", IsRequired = true, ControlConfiguration = DateTimeOptions)]
			DateTime startsAt,
			[DisplayData(Label = "Optional reminder", Description = "Nullable DateTime can be left empty.", ControlConfiguration = DateTimeOptions)]
			DateTime? reminderAt,
			[DisplayData(Label = "Default review date", Description = "Default value is loaded into the picker.", DefaultValue = "01/20/2027 09:30 AM", ControlConfiguration = DateTimeOptions)]
			DateTime reviewAt)
		{
			Console.WriteLine($"Feature Matrix dates: starts {startsAt:o}, reminder {reminderAt:o}, review {reviewAt:o}");
		}

		[DisplayName("Recurring job with custom name")]
		[Description("Covers the AllowMultiple recurring-job name input.")]
		[Queue("critical")]
		[AllowMultiple]
		[ShowMetaData(true)]
		[AutomaticRetry(Attempts = 2)]
		public void RecurringNamedJob(
			PerformContext context,
			IJobCancellationToken token,
			[DisplayData(Label = "Report name", Description = "Used only as a regular job argument.", DefaultValue = "Daily summary", IsRequired = true)]
			string reportName)
		{
			Console.WriteLine($"Feature Matrix recurring report: {reportName}");
		}
	}

	[ManagementPage(MenuName = "Feature Matrix", Title = "03 Object Graphs")]
	public class FeatureMatrixObjects : IJob
	{
		[DisplayName("Nested object")]
		[Description("Covers custom classes, nested classes, lists, and list templates.")]
		[Queue("default")]
		public void NestedObject(
			PerformContext context,
			IJobCancellationToken token,
			[DisplayData(Label = "Import request", Description = "Custom class with nested properties.")]
			ImportRequest request)
		{
			Console.WriteLine($"Feature Matrix import request: {request?.Name}, rows {request?.Rows?.Count ?? 0}");
		}

		[DisplayName("Nested collections")]
		[Description("Covers List<string>, List<class>, and List<List<string>> index updates while adding and removing items.")]
		[Queue("default")]
		public void NestedCollections(
			PerformContext context,
			IJobCancellationToken token,
			[DisplayData(Label = "Tags", Description = "Simple list of strings.")]
			List<string> tags,
			[DisplayData(Label = "Rows", Description = "List of custom classes.")]
			List<ImportRow> rows,
			[DisplayData(Label = "Matrix", Description = "Nested list of strings.")]
			List<List<string>> matrix)
		{
			Console.WriteLine($"Feature Matrix collections: tags {tags?.Count ?? 0}, rows {rows?.Count ?? 0}, matrix {matrix?.Count ?? 0}");
		}

		public class ImportRequest
		{
			[DisplayData(Label = "Import name", Description = "Required nested string.", DefaultValue = "Customer import", IsRequired = true)]
			public string Name { get; set; }

			[DisplayData(Label = "Source", Description = "Nested enum default.", DefaultValue = ImportSource.Api)]
			public ImportSource Source { get; set; }

			[DisplayData(Label = "Rows", Description = "Nested list of custom classes.")]
			public List<ImportRow> Rows { get; set; }
		}

		public class ImportRow
		{
			[DisplayData(Label = "External id", IsRequired = true)]
			public string ExternalId { get; set; }

			[DisplayData(Label = "Quantity", DefaultValue = 1)]
			public int Quantity { get; set; }

			[DisplayData(Label = "Active", DefaultValue = true)]
			public bool Active { get; set; }
		}
	}

	[ManagementPage(MenuName = "Feature Matrix", Title = "04 Interfaces")]
	public class FeatureMatrixInterfaces : IJob
	{
		[DisplayName("Multiple implementations")]
		[Description("Covers interface dropdowns, DisplayName on implementations, and nested interface panels.")]
		[Queue("default")]
		public void MultipleImplementations(
			PerformContext context,
			IJobCancellationToken token,
			[DisplayData(Label = "Delivery channel", Description = "Choose one of several concrete implementations.", IsRequired = true)]
			IDeliveryChannel channel)
		{
			Console.WriteLine($"Feature Matrix channel: {channel?.Name}");
		}

		[DisplayName("Single implementation")]
		[Description("Covers interfaces that have exactly one concrete implementation.")]
		[Queue("default")]
		public void SingleImplementation(
			PerformContext context,
			IJobCancellationToken token,
			[DisplayData(Label = "Audit sink", Description = "There is exactly one implementation in the assembly.")]
			IAuditSink sink)
		{
			Console.WriteLine($"Feature Matrix audit sink: {sink?.Destination}");
		}

		public interface IDeliveryChannel
		{
			string Name { get; set; }
		}

		[DisplayName("Email channel")]
		public class EmailChannel : IDeliveryChannel
		{
			[DisplayData(Label = "Channel name", DefaultValue = "Email")]
			public string Name { get; set; }

			[DisplayData(Label = "Recipient email", Placeholder = "ops@example.test", IsRequired = true)]
			public string RecipientEmail { get; set; }
		}

		[DisplayName("Webhook channel")]
		public class WebhookChannel : IDeliveryChannel
		{
			[DisplayData(Label = "Channel name", DefaultValue = "Webhook")]
			public string Name { get; set; }

			[DisplayData(Label = "Endpoint", Placeholder = "https://example.test/hook", Description = "Plain string URL field, used to avoid Uri serialization quirks while still testing required interface properties.", IsRequired = true)]
			public string Endpoint { get; set; }

			[DisplayData(Label = "Fallback channel", Description = "Nested interface selection should hide circular choices.")]
			public IDeliveryChannel FallbackChannel { get; set; }
		}

		public interface IAuditSink
		{
			string Destination { get; set; }
		}

		[DisplayName("File audit sink")]
		public class FileAuditSink : IAuditSink
		{
			[DisplayData(Label = "Destination path", DefaultValue = "logs/forms-feature-matrix.log")]
			public string Destination { get; set; }
		}
	}

	public enum MatrixPriority
	{
		Low = 1,
		Normal = 5,
		High = 10
	}

	public enum ImportSource
	{
		Manual = 0,
		Api = 1,
		File = 2
	}
}
