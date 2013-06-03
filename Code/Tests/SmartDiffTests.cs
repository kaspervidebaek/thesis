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
        [TestMethod]
        [TestCategory("Not Testing Anything")]
        public void TestExampleTree()
        {
            var left = SyntaxDiff.Examples.ConflictingLeftTree.GetRoot();
            var bas = SyntaxDiff.Examples.ConflictingBaseTree.GetRoot();
            var right = SyntaxDiff.Examples.ConflictingRightTree.GetRoot();

            var diff = new SyntaxDiff.SmartDiff<SyntaxNode>(new SyntaxNodeSmartDiff());

            var merge = diff.Merge(left, bas, right);

            merge.ForEach(Console.WriteLine);
        }

        [TestMethod]
        [TestCategory("Not Testing Anything")]
        public void TestExampleTreeMerge()
        {
            var left = SyntaxDiff.Examples.leftTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var bas = SyntaxDiff.Examples.baseTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var right = SyntaxDiff.Examples.rightTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var diff = new SyntaxDiff.SmartDiff<SyntaxNode>(new SyntaxNodeSmartDiff());

            var merge = diff.Merge(left, bas, right);

            merge.ForEach(Console.WriteLine);
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
            var match = Tree<int?>.ThreeWayMatch(left, bas, right, intEquals);
            var v = match.PostOrderEnumeration().Select(x => x.A != null ? x.A : (x.B != null ? x.B : x.O)).ToList();
            var result = new List<int?> { 9, 6, 9, 9, 2, 5, 4, 10, 3, 2, 1 };
            Assert.IsTrue(Enumerable.SequenceEqual(v, result));
        }


        [TestMethod]
        [TestCategory("Tree")]
        public void TestChunkMatch()
        {
            var match = Tree<int?>.ChunkMatch(left, bas, right, intEquals);
            //var v = match.PostOrderEnumeration().Select(x => x.A != null ? x.A : (x.B != null ? x.B : x.O)).ToList();
            //var result = new List<int?> { 9, 6, 9, 9, 2, 5, 4, 10, 3, 2, 1 };
            //Assert.IsTrue(Enumerable.SequenceEqual(v, result));
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
