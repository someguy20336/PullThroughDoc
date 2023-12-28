using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PullThroughDoc.CodeFixes;
using System;
using TestHelper;

namespace PullThroughDoc.Test;

[TestClass]
public class PromoteDocsToBaseMemberTest : PullThroughDocCodeFixVerifier
{
	protected override CodeFixProvider CodeFixProvider => new PromoteDocToBaseMemberFixProvider();

	protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
	{
		return new PromoteDocToBaseMemberAnalyzer();
	}

	[TestMethod]
	public void Analyzer_BaseClassNotInProject_DiagnosticNotFound()
	{
		var test = """
		namespace ConsoleApplication1
		{
			class TypeName
			{
				/// <summary>
				/// Override Docs
				/// </summary>
				public override string ToString() => null;
			}
		}
		""";
		VerifyCSharpDiagnostic(test);
	}

	[TestMethod]
	public void Analyzer_BaseClassHasSameTrivia_DiagnosticNotFound()
	{
		var test = """
		namespace ConsoleApplication1
		{
			class BaseClass 
			{
				/// <summary>
				/// Base Docs
				/// </summary>
				public virtual string TestMember() => null;
				
			}

			class TypeName : BaseClass
			{
				/// <summary>
				/// Base Docs
				/// </summary>
				public override string TestMember() => null;
			}
		}
		""";
		VerifyCSharpDiagnostic(test);
	}

	[TestMethod]
	public void Analyzer_TargetMemberIsInheritDoc_DiagnosticNotFound()
	{
		var test = """
		namespace ConsoleApplication1
		{
			class BaseClass 
			{
				/// <summary>
				/// Base Docs
				/// </summary>
				public virtual string TestMember() => null;
				
			}

			class TypeName : BaseClass
			{
				/// <inheritdoc/>
				public override string TestMember() => null;
			}
		}
		""";
		VerifyCSharpDiagnostic(test);
	}

	[TestMethod]
	public void Analyzer_TargetMemberHasDiffDoc_DiagnosticIsFound()
	{
		var test = """
		namespace ConsoleApplication1
		{
			class BaseClass 
			{
				/// <summary>
				/// Base Docs
				/// </summary>
				public virtual string TestMember() => null;				
			}

			class TypeName : BaseClass
			{				
				/// <summary>
				/// Override Docs
				/// </summary>
				public override string TestMember() => null;
			}
		}
		""";
		ExpectDiagnosticAt(test, 16, 26);
	}

	private void ExpectDiagnosticAt(string text, int line, int col)
	{
		var expectedDiagnostic = new DiagnosticResult
		{
			Id = PromoteDocToBaseMemberAnalyzer.DiagnosticId,
			Message = String.Format(PromoteDocToBaseMemberAnalyzer.Rule.MessageFormat.ToString()),
			Severity = DiagnosticSeverity.Hidden,
			Locations =
				new[] {
							new DiagnosticResultLocation("Test0.cs", line, col)
					}
		};

		VerifyCSharpDiagnostic(text, expectedDiagnostic);
	}
}
