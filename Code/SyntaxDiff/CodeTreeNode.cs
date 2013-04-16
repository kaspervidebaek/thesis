using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{
    /*public class MergedTreeNode
    {
        public readonly List<MergedTreeNode> children;
        public readonly SyntaxNode node;

        public MergedTreeNode(List<MergedTreeNode> children, SyntaxNode node)
        {
            this.children = children;
            this.node = node;
        }
        public MergedTreeNode(List<CodeTreeNode> children, SyntaxNode node)
        {
            this.children = children.Select(x => new MergedTreeNode(x.children, x.node)).ToList();
            this.node = node;
        }
    }*/
   

   /*
    public class CodeTreeNode
    { 
        public readonly CodeTreeType childtype;
        public readonly List<CodeTreeNode> children;

        public readonly string label;
        public readonly SyntaxNode node;

        public CodeTreeNode(SyntaxNode sn)
        {
            this.childtype = getChildType(sn);
            this.label = sn.getLabel();
            this.node = sn;
            this.children = sn.ChildNodes().Select(x => new CodeTreeNode(x)).ToList();
        }

        public CodeTreeNode()
        {
            // TODO: Complete member initialization
        }
    */

}
