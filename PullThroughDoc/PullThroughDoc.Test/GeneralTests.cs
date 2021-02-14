using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PullThroughDoc.Test
{
	[TestClass]
	public class GeneralTests : PullThroughDocCodeFixVerifier
	{
		[TestInitialize]
		public void Init()
		{
			CodeFixProvider = new PullThroughDocCodeFixProvider();
		}

		//No diagnostics expected to show up
		[TestMethod]
		public void Doesnt_Trigger_For_Nothing()
		{
			var test = @"";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void BaseObjectMethodOverride_NoAnalyzer()
		{
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
		public void Regions_Documentation_ExcludingRegion()
		{
			var test = @"
    namespace ConsoleApplication1
    {
		class BaseClass 
		{
			#region Properties
			/// <summary>Does A Thing </summary>
			public virtual string DoThing() { }
			#endregion
		}
        class TypeName : BaseClass
        {   
			public override string DoThing() {}
        }
    }";
			ExpectPullThroughDiagnosticAt(test, "DoThing", 13, 27);

			var fixtest = @"
    namespace ConsoleApplication1
    {
		class BaseClass 
		{
			#region Properties
			/// <summary>Does A Thing </summary>
			public virtual string DoThing() { }
			#endregion
		}
        class TypeName : BaseClass
        {   
			/// <summary>Does A Thing </summary>
			public override string DoThing() {}
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

	}
}
