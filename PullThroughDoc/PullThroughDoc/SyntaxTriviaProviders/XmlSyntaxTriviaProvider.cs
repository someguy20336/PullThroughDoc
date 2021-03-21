using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;
using System.Threading;
using System.Xml;

namespace PullThroughDoc
{
	internal abstract class XmlSyntaxTriviaProvider : SyntaxTriviaProvider
	{
		private readonly ISymbol _targetMember;

		public XmlSyntaxTriviaProvider(ISymbol targetMember, CancellationToken cancellation)
			: base(cancellation)
		{
			_targetMember = targetMember;
		}


		protected SyntaxTriviaList ParseExternalXml(string xml)
		{
			if (string.IsNullOrEmpty(xml))
			{
				return new SyntaxTriviaList();
			}
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);

			var docNode = _targetMember.GetDocNodeForSymbol(Cancellation);
			string indent = docNode.GetIndentation().ToString();

			StringBuilder csharpDocComments = new StringBuilder();
			foreach (XmlNode node in doc.FirstChild.ChildNodes)
			{
				csharpDocComments.AppendLine($"{indent}/// {node.OuterXml}");
			}

			return SyntaxFactory.ParseLeadingTrivia(csharpDocComments.ToString());
		}
	}
}
