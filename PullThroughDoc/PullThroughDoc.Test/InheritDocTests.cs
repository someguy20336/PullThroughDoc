using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace PullThroughDoc.Test
{
	[TestClass]
	public class InheritDocTests : PullThroughDocCodeFixVerifier
	{
		[TestMethod]
		public void BaseClass_WithExtraSpace_AddsInhertiDoc()
		{
			var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
		class BaseClass 
		{
			/// <summary>Does A Thing </summary>
			public virtual string DoThing() { }
		}
        class TypeName : BaseClass
        {

			public override string DoThing() {}
        }
    }";
			var expected = new DiagnosticResult
			{
				Id = PullThroughDocAnalyzer.PullThroughDocDiagId,
				Message = String.Format("Pull through documentation for {0}.", "DoThing"),
				Severity = DiagnosticSeverity.Info,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", 19, 27)
						}
			};

			VerifyCSharpDiagnostic(test, expected);

			var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
		class BaseClass 
		{
			/// <summary>Does A Thing </summary>
			public virtual string DoThing() { }
		}
        class TypeName : BaseClass
        {

			/// <inheritdoc/>
			public override string DoThing() {}
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

		[TestMethod]
		public void BaseClass_NowLineBreakAfterBrace_AddsInhertiDoc()
		{
			var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
		class BaseClass 
		{
			/// <summary>Does A Thing </summary>
			public virtual string DoThing() { }
		}
        class TypeName : BaseClass
        {
			public override string DoThing() {}
        }
    }";
			var expected = new DiagnosticResult
			{
				Id = PullThroughDocAnalyzer.PullThroughDocDiagId,
				Message = String.Format("Pull through documentation for {0}.", "DoThing"),
				Severity = DiagnosticSeverity.Info,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", 18, 27)
						}
			};

			VerifyCSharpDiagnostic(test, expected);

			var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
		class BaseClass 
		{
			/// <summary>Does A Thing </summary>
			public virtual string DoThing() { }
		}
        class TypeName : BaseClass
        {
			/// <inheritdoc/>
			public override string DoThing() {}
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}



		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new InsertInheritDocCodeFixProvider();
		}
	}
}
