using System;
using SyntaxDiff.Diff3;
using System.Linq;
using System.Collections.Generic;
using Roslyn.Compilers.CSharp;

namespace ConsoleApp
{
    class Program
    {


        private static void HandleConflict(List<String> mergedfile, Chunk<String> chunck)
        {
            mergedfile.Add(">>> A");
            foreach (var line in chunck.A)
                mergedfile.Add(line);
            mergedfile.Add(">>> O");
            foreach (var line in chunck.O)
                mergedfile.Add(line);
            mergedfile.Add(">>> B");
            foreach (var line in chunck.B)
                mergedfile.Add(line);
            mergedfile.Add("<<<");
        }

        static void Main(string[] args)
        {
#if true
            var fileA = new List<String>(new string[] { "1", "4", "5", "2", "3", "6" });
            var fileO = new List<String>(new string[] { "1", "2", "3", "4", "5", "6" });
            var fileB = new List<String>(new string[] { "1", "2", "4", "5", "3", "6" });
#else
            var fileA = new List<String>(new string[] { "1", "3", "5" });
            var fileO = new List<String>(new string[] { "1", "2", "5"});
            var fileB = new List<String>(new string[] { "1", "5" });
#endif
            //var merge = Diff3<string>.Merge(fileA, fileO, fileB, (a, b)  => a == b, HandleConflict);
            /*
            foreach (var l in merge)
            {
                Console.WriteLine(l);
            }*/


            var left = SyntaxDiff.Examples.leftTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var bas = SyntaxDiff.Examples.baseTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var right = SyntaxDiff.Examples.rightTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            var merge = SyntaxDiff.SmartDiff.Merge(left, bas, right);

            merge.ForEach(Console.WriteLine);

            Console.ReadLine();

            /*var deltas = SyntaxDiff.Differ.GetDiff(SyntaxDiff.Differ.leftTree, SyntaxDiff.Differ.rightTree);
            foreach (var diff in deltas) {
                Console.WriteLine(SyntaxDiff.Differ.getType(diff.Item1) + "->" + SyntaxDiff.Differ.getType(diff.Item2));
            }
            Console.ReadLine();*/
        }
    }
}
