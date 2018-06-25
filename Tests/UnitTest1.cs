using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        // Helpers
        // ----------------------------------------
        private TestContext testContextInstance;

        /// <summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        // Tests
        // ----------------------------------------
        [TestMethod]
        public void TestMethod1()
        {
            TestContext.WriteLine(message: PowerGlue.DisplayMethods.LookupPathFromMatch(
                    new PowerGlue.DisplayMethods.MatchParamters{FriendlyNameContains = "U2515H"}
                ));
            TestContext.WriteLine(message: PowerGlue.DisplayMethods.LookupPathFromMatch(
                 new PowerGlue.DisplayMethods.MatchParamters { EDIDManufactureCode = "LEN" }
             ));
        }
    }
}
