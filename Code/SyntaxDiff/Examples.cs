using Roslyn.Compilers.CSharp;
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
                        static void ShouldConflict()
                        {
                            if(true)
                                    Console.WriteLine(""Hello, new World!"");
                        }
                        static void Conflicts()
                        {
                            Console.WriteLine(""Hello, base!"");
                        }
                        static void EndsAsLeft()
                        {
                            Console.WriteLine(""Base"");
                        }
                        static void EndsAsRight()
                        {
                            Console.WriteLine(""Base"");
                        }
                    }
                }");

        public static SyntaxTree leftTree = SyntaxTree.ParseText(@"using System;
                namespace HelloWorld2
                {
                    class Program
                    {
                        static void ShouldConflict()
                        {
                            if(false)
                                Console.WriteLine(""Hello, new World!"");
                        }   
                        static void InsertedInLeft()
                        {
                        }
                        static void Conflicts()
                        {
                            Console.WriteLine(""Hello, left!"");
                        }
                        static void EndsAsLeft()
                        {
                            Console.WriteLine(""Left"");
                        }
                        static void EndsAsRight()
                        {
                            Console.WriteLine(""Base"");
                        }
                    }
                }");
        public static SyntaxTree rightTree = SyntaxTree.ParseText(@"using System;
                using System.Linq;
                namespace HelloWorld
                {
                    class Program
                    {
                        static void Conflicts(string args)
                        {
                            Console.WriteLine(""Hello, new right World!"");
                        }
                        static void InsertedInRight()
                        {
                        }
                        static void ShouldConflict(string args)
                        {
                            Console.WriteLine(""Hello, right!"");
                        }
                        static void EndsAsLeft()
                        {
                            Console.WriteLine(""Base"");
                        }
                        static void EndsAsRight()
                        {
                            Console.WriteLine(""Right"");
                        }
                    }
                }");


        public static SyntaxTree SmallBaseTree = SyntaxTree.ParseText(@"using System;
                using System2.Linq;
                namespace HelloWorld
                {
                    class Program
                    {
                        static void Conflicts()
                        {
                            Console.WriteLine(""Hello, base!"");
                            Console.WriteLine(""Hello, base2!"");
                        }
                    }
                }");

        public static SyntaxTree SmallLeftTree = SyntaxTree.ParseText(@"using System;
                namespace HelloWorld2
                {
                    class Program
                    {
                        static void Conflicts()
                        {
                            Console.WriteLine(""Hello, base!"");
                            Console.WriteLine(""Hello, left!"");
                        }
                    }
                }");
        public static SyntaxTree SmallRightTree = SyntaxTree.ParseText(@"using System;
                using System.Linq;
                namespace HelloWorld
                {
                    class Program
                    {
                        static void Conflicts(string args)
                        {
                            Console.WriteLine(""Hello, new right World!"");
                        }
                    }
                }");

    }
}
