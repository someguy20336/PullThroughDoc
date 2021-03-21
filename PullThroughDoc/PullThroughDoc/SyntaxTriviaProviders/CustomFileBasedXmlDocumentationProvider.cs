using Microsoft.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading;

namespace PullThroughDoc
{
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
