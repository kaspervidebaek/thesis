using System;
using SyntaxDiff;
using System.Linq;
using System.Collections.Generic;
using Roslyn.Compilers.CSharp;

namespace ConsoleApp
{


    class Program
    {

        static void Main(string[] args)
        {
#if false
            var bas = new Tree<string>("A", new Tree<string>("B", "X"), new Tree<string>("C", "Y"));
            var other = new Tree<string>("A", new Tree<string>("I", "X"), new Tree<string>("K", "Y"));
#elif false
            var bas = new Tree<string>("A", new Tree<string>("B"), new Tree<string>("C"));
            var other = new Tree<string>("A", new Tree<string>("C"), new Tree<string>("B"));
#elif true
            var bas = new Tree<string>("A", new Tree<string>("B", "X"), new Tree<string>("B", "X"), new Tree<string>("C", "Y"));
            var other = new Tree<string>("A", new Tree<string>("C", "Y"), new Tree<string>("B", "X"));
#else
            var bas = new Tree<string>("A", new Tree<string>("B", "X"), new Tree<string>("C", "Y"));
            var other = new Tree<string>("A", new Tree<string>("C", "Y"), new Tree<string>("B", "X"));
#endif

            Func<Tree<string>, string> getLabel = x => x.value;

            //var mapping = SyntaxDiff.JavaMatching<string>.getMapping(bas, other, getLabel);

            //mapping.ForEach(Console.WriteLine);

/*            var smartDiff = new Tests.SmartDiffTests();
            smartDiff.TestExampleTree();*/



            Console.ReadLine();
        }
    }
}
