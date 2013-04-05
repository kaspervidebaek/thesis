using System;
using SyntaxDiff.Diff3;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileA = new File();
            var fileB = new File();
            fileA.Add("0");
            fileA.Add("2");
            fileA.Add("3");
            fileA.Add("4");
            fileA.Add("4");
            fileA.Add("5");
            fileA.Add("6");

            fileB.Add("0");
            fileB.Add("1");
            fileB.Add("2");
            fileB.Add("3");
            fileB.Add("4");
            fileB.Add("6");
            fileB.Add("7");
            
            var allignment = NeedlemanWunsch.Allignment(fileA, fileB);
            foreach (var a in allignment)
            {
                Console.WriteLine(a.Item1 + "-" + a.Item2);
            }
            Console.ReadLine();



            /*var deltas = SyntaxDiff.Differ.GetDiff(SyntaxDiff.Differ.leftTree, SyntaxDiff.Differ.rightTree);
            foreach (var diff in deltas) {
                Console.WriteLine(SyntaxDiff.Differ.getType(diff.Item1) + "->" + SyntaxDiff.Differ.getType(diff.Item2));
            }
            Console.ReadLine();*/
        }
    }
}
