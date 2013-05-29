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
                var convertback = ConvertBack(child);
                if(convertback != null)
                    newChildren.Add(convertback);
            }

            var oldNode = tree.value;
            SyntaxNode newNode = null;

            var result =
                checkAndCast<MethodDeclarationSyntax>(oldNode, out newNode, newChildren, (x, y) => merge(x, y), (x, y) => insert(x, y)) ||
                checkAndCast<PredefinedTypeSyntax>(oldNode, out newNode, newChildren, (x, y) => merge(x, y), (x, y) => insert(x, y)) ||
                checkAndCast<ParameterListSyntax>(oldNode, out newNode, newChildren, (x, y) => merge(x, y), (x, y) => insert(x, y)) ||
                checkAndCast<ParameterSyntax>(oldNode, out newNode, newChildren, (x, y) => merge(x, y), (x, y) => insert(x, y)) ||
                checkAndCast<MemberAccessExpressionSyntax>(oldNode, out newNode, newChildren, (x, y) => merge(x, y), (x, y) => insert(x, y)) ||
                checkAndCast<LiteralExpressionSyntax>(oldNode, out newNode, newChildren, (x, y) => merge(x, y), (x, y) => insert(x, y)) ||
                checkAndCast<ArgumentSyntax>(oldNode, out newNode, newChildren, (x, y) => merge(x, y), (x, y) => insert(x, y)) ||
                checkAndCast<ArgumentListSyntax>(oldNode, out newNode, newChildren, (x, y) => merge(x, y), (x, y) => insert(x, y)) ||
                checkAndCast<InvocationExpressionSyntax>(oldNode, out newNode, newChildren, (x, y) => merge(x, y), (x, y) => insert(x, y)) ||
                checkAndCast<ExpressionStatementSyntax>(oldNode, out newNode, newChildren, (x, y) => merge(x, y), (x, y) => insert(x, y)) ||
                checkAndCast<BlockSyntax>(oldNode, out newNode, newChildren, (x, y) => merge(x, y), (x, y) => insert(x, y)) ||
                checkAndCast<IdentifierNameSyntax>(oldNode, out newNode, newChildren, (x, y) => merge(x, y), (x, y) => insert(x, y));

            if (!result)
                throw new NotImplementedException();
            return newNode;
        }

        public static bool checkAndCast<Y>(Diff<SyntaxNode> n, out SyntaxNode on, List<SyntaxNode> newchildren, Func<Diff<Y>, List<SyntaxNode>, SyntaxNode> convert, Func<Y, List<SyntaxNode>, SyntaxNode> insert) where Y : SyntaxNode
        {
            Func<SyntaxNode, bool> nullOrY = x => x is Y || x == null;

            if (nullOrY(n.A) && nullOrY(n.O) && nullOrY(n.B))
            {
                if (n.A != null && n.O == null && n.B == null)
                {
                    on = insert((Y)n.A, newchildren);
                    return true;
                }
                else if (n.A == null && n.O == null && n.B != null)
                {
                    on = insert((Y)n.B, newchildren);
                    return true;
                }
                else if (n.A != null && n.O == null && n.B != null)
                    throw new Exception("WTF");
                else if (n.A != null && n.O != null && n.B != null)
                {
                    on = convert(n.cast<Y>(), newchildren);
                    return true;
                }

                on = null;
                return true; // If we reach this part is a delete, and we can simply return "processed"
            }
            on = null;
            return false; // We did not process this node
        }


        public static PredefinedTypeSyntax merge(Diff<PredefinedTypeSyntax> n, List<SyntaxNode> children)
        {
            return insert(n.O, children); // Since they are matched, we know that all keywords are the same.
        }
        public static PredefinedTypeSyntax insert(PredefinedTypeSyntax n, List<SyntaxNode> children)
        {
            return Syntax.PredefinedType(n.Keyword); // We have no children.
        }

        public static ParameterSyntax merge(Diff<ParameterSyntax> n, List<SyntaxNode> children)
        {
            if (n.A.Identifier.ValueText != n.O.Identifier.ValueText && n.O.Identifier.ValueText == n.B.Identifier.ValueText)
                return insert(n.A, children);
            if (n.A.Identifier.ValueText == n.O.Identifier.ValueText && n.O.Identifier.ValueText != n.B.Identifier.ValueText)
                return insert(n.B, children);
            if (n.A.Identifier.ValueText == n.O.Identifier.ValueText && n.O.Identifier.ValueText == n.B.Identifier.ValueText)
                return insert(n.O, children);
            throw new Exception("This might be a conflict");

        }

        public static ParameterSyntax insert(ParameterSyntax n, List<SyntaxNode> children)
        {
            return Syntax.Parameter(n.Identifier); // TODO
        }
        
        public static ParameterListSyntax merge(Diff<ParameterListSyntax> n, List<SyntaxNode> children)
        {
            return insert(n.O, children);
        }

        public static ParameterListSyntax insert(ParameterListSyntax n, List<SyntaxNode> children)
        {
            var c = new SeparatedSyntaxList<ParameterSyntax>();
            children.ForEach(x => c.Add((ParameterSyntax)x));
            return Syntax.ParameterList(c);
        }

        public static IdentifierNameSyntax merge(Diff<IdentifierNameSyntax> n, List<SyntaxNode> children)
        {
            return insert(n.O, children);
        }
        public static IdentifierNameSyntax insert(IdentifierNameSyntax n, List<SyntaxNode> children)
        {
            return Syntax.IdentifierName(n.Identifier);
        }

        public static MemberAccessExpressionSyntax merge(Diff<MemberAccessExpressionSyntax> n, List<SyntaxNode> children)
        {
            return insert(n.O, children);
        }
        public static MemberAccessExpressionSyntax insert(MemberAccessExpressionSyntax n, List<SyntaxNode> children)
        {
            return Syntax.MemberAccessExpression(n.Kind, (ExpressionSyntax)children[0], (SimpleNameSyntax)children[1]);
        }

        public static LiteralExpressionSyntax merge(Diff<LiteralExpressionSyntax> n, List<SyntaxNode> children)
        {
            return null;
        }
        public static LiteralExpressionSyntax insert(LiteralExpressionSyntax n, List<SyntaxNode> children)
        {
            return Syntax.LiteralExpression(n.Kind, n.Token);
        }

        public static ArgumentSyntax merge(Diff<ArgumentSyntax> n, List<SyntaxNode> children)
        {
            return insert(n.O, children);
        }
        public static ArgumentSyntax insert(ArgumentSyntax n, List<SyntaxNode> children)
        {
            return Syntax.Argument((ExpressionSyntax)children [0]);
        }

        public static ArgumentListSyntax merge(Diff<ArgumentListSyntax> n, List<SyntaxNode> children)
        {
            return insert(n.O, children);
        }
        public static ArgumentListSyntax insert(ArgumentListSyntax n, List<SyntaxNode> children)
        {
            var c = Syntax.SeparatedList(children.Select(x => (ArgumentSyntax)x), Enumerable.Repeat(Syntax.Token(SyntaxKind.CommaToken), children.Count-1));
            return Syntax.ArgumentList(c);
        }


        public static InvocationExpressionSyntax merge(Diff<InvocationExpressionSyntax> n, List<SyntaxNode> children)
        {
            return insert(n.O, children);
        }
        public static InvocationExpressionSyntax insert(InvocationExpressionSyntax n, List<SyntaxNode> children)
        {
            return Syntax.InvocationExpression((ExpressionSyntax)children[0], (ArgumentListSyntax)children[1]);
        }

        public static ExpressionStatementSyntax merge(Diff<ExpressionStatementSyntax> n, List<SyntaxNode> children)
        {
            return insert(n.O, children);
        }
        public static ExpressionStatementSyntax insert(ExpressionStatementSyntax n, List<SyntaxNode> children)
        {
            return Syntax.ExpressionStatement((ExpressionSyntax)children[0]);
        }

        public static BlockSyntax merge(Diff<BlockSyntax> n, List<SyntaxNode> children)
        {
            return insert(n.O, children);
        }
        public static BlockSyntax insert(BlockSyntax n, List<SyntaxNode> children)
        {
            var c = Syntax.SeparatedList(children.Select(x => (StatementSyntax)x), Enumerable.Repeat(Syntax.Token(SyntaxKind.CommaToken), children.Count - 1));
            return Syntax.Block(c);
        }

        public static SyntaxList<AttributeListSyntax> merge(SyntaxList<AttributeListSyntax> A, SyntaxList<AttributeListSyntax> O, SyntaxList<AttributeListSyntax> B)
        {
            var attributesLists = new SyntaxList<AttributeListSyntax>();

            return attributesLists; // TODO
        }
        

        public static MethodDeclarationSyntax merge(Diff<MethodDeclarationSyntax> n, List<SyntaxNode> children)
        {
            return insert(n.O, children);
        }
        public static MethodDeclarationSyntax insert(MethodDeclarationSyntax n, List<SyntaxNode> children)
        {
            return Syntax.MethodDeclaration(
                n.AttributeLists,
                n.Modifiers,
                (TypeSyntax)children[0],
                n.ExplicitInterfaceSpecifier,
                n.Identifier,
                n.TypeParameterList,
                (ParameterListSyntax)children[1],
                n.ConstraintClauses,
                (BlockSyntax)children[2]) ;
        }
    }
}
