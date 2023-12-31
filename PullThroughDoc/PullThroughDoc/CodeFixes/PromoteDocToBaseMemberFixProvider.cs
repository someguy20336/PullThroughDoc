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
	public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(PromoteDocToBaseMemberAnalyzer.DiagnosticId);

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
		if (!pullThroughInfo.HasDocComments() || pullThroughInfo.IsInheritingDoc())
		{
			return solution;
		}

		// TODO: would be really nice to simplify this...

		// Update the base member with the target member syntax
		ISymbol baseMemberSymb = pullThroughInfo.GetBaseSummaryDocSymbol();
		SyntaxNode baseSyntax = await baseMemberSymb.DeclaringSyntaxReferences.First().GetSyntaxAsync(cancellationToken);
		var promotedTrivia = SyntaxExtensions.CreateNewTrivia(pullThroughInfo.GetTargetMemberTrivia(), baseSyntax);
		var newBaseSyntax = baseSyntax.WithLeadingTrivia(promotedTrivia);
		// TODO: multiple projects!!
		Document baseDoc = solution.Projects.First().GetDocument(baseSyntax.SyntaxTree);
		SyntaxNode baseRoot = await baseDoc.GetSyntaxRootAsync(cancellationToken);
		SyntaxNode newBaseRoot = baseRoot.ReplaceNode(baseSyntax, newBaseSyntax);
		// TODO: I don't know what this preserver identity does
		solution = solution.WithDocumentSyntaxRoot(baseDoc.Id, newBaseRoot, PreservationMode.PreserveIdentity);

		// Update the target member with inherit doc
		document = solution.GetDocument(document.Id);		// could be changed above
		var inheritDocTrivia = SyntaxExtensions.GetInheritDocTriviaForMember(membDecl);
		var newMembDecl = membDecl.WithLeadingTrivia(inheritDocTrivia);
		SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
		// TODO: this isn't replacing the node, probably because it can change.  Figure this out
		SyntaxNode newRoot = root.ReplaceNode(membDecl, newMembDecl);
		solution = solution.WithDocumentSyntaxRoot(document.Id, newRoot);

		return solution;
	}
}
