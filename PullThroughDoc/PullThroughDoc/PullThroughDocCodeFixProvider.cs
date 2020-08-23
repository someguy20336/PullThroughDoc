using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace PullThroughDoc
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PullThroughDocCodeFixProvider)), Shared]
	public class PullThroughDocCodeFixProvider : CodeFixProvider
	{
		private const string title = "Pull Through Documentation";

		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create(PullThroughDocAnalyzer.DiagnosticId); }
		}

		public sealed override FixAllProvider GetFixAllProvider()
		{
			// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
			return WellKnownFixAllProviders.BatchFixer;
		}

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			// Find the type declaration identified by the diagnostic.
			var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MemberDeclarationSyntax>().First();

			// Register a code action that will invoke the fix.
			context.RegisterCodeFix(
				CodeAction.Create(
					title: title,
					createChangedDocument: c => PullThroughDocumentation(context.Document, declaration, c),
					equivalenceKey: title),
				diagnostic);
		}

		private async Task<Document> PullThroughDocumentation(Document document, MemberDeclarationSyntax membDecl, CancellationToken cancellationToken)
		{

			// Get the symbol representing the type to be renamed.
			var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
			var memberSymbol = semanticModel.GetDeclaredSymbol(membDecl, cancellationToken);

			ISymbol overrideSymb = memberSymbol.GetBaseOrInterfaceMember();

			if (overrideSymb == null)
			{
				return document;
			}

			if (overrideSymb.DeclaringSyntaxReferences.IsEmpty)
			{
				return document;
			}

			// Just use the first syntax reference because who cares at this point
			var syntax = overrideSymb.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken);
			var trivia = RemoveRegionsAndDuplicateWhitespace(syntax);
			MemberDeclarationSyntax newMembDecl = membDecl.WithLeadingTrivia(trivia);

			// Produce a new document
			SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
			SyntaxNode other = root.ReplaceNode(membDecl, newMembDecl);
			return document.WithSyntaxRoot(other);
		}

		private IEnumerable<SyntaxTrivia> RemoveRegionsAndDuplicateWhitespace(SyntaxNode syntax)
		{
			// Remove regions
			var nonRegion = syntax.GetLeadingTrivia().Where(tr => !tr.IsKind(SyntaxKind.RegionDirectiveTrivia)).ToList();
			if (nonRegion.Count == 0)
			{
				return nonRegion;
			}

			// Cut out duplicate whitespace trivia that might result from removing regions
			var newList = new List<SyntaxTrivia>() { nonRegion[0] };
			for (int i = 1; i < nonRegion.Count; i++)
			{
				bool isDoubleWhitespace = nonRegion[i - 1].Kind() == nonRegion[i].Kind() && nonRegion[i].IsKind(SyntaxKind.WhitespaceTrivia);
				if (!isDoubleWhitespace)
				{
					newList.Add(nonRegion[i]);
				}
			}

			return newList;
		}
	}
}
