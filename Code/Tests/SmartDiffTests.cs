using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn.Compilers.CSharp;
using System.Linq;
using SyntaxDiff;
using System.Diagnostics;

namespace Tests
{
    [TestClass]
    public class SmartDiffTests
    {
        public static Tree<SyntaxNode> convert(SyntaxNode n)
        {
            var children = n.ChildNodes().Select(x => convert(x)).ToArray();
            return new Tree<SyntaxNode>(n, children);
        }

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
    }
}
