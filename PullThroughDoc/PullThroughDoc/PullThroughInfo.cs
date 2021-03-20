using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Xml;

namespace PullThroughDoc
{
	public class PullThroughInfo
	{
		private readonly ISymbol _targetMember;
		private readonly CancellationToken _cancellation;
		private ISymbol _summaryDocSymbol;

		public PullThroughInfo(ISymbol targetMember, CancellationToken cancellation)
		{
			_targetMember = targetMember;
			_cancellation = cancellation;
		}

		public bool SupportsPullingThroughDoc()
		{
			INamedTypeSymbol containingType = _targetMember.ContainingType;
			if (containingType.BaseType == null)
			{
				return false; // This is an interface
			}

			if (GetSummaryDocSymbol() == null)
			{
				return false;
			}

			return true;
		}

		public bool HasBaseSummaryDocumentation()
		{
			var trivia = GetMemberTrivia();
			return !trivia.IsEmpty;
		}

		public ImmutableArray<SyntaxTrivia> GetMemberTrivia()
		{
			if (!SupportsPullingThroughDoc())
			{
				return ImmutableArray.Create<SyntaxTrivia>();
			}

			var summaryDoc = GetSummaryDocSymbol();
			if (summaryDoc == null)
			{
				return ImmutableArray.Create<SyntaxTrivia>();
			}

			if (summaryDoc.DeclaringSyntaxReferences.IsEmpty)
			{
				return ParseExternalXml();
			}

			var syntax = summaryDoc.DeclaringSyntaxReferences[0].GetSyntax(_cancellation);
			return syntax.GetLeadingTrivia().ToImmutableArray();

		}

		private ISymbol GetSummaryDocSymbol()
		{
			if (_summaryDocSymbol != null)
			{
				return _summaryDocSymbol;
			}

			_summaryDocSymbol = GetBaseOrInterfaceMember(_targetMember);
			while (_summaryDocSymbol != null)
			{
				string baseDoc = _summaryDocSymbol.GetDocumentationCommentXml(cancellationToken: _cancellation);

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

		private ImmutableArray<SyntaxTrivia> ParseExternalXml()
		{
			string xml = GetSummaryDocSymbol().GetDocumentationCommentXml(cancellationToken: _cancellation);
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);

			// TODO how to parse this xml into syntax trivia?

			SyntaxTriviaList trivia = SyntaxFactory.ParseLeadingTrivia(xml);
			return trivia.ToImmutableArray();
		}
	}
}
