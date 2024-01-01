using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;

namespace PullThroughDoc.CodeFixes
{

	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InsertInheritDocCodeFixProvider)), Shared]
	public class InsertInheritDocCodeFixProvider : DocumentationCodeFixProviderBase
	{
		protected override string TitleForDiagnostic(string diagId)
		{
			switch (diagId)
			{
				case PullThroughDocAnalyzer.SwapToInheritDocId:
					return "Change to <inheritdoc />";
				case PullThroughDocAnalyzer.PullThroughDocDiagId:
				default:
					return "Insert <inheritdoc />";
			}
		}

		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create(PullThroughDocAnalyzer.PullThroughDocDiagId, PullThroughDocAnalyzer.SwapToInheritDocId); }
		}


		protected override IEnumerable<SyntaxTrivia> GetTriviaFromMember(PullThroughInfo pullThroughInfo, SyntaxNode targetMember) 
			=> SyntaxExtensions.GetInheritDocTriviaForMember(targetMember);
	}
}
