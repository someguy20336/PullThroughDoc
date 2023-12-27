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
}
