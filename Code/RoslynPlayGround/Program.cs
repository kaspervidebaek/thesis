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
            var tree = SyntaxTree.ParseText("class test { void test1() {} void test2{} }");
            var members = tree.GetRoot().ChildNodes().First();
        }
    }
}
