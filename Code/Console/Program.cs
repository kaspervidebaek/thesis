using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var deltas = SyntaxDiff.Differ.GetDiff(SyntaxDiff.Differ.leftTree, SyntaxDiff.Differ.rightTree);
            foreach (var diff in deltas) {
                Console.WriteLine(SyntaxDiff.Differ.getType(diff.Item1) + "->" + SyntaxDiff.Differ.getType(diff.Item2));
            }
            Console.ReadLine();
        }
    }
}
