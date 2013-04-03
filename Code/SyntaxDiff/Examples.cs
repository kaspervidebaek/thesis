﻿using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{
    public class Examples
    {
        public static SyntaxTree smartAlgorithmTree = SyntaxTree.ParseFile(@"CodeSnippets\SmartAlgorithm.cs");
        public static SyntaxTree flowAlgorithm = SyntaxTree.ParseFile(@"CodeSnippets\FlowAlgorithm.cs");

        public static SyntaxTree QSbaseTree = SyntaxTree.ParseFile(@"CodeSnippets\QuickSortBasE.cs");
        public static SyntaxTree QSleftTree = SyntaxTree.ParseFile(@"CodeSnippets\QuickSortLeft.cs");
        public static SyntaxTree QSrightTree = SyntaxTree.ParseFile(@"CodeSnippets\QuickSortRight.cs");

        public static SyntaxTree baseTree = SyntaxTree.ParseText(@"using System;
                using System2.Linq;
                namespace HelloWorld
                {
                    class Program
                    {
                        static void Main()
                        {
                            if(false)
                                while(1 == 1)
                                    Console.WriteLine(""Hello, new World!"");
                        }
                    }
                }");

        public static SyntaxTree leftTree = SyntaxTree.ParseText(@"using System;
                namespace HelloWorld2
                {
                    class Program
                    {
                        static void Main()
                        {
                            if(false)
                                Console.WriteLine(""Hello, new World!"");
                        }   
                    }
                }");
        public static SyntaxTree rightTree = SyntaxTree.ParseText(@"using System;
                using System.Linq;
                namespace HelloWorld
                {
                    class Program2
                    {
                        static void Main2(string args)
                        {
                            Console.WriteLine(""Hello, new right World!"");
                        }
                    }
                }");

    }
}