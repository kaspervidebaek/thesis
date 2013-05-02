using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn.Compilers.CSharp;
using System.Linq;
using SyntaxDiff;

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
        public void TestTreeMatching()
        {
            var left = SyntaxDiff.Examples.flowAlgorithm.GetRoot();
            var bas = SyntaxDiff.Examples.flowAlgorithm.GetRoot();

            var treeL = convert(left);
            var treeB = convert(bas);

            var merge = JavaMatching<SyntaxNode>.getMapping(treeB, treeL, x => x.getLabel());

            merge.ForEach(Console.WriteLine);


        }

        [TestMethod]
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
