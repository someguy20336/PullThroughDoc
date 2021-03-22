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
			_xml = EnsureWellFormed(xml);
		}

		protected override string GetWellFormedXml()
		{
			return _xml;
		}

		private string EnsureWellFormed(string xml)
		{
			if (xml.TrimStart().StartsWith("<summary>", System.StringComparison.OrdinalIgnoreCase))
			{
				return $"<doc>{xml}</doc>";
			}
			return xml;
		}
	}
}
