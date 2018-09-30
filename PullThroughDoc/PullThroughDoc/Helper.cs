using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PullThroughDoc
{
	static class Helper
	{
		/// <summary>
		/// Gets the base member for the symbol, defined as either the base class member or interface definition
		/// </summary>
		/// <param name="symbol"></param>
		/// <returns></returns>
		public static ISymbol GetBaseOrInterfaceMember(this ISymbol symbol)
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

		private static ISymbol GetInterfaceMember<T>(T symbol) where T : ISymbol
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
