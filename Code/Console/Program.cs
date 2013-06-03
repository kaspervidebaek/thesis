using System;
using SyntaxDiff;
using System.Linq;
using System.Collections.Generic;
using Roslyn.Compilers.CSharp;
using System.Diagnostics;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = new Tests.SmartDiffTests();
            x.TestExampleTreeMerge();
            Console.ReadLine();
            return;

#if false
            var bas = new Tree<string>("A", new Tree<string>("B", "X"), new Tree<string>("C", "Y"));
            var other = new Tree<string>("A", new Tree<string>("I", "X"), new Tree<string>("K", "Y"));
#elif false
            var bas = new Tree<string>("A", new Tree<string>("B"), new Tree<string>("C"));
            var other = new Tree<string>("A", new Tree<string>("C"), new Tree<string>("B"));
#elif false
            var bas = new Tree<string>("A", new Tree<string>("B", "X"), new Tree<string>("B", "X"), new Tree<string>("C", "Y"));
            var other = new Tree<string>("A", new Tree<string>("C", "Y"), new Tree<string>("B", "X"));
#elif false
            var bas = new Tree<string>("A", new Tree<string>("B", "X"), new Tree<string>("C", "Y"));
            var other = new Tree<string>("A", new Tree<string>("C", "Y"), new Tree<string>("B", "X"));
#endif
            /*
            var b = new Tree<int?>(4, new Tree<int?>(3), new Tree<int?>(7), new Tree<int?>(2, 1, 0));
            var l = new Tree<int?>(4, new Tree<int?>(3), new Tree<int?>(10, 1, 0), 9);

            Func<int?, string> getLabel = x => x.ToString();
            JavaMatching<int?>.getMappingTree(b, l, getLabel);
            */
            //var mapping = SyntaxDiff.JavaMatching<string>.getMapping(bas, other, getLabel);

            //mapping.ForEach(Console.WriteLine);

/*            var smartDiff = new Tests.SmartDiffTests();
            smartDiff.TestExampleTree();*/



//            Console.ReadLine();
        }
    }
}
