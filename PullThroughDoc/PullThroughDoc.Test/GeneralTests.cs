using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PullThroughDoc.CodeFixes;

namespace PullThroughDoc.Test;

[TestClass]
public class GeneralTests : PullThroughDocCodeFixVerifier
{
	protected override CodeFixProvider GetCSharpCodeFixProvider() => new PullThroughDocCodeFixProvider();

	//No diagnostics expected to show up
	[TestMethod]
	public void Doesnt_Trigger_For_Nothing()
	{
		var test = @"";

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

	[TestMethod]
	public void Parameters_DocumentationIncluded()
	{
		var test = @"
	namespace ConsoleApplication1
	{
		class BaseClass 
		{
			/// <summary>
			/// Returns a thing
			/// </summary>
			/// <param name=""pullThroughInfo""></param>
			/// <param name=""targetMember""></param>
			/// <returns></returns>
			public virtual string DoThing(string variable) { }
		}
		class TypeName : BaseClass
		{   
			public override string DoThing(string variable) {}
		}
	}";


		var fixtest = @"
	namespace ConsoleApplication1
	{
		class BaseClass 
		{
			/// <summary>
			/// Returns a thing
			/// </summary>
			/// <param name=""pullThroughInfo""></param>
			/// <param name=""targetMember""></param>
			/// <returns></returns>
			public virtual string DoThing(string variable) { }
		}
		class TypeName : BaseClass
		{   
			/// <summary>
			/// Returns a thing
			/// </summary>
			/// <param name=""pullThroughInfo""></param>
			/// <param name=""targetMember""></param>
			/// <returns></returns>
			public override string DoThing(string variable) {}
		}
	}";
		VerifyCSharpFix(test, fixtest);
	}
}
