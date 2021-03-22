using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PullThroughDoc
{
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
	}
}
