using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace PullThroughDoc
{
	internal class PullThroughInfo
	{
		private readonly ISymbol _targetMember;
		private readonly CancellationToken _cancellation;
		private List<ISymbol> _baseMembers;


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

			var baseMembers = GetBaseMembers();
			if (baseMembers.Count == 0)
			{
				return false;
			}

			return true;
		}

		public bool HasBaseDocumentation()
		{
			return !string.IsNullOrEmpty(GetSummaryDocumentation());
		}

		public ISymbol GetSummaryDocSymbol()
		{
			if (!SupportsPullingThroughDoc())
			{
				return null;      // TODO exception?
			}

			return GetBaseMembers().First();
		}

		public string GetSummaryDocumentation()
		{
			if (!SupportsPullingThroughDoc())
			{
				return "";		// TODO exception?
			}

			return GetSummaryDocSymbol().GetDocumentationCommentXml(cancellationToken: _cancellation);
		}

		private List<ISymbol> GetBaseMembers()
		{
			if (_baseMembers != null)
			{
				return _baseMembers;
			}
			_baseMembers = new List<ISymbol>();

			ISymbol symbol = GetBaseOrInterfaceMember(_targetMember);
			while (symbol != null)
			{
				// Must exist in project
				if (symbol.DeclaringSyntaxReferences.IsEmpty)
				{
					break;
				}

				_baseMembers.Add(symbol);
				symbol = GetBaseOrInterfaceMember(symbol);
			}
			return _baseMembers;
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
