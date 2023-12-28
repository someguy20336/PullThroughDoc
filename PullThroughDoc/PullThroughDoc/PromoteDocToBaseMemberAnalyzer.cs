using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace PullThroughDoc;


[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PromoteDocToBaseMemberAnalyzer : DiagnosticAnalyzer
{

	public const string DiagnosticId = "PullThroughDoc04";

	public static readonly DiagnosticDescriptor Rule
		= new(DiagnosticId, "Set as base doc  (todo name)", "Set as base doc (todo name)",
			"Documentation", DiagnosticSeverity.Hidden, isEnabledByDefault: true, description: "Todo this description");


	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		=> ImmutableArray.Create(Rule);

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);        // Disabled for generated code - don't need it
		context.EnableConcurrentExecution();

		// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
		context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Property, SymbolKind.Method);
	}

	private static void AnalyzeSymbol(SymbolAnalysisContext context)
	{

		PullThroughInfo pullThroughInfo = new(
			context.Symbol,
			context.CancellationToken
			);

		if (pullThroughInfo.SupportsPromotingToBaseMember())
		{
			var diagnostic = Diagnostic.Create(Rule, context.Symbol.Locations[0], context.Symbol.Name);
			context.ReportDiagnostic(diagnostic);
		}

	}
}
