using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PullThroughDoc;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PullThroughDocAnalyzer : DiagnosticAnalyzer
{
	public const string PullThroughDocDiagId = "PullThroughDoc01";
	public const string SwapToInheritDocId = "PullThroughDoc02";
	public const string SwapToPullThroughDocId = "PullThroughDoc03";
	public const string Category = "Documentation";

	// You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
	// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
	private static readonly LocalizableString s_title = new LocalizableResourceString(nameof(Resources.PullThroghDocTitle), Resources.ResourceManager, typeof(Resources));
	private static readonly LocalizableString s_messageFormat = new LocalizableResourceString(nameof(Resources.PullThroughDocMessageFormat), Resources.ResourceManager, typeof(Resources));
	private static readonly LocalizableString s_description = new LocalizableResourceString(nameof(Resources.PullThroughDocDescription), Resources.ResourceManager, typeof(Resources));
	

	private static readonly DiagnosticDescriptor s_pullThroughDocRule 
		= new DiagnosticDescriptor(PullThroughDocDiagId, s_title, s_messageFormat, 
			Category, DiagnosticSeverity.Hidden, isEnabledByDefault: true, description: s_description);

	private static readonly DiagnosticDescriptor s_swapToInheritDocRule
		= new DiagnosticDescriptor(SwapToInheritDocId, "Replace with <inheritdoc/>", "Replace with <inheritdoc/>", 
			Category, DiagnosticSeverity.Hidden, isEnabledByDefault: true, description: "Replace <summary> with <inheritdoc>");

	private static readonly DiagnosticDescriptor s_swapToPullThroughDocRule
		= new DiagnosticDescriptor(SwapToPullThroughDocId, "Replace with base <summary>", "Replace with base <summary>", 
			Category, DiagnosticSeverity.Hidden, isEnabledByDefault: true, description: "Change to use the <summary> tag from the base class");


	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics 
		=> ImmutableArray.Create(s_pullThroughDocRule, s_swapToInheritDocRule, s_swapToPullThroughDocRule, PromoteDocToBaseMemberDiagnostic.Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);		// Disabled for generated code - don't need it
		context.EnableConcurrentExecution();

		// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
		context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Property, SymbolKind.Method);
	}

	private static void AnalyzeSymbol(SymbolAnalysisContext context)
	{

		PullThroughInfo pullThroughInfo = new PullThroughInfo(
			context.Symbol, 
			context.CancellationToken
			);

		// Check if we can pull through the doc
		if (pullThroughInfo.SupportsPullingThroughDoc() && pullThroughInfo.HasBaseSummaryDocumentation())
		{
			DiagnosticDescriptor diagDesc = null;
			if (!pullThroughInfo.HasDocComments())
			{
				diagDesc = s_pullThroughDocRule;
			}
			else if (pullThroughInfo.SuggestReplaceWithInheritDoc())
			{
				diagDesc = s_swapToInheritDocRule;
			}
			else if (pullThroughInfo.IsInheritingDoc())
			{
				diagDesc = s_swapToPullThroughDocRule;
			}

			if (diagDesc != null)
			{
				var diagnostic = Diagnostic.Create(diagDesc, context.Symbol.Locations[0], context.Symbol.Name);
				context.ReportDiagnostic(diagnostic);
			}
		}

		PromoteDocToBaseMemberDiagnostic.AnalyzeSymbol(pullThroughInfo, context);
	}
}
