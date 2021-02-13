using System.Collections.Generic;
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
		protected override string Title => "Pull Through Documentation";

		protected override IEnumerable<SyntaxTrivia> GetTriviaFromMember(SyntaxNode syntax, SyntaxNode targetMember)
		{
			// Remove regions
			var nonRegion = syntax.GetLeadingTrivia().Where(tr => !tr.IsKind(SyntaxKind.RegionDirectiveTrivia)).ToList();
			if (nonRegion.Count == 0)
			{
				return nonRegion;
			}

			// Cut out duplicate whitespace trivia that might result from removing regions
			var newList = new List<SyntaxTrivia>() { nonRegion[0] };
			for (int i = 1; i < nonRegion.Count; i++)
			{
				bool isDoubleWhitespace = nonRegion[i - 1].Kind() == nonRegion[i].Kind() && nonRegion[i].IsKind(SyntaxKind.WhitespaceTrivia);
				if (!isDoubleWhitespace)
				{
					newList.Add(nonRegion[i]);
				}
			}

			return newList;
		}
	}
}
