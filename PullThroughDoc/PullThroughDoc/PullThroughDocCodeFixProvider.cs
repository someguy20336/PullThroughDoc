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
		protected override string TitleForDiagnostic(string diagId) => "Pull Through Documentation";

		protected override IEnumerable<SyntaxTrivia> GetTriviaFromMember(SyntaxNode syntax, SyntaxNode targetMember)
		{
			// Remove regions
			var nonRegion = syntax.GetLeadingTrivia().Where(tr => !tr.IsKind(SyntaxKind.RegionDirectiveTrivia)).ToList();
			if (nonRegion.Count == 0)
			{
				return nonRegion;
			}

			return CollapseWhitespace(nonRegion);
		}
	}
}
