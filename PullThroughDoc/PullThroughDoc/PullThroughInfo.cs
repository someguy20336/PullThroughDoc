using Microsoft.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace PullThroughDoc
{
	internal class PullThroughInfo
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
			return !string.IsNullOrEmpty(GetSummaryDocumentation());
		}


		public string GetSummaryDocumentation()
		{
			if (!SupportsPullingThroughDoc())
			{
				return "";
			}

			return GetSummaryDocSymbol().GetDocumentationCommentXml(cancellationToken: _cancellation);
		}

		public ISymbol GetSummaryDocSymbol()
		{
			if (_summaryDocSymbol != null)
			{
				return _summaryDocSymbol;
			}

			ISymbol symbol = GetBaseOrInterfaceMember(_targetMember);
			while (symbol != null)
			{
				// Must exist in project
				if (symbol.DeclaringSyntaxReferences.IsEmpty)
				{
					break;
				}

				string baseDoc = symbol.GetDocumentationCommentXml(cancellationToken: _cancellation);

				// The first base member with a <summary> is what we will use
				if (baseDoc.Contains("<summary>"))
				{
					_summaryDocSymbol = symbol;
					break;
				}

				symbol = GetBaseOrInterfaceMember(symbol);
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
