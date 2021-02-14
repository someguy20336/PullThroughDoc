using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PullThroughDoc
{
	public abstract class DocumentationCodeFixProviderBase : CodeFixProvider
	{

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
			string title = TitleForDiagnostic(diagnostic.Id);
			context.RegisterCodeFix(
				CodeAction.Create(
					title: title,
					createChangedDocument: c => Execute(context.Document, declaration, c),
					equivalenceKey: title),
				diagnostic);
		}

		private async Task<Document> Execute(Document document, MemberDeclarationSyntax membDecl, CancellationToken cancellationToken)
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
			IEnumerable<SyntaxTrivia> trivia = GetTriviaFromMember(syntax, membDecl);
			MemberDeclarationSyntax newMembDecl = membDecl.WithLeadingTrivia(trivia);

			// Produce a new document
			SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
			SyntaxNode other = root.ReplaceNode(membDecl, newMembDecl);
			return document.WithSyntaxRoot(other);
		}

		protected abstract string TitleForDiagnostic(string diagId);

		protected abstract IEnumerable<SyntaxTrivia> GetTriviaFromMember(SyntaxNode baseMember, SyntaxNode targetMember);

		protected IEnumerable<SyntaxTrivia> CollapseWhitespace(IEnumerable<SyntaxTrivia> trivia)
		{

			var triviaList = trivia.ToList();
			// Cut out duplicate whitespace trivia that might result from removing regions
			var newList = new List<SyntaxTrivia>() { triviaList[0] };
			for (int i = 1; i < triviaList.Count; i++)
			{
				bool isDoubleWhitespace = triviaList[i - 1].Kind() == triviaList[i].Kind() && triviaList[i].IsKind(SyntaxKind.WhitespaceTrivia);
				if (!isDoubleWhitespace)
				{
					newList.Add(triviaList[i]);
				}
			}
			return newList;
		}
	}
}
