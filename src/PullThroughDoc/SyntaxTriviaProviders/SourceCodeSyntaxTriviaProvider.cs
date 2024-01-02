using Microsoft.CodeAnalysis;
using System.Threading;

namespace PullThroughDoc
{
	internal class SourceCodeSyntaxTriviaProvider : SyntaxTriviaProvider
	{
		private readonly ISymbol _symbol;

		public SourceCodeSyntaxTriviaProvider(ISymbol symbol, CancellationToken cancellation)
			: base(cancellation)
		{
			_symbol = symbol;
		}

		protected override SyntaxTriviaList GetSyntaxTriviaCore()
		{
			return _symbol.GetDocNodeForSymbol(Cancellation).GetLeadingTrivia();
		}
	}
}
