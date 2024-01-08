using Microsoft.CodeAnalysis.Diagnostics;
using System;
using TestHelper;

namespace PullThroughDoc.Test
{
	public abstract class PullThroughDocCodeFixVerifier : CodeFixVerifier
	{
		protected void ExpectPullThroughDiagnosticAt(string text, string member, int line, int col)
		{
			var expectedDiagnostic = new DiagnosticResult
			{
				Id = PullThroughDocAnalyzer.PullThroughDocDiagId,
				Message = String.Format(PullThroughDocAnalyzer.PullThroughDocRule.MessageFormat.ToString(), member),
				Severity = PullThroughDocAnalyzer.PullThroughDocRule.DefaultSeverity,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", line, col)
						}
			};

			VerifyCSharpDiagnostic(text, expectedDiagnostic);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new PullThroughDocAnalyzer();
		}
	}
}
