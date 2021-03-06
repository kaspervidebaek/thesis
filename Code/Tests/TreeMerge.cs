﻿using System;
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
        public void TreeChangeSameline()
        {
            var right = @"void F()
                        {
                            O.G(""N"");
                        }
                        ";
            var bas = @"void F()
                        {
                            O.G(""O"");
                        }   
                        ";
            var left = @"void F()
                        {
                            O.H(""O"");
                        }
                        ";
            var x = new SyntaxNodeSmartDiff();

            var lTree = x.SyntaxFromLines(left);
            var bTree = x.SyntaxFromLines(bas);
            var rTree = x.SyntaxFromLines(right);

            var merge = x.MergeNode(lTree, bTree, rTree);

            Console.WriteLine(merge);
        }

        [TestMethod]
        [TestCategory("TreeMerge")]
        public void TreeChangeTwoToSame()
        {
            var right = @"static void TreeChangeTwoToSame()
                        {
                            if(true)
                                Console.WriteLine(""Hello, new World!"");
                        }
                        ";
            var bas = @"static void TreeChangeTwoToSame()
                        {
                            if(false)
                                Console.WriteLine(""Hello, new World!"");
                        }   
                        ";
            var left = @"static void TreeChangeTwoToSame()
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
        public void TreeChangeParameterOrder()
        {
            var right = @"static void TreeChangeTwoToSame(int a, string b)
                        {
                            Console.WriteLine(1, 2, 4, 3, 5);
                        }
                        ";
            var bas = @"static void TreeChangeTwoToSame(int a)
                        {
                            Console.WriteLine(1, 2, 3, 5, 4);
                        }   
                        ";
            var left = @"static void TreeChangeTwoToSame(int b)
                        {
                            Console.WriteLine(4, 1, 2, 3, 5);
                        }
                        ";
            var x = new SyntaxNodeSmartDiff();

            var lTree = x.SyntaxFromLines(left);
            var bTree = x.SyntaxFromLines(bas);
            var rTree = x.SyntaxFromLines(right);

            var merge = x.MergeNode(lTree, bTree, rTree);
            Console.WriteLine(merge);
            Console.ReadLine();
        }



        [TestMethod]
        [TestCategory("TreeMerge")]
        public void TreeChangeParameterOrderAndType()
        {
            var right = @"static void TreeChangeTwoToSame(string b, int a)
                        {
                            TreeChangeTwoToSame('test', 1);
                        }
                        ".Replace('\'', '"'); ;
            var bas = @"static void TreeChangeTwoToSame(int a, string b)
                        {
                            TreeChangeTwoToSame(1, 'test');
                        }   
                        ".Replace('\'', '"'); ;
            var left = @"static void TreeChangeTwoToSame(int a, string b, int newInt)
                        {
                            TreeChangeTwoToSame(1, 'test', 2);
                        }
                        ".Replace('\'', '"'); ;
            var x = new SyntaxNodeSmartDiff();

            var lTree = x.SyntaxFromLines(left);
            var bTree = x.SyntaxFromLines(bas);
            var rTree = x.SyntaxFromLines(right);

            var merge = x.MergeNode(lTree, bTree, rTree);
            Console.WriteLine(merge);
        }
        [TestMethod]
        [TestCategory("TreeMerge")]
        public void TreeChangeAndDeleteSingleInBlock()
        {
            var right = @"static void TreeChangeAndDeleteSingleInBlock()
                        {
                            Test ();
                        }
                        ";
            var bas = @"static void TreeChangeAndDeleteSingleInBlock()
                        {
                            Test();
                        }   
                        ";
            var left = @"static void TreeChangeAndDeleteSingleInBlock()
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
            var right = @"static void TreeChangeInsertCall()
                        {
                            Test();
                            Test2 ();
                            Test();
                        }
                        ";
            var bas = @"static void TreeChangeInsertCall()
                        {
                            Test ();
                            Test ();
                        }   
                        ";
            var left = @"static void TreeChangeInsertCall()
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
            Console.WriteLine(merge);

            var mTree = x.SyntaxFromLines(merge);
        }


        [TestMethod]
        [TestCategory("TreeMerge")]
        public void TreeChangeInsertIfLikeOnPaper()
        {
            var B = @"static void TreeChangeInsertIfLikeOnPaper()
                        {
                            if(true) {
                                F1(1, 2, 3);
                                I1 ();
                                F2();
                            }
                            F3();
                            F4();
                            I2();
                            F5(1, 2, 3, 4);
                        }
                        ";
            var O = @"static void TreeChangeInsertIfLikeOnPaper()
                        {
                            F1 (1, 2, 3);
                            F2 ();
                            F3 ();
                            F4 ();
                            F5 (1, 2, 3);
                        }   
                        ";
            var A = @"static void TreeChangeInsertIfLikeOnPaper()
                        {
                            F1  (1, 2, 3, 4);
                            F2  ();
                            F3  ();
                            if(false) {
                                F4  ();
                                F5  (1, 2, 3);
                                I3  ();
                            }
                            I4  ();
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
            var right = @"static void TreeChangeInsertIfAsBlock()
                        {
                            Test1();
                            Test2();
                            TestNew();
                            TestNew2(;)
                            Test3(1, 2, 3, 4, 5);
                            Test4();
                        }
                        ";
            var bas = @"static void TreeChangeInsertIfAsBlock()
                        {
                            Test1 ();
                            Test2 ();
                            Test3 (10, 6, 5);
                            Test4 ();
                        }   
                        ";
            var left = @"static void TreeChangeInsertIfAsBlock()
                        {
                            Test1  ();
                            if(true) {
                                Test2  ();
                                Test3  6(, 7, 8, 9);
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
            var right = @"static void TreeChangeInsertIfAsStatement()
                        {
                            Test1();
                            Test2  (1, 2, 2, 19);
                            Test3();
                        }
                        ";
            var bas = @"static void TreeChangeInsertIfAsStatement()
                        {
                            Test1 ();
                            Test2 (1, 2, 2, 3);
                            Test3 ();
                        }   
                        ";
            var left = @"static void TreeChangeInsertIfAsStatement()
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
            var B = @"static void TreeChangeIfFromBlockToStatement()
                        {
                            Test1();
                            if(true) {
                                Test2(2, 2, 3, 4);
                                TestNew();
                            }
                            Test3();
                        }
                        ";
            var O = @"static void TreeChangeIfFromBlockToStatement()
                        {
                            Test1 ();
                            if(true) {
                                Test2 (2, 2, 3);
                                TestNew ();
                            }
                            Test3 ();
                        }   
                        ";
            var A = @"static void TreeChangeIfFromBlockToStatement()
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
            var B = @"static void TreeChangeIfFromBlockToStatementInsertOnTheOutside()
                        {
                            Test1();
                            if(true) {
                                Test2(2, 2, 3, 4);
                                TestNew();
                            }
                            Test3();
                        }
                        ";
            var O = @"static void TreeChangeIfFromBlockToStatementInsertOnTheOutside()
                        {
                            Test1 ();
                            if(true) {
                                Test2 (2, 2, 3);
                                TestNew ();
                            }
                            Test3 ();
                        }   
                        ";
            var A = @"static void TreeChangeIfFromBlockToStatementInsertOnTheOutside()
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
            var B = @"static void TreeChangeInsertTwoNewIfStatements()
                        {
                            Test1();
                            TestOutside1();
                            Test2();
                            Test3();
                            TestOutside2();
                            Test4();   
                        }
                        ";
            var O = @"static void TreeChangeInsertTwoNewIfStatements()
                        {
                            Test1 ();
                            Test2 ();
                            Test3 ();
                            Test4 ();   
                        }   
                        ";
            var A = @"static void TreeChangeInsertTwoNewIfStatements()
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
        public void TreeChangeInsertSimpleBlock()
        {
            var B = @"static void TreeChangeInsertTwoNewIfStatements()
                        {
                            {
                            Test1();
                            Test2();
                            }
                            Test3();
                            Test4();   
                        }
                        ";
            var O = @"static void TreeChangeInsertTwoNewIfStatements()
                        {
                            Test1 ();
                            Test2 ();
                            Test3 ();
                            Test4 ();   
                        }   
                        ";
            var A = @"static void TreeChangeInsertTwoNewIfStatements()
                        {
                            Test1  ();
                            {
                                Test2  ();
                                Test3  ();
                            }
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
        public void TreeChangeIfMoveOutsideBlock()
        {
            var right = @"static void TreeChangeIfMoveOutsideBlock()
                        {
                            Test1();
                            if(true) {
                                Test2 (2, 2, 3);
                                TestNew (2, 3, 4);
                            }
                            Test3();
                        }
                        ";
            var bas = @"static void TreeChangeIfMoveOutsideBlock()
                        {
                            Test1 ();
                            if(true) {
                                Test2 (2, 2, 3);
                                TestNew (2, 3, 5);
                            }
                            Test3 ();
                        }   
                        ";
            var left = @"static void TreeChangeIfMoveOutsideBlock()
                        {
                            Test1  ();
                            if(true)
                                Test2 (2, 2, 3);
                            TestNew (2, 3);
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
        public void TreeChangeIfFromStatementToBlock()
        {
            var right = @"static void TreeChangeIfFromStatementToBlock()
                        {
                            Test1();
                            if(true) {
                                Test2 (2, 2, 3);
                                TestNew ();
                            }
                            Test3();
                        }
                        ";
            var bas = @"static void TreeChangeIfFromStatementToBlock()
                        {
                            Test1 ();
                            if(true)
                                Test2 (1, 2, 2, 3);
                            Test3 ();
                        }   
                        ";
            var left = @"static void TreeChangeIfFromStatementToBlock()
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
            var right = @"static void TreeChangeAndDeleteSingleInBlockWithChangeInOther()
                        {
                            Test2();
                        }
                        ";
            var bas = @"static void TreeChangeAndDeleteSingleInBlockWithChangeInOther()
                        {
                            Test();
                        }   
                        ";
            var left = @"static void TreeChangeAndDeleteSingleInBlockWithChangeInOther()
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
