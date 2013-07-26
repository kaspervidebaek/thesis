using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn.Compilers.CSharp;
using SyntaxDiff;

namespace Tests
{
    /// <summary>
    /// Summary description for TreeMerge
    /// </summary>
    [TestClass]
    public class TreeMerge
    {
        public TreeMerge()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        [TestCategory("TreeMerge")]
        public void TreeChangeTwoToSame()
        {
            var right = @"static void ShouldConflict()
                        {
                            if(true)
                                Console.WriteLine(""Hello, new World!"");
                        }
                        ";
            var bas = @"static void ShouldConflict()
                        {
                            if(false)
                                Console.WriteLine(""Hello, new World!"");
                        }   
                        ";
            var left = @"static void ShouldConflict()
                        {
                            if(true)
                                Console.WriteLine(""Hello, right!"");
                        }
                        ";
            var x = new SyntaxNodeSmartDiff();

            var lTree = x.SyntaxFromLines(left);
            var bTree = x.SyntaxFromLines(bas);
            var rTree = x.SyntaxFromLines(right);

            var merge = x.MergeNode(lTree, bTree, rTree);
        }


        [TestMethod]
        [TestCategory("TreeMerge")]
        public void TreeChangeAndDeleteSingleInBlock()
        {
            var right = @"static void ShouldConflict()
                        {
                            Test ();
                        }
                        ";
            var bas = @"static void ShouldConflict()
                        {
                            Test();
                        }   
                        ";
            var left = @"static void ShouldConflict()
                        {
                        }
                        ";
            var x = new SyntaxNodeSmartDiff();

            var lTree = x.SyntaxFromLines(left);
            var bTree = x.SyntaxFromLines(bas);
            var rTree = x.SyntaxFromLines(right);

            var merge = x.MergeNode(lTree, bTree, rTree);

            var mTree = x.SyntaxFromLines(merge);
        }

        [TestMethod]
        [TestCategory("TreeMerge")]
        public void TreeChangeInsertCall()
        {
            var right = @"static void ShouldConflict()
                        {
                            Test();
                            Test2 ();
                            Test();
                        }
                        ";
            var bas = @"static void ShouldConflict()
                        {
                            Test ();
                            Test ();
                        }   
                        ";
            var left = @"static void ShouldConflict()
                        {
                            Test  ();
                            Test3  ();
                        }
                        ";
            var x = new SyntaxNodeSmartDiff();

            var lTree = x.SyntaxFromLines(left);
            var bTree = x.SyntaxFromLines(bas);
            var rTree = x.SyntaxFromLines(right);

            var merge = x.MergeNode(lTree, bTree, rTree);

            var mTree = x.SyntaxFromLines(merge);
        }


        [TestMethod]
        [TestCategory("TreeMerge")]
        public void TreeChangeInsertIfLikeOnPaper()
        {
            var B = @"static void TreeChangeInsertIfLikeOnPaper()
                        {
                            Test1();
                            if(true) {
                                Test2(2, 2, 3, 4);
                                Test78();
                                Test3();
                            Test55()
                            Test4();
                            Test5();
                            }
                            Test6();
                            Test7();
                        }
                        ";
            var O = @"static void TreeChangeInsertIfLikeOnPaper()
                        {
                            Test1 ();
                            Test2 (2, 2, 3);
                            Test3 ();
                            Test4 ();
                            Test99();
                            Test5 ();
                            Test6 ();
                            Test7 ();
                        }   
                        ";
            var A = @"static void TreeChangeInsertIfLikeOnPaper()
                        {
                            Test1  ();
                            Test2  (2, 2, 3);
                            Test3  ();
                            if(true) {
                                Test4  ();
                                Test5  ();
                                Test78();
                                Test6  ();
                                Test7  ();
                            }
                            Test77  ();
                        }
                        ";
            var x = new SyntaxNodeSmartDiff();

            var aTree = x.SyntaxFromLines(A);
            var oTree = x.SyntaxFromLines(O);
            var bTree = x.SyntaxFromLines(B);

            var merge = x.MergeNode(aTree, oTree, bTree);

            Console.WriteLine(merge);

            var mTree = x.SyntaxFromLines(merge);


        }


        [TestMethod]
        [TestCategory("TreeMerge")]
        public void TreeChangeInsertIfAsBlock()
        {
            var right = @"static void ShouldConflict()
                        {
                            Test1();
                            Test2();
                            TestNew();
                            TestNew2();
                            Test3(1, 2, 3, 4, 5);
                            Test4();
                        }
                        ";
            var bas = @"static void ShouldConflict()
                        {
                            Test1 ();
                            Test2 ();
                            Test3 (10, 6, 5);
                            Test4 ();
                        }   
                        ";
            var left = @"static void ShouldConflict()
                        {
                            Test1  ();
                            if(true) {
                                Test2  ();
                                Test3  (6, 7, 8, 9);
                            }
                            Test4  ();
                        }
                        ";
            var x = new SyntaxNodeSmartDiff();

            var lTree = x.SyntaxFromLines(left);
            var bTree = x.SyntaxFromLines(bas);
            var rTree = x.SyntaxFromLines(right);

            var merge = x.MergeNode(lTree, bTree, rTree);

            Console.WriteLine(merge);

            var mTree = x.SyntaxFromLines(merge);

            
        }

