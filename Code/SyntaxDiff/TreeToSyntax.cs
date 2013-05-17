using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{
    public static class TreeToSyntax
    {

        public static bool checkAndCast<Y>(SyntaxNode n, out SyntaxNode on, Func<Y, SyntaxNode> convert) where Y : SyntaxNode
        {
            if (n is Y)
            {
                on = convert(n as Y);
                return true;
            }
            on = null;
            return false;
        }

        public static SyntaxList<Y> AssignList<Y>(List<SyntaxNode> n) where Y : SyntaxNode
        {
            return (Y)n.SingleOrDefault(x => x is SyntaxList<Y>);
        }

        public static Y AssignNode<Y>(List<SyntaxNode> n) where Y : SyntaxNode
        {
            return (Y)n.SingleOrDefault(x => x is Y);
        }

        public static SyntaxNode convertNode(Tree<SyntaxNode> tree)
        {
            var newChildren = new List<SyntaxNode>();

            foreach (var child in tree.children)
            {
                newChildren.Add(convertNode(child));
            }

            var oldNode = tree.value;
            SyntaxNode newNode = null;

            if (checkAndCast<MethodDeclarationSyntax>(oldNode, out newNode, x => convert(x, newChildren))) ;
            else if (checkAndCast<PredefinedTypeSyntax>(oldNode, out newNode, x => convert(x, newChildren))) ;
            else if (checkAndCast<ParameterListSyntax>(oldNode, out newNode, x => convert(x, newChildren))) ;
            /*else if(checkAndCast<BlockSyntax>                   (oldNode, out newNode, x => convert(x, newChildren)));
            else if(checkAndCast<ExpressionStatementSyntax>     (oldNode, out newNode, x => convert(x, newChildren)));
            return ;*/
            else throw new NotImplementedException();
            return newNode;
        }


        public static PredefinedTypeSyntax convert(PredefinedTypeSyntax n, List<SyntaxNode> children)
        {
            var z = Syntax.PredefinedType(n.Keyword);
            return z;
        }

        public static ParameterSyntax convert(ParameterSyntax n, List<SyntaxNode> children)
        {
            return null;
        }
        public static ParameterListSyntax convert(ParameterListSyntax n, List<SyntaxNode> children)
        {
            
            return null;
        }
        public static MethodDeclarationSyntax convert(MethodDeclarationSyntax n, List<SyntaxNode> children)
        {

            /*var attributeLists = AssignList<AttributeListSyntax>(children);
            SyntaxTokenList modifiers SyntaxT;
            TypeSyntax returnType;
            ExplicitInterfaceSpecifierSyntax explicitInterfaceSpecifier;
            SyntaxToken identifier;
            TypeParameterListSyntax typeParameterList;
            ParameterListSyntax parameterList;
            SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses;
            BlockSyntax body;
            SyntaxToken semicolonToken;

            var node = Syntax.MethodDeclaration(attributeLists, modifiers, returnType, explicitInterfaceSpecifier, identifier, typeParameterList, parameterList, constraintClauses, body, semicolonToken);
            return node;*/
            return null;
        }
    }
}
