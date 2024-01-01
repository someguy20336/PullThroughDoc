using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace PullThroughDoc;


[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PromoteDocToBaseMemberAnalyzer : DiagnosticAnalyzer
{

	public const string DiagnosticId = "PullThroughDoc04";

	public static readonly DiagnosticDescriptor Rule
		= new(DiagnosticId, "Promote doc to base member", "Promote doc to base member",
			"Documentation", DiagnosticSeverity.Hidden, isEnabledByDefault: true, 
			description: "Promotes the documentation of this member to the base member and inserts <inheritdoc/>.");


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
