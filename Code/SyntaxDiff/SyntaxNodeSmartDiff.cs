using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{
    public class SyntaxNodeSmartDiff : ISmartDiffInterface<SyntaxNode>
    {
        public SyntaxNodeSmartDiff()
        {
        }

        public CodeTreeType getChildType(SyntaxNode sn)
        {
            if (sn is CompilationUnitSyntax)
                return CodeTreeType.Set;
            else if (sn is NamespaceDeclarationSyntax)
                return CodeTreeType.Set;
            else if (sn is ClassDeclarationSyntax)
                return CodeTreeType.Set;
            return CodeTreeType.Sequence;
        }

        public string getLabel(SyntaxNode t)
        {
            if (t == null)
                return "empty";

            string s = t.GetType().ToString().Substring(24);

            s += t.getSyntaxString<IdentifierNameSyntax>(x => x.Identifier.ToString());
            s += t.getSyntaxString<ClassDeclarationSyntax>(x => x.Identifier.ToString());
            s += t.getSyntaxString<LiteralExpressionSyntax>(x => x.Token.ToString());
            s += t.getSyntaxString<ArrayTypeSyntax>(x => x.ElementType.ToString());
            s += t.getSyntaxString<PredefinedTypeSyntax>(x => x.ToString());
            s += t.getSyntaxString<ParameterSyntax>(x => x.Identifier.ToString());

            return s;
        }

        public SyntaxNode ConvertBack(Tree<Diff<SyntaxNode>> tree)
        {

            var newChildren = new List<SyntaxNode>();

            foreach (var child in tree.children)
            {
                newChildren.Add(ConvertBack(child));
            }

            var oldNode = tree.value;
            SyntaxNode newNode = null;

            var result =
                checkAndCast<MethodDeclarationSyntax>(oldNode, out newNode, x => convert(x, newChildren)) ||
                checkAndCast<PredefinedTypeSyntax>(oldNode, out newNode, x => convert(x, newChildren)) ||
                checkAndCast<ParameterListSyntax>(oldNode, out newNode, x => convert(x, newChildren)) ||
                checkAndCast<ParameterSyntax>(oldNode, out newNode, x => convert(x, newChildren)) ||
                checkAndCast<IdentifierNameSyntax>(oldNode, out newNode, x => convert(x, newChildren));

            if (!result || newNode == null)
                throw new NotImplementedException();
            return newNode;
        }


        public static bool checkAndCast<Y>(Diff<SyntaxNode> n, out SyntaxNode on, Func<Diff<Y>, SyntaxNode> convert) where Y : SyntaxNode
        {
            Func<SyntaxNode, bool> nullOrY = x => x is Y || x == null;

            if (nullOrY(n.A) && nullOrY(n.O) && nullOrY(n.B))
            {
                on = convert(n.cast<Y>());
                return true;
            }
            on = null;
            return false;
        }


        public static PredefinedTypeSyntax convert(Diff<PredefinedTypeSyntax> n, List<SyntaxNode> children)
        {
            var z = Syntax.PredefinedType(n.O.Keyword); // Since they are matched, we know that all keywords are the same.

            return z; // We have no children.
        }

        public static ParameterSyntax convert(Diff<ParameterSyntax> n, List<SyntaxNode> children)
        {
            return Syntax.Parameter(n.O.Identifier); // TODO
        }
        public static ParameterListSyntax convert(Diff<ParameterListSyntax> n, List<SyntaxNode> children)
        {
            var c = new SeparatedSyntaxList<ParameterSyntax>();
            children.ForEach(x => c.Add((ParameterSyntax)x));
            return Syntax.ParameterList(c);
        }

        public static IdentifierNameSyntax convert(Diff<IdentifierNameSyntax> n, List<SyntaxNode> children)
        {
            return Syntax.IdentifierName(n.O.Identifier);
        }
        public static MethodDeclarationSyntax convert(Diff<MethodDeclarationSyntax> n, List<SyntaxNode> children)
        {
            return null;
        }
    }
}
