using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

namespace PullThroughDoc
{
	public class PullThroughInfo
	{
		private SyntaxTriviaList? _lazyTargetMemberTrivia;
		private SyntaxTriviaList? _lazyBaseMemberTrivia;
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

			if (GetBaseSummaryDocSymbol() == null)
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
				_lazyBaseMemberTrivia = GetBaseMemberTriviaCore();
			}

			return _lazyBaseMemberTrivia.Value;
		}

		public SyntaxTriviaList GetBaseMemberTriviaCore()
		{
			if (!SupportsPullingThroughDoc())
			{
				return new SyntaxTriviaList();
			}

			var summaryDoc = GetBaseSummaryDocSymbol();
			if (summaryDoc == null)
			{
				return new SyntaxTriviaList();
			}

			if (summaryDoc.DeclaringSyntaxReferences.IsEmpty)
			{
				return ParseExternalXml(
					GetBaseSummaryDocSymbol().GetDocumentationCommentXml(cancellationToken: _cancellation)
					);
			}

			var syntax = summaryDoc.GetDocNodeForSymbol(_cancellation);
			return syntax.GetLeadingTrivia();

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

		public bool SuggestReplaceWithPullThroughDoc()
		{
			SyntaxTriviaList trivia = GetTargetMemberTrivia();
			return HasDocComments() && trivia.ToString().Contains("inheritdoc");
		}

		private SyntaxTriviaList GetTargetMemberTrivia()
		{
			if (!_lazyTargetMemberTrivia.HasValue)
			{
				_lazyTargetMemberTrivia = _targetMember.GetDocNodeForSymbol(_cancellation).GetLeadingTrivia();
			}
			return _lazyTargetMemberTrivia.Value;
		}

		private ISymbol GetBaseSummaryDocSymbol()
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

		private SyntaxTriviaList ParseExternalXml(string xml)
		{
			if (string.IsNullOrEmpty(xml))
			{
				return new SyntaxTriviaList();
			}
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);

			var docNode = _targetMember.GetDocNodeForSymbol(_cancellation);
			string indent = docNode.GetIndentation().ToString();

			StringBuilder csharpDocComments = new StringBuilder();
			foreach (XmlNode node in doc.FirstChild.ChildNodes)
			{
				csharpDocComments.AppendLine($"{indent}/// {node.OuterXml}");
			}

			return SyntaxFactory.ParseLeadingTrivia(csharpDocComments.ToString());
		}
	}
}
