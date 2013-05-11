using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn.Compilers.CSharp;
using System.Linq;
using SyntaxDiff;
using System.Diagnostics;
using System.Collections.Generic;

namespace Tests
{
    [TestClass]
    public class SmartDiffTests
    {
        /*
        [TestMethod]
        [TestCategory("Tree")]

        public void TestTreeMatching()
        {
            var left = SyntaxDiff.Examples.leftTree.GetRoot();
            var bas = SyntaxDiff.Examples.leftTree.GetRoot();

            var treeL = convert(left);
            var treeB = convert(bas);
            Console.WriteLine("Left size: " + treeL.Size());
            Console.WriteLine("Base size: " + treeB.Size());

            var t = new Stopwatch();
            t.Start();
            var merge = JavaMatching<SyntaxNode>.getMapping(treeB, treeL, x => x.getLabel());
            t.Stop();
            Console.WriteLine("Time: " + t.Elapsed.ToString());

            //merge.ForEach(Console.WriteLine);


        }

        [TestMethod]
        [TestCategory("Tree")]
        public void TestExampleTree()
        {
            var left = SyntaxDiff.Examples.leftTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var bas = SyntaxDiff.Examples.baseTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var right = SyntaxDiff.Examples.rightTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var merge = SyntaxDiff.SmartDiff.Merge(left, bas, right);

            merge.ForEach(Console.WriteLine);
        }
        */
        [TestMethod]
        [TestCategory("Tree")]
        public void TestExampleTreeMerge()
        {
            var left = SyntaxDiff.Examples.leftTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First().ConvertToTree();
            var bas = SyntaxDiff.Examples.baseTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First().ConvertToTree();
            var right = SyntaxDiff.Examples.rightTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First().ConvertToTree();

            Func<SyntaxNode, SyntaxNode, bool> equals = (x, y) => x.getLabel() == y.getLabel();

            
            var merge = Tree<SyntaxNode>.TreeWayMatch(left, bas, right, equals);

            //merge.ForEach(Console.WriteLine);
        }

        static Tree<int?> left = new Tree<int?>(1, // Removed 2, added 9, added 2, 9, 9 tree
                            3,
                            new Tree<int?>(4, 5, new Tree<int?>(2, 9, 9)),
                            6,
                            9);
        static Tree<int?> bas = new Tree<int?>(1,
                                    2,
                                    3,
                                    new Tree<int?>(4, 5),
                                    6);
        static Tree<int?> right = new Tree<int?>(1,
                                    2,
                                    new Tree<int?>(3, 10),
                                    new Tree<int?>(4, 5));
        static Func<int?, int?, bool> intEquals = (x, y) => x == y;
        static Tree<int?> result = new Tree<int?>(1,
                new Tree<int?>(3, 10),
                new Tree<int?>(4, 5, new Tree<int?>(2, 9, 9)),
                9);
                    Func<int?, int?, bool> equals = (x, y) => x == y;
        [TestMethod]
        [TestCategory("Tree")]
        public void TestMatch()
        {
            var match = Tree<int?>.TreeWayMatch(left, bas, right, intEquals);
            var v = match.PostOrderEnumeration().Select(x => x.A != null ? x.A : (x.B != null ? x.B : x.O)).ToList();
            var result = new List<int?> { 9, 6, 9, 9, 2, 5, 4, 10, 3, 2, 1 };
            Assert.IsTrue(Enumerable.SequenceEqual(v, result));
        }

        [TestMethod]
        [TestCategory("Tree")]
        public void TestMerge()
        {
            var merge = Tree<int?>.Merge(left, bas, right, equals);
            var itt = result.PostOrderEnumeration();
            Assert.IsTrue(Enumerable.SequenceEqual(itt, merge.PostOrderEnumeration()));

        }
    }
}
