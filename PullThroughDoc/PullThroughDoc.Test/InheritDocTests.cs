using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PullThroughDoc.Test
{
	[TestClass]
	public class InheritDocTests : PullThroughDocCodeFixVerifier
	{
		[TestInitialize]
		public void Init()
		{
			CodeFixProvider = new InsertInheritDocCodeFixProvider();
		}

		[TestMethod]
		public void BaseClass_WithExtraSpace_AddsInhertiDoc()
		{
			var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

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
			ExpectPullThroughDiagnosticAt(test, "DoThing", 19, 27);

			var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
		class BaseClass 
		{
			/// <summary>Does A Thing </summary>
			public virtual string DoThing() { }
		}
        class TypeName : BaseClass
        {

			/// <inheritdoc/>
			public override string DoThing() {}
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

		[TestMethod]
		public void BaseClass_NowLineBreakAfterBrace_AddsInhertiDoc()
		{
			var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

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
			ExpectPullThroughDiagnosticAt(test, "DoThing", 18, 27);

			var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
		class BaseClass 
		{
			/// <summary>Does A Thing </summary>
			public virtual string DoThing() { }
		}
        class TypeName : BaseClass
        {
			/// <inheritdoc/>
			public override string DoThing() {}
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

		[TestMethod]
		public void SwapToInherit_AddsInhertiDoc()
		{
			var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
		class BaseClass 
		{
			/// <summary>Does A Thing </summary>
			public virtual string DoThing() { }
		}
        class TypeName : BaseClass
        {
			/// <summary>test</summary>
			public override string DoThing() {}
        }
    }";

			var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
		class BaseClass 
		{
			/// <summary>Does A Thing </summary>
			public virtual string DoThing() { }
		}
        class TypeName : BaseClass
        {
			/// <inheritdoc/>
			public override string DoThing() {}
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

        [TestMethod]
        public void SwapToInherit_MultipleLineBreaks_AddsInhertiDoc()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
		class BaseClass 
		{
			/// <summary>Does A Thing </summary>
			public virtual string DoThing() { }
		}
        class TypeName : BaseClass
        {


			/// <summary>test</summary>
			public override string DoThing() {}
        }
    }";

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
		class BaseClass 
		{
			/// <summary>Does A Thing </summary>
			public virtual string DoThing() { }
		}
        class TypeName : BaseClass
        {


			/// <inheritdoc/>
			public override string DoThing() {}
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        [TestMethod]
        public void SwapToInherit_MultipleLineSummary_AddsInhertiDoc()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
		class BaseClass 
		{
			/// <summary>Does A Thing </summary>
			public virtual string DoThing() { }
		}
        class TypeName : BaseClass
        {
			/// <summary>
            /// test
            /// </summary>
			public override string DoThing() {}
        }
    }";

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
		class BaseClass 
		{
			/// <summary>Does A Thing </summary>
			public virtual string DoThing() { }
		}
        class TypeName : BaseClass
        {
			/// <inheritdoc/>
			public override string DoThing() {}
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }
    }
}
