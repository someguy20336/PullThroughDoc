using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Composition;

namespace PullThroughDoc
{

	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InsertInheritDocCodeFixProvider)), Shared]
	public class InsertInheritDocCodeFixProvider : DocumentationCodeFixProviderBase
	{
		protected override string Title => "Insert <inhericdoc />";

		protected override IEnumerable<SyntaxTrivia> GetTriviaFromMember(SyntaxNode baseMember)
		{
            // https://stackoverflow.com/questions/41555962/how-to-add-xml-comments-to-a-methoddeclarationsyntax-node-using-roslyn
            var trivia = SyntaxFactory.DocumentationCommentTrivia(
                    SyntaxKind.SingleLineDocumentationCommentTrivia,
                    SyntaxFactory.List<XmlNodeSyntax>(
                        new XmlNodeSyntax[]{
                            SyntaxFactory.XmlText()
                            .WithTextTokens(
                                SyntaxFactory.TokenList(
                                    SyntaxFactory.XmlTextLiteral(
                                        SyntaxFactory.TriviaList(
                                            SyntaxFactory.DocumentationCommentExterior("///")),
                                        " ",
                                        " ",
                                        SyntaxFactory.TriviaList()))),
                            SyntaxFactory.XmlEmptyElement(SyntaxFactory.XmlName("inheritdoc")),
                            SyntaxFactory.XmlText()
                            .WithTextTokens(
                                SyntaxFactory.TokenList(
                                    SyntaxFactory.XmlTextNewLine(
                                        SyntaxFactory.TriviaList(),
                                        "\n",
                                        "\n",
                                        SyntaxFactory.TriviaList())))}));

            // var text = SyntaxFactory.XmlText("/// <inheritdoc />");

            return new[] { SyntaxFactory.Trivia(trivia) };
		}
	}
}
