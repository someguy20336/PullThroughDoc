using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PullThroughDoc.Test.Helpers;
using System.Collections.Generic;

namespace PullThroughDoc.Test
{
	[TestClass]
	public class ExternalReferenceTests : PullThroughDocCodeFixVerifier
	{
		private string _docXml;
		protected override CodeFixProvider CodeFixProvider => new PullThroughDocCodeFixProvider();

		public override List<MetadataReference> References
		{
			get
			{
				var refs = base.References;
				refs[0] = MetadataReference.CreateFromFile(typeof(object).Assembly.Location, documentation: new FakeVisualStudioDocumentationProvider(_docXml));
				return refs;
			}
		}

		[TestInitialize]
		public void Initialize()
		{
			_docXml = "";
		}

		[TestMethod]
		public void NoBaseDocumentationExternal_NoAnalyzer()
		{
			// Not setting _docXml will result in no analyzer

			var test = @"
    namespace ConsoleApplication1
    {
        class TypeName 
        {   
			public override string ToString() {}
        }
    }";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void OverrideExternalMethod_PullsThrough()
		{

			_docXml = @"
<doc>
  <summary>Searches things.</summary>
  <param name=""value"">The thing to find</param>
  <returns>The result</returns>
</doc>
";

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

			var fixtest = @"
using System.Collections;

namespace ConsoleApplication1
{
	class ArrayListOverride : ArrayList
	{
		/// <summary>Searches things.</summary>
		/// <param name=""value"">The thing to find</param>
		/// <returns>The result</returns>
		public override int BinarySearch(object value) => 1;
	}
}";
			VerifyCSharpFix(test, fixtest);
		}

	}

}