        [TestMethod]
        [TestCategory("TreeMerge")]
        public void TreeChangeInsertIfAsStatement()
        {
            var right = @"static void ShouldConflict()
                        {
                            Test1();
                            Test2  (1, 2, 2, 19);
                            Test3();
                        }
                        ";
            var bas = @"static void ShouldConflict()
                        {
                            Test1 ();
                            Test2 (1, 2, 2, 3);
                            Test3 ();
                        }   
                        ";
            var left = @"static void ShouldConflict()
                        {
                            Test1  ();
                            if(true)
                                Test2(2, 2, 3);
                            Test3  ();
                        }
                        ";
            var x = new SyntaxNodeSmartDiff();

            var lTree = x.SyntaxFromLines(left);
            var bTree = x.SyntaxFromLines(bas);
            var rTree = x.SyntaxFromLines(right);

            var merge = x.MergeNode(lTree, bTree, rTree);

            Console.WriteLine(merge);

            var mTree = x.SyntaxFromLines(merge);


        }

        [TestMethod]
        [TestCategory("TreeMerge")]
        public void TreeChangeIfFromBlockToStatement()
        {
            var B = @"static void ShouldConflict()
                        {
                            Test1();
                            if(true) {
                                Test2(2, 2, 3, 4);
                                TestNew();
                            }
                            Test3();
                        }
                        ";
            var O = @"static void ShouldConflict()
                        {
                            Test1 ();
                            if(true) {
                                Test2 (2, 2, 3);
                                TestNew ();
                            }
                            Test3 ();
                        }   
                        ";
            var A = @"static void ShouldConflict()
                        {
                            Test1  ();
                            if(true)
                                Test2 (2, 2, 3);
                            Test3  ();
                        }
                        ";
            var x = new SyntaxNodeSmartDiff();

            var aTree = x.SyntaxFromLines(A);
            var oTree = x.SyntaxFromLines(O);
            var bTree = x.SyntaxFromLines(B);

            var merge = x.MergeNode(aTree, oTree, bTree);

            Console.WriteLine(merge);

            var mTree = x.SyntaxFromLines(merge);
        }

        [TestMethod]
        [TestCategory("TreeMerge")]
        public void TreeChangeIfFromBlockToStatementInsertOnTheOutside()
        {
            var B = @"static void ShouldConflict()
                        {
                            Test1();
                            if(true) {
                                Test2(2, 2, 3, 4);
                                TestNew();
                            }
                            Test3();
                        }
                        ";
            var O = @"static void ShouldConflict()
                        {
                            Test1 ();
                            if(true) {
                                Test2 (2, 2, 3);
                                TestNew ();
                            }
                            Test3 ();
                        }   
                        ";
            var A = @"static void ShouldConflict()
                        {
                            Test1  ();
                            Teeeest();
                            if(true)
                                Test2 (2, 2, 3);
                            TeeeestTWOOO();
                            Test3  ();
                        }
                        ";
            var x = new SyntaxNodeSmartDiff();

            var aTree = x.SyntaxFromLines(A);
            var oTree = x.SyntaxFromLines(O);
            var bTree = x.SyntaxFromLines(B);

            var merge = x.MergeNode(aTree, oTree, bTree);

            Console.WriteLine(merge);

            var mTree = x.SyntaxFromLines(merge);


        }


        [TestMethod]
        [TestCategory("TreeMerge")]
        public void TreeChangeInsertTwoNewIfStatements()
        {
            var B = @"static void ShouldConflict()
                        {
                            Test1();
                            TestOutside1();
                            Test2();
                            Test3();
                            TestOutside2();
                            Test4();   
                        }
                        ";
            var O = @"static void ShouldConflict()
                        {
                            Test1 ();
                            Test2 ();
                            Test3 ();
                            Test4 ();   
                        }   
                        ";
            var A = @"static void ShouldConflict()
                        {
                            Test1  ();
                            if(true)
                                Test2  ();
                            if(false)
                                Test3  ();
                            Test4  ();   
                        }
                        ";
            var x = new SyntaxNodeSmartDiff();

            var aTree = x.SyntaxFromLines(A);
            var oTree = x.SyntaxFromLines(O);
            var bTree = x.SyntaxFromLines(B);

            var merge = x.MergeNode(aTree, oTree, bTree);

            Console.WriteLine(merge);

            var mTree = x.SyntaxFromLines(merge);


        }

        [TestMethod]
        [TestCategory("TreeMerge")]
        public void TreeChangeIfFromStatementToBlock()
        {
            var right = @"static void ShouldConflict()
                        {
                            Test1();
                            if(true) {
                                Test2 (2, 2, 3);
                                TestNew ();
                            }
                            Test3();
                        }
                        ";
            var bas = @"static void ShouldConflict()
                        {
                            Test1 ();
                            if(true)
                                Test2 (1, 2, 2, 3);
                            Test3 ();
                        }   
                        ";
            var left = @"static void ShouldConflict()
                        {
                            Test1  ();
                            if(true)
                                Test2 (1, 2, 2, 3, 4);
                            Test3  ();
                        }
                        ";
            var x = new SyntaxNodeSmartDiff();

            var lTree = x.SyntaxFromLines(left);
            var bTree = x.SyntaxFromLines(bas);
            var rTree = x.SyntaxFromLines(right);

            var merge = x.MergeNode(lTree, bTree, rTree);

            Console.WriteLine(merge);

            var mTree = x.SyntaxFromLines(merge);


        }

        [TestMethod]
        [TestCategory("TreeMerge")]
        public void TreeChangeAndDeleteSingleInBlockWithChangeInOther()
        {
            var right = @"static void ShouldConflict()
                        {
                            Test2();
                        }
                        ";
            var bas = @"static void ShouldConflict()
                        {
                            Test();
                        }   
                        ";
            var left = @"static void ShouldConflict()
                        {
                        }
                        ";
            var x = new SyntaxNodeSmartDiff();

            var lTree = x.SyntaxFromLines(left);
            var bTree = x.SyntaxFromLines(bas);
            var rTree = x.SyntaxFromLines(right);

            var merge = x.MergeNode(lTree, bTree, rTree);

            var mTree = x.SyntaxFromLines(merge);
        }
    }
}
