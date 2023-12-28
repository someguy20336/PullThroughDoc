using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PullThroughDoc.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PromoteDocToBaseMemberFixProvider)), Shared]
public class PromoteDocToBaseMemberFixProvider : CodeFixProvider
{
	public override ImmutableArray<string> FixableDiagnosticIds => throw new NotImplementedException();

	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		var diagnostic = context.Diagnostics.First();
		var diagnosticSpan = diagnostic.Location.SourceSpan;

		// Find the type declaration identified by the diagnostic.
		var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MemberDeclarationSyntax>().First();

		// Register a code action that will invoke the fix.
		string title = "Promote to base member (TODO)";
		context.RegisterCodeFix(
			CodeAction.Create(
				title: title,
				createChangedSolution: c => Execute(context.Document, declaration, c),
				equivalenceKey: title),
			diagnostic);
	}

	private async Task<Solution> Execute(Document document, MemberDeclarationSyntax membDecl, CancellationToken cancellationToken)
	{
		// Get the symbol representing the type to be renamed.
		var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
		var memberSymbol = semanticModel.GetDeclaredSymbol(membDecl, cancellationToken);

		PullThroughInfo pullThroughInfo = new(memberSymbol, cancellationToken);

		// No doc comments or it is <inheritdoc> already, don't do anything
		var solution = document.Project.Solution;
		if (!pullThroughInfo.HasDocComments() || pullThroughInfo.SuggestReplaceWithPullThroughDoc())
		{
			return solution;
		}

		ISymbol baseMemberSymb = pullThroughInfo.GetBaseSummaryDocSymbol();
		// TODO: Base member needs to exist in this project... so look out for that
		// Maybe move that logic to the handler class?
		// TODO: the doc also needs to be different between the two

		// TODO: Get the trivia from the target member

		// Update the target member with inherit doc
		var inheritDocTrivia = SyntaxExtensions.GetInheritDocTriviaForMember(membDecl);
		var newMembDecl = membDecl.WithLeadingTrivia(inheritDocTrivia);
		SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
		SyntaxNode newRoot = root.ReplaceNode(membDecl, newMembDecl);
		solution = solution.WithDocumentSyntaxRoot(document.Id, newRoot);

		// TODO Update the base member with that trivia from the target member


		return solution;
	}
}
