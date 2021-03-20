using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PullThroughDoc.Test
{
	/// <summary>
	/// Holds a bunch of tests specific to base classes
	/// </summary>
	[TestClass]
	public class BaseClassDocumentationTests : PullThroughDocCodeFixVerifier
	{

		protected override CodeFixProvider CodeFixProvider => new PullThroughDocCodeFixProvider();


		[TestMethod]
		public void BaseClass_Documentation_PullsThrough()
		{
			var test = @"
    namespace ConsoleApplication1
    {
		class BaseClass 
		{
			/// <summary>Does A Thing </summary>
			public virtual string DoThing() { }
		}
        class TypeName : BaseClass
        {   
			public override string DoThing() {}
        }
    }";

			ExpectPullThroughDiagnosticAt(test, "DoThing", 11, 27);

			var fixtest = @"
    namespace ConsoleApplication1
    {
		class BaseClass 
		{
			/// <summary>Does A Thing </summary>
			public virtual string DoThing() { }
		}
        class TypeName : BaseClass
        {   
			/// <summary>Does A Thing </summary>
			public override string DoThing() {}
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}


		[TestMethod]
		public void BaseClass_GetSetProperty_DocumentationPulledThrough()
		{
			var test = @"
    namespace ConsoleApplication1
    {
		class BaseClass 
		{
			/// <summary>Gets A Thing </summary>
			public virtual string GetsThing { get; set; }
		}
        class TypeName : BaseClass
        {   
			public override string GetsThing { get; set; }
        }
    }";

			ExpectPullThroughDiagnosticAt(test, "GetsThing", 11, 27);

			var fixtest = @"
    namespace ConsoleApplication1
    {
		class BaseClass 
		{
			/// <summary>Gets A Thing </summary>
			public virtual string GetsThing { get; set; }
		}
        class TypeName : BaseClass
        {   
			/// <summary>Gets A Thing </summary>
			public override string GetsThing { get; set; }
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

		[TestMethod]
		public void BaseClass_GetterOnlyProperty_DocumentationPulledThrough()
		{
			var test = @"
    namespace ConsoleApplication1
    {
		class BaseClass 
		{
			/// <summary>Gets A Thing </summary>
			public virtual string GetsThing { get => null; }
		}
        class TypeName : BaseClass
        {
			public override string GetsThing { get => null; }
        }
    }";
            ExpectPullThroughDiagnosticAt(test, "GetsThing", 11, 27);

            var fixtest = @"
    namespace ConsoleApplication1
    {
		class BaseClass 
		{
			/// <summary>Gets A Thing </summary>
			public virtual string GetsThing { get => null; }
		}
        class TypeName : BaseClass
        {
			/// <summary>Gets A Thing </summary>
			public override string GetsThing { get => null; }
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

        [TestMethod]
        public void ChangeToSummary_Works()
        {
            var test = @"
namespace ConsoleApplication1
{
	class BaseClass 
	{
		/// <summary>Gets A Thing </summary>
		public virtual string GetsThing { get => null; }
	}
    class TypeName : BaseClass
    {
		/// <inheritdocs />
		public override string GetsThing { get => null; }
    }
}";

            var fixtest = @"
namespace ConsoleApplication1
{
	class BaseClass 
	{
		/// <summary>Gets A Thing </summary>
		public virtual string GetsThing { get => null; }
	}
    class TypeName : BaseClass
    {
		/// <summary>Gets A Thing </summary>
		public override string GetsThing { get => null; }
    }
}";
            VerifyCSharpFix(test, fixtest);
        }


        [TestMethod]
        public void ChangeToSummary_MultipleLineBreaks_Works()
        {
            var test = @"
namespace ConsoleApplication1
{
	class BaseClass 
	{
		/// <summary>Gets A Thing </summary>
		public virtual string GetsThing { get => null; }
	}
    class TypeName : BaseClass
    {


		/// <inheritdocs />
		public override string GetsThing { get => null; }
    }
}";

            var fixtest = @"
namespace ConsoleApplication1
{
	class BaseClass 
	{
		/// <summary>Gets A Thing </summary>
		public virtual string GetsThing { get => null; }
	}
    class TypeName : BaseClass
    {


		/// <summary>Gets A Thing </summary>
		public override string GetsThing { get => null; }
    }
}";
            VerifyCSharpFix(test, fixtest);
        }

        [TestMethod]
        public void ChangeToSummary_MultipleLineSummary_Works()
        {
            var test = @"
namespace ConsoleApplication1
{
	class BaseClass 
	{
		/// <summary>
        /// Gets A Thing
        /// </summary>
		public virtual string GetsThing { get => null; }
	}
    class TypeName : BaseClass
    {
		/// <inheritdocs />
		public override string GetsThing { get => null; }
    }
}";

            var fixtest = @"
namespace ConsoleApplication1
{
	class BaseClass 
	{
		/// <summary>
        /// Gets A Thing
        /// </summary>
		public virtual string GetsThing { get => null; }
	}
    class TypeName : BaseClass
    {
		/// <summary>
        /// Gets A Thing
        /// </summary>
		public override string GetsThing { get => null; }
    }
}";
            VerifyCSharpFix(test, fixtest);
        }

		[TestMethod]
		public void MultiGeneration_SwitchToSummary_PullsFirstSummaryInHierarchy()
		{
			var test = @"
namespace ConsoleApplication1
{
	class BaseClass 
	{
		/// <summary>
        /// Gets A Thing
        /// </summary>
		public virtual string GetsThing { get => null; }
	}
    class SecondGen : BaseClass
    {
		/// <inheritdocs />
		public override string GetsThing { get => null; }
    }
    class ThirdGen : SecondGen
    {
		/// <inheritdocs />
		public override string GetsThing { get => null; }
    }
}";

			var fixtest = @"
namespace ConsoleApplication1
{
	class BaseClass 
	{
		/// <summary>
        /// Gets A Thing
        /// </summary>
		public virtual string GetsThing { get => null; }
	}
    class SecondGen : BaseClass
    {
		/// <inheritdocs />
		public override string GetsThing { get => null; }
    }
    class ThirdGen : SecondGen
    {
		/// <summary>
        /// Gets A Thing
        /// </summary>
		public override string GetsThing { get => null; }
    }
}";
			VerifySpecificCSharpFix(test, fixtest, 1);
		}
    }
}
