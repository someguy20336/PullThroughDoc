﻿using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace PullThroughDoc.Test
{
	/// <summary>
	/// Verifies a bunch of tests specific to interfaces
	/// </summary>
	[TestClass]
	public class InterfaceDocumentationTests : PullThroughDocCodeFixVerifier
	{

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

	}
}