using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Threading;

namespace PullThroughDoc
{
	public class PullThroughInfo
	{
		private readonly SyntaxTriviaProvider _targetMemberTriviaProvider;
		private SyntaxTriviaList? _lazyBaseMemberTrivia;
		private readonly ISymbol _targetMember;
		private readonly CancellationToken _cancellation;
		private ISymbol _summaryDocSymbol;

		public PullThroughInfo(
			ISymbol targetMember, 
			CancellationToken cancellation)
		{
			_targetMember = targetMember;
			_cancellation = cancellation;
			_targetMemberTriviaProvider = new SourceCodeSyntaxTriviaProvider(_targetMember, cancellation);
		}

		public bool SupportsPullingThroughDoc()
		{
			INamedTypeSymbol containingType = _targetMember.ContainingType;
			if (containingType.BaseType == null)
			{
				return false; // This is an interface
			}

			if (GetBaseSummaryDocSymbol() == null)
			{
				return false;
			}

			return true;
		}

		public bool SupportsPromotingToBaseMember()
		{
			var baseSymbol = GetBaseSummaryDocSymbol();
			if (baseSymbol == null)
			{
				return false;
			}

			if (baseSymbol.DeclaringSyntaxReferences.Length == 0)
			{
				return false;
			}

			if (IsInheritingDoc())
			{
				return false;
			}

			string baseDoc = GetBaseMemberTrivia().ToString();
			string targetDoc = GetTargetMemberTrivia().ToString();

			// TODO: normalize whitespace
			if (baseDoc == targetDoc)
			{
				return false;
			}

			return true;
		}

		public bool HasBaseSummaryDocumentation()
		{
			var trivia = GetBaseMemberTrivia();
			return trivia.Count > 0;
		}

		public SyntaxTriviaList GetBaseMemberTrivia()
		{
			if (!_lazyBaseMemberTrivia.HasValue)
			{
				var summaryDoc = GetBaseSummaryDocSymbol();
				if (summaryDoc == null)
				{
					return new SyntaxTriviaList();
				}
				SyntaxTriviaProvider prov = GetTriviaProviderForSymbol(summaryDoc);
				_lazyBaseMemberTrivia = prov.GetSyntaxTrivia();
			}

			return _lazyBaseMemberTrivia.Value;
		}

		private SyntaxTriviaProvider GetTriviaProviderForSymbol(ISymbol symbol)
		{
			if (symbol.DeclaringSyntaxReferences.IsEmpty)
			{
				string xml = symbol.GetDocumentationCommentXml(cancellationToken: _cancellation);
				if (!string.IsNullOrEmpty(xml))
				{
					return new SpecifiedXmlSyntaxTriviaProvider(
					xml,
					_targetMember,
					_cancellation
					);
				}
				return new NullSyntaxTriviaProvider();
			}

			return new SourceCodeSyntaxTriviaProvider(symbol, _cancellation);

		}

		public bool HasDocComments()
		{
			return GetTargetMemberTrivia()
				.Where(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
				.Count() > 0;
		}

		public bool SuggestReplaceWithInheritDoc()
		{
			SyntaxTriviaList trivia = GetTargetMemberTrivia();
			return HasDocComments() && !trivia.ToString().Contains("inheritdoc");
		}

		public bool IsInheritingDoc()
		{
			SyntaxTriviaList trivia = GetTargetMemberTrivia();
			return HasDocComments() && trivia.ToString().Contains("inheritdoc");
		}

		private SyntaxTriviaList GetTargetMemberTrivia()
		{
			return _targetMemberTriviaProvider.GetSyntaxTrivia();
		}

		public ISymbol GetBaseSummaryDocSymbol()
		{
			if (_summaryDocSymbol != null)
			{
				return _summaryDocSymbol;
			}

			_summaryDocSymbol = GetBaseOrInterfaceMember(_targetMember);
			while (_summaryDocSymbol != null)
			{
				var prov = GetTriviaProviderForSymbol(_summaryDocSymbol);
				string baseDoc = prov.GetSyntaxTrivia().ToString();

				// The first base member with a <summary> is what we will use
				if (baseDoc.Contains("<summary>"))
				{
					break;
				}

				_summaryDocSymbol = GetBaseOrInterfaceMember(_summaryDocSymbol);
			}
			return _summaryDocSymbol;
		}

		/// <summary>
		/// Gets the base member for the symbol, defined as either the base class member or interface definition
		/// </summary>
		/// <param name="symbol"></param>
		/// <returns></returns>
		private ISymbol GetBaseOrInterfaceMember(ISymbol symbol)
		{
			if (symbol is IMethodSymbol method)
			{
				if (method.IsOverride)
				{
					return method.OverriddenMethod;
				}
				return GetInterfaceMember(method);
			}
			else if (symbol is IPropertySymbol property)
			{
				if (property.IsOverride)
				{
					return property.OverriddenProperty;
				}
				return GetInterfaceMember(property);
			}

			return null;
		}

		private ISymbol GetInterfaceMember<T>(T symbol) where T : ISymbol
		{
			var members = symbol.ContainingType.AllInterfaces
				.SelectMany(inter => inter.GetMembers().OfType<T>());

			foreach (T member in members)
			{
				ISymbol impl = symbol.ContainingType.FindImplementationForInterfaceMember(member);
				if (impl != null && impl.Equals(symbol))
				{
					return member;
				}
			}

			return null;
		}


	}
}
