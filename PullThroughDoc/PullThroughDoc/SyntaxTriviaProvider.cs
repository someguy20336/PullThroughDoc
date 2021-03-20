using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;

namespace PullThroughDoc
{
	public abstract class SyntaxTriviaProvider
	{

		private SyntaxTriviaList? _lazyTriviaList;

		protected CancellationToken Cancellation { get; }

		public SyntaxTriviaProvider(CancellationToken cancellation)
		{
			Cancellation = cancellation;
		}

		public static SyntaxTriviaProvider GetForSymbol(ISymbol targetMember, CancellationToken cancellation)
		{
			if (targetMember.GetDocNodeForSymbol(cancellation) != null)
			{
				return new SourceCodeSyntaxTriviaProvider(targetMember, cancellation);
			}
			else if (targetMember.DeclaringSyntaxReferences.IsEmpty)
			{
				// could have a doc provider, could not...

			}

			return null;
		}

		public SyntaxTriviaList GetSyntaxTrivia()
		{
			if (!_lazyTriviaList.HasValue)
			{
				_lazyTriviaList = GetSyntaxTriviaCore();
			}
			return _lazyTriviaList.Value;
		}

		protected abstract SyntaxTriviaList GetSyntaxTriviaCore();
	}

	internal class NullSyntaxTriviaProvider : SyntaxTriviaProvider
	{
		public NullSyntaxTriviaProvider() : base(default)
		{
		}

		protected override SyntaxTriviaList GetSyntaxTriviaCore()
		{
			return new SyntaxTriviaList();
		}
	}

	internal class SourceCodeSyntaxTriviaProvider : SyntaxTriviaProvider
	{
		private readonly ISymbol _symbol;

		public SourceCodeSyntaxTriviaProvider(ISymbol symbol, CancellationToken cancellation)
			:base(cancellation)
		{
			_symbol = symbol;
		}

		protected override SyntaxTriviaList GetSyntaxTriviaCore()
		{
			return _symbol.GetDocNodeForSymbol(Cancellation).GetLeadingTrivia();
		}
	}

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

	internal class XmlFileSyntaxTriviaProvider : XmlSyntaxTriviaProvider
	{
		private readonly ISymbol _baseSymbol;
		private readonly IEnumerable<MetadataReference> _metadataReferences;

		public XmlFileSyntaxTriviaProvider(
			ISymbol baseSymbol, 
			ISymbol targetMember, 
			IEnumerable<MetadataReference> metadataReferences,
			CancellationToken cancellation)
			: base(targetMember, cancellation)
		{
			_baseSymbol = baseSymbol;
			_metadataReferences = metadataReferences;
		}

		protected override SyntaxTriviaList GetSyntaxTriviaCore()
		{
			// TODO: load xml for metadata refs
			return ParseExternalXml("");
		}
	}
}
