using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PullThroughDoc;

internal static class SyntaxExtensions
{

	public static SyntaxNode GetDocNodeForSymbol(this ISymbol node, CancellationToken cancellation = default)
	{
		return node.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellation);
	}

	public static SyntaxTrivia GetIndentation(this SyntaxNode node)
	{
		IEnumerable<SyntaxTrivia> leadingTrivia = node.GetLeadingTrivia();
		return leadingTrivia.GetIndentation();
	}

	public static SyntaxTrivia GetIndentation(this IEnumerable<SyntaxTrivia> leadingTrivia)
	{
		return leadingTrivia.Last();
	}

    public static IEnumerable<SyntaxTrivia> CollapseWhitespace(this IEnumerable<SyntaxTrivia> trivia)
    {

        var triviaList = trivia.ToList();
        // Cut out duplicate whitespace trivia that might result from removing regions
        var newList = new List<SyntaxTrivia>() { triviaList[0] };
        for (int i = 1; i < triviaList.Count; i++)
        {
            bool isDoubleWhitespace = triviaList[i - 1].Kind() == triviaList[i].Kind() && triviaList[i].IsKind(SyntaxKind.WhitespaceTrivia);
            if (!isDoubleWhitespace)
            {
                newList.Add(triviaList[i]);
            }
        }
        return newList;
    }

	public static IEnumerable<SyntaxTrivia> GetTriviaFromBaseMember(PullThroughInfo pullThroughInfo, SyntaxNode targetMember) 
		=> CreateNewTrivia(pullThroughInfo.GetBaseMemberTrivia(), targetMember);

	public static IEnumerable<SyntaxTrivia> CreateNewTrivia(SyntaxTriviaList usingTrivia, SyntaxNode forMember)
	{
		IEnumerable<SyntaxTrivia> leadingTrivia = forMember.GetLeadingTrivia();
		var indentWhitespace = leadingTrivia.GetIndentation();
		leadingTrivia = leadingTrivia.Where(t => !t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)).CollapseWhitespace();

		// Grab only the doc comment trivia.  Seems to include a line break at the end
		IEnumerable<SyntaxTrivia> nonRegion = usingTrivia
			.Where(tr => tr.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
			.ToList();

		return leadingTrivia
			.Concat(nonRegion)
			.Concat(new[] { indentWhitespace });
	}

	public static IEnumerable<SyntaxTrivia> GetInheritDocTriviaForMember(SyntaxNode targetMember)
	{
		IEnumerable<SyntaxTrivia> leadingTrivia = targetMember.GetLeadingTrivia();
		SyntaxTrivia indentWhitespace = leadingTrivia.GetIndentation();

		leadingTrivia = leadingTrivia.Where(t => !t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)).CollapseWhitespace();

		var triviaList = SyntaxFactory.ParseLeadingTrivia("/// <inheritdoc/>");
		return leadingTrivia
			.Concat(triviaList)
			.Concat(new[] { SyntaxFactory.CarriageReturnLineFeed })
			.Concat(new[] { indentWhitespace });
	}
}
