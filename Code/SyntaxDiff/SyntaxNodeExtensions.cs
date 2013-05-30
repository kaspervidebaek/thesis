using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{
    public enum CodeTreeType
    {
        Set, Sequence, Tree
    }
    public static class SyntaxNodeExtensions
    {
        public static string getMemberDeclerationIdentifier(this MemberDeclarationSyntax node)
        {
            if (node is MethodDeclarationSyntax)
                return (node as MethodDeclarationSyntax).Identifier.ToString();
            throw new NotImplementedException();
        }


        public static List<SyntaxNode> getChildrenEnum(this SyntaxNode n)
        {
            var childEnum = (n == null ? new List<SyntaxNode>() : n.ChildNodes()).ToList();
            return childEnum;
        }

    }
}
