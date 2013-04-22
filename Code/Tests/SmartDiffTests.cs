using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn.Compilers.CSharp;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class SmartDiffTests
    {
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
