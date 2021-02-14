using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PullThroughDoc
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class PullThroughDocAnalyzer : DiagnosticAnalyzer
	{
		public const string PullThroughDocDiagId = "PullThroughDoc01";
		public const string SwapToInheritDocId = "PullThroughDoc02";

		// You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
		// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
		private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
		private const string Category = "Design";

		private static DiagnosticDescriptor PullThroughDocRule 
			= new DiagnosticDescriptor(PullThroughDocDiagId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);

		private static DiagnosticDescriptor SwapToInheritDocRule
			= new DiagnosticDescriptor(SwapToInheritDocId, "Replace With <inheritdoc/>", MessageFormat, Category, DiagnosticSeverity.Hidden, isEnabledByDefault: true, description: Description);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics 
			=> ImmutableArray.Create(PullThroughDocRule, SwapToInheritDocRule);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);		// Disabled for generated code - don't need it
			context.EnableConcurrentExecution();
			// TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
			// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
			context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Property, SymbolKind.Method);
		}

		private static void AnalyzeSymbol(SymbolAnalysisContext context)
		{
			string currentDoc = context.Symbol.GetDocumentationCommentXml(cancellationToken: context.CancellationToken);

			// Check if we can pull through the doc
			if (TryGetBaseMemberDoc(context, context.CancellationToken, out var baseDoc) && !string.IsNullOrEmpty(baseDoc))
			{
				DiagnosticDescriptor diagDesc = null;
				if (SuggestPullThroughOrInherit(currentDoc))
				{
					diagDesc = PullThroughDocRule;
					
				}
				else if (SuggestReplaceWithInheritDoc(currentDoc))
				{
					diagDesc = SwapToInheritDocRule;
				}

				if (diagDesc != null)
				{
					var diagnostic = Diagnostic.Create(diagDesc, context.Symbol.Locations[0], context.Symbol.Name);
					context.ReportDiagnostic(diagnostic);
				}
			}
		}

		private static bool SuggestPullThroughOrInherit(string currentDoc)
		{
			return string.IsNullOrEmpty(currentDoc);
		}

		private static bool SuggestReplaceWithInheritDoc(string currentDoc)
		{
			return !string.IsNullOrEmpty(currentDoc) && !currentDoc.Contains("inheritdoc");
		}

		private static bool TryGetBaseMemberDoc(SymbolAnalysisContext context, CancellationToken token, out string baseDoc)
		{
			baseDoc = null;
			// The containing type isn't an interface
			ISymbol symbol = context.Symbol;
			INamedTypeSymbol containingType = symbol.ContainingType;
			if (containingType.BaseType == null)
			{
				return false; // This is an interface
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
			baseDoc = baseSymbol.GetDocumentationCommentXml(cancellationToken: token);
			return true;

		}
	}
}
