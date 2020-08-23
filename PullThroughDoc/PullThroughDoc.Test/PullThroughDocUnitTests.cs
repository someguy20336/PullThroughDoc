using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using PullThroughDoc;

namespace PullThroughDoc.Test
{
	[TestClass]
	public class UnitTest : CodeFixVerifier
	{

		//No diagnostics expected to show up
		[TestMethod]
		public void Doesnt_Trigger_For_Nothing()
		{
			var test = @"";

			VerifyCSharpDiagnostic(test);
		}

		//Diagnostic and CodeFix both triggered and checked for
		[TestMethod]
		public void Interface_Documentation_PullsThrough()
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
			var expected = new DiagnosticResult
			{
				Id = "PullThroughDoc",
				Message = String.Format("Pull through documentation for {0}.", "DoThing"),
				Severity = DiagnosticSeverity.Info,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", 18, 18)
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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

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
			var expected = new DiagnosticResult
			{
				Id = "PullThroughDoc",
				Message = String.Format("Pull through documentation for {0}.", "DoThing"),
				Severity = DiagnosticSeverity.Info,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", 20, 18)
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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

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

		[TestMethod]
		public void BaseObjectMethodOverride_NoAnalyzer()
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
        class TypeName 
        {   
			public override string ToString() {}
        }
    }";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void BaseClass_Documentation_PullsThrough()
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
				Id = "PullThroughDoc",
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
			/// <summary>Does A Thing </summary>
			public override string DoThing() {}
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}


		[TestMethod]
		public void Regions_Documentation_ExcludingRegion()
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
			var expected = new DiagnosticResult
			{
				Id = "PullThroughDoc",
				Message = String.Format("Pull through documentation for {0}.", "DoThing"),
				Severity = DiagnosticSeverity.Info,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", 20, 27)
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

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new PullThroughDocCodeFixProvider();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new PullThroughDocAnalyzer();
		}
	}
}
