using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{
    public static class SyntaxNodeExtensions
    {
        public static List<SyntaxNode> getChildrenEnum(this SyntaxNode n)
        {
            var childEnum = (n == null ? new List<SyntaxNode>() : n.ChildNodes()).ToList();
            return childEnum;
        }
    }
}
