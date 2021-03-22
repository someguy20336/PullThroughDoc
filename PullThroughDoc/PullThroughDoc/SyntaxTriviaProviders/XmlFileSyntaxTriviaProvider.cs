using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace PullThroughDoc
{
	// Note: I don't know that this would even be used that often
	internal class XmlFileSyntaxTriviaProvider : XmlSyntaxTriviaProvider
	{
		private static readonly ConcurrentDictionary<string, CustomFileBasedXmlDocumentationProvider> s_cachedDocProv
			= new ConcurrentDictionary<string, CustomFileBasedXmlDocumentationProvider>();
		private readonly ISymbol _baseSymbol;
		private readonly IEnumerable<MetadataReference> _metadataReferences;

		private static readonly string[] s_refAssembBasePaths = new[]
		{
			@"%ProgramFiles(x86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.X"
		};

		private static readonly HashSet<string> s_redirectToMsCorLib = new HashSet<string>()
		{
			"system.private.corelib.dll",
			// "netstandard.dll"
		};

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

		protected override string GetWellFormedXml()
		{

			PortableExecutableReference assembly = FindReference();

			if (assembly == null)
			{
				return "";
			}

			CustomFileBasedXmlDocumentationProvider docProvider = s_cachedDocProv.GetOrAdd(assembly.FilePath, assembPath =>
			{
				CustomFileBasedXmlDocumentationProvider foundProv;
				if (TryLoadXmlFileForAssemblyPath(assembPath, out foundProv))
				{
					return foundProv;
				}
				else if (TryProbeReferenceAssemblies(assembPath, out foundProv))
				{
					return foundProv;
				}
				return null;
			});

			if (docProvider == null)
			{
				return "";
			}

			return docProvider.GetDocumentation(_baseSymbol.GetDocumentationCommentId());
		}

		private PortableExecutableReference FindReference()
		{
			string assemb = _baseSymbol.ContainingAssembly.Name + ".dll";
			return _metadataReferences
				.OfType<PortableExecutableReference>()
				.Where(r => r.FilePath.EndsWith(assemb, StringComparison.OrdinalIgnoreCase))
				.FirstOrDefault();
		}

		private bool TryProbeReferenceAssemblies(string assembPath, out CustomFileBasedXmlDocumentationProvider docProvider)
		{
			FileInfo fileInfo = new FileInfo(assembPath);
			string assembName = fileInfo.Name;

			// hack because i don't know
			if (s_redirectToMsCorLib.Contains(assembName.ToLower()))
			{
				assembName = "mscorlib.dll";
			}

			foreach (string basePath in s_refAssembBasePaths)
			{
				string probePath = Environment.ExpandEnvironmentVariables(Path.Combine(basePath, assembName));

				if (TryLoadXmlFileForAssemblyPath(probePath, out docProvider))
				{
					return true;
				}
			}

			docProvider = null;
			return false;
		}


		private bool TryLoadXmlFileForAssemblyPath(string assembPath, out CustomFileBasedXmlDocumentationProvider docProvider)
		{
			docProvider = null;
			string xmlPath = Path.ChangeExtension(assembPath, "xml");
			if (string.IsNullOrEmpty(xmlPath))
			{
				return false;
			}

			if (!File.Exists(xmlPath))
			{
				return false;
			}

			docProvider = new CustomFileBasedXmlDocumentationProvider(xmlPath);

			return true;
		}

	}
}
