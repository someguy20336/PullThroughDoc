using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace PullThroughDoc.CodeFixes
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
            => SyntaxExtensions.GetTriviaFromBaseMember(pullThroughInfo, targetMember);
	}
}
