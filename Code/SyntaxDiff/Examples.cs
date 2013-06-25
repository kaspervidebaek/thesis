using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{
    public class Examples
    {
        public static string LoadFile(string path) {
            return File.ReadAllText(path);
        }

        public static string StringToFile(string code)
        {
            return code;
        }

        public static string smartAlgorithmTree = LoadFile(@"CodeSnippets\SmartAlgorithm.cs");
        public static string flowAlgorithm = LoadFile(@"CodeSnippets\FlowAlgorithm.cs");

        public static string QSbaseTree = LoadFile(@"CodeSnippets\QuickSortBasE.cs");
        public static string QSleftTree = LoadFile(@"CodeSnippets\QuickSortLeft.cs");
        public static string QSrightTree = LoadFile(@"CodeSnippets\QuickSortRight.cs");

        public static string baseTree = StringToFile(@"using System;
                using System2.Linq;
                namespace HelloWorld
                {
                    class Program
                    {
                        static void WillBeDeletedInLeft()
                        {
                        }
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
                        static void WhileInsertion()
                        {
                            Console.WriteLine(""Base"");
                            Console.WriteLine(""BaseInside"");
                        }
                    }
                }");

        public static string leftTree = StringToFile(@"using System;
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
                        static void WhileInsertion()
                        {
                            Console.WriteLine(""Right"");
                            Console.WriteLine(""BaseInside"");
                        }

                    }
                }");
        public static string rightTree = StringToFile(@"using System;
                using System.Linq;
                namespace HelloWorld
                {
                    class Program
                    {
                        static void WillBeDeletedInLeft()
                        {
                        }
                        static void Conflicts(string args)
                        {
                            Console.WriteLine(""Hello, new right World!"");
                        }
                        static void InsertedInRight()
                        {
                        }
                        static void ShouldConflict(string args)
                        {
                            if(true)
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
                        static void WhileInsertion()
                        {
                            Console.WriteLine(""Right"");
                            while(true)
                                Console.WriteLine(""BaseInside"");
                        }
                    }
                }");


        public static string SmallBaseTree = StringToFile(@"
                    class Program
                    {
                        static void Conflicts()
                        {
                            var x = 2;
                            Console.WriteLine(""Hello, base!"");
                            Console.WriteLine(""Hello, base!"");
                        }
                    }
                ");

        public static string SmallLeftTree = StringToFile(@"
                    class Program
                    {
                        static void Conflicts()
                        {
                            Console.WriteLine(""Hello, base!"");
                            var x = 2;
                        }
                    }
                ");
        public static string SmallRightTree = StringToFile(@"
                    class Program
                    {
                        static void Conflicts(string args)
                        {
                            Console.WriteLine(""Hello, new right World!"");
                        }
                    }
                ");

        public static string ConflictingBaseTree = StringToFile(@"
                        static void Conflicts(string args)
                        {
                            Console.WriteLine(""Hello, base!"");
                        }
                ");

        public static string ConflictingLeftTree = StringToFile(@"
                        static void Conflicts(string args)
                        {
                            Console.Write(""Hello, base!"");
                        }
                ");
        public static string ConflictingRightTree = StringToFile(@"
                        static void Conflicts(string args)
                        {
                            Console.WriteLine(""Hello, new right World!"");
                        }
                ");

    }
}
