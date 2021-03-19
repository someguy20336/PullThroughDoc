using Microsoft.CodeAnalysis;
using System.Globalization;
using System.Threading;

namespace PullThroughDoc.Test.Helpers
{
	class FakeVisualStudioDocumentationProvider : DocumentationProvider
	{
		public override bool Equals(object obj)
		{
			return Equals(this, obj);
		}

		public override int GetHashCode() => FakeFullDoc().GetHashCode();
		

		protected override string GetDocumentationForSymbol(string documentationMemberID, CultureInfo preferredCulture, CancellationToken cancellationToken = default)
		{
			return FakeFullDoc();
		}


		public static string FakeSummaryDoc() => "<summary>This Is dummy doc</summary>";
		public static string FakeFullDoc() => $"<doc>{FakeSummaryDoc()}</doc>";
	}
}
