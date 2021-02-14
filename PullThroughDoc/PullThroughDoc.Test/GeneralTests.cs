using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;

namespace PullThroughDoc.Test
{
	[TestClass]
	public class GeneralTests : PullThroughDocCodeFixVerifier
	{

		//No diagnostics expected to show up
		[TestMethod]
		public void Doesnt_Trigger_For_Nothing()
		{
			var test = @"";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void BaseObjectMethodOverride_NoAnalyzer()
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
        class TypeName 
        {   
			public override string ToString() {}
        }
    }";

			VerifyCSharpDiagnostic(test);
		}

		[TestMethod]
		public void Regions_Documentation_ExcludingRegion()
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
			#region Properties
			/// <summary>Does A Thing </summary>
			public virtual string DoThing() { }
			#endregion
		}
        class TypeName : BaseClass
        {   
			public override string DoThing() {}
        }
    }";
			ExpectPullThroughDiagnosticAt(test, "DoThing", 20, 27);

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
			#region Properties
			/// <summary>Does A Thing </summary>
			public virtual string DoThing() { }
			#endregion
		}
        class TypeName : BaseClass
        {   
			/// <summary>Does A Thing </summary>
			public override string DoThing() {}
        }
    }";
			VerifyCSharpFix(test, fixtest);
		}

	}
}
