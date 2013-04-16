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


        public static string getSyntaxString<T>(this SyntaxNode node, Func<T, string> f) where T : SyntaxNode
        {
            if (node is T)
            {
                var c = node as T;
                return "[" + f(c) + "]";
            }
            return "";
        }

        public static string getLabel(this SyntaxNode t)
        {
            if (t == null)
                return "empty";

            string s = t.GetType().ToString().Substring(24);

            s += t.getSyntaxString<IdentifierNameSyntax>(x => x.Identifier.ToString());
            s += t.getSyntaxString<ClassDeclarationSyntax>(x => x.Identifier.ToString());
            s += t.getSyntaxString<LiteralExpressionSyntax>(x => x.Token.ToString());
            s += t.getSyntaxString<ArrayTypeSyntax>(x => x.ElementType.ToString());
            s += t.getSyntaxString<PredefinedTypeSyntax>(x => x.ToString());

            return s;
        }

        public static List<SyntaxNode> getChildrenEnum(this SyntaxNode n)
        {
            var childEnum = (n == null ? new List<SyntaxNode>() : n.ChildNodes()).ToList();
            return childEnum;
        }


        public static CodeTreeType getChildType(this SyntaxNode sn)
        {
            if (sn is CompilationUnitSyntax)
                return CodeTreeType.Set;
            else if (sn is NamespaceDeclarationSyntax)
                return CodeTreeType.Set;
            else if (sn is ClassDeclarationSyntax)
                return CodeTreeType.Set;

            return CodeTreeType.Sequence;
        }
    }
}
