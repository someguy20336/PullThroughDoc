using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PullThroughDoc.CodeFixes;
using System;
using System.Linq;
using TestHelper;

namespace PullThroughDoc.Test;

[TestClass]
public class PromoteDocsToBaseMemberTests : PullThroughDocCodeFixVerifier
{
	protected override CodeFixProvider GetCSharpCodeFixProvider() => new PromoteDocToBaseMemberFixProvider();

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
		VeriftySpecificDiagnosticIsNotPresent(test, PromoteDocToBaseMemberDiagnostic.DiagnosticId);
	}

	[TestMethod]
	public void Analyzer_BaseClassHasSameTrivia_DifferentIndentation_DiagnosticNotFound()
	{
		var test = """
		class BaseClass 
		{
			/// <summary>
			/// Base Docs
			/// </summary>
			public virtual string TestMember() => null;
				
		}
		namespace ConsoleApplication1
		{
			class TypeName : BaseClass
			{
				/// <summary>
				/// Base Docs
				/// </summary>
				public override string TestMember() => null;
			}
		}
		""";
		VeriftySpecificDiagnosticIsNotPresent(test, PromoteDocToBaseMemberDiagnostic.DiagnosticId);
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
		VeriftySpecificDiagnosticIsNotPresent(test, PromoteDocToBaseMemberDiagnostic.DiagnosticId);
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


	[TestMethod]
	public void CodeFix_TargetMemberHasDiffDoc_SameFile_CodeFixIsApplied()
	{
		var oldSource = """
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

		var newSource = """
		namespace ConsoleApplication1
		{
			class BaseClass 
			{
				/// <summary>
				/// Override Docs
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
		VerifyCSharpFix(oldSource, newSource);
	}

	[TestMethod]
	public void CodeFix_TargetMemberHasDiffDoc_DiffFilesSameProject_CodeFixIsApplied()
	{
		Project proj = CreateProject([
			"""
			namespace ConsoleApplication1
			{
				class BaseClass 
				{				
					/// <summary>
					/// Base Docs
					/// </summary>
					public virtual string TestMember() => null;
				}
			}
			""",
			"""
			namespace ConsoleApplication1
			{
				class TypeName : BaseClass
				{				
					/// <summary>
					/// Override Docs
					/// </summary>
					public override string TestMember() => null;
				}
			}
			""",
		]);
		Solution newSol = ApplyFixAndGetNewSolution(proj.Documents.Last());

		string newFile1 = GetStringFromDocument(newSol.Projects.First().Documents.First());
		string newFile2 = GetStringFromDocument(newSol.Projects.First().Documents.Last());

		AssertEqualStrings("""
			namespace ConsoleApplication1
			{
				class BaseClass 
				{				
					/// <summary>
					/// Override Docs
					/// </summary>
					public virtual string TestMember() => null;
				}
			}
			""", newFile1);

		AssertEqualStrings("""
			namespace ConsoleApplication1
			{
				class TypeName : BaseClass
				{				
					/// <inheritdoc/>
					public override string TestMember() => null;
				}
			}
			""", newFile2);
	}

	// TODO: also set up a multiple project scenario

	private void ExpectDiagnosticAt(string text, int line, int col)
	{
		var expectedDiagnostic = new DiagnosticResult
		{
			Id = PromoteDocToBaseMemberDiagnostic.DiagnosticId,
			Message = String.Format(PromoteDocToBaseMemberDiagnostic.Rule.MessageFormat.ToString()),
			Severity = DiagnosticSeverity.Hidden,
			Locations = [ new DiagnosticResultLocation("Test0.cs", line, col) ]
		};

		VeriftySpecificDiagnosticIsPresent(text, expectedDiagnostic);
	}
}
