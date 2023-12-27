using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;

namespace PullThroughDoc
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PullThroughDocCodeFixProvider)), Shared]
	public class PullThroughDocCodeFixProvider : DocumentationCodeFixProviderBase
	{
		public override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create(PullThroughDocAnalyzer.PullThroughDocDiagId, PullThroughDocAnalyzer.SwapToPullThroughDocId); }
		}

		protected override string TitleForDiagnostic(string diagId)
		{
			switch (diagId)
			{
				case PullThroughDocAnalyzer.SwapToPullThroughDocId:
					return "Change to <summary>";
				case PullThroughDocAnalyzer.PullThroughDocDiagId:
				default:
					return "Pull Through Documentation";
			}
		}

		protected override IEnumerable<SyntaxTrivia> GetTriviaFromMember(PullThroughInfo pullThroughInfo, SyntaxNode targetMember)
		{
			IEnumerable<SyntaxTrivia> leadingTrivia = targetMember.GetLeadingTrivia();
			var indentWhitespace = leadingTrivia.GetIndentation();
			leadingTrivia = leadingTrivia.Where(t => !t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)).CollapseWhitespace();

			// Grab only the doc comment trivia.  Seems to include a line break at the end
			IEnumerable<SyntaxTrivia> nonRegion = pullThroughInfo.GetBaseMemberTrivia()
				.Where(tr => tr.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
				.ToList();
			
			return leadingTrivia
				.Concat(nonRegion)
				.Concat(new[] { indentWhitespace });
		}
	}
}
