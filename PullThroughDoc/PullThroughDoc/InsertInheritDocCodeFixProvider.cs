using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;

namespace PullThroughDoc
{

	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InsertInheritDocCodeFixProvider)), Shared]
	public class InsertInheritDocCodeFixProvider : DocumentationCodeFixProviderBase
	{
		protected override string Title => "Insert <inhericdoc />";

		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create(PullThroughDocAnalyzer.PullThroughDocDiagId, PullThroughDocAnalyzer.SwapToInheritDocId); }
		}

		protected override IEnumerable<SyntaxTrivia> GetTriviaFromMember(SyntaxNode baseMember, SyntaxNode targetMember)
		{
            var leadingTrivia = targetMember.GetLeadingTrivia();
			var indentWhitespace = leadingTrivia.Last();

            var triviaList = SyntaxFactory.ParseLeadingTrivia("/// <inheritdoc/>");
            return leadingTrivia
				.Concat(triviaList)
				.Concat(new [] { SyntaxFactory.CarriageReturnLineFeed })
				.Concat(new[] { indentWhitespace });
		}
	}
}
