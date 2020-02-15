using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PullThroughDoc
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class PullThroughDocAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "PullThroughDoc";

		// You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
		// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
		private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
		private const string Category = "Design";

		private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

		public override void Initialize(AnalysisContext context)
		{
			// TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
			// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
			context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Property, SymbolKind.Method);
		}

		private static void AnalyzeSymbol(SymbolAnalysisContext context)
		{
			// Check if we can pull through the doc
			if (CanPullThroughDoc(context, context.CancellationToken))
			{
				var diagnostic = Diagnostic.Create(Rule, context.Symbol.Locations[0], context.Symbol.Name);

				context.ReportDiagnostic(diagnostic);
			}
		}

		private static bool CanPullThroughDoc(SymbolAnalysisContext context, CancellationToken token)
		{
			// The containing type isn't an interface
			ISymbol symbol = context.Symbol;
			INamedTypeSymbol containingType = symbol.ContainingType;
			if (containingType.BaseType == null)
			{
				return false; // This is an interface
			}

			// Doesn't already have doc
			string currentDoc = symbol.GetDocumentationCommentXml(cancellationToken: token);
			if (!string.IsNullOrEmpty(currentDoc))
			{
				return false;
			}

			// Has a base symbol/interface method
			ISymbol baseSymbol = symbol.GetBaseOrInterfaceMember();
			if (baseSymbol == null)
			{
				return false;
			}

			// The base symbol should exist in this project
			if (baseSymbol.DeclaringSyntaxReferences.IsEmpty)
			{
				return false;
			}

			// Base method has documentation
			string baseDoc = baseSymbol.GetDocumentationCommentXml(cancellationToken: token);
			return !string.IsNullOrEmpty(baseDoc);

		}
	}
}
