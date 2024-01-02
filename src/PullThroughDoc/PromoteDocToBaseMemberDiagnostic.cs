using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;

namespace PullThroughDoc;

public class PromoteDocToBaseMemberDiagnostic
{

	public const string DiagnosticId = "PullThroughDoc04";

	public static readonly DiagnosticDescriptor Rule
		= new(DiagnosticId, "Promote doc to base member", "Promote doc to base member",
			"Documentation", DiagnosticSeverity.Hidden, isEnabledByDefault: true, 
			description: "Promotes the documentation of this member to the base member and inserts <inheritdoc/>.");

	public static void AnalyzeSymbol(PullThroughInfo pullThroughInfo, SymbolAnalysisContext context)
	{
		if (pullThroughInfo.SupportsPromotingToBaseMember())
		{
			var diagnostic = Diagnostic.Create(Rule, context.Symbol.Locations[0], context.Symbol.Name);
			context.ReportDiagnostic(diagnostic);
		}

	}
}
