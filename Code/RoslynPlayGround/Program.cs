using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynPlayGround
{
    class Program
    {
        static void Main(string[] args)
        {
            ReplaceTwoNodes();
        }


        static void ReplaceTwoNodes()
        {
            var tree = SyntaxTree.ParseText("int test = 2 ?? 3;");
            var members = ((FieldDeclarationSyntax)tree.GetRoot().ChildNodes().First()).Declaration;

            var tree2 = SyntaxTree.ParseText("Nullable<int> test;");
            var members2 = ((FieldDeclarationSyntax)tree2.GetRoot().ChildNodes().First()).Declaration;
        }
    }
}
