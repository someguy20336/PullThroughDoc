using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

namespace PullThroughDoc
{
	public abstract class SyntaxTriviaProvider
	{

		private SyntaxTriviaList? _lazyTriviaList;

		protected CancellationToken Cancellation { get; }

		protected SyntaxTriviaProvider(CancellationToken cancellation)
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
		private static readonly ConcurrentDictionary<string, CustomFileBasedXmlDocumentationProvider> s_cachedDocProv 
			= new ConcurrentDictionary<string, CustomFileBasedXmlDocumentationProvider>();
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
			PortableExecutableReference assembly = FindReference();

			string xml = "";

			if (TryLoadXmlFile(assembly, out CustomFileBasedXmlDocumentationProvider docProvider))
			{
				xml = docProvider.GetDocumentation(_baseSymbol.GetDocumentationCommentId());
			}

			// TODO: this dummy thing... works.  But when would I actually want it?  It would cause
			// the action for every single prop/method
			//else
			//{
			//	xml = "<doc><summary>dummy</summary></doc>";
			//}

			return ParseExternalXml(xml);  
		}

		private PortableExecutableReference FindReference()
		{
			string assemb = _baseSymbol.ContainingAssembly.Name + ".dll";
			return _metadataReferences
				.OfType<PortableExecutableReference>()
				.Where(r => r.Display.EndsWith(assemb))
				.FirstOrDefault();
		}

		private bool TryLoadXmlFile(PortableExecutableReference assembly, out CustomFileBasedXmlDocumentationProvider docProvider)
		{
			docProvider = null;
			if (assembly == null)
			{
				return false;
			}

			string xmlPath = Path.ChangeExtension(assembly.FilePath, "xml");
			if (!File.Exists(xmlPath))
			{
				return false;
			}

			docProvider = s_cachedDocProv.GetOrAdd(xmlPath, key =>
			{
				return new CustomFileBasedXmlDocumentationProvider(xmlPath);
			});

			return true;
		}

		// https://github.com/dotnet/roslyn/blob/main/src/Workspaces/Core/Portable/Utilities/Documentation/XmlDocumentationProvider.cs
		internal sealed class CustomFileBasedXmlDocumentationProvider : XmlDocumentationProvider
		{
			private readonly string _filePath;

			public CustomFileBasedXmlDocumentationProvider(string filePath)
			{
				_filePath = filePath;
			}

			public string GetDocumentation(string documentationId)
				=> GetDocumentationForSymbol(documentationId, CultureInfo.CurrentCulture);

			protected override string GetDocumentationForSymbol(string documentationMemberID, CultureInfo preferredCulture, CancellationToken cancellationToken = default)
			{
				string xml = base.GetDocumentationForSymbol(documentationMemberID, preferredCulture, cancellationToken);

				// It doesn't appear to return in structured xml, so wrap with "<doc>"
				return $"<doc>{xml}</doc>";
			}

			protected override Stream GetSourceStream(CancellationToken cancellationToken)
				=> new FileStream(_filePath, FileMode.Open, FileAccess.Read);

			public override bool Equals(object obj)
			{
				return obj is CustomFileBasedXmlDocumentationProvider other && _filePath == other._filePath;
			}

			public override int GetHashCode()
				=> _filePath.GetHashCode();
		}
	}
}
