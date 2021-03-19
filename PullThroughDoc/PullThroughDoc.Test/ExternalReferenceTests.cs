using Microsoft.VisualStudio.TestTools.UnitTesting;
using PullThroughDoc.Test.Helpers;

namespace PullThroughDoc.Test
{
	[TestClass]
	public class ExternalReferenceTests : PullThroughDocCodeFixVerifier
	{
		[TestMethod]
		public void OverrideExternalMethod_PullsThrough()
		{
			var test = @"
using System.Collections;

namespace ConsoleApplication1
{
	class ArrayListOverride : ArrayList 
	{
		public override int BinarySearch(object value) => 1;
	}
}";

			ExpectPullThroughDiagnosticAt(test, "BinarySearch", 8, 23);

			var fixtest = @$"
using System.Collections;

namespace ConsoleApplication1
{{
	class ArrayListOverride : ArrayList
	{{
		{FakeVisualStudioDocumentationProvider.FakeSummaryDoc()}
		public override int BinarySearch(object value) => 1;
	}}
}}";
			VerifyCSharpFix(test, fixtest);
		}
	}

}
