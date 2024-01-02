using Microsoft.CodeAnalysis;
using System.Globalization;
using System.Threading;

namespace PullThroughDoc.Test.Helpers
{
	class FakeVisualStudioDocumentationProvider : DocumentationProvider
	{
		private readonly string _doc;

		public override bool Equals(object obj)
		{
			return Equals(this, obj);
		}

		public override int GetHashCode() => _doc.GetHashCode();
		
		public FakeVisualStudioDocumentationProvider(string doc)
		{
			_doc = doc;
		}

		protected override string GetDocumentationForSymbol(string documentationMemberID, CultureInfo preferredCulture, CancellationToken cancellationToken = default)
		{
			return _doc;
		}


	}
}
