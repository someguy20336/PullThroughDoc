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

		protected sealed override SyntaxTriviaList GetSyntaxTriviaCore()
		{
			string xml = GetWellFormedXml();
			return ParseExternalXml(xml);
		}
		protected abstract string GetWellFormedXml();

		private SyntaxTriviaList ParseExternalXml(string xml)
		{
			if (string.IsNullOrEmpty(xml))
			{
				return new SyntaxTriviaList();
			}
			XmlDocument doc = new();

			try
			{
				doc.LoadXml(xml);
			}
			catch (System.Exception)
			{
				return new SyntaxTriviaList();
			}

			var docNode = _targetMember.GetDocNodeForSymbol(Cancellation);
			string indent = docNode.GetIndentation().ToString();

			StringBuilder csharpDocComments = new StringBuilder();
			foreach (XmlNode node in doc.FirstChild.ChildNodes)
			{
				string[] lines =  node.OuterXml.Split(new[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
				foreach (var line in lines)
				{
					csharpDocComments.AppendLine($"{indent}/// {line.Trim()}");
				}
			}

			return SyntaxFactory.ParseLeadingTrivia(csharpDocComments.ToString());
		}
	}
}
