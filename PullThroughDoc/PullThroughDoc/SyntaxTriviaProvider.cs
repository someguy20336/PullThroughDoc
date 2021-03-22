using Microsoft.CodeAnalysis;
using System.Threading;

namespace PullThroughDoc
{
	public abstract class SyntaxTriviaProvider
	{

		private SyntaxTriviaList? _lazyTriviaList;

		protected CancellationToken Cancellation { get; }

		protected SyntaxTriviaProvider(CancellationToken cancellation)
		{
			Cancellation = cancellation;
		}

		public static SyntaxTriviaProvider GetForSymbol(ISymbol targetMember, CancellationToken cancellation)
		{
			if (targetMember.GetDocNodeForSymbol(cancellation) != null)
			{
				return new SourceCodeSyntaxTriviaProvider(targetMember, cancellation);
			}
			else if (targetMember.DeclaringSyntaxReferences.IsEmpty)
			{
				// could have a doc provider, could not...

			}

			return null;
		}

		public SyntaxTriviaList GetSyntaxTrivia()
		{
			if (!_lazyTriviaList.HasValue)
			{
				_lazyTriviaList = GetSyntaxTriviaCore();
			}
			return _lazyTriviaList.Value;
		}

		protected abstract SyntaxTriviaList GetSyntaxTriviaCore();
	}

	internal class NullSyntaxTriviaProvider : SyntaxTriviaProvider
	{
		public NullSyntaxTriviaProvider() : base(default)
		{
		}

		protected override SyntaxTriviaList GetSyntaxTriviaCore()
		{
			return new SyntaxTriviaList();
		}
	}
}
