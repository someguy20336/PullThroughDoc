using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PullThroughDoc.Test
{
	/// <summary>
	/// Verifies a bunch of tests specific to interfaces
	/// </summary>
	[TestClass]
	public class InterfaceDocumentationTests : PullThroughDocCodeFixVerifier
	{
		[TestInitialize]
		public void Init()
		{
			CodeFixProvider = new PullThroughDocCodeFixProvider();
		}

		//Diagnostic and CodeFix both triggered and checked for
		[TestMethod]
		public void Interface_Documentation_PullsThrough()
		{
			var test = @"
    namespace ConsoleApplication1
    {
		interface IInterface 
		{
			/// <summary>Does A Thing </summary>
			string DoThing();
		}
        class TypeName : IInterface
        {   
			public string DoThing() {}
        }
    }";

			ExpectPullThroughDiagnosticAt(test, "DoThing", 11, 18);

			var fixtest = @"
    namespace ConsoleApplication1
    {
		interface IInterface 
		{
			/// <summary>Does A Thing </summary>
			string DoThing();
		}
        class TypeName : IInterface
        {   
			/// <summary>Does A Thing </summary>
			public string DoThing() {}
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

		[TestMethod]
		public void Interface_MultiLineDocumentation_PullsThrough()
		{
			var test = @"
    namespace ConsoleApplication1
    {
		interface IInterface 
		{
			/// <summary>Does A Thing </summary>
			/// <param name=""param1"">parameter</param>
			/// <returns>A string</returns>
			string DoThing(string param1);
		}
        class TypeName : IInterface
        {   
			public string DoThing(string param1) {}
        }
    }";

			ExpectPullThroughDiagnosticAt(test, "DoThing", 13, 18);

			var fixtest = @"
    namespace ConsoleApplication1
    {
		interface IInterface 
		{
			/// <summary>Does A Thing </summary>
			/// <param name=""param1"">parameter</param>
			/// <returns>A string</returns>
			string DoThing(string param1);
		}
        class TypeName : IInterface
        {   
			/// <summary>Does A Thing </summary>
			/// <param name=""param1"">parameter</param>
			/// <returns>A string</returns>
			public string DoThing(string param1) {}
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

		[TestMethod]
		public void Interface_WithoutDocumentation_NoAnalyzer()
		{
			var test = @"
    namespace ConsoleApplication1
    {
		interface IInterface 
		{
			string DoThing();
		}
        class TypeName : IInterface
        {   
			public string DoThing() {}
        }
    }";

			VerifyCSharpDiagnostic(test);
		}

	}
}
