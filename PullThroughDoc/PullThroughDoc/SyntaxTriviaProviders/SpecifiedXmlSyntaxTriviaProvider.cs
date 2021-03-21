using Microsoft.CodeAnalysis;
using System.Threading;

namespace PullThroughDoc
{
	internal class SpecifiedXmlSyntaxTriviaProvider : XmlSyntaxTriviaProvider
	{
		private readonly string _xml;

		public SpecifiedXmlSyntaxTriviaProvider(string xml, ISymbol targetMember, CancellationToken cancellation)
			: base(targetMember, cancellation)
		{
			_xml = xml;
		}

		protected override SyntaxTriviaList GetSyntaxTriviaCore()
		{
			return ParseExternalXml(_xml);
		}
	}
}
