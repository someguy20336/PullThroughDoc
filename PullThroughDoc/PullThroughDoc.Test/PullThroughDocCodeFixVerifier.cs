using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;

namespace PullThroughDoc.Test
{
	public class PullThroughDocCodeFixVerifier : CodeFixVerifier
	{
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
