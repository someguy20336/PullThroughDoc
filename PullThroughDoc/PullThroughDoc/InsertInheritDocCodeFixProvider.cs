using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Composition;
using System.Linq;

namespace PullThroughDoc
{

	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InsertInheritDocCodeFixProvider)), Shared]
	public class InsertInheritDocCodeFixProvider : DocumentationCodeFixProviderBase
	{
		protected override string Title => "Insert <inhericdoc />";

		protected override IEnumerable<SyntaxTrivia> GetTriviaFromMember(SyntaxNode baseMember, SyntaxNode targetMember)
		{
            var leadingTrivia = targetMember.GetLeadingTrivia();
            var triviaList = SyntaxFactory.ParseLeadingTrivia("/// <inheritdoc/>");
            return leadingTrivia
				.Concat(triviaList)
				.Concat(new[] { SyntaxFactory.CarriageReturnLineFeed })
				.Concat(leadingTrivia);
		}
	}
}
