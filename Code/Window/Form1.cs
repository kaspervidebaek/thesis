using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Roslyn.Compilers.CSharp;
using SyntaxDiff;

namespace WindowApp
{
    public partial class Form1 : Form
    {
        public static Tree<SyntaxNode> convert(SyntaxNode n)
        {
            var children = n.ChildNodes().Select(x => convert(x)).ToArray();
            return new Tree<SyntaxNode>(n, children);
        }

        public static TreeNode buildTree<T>(TreeDiff<T>.MergeTreeNode node, Func<TreeDiff<T>.MergeTreeNode, string> getLabel)
        {
            var children = new List<TreeNode>();
            foreach (var child in node.children)
            {
                var vchild = buildTree(child, getLabel);

                switch (child.type)
                {
                    case MergeType.Delete:
                        vchild.ForeColor = Color.Red;
                        break;
                    case MergeType.Insert:
                        vchild.ForeColor = Color.Blue;
                        break;
                    case MergeType.Update:
                        vchild.ForeColor = Color.Green;
                        break;
                }

                children.Add(vchild);
            }

            return new TreeNode(getLabel(node), children.ToArray());
        }

        public void addTreeToView<T>(TreeView view, Tree<T> btree, Tree<T> otree, int pos, Func<T, string> getLabel, Func<TreeDiff<T>.MergeTreeNode, string> getLabelMt)
        {

            List<Matching<T>> diffs = null;
            if (btree != otree)
            {
                diffs = TreeDiff<T>.GetDiff(btree, otree, getLabel);
            }

            var mTree = TreeDiff<T>.mergeTree(btree, otree, diffs, getLabel);
            var tree = buildTree<T>(mTree, getLabelMt);

            view.Nodes.Add(tree);
            view.Click += (x, y) =>
            {
                var e = (MouseEventArgs)y;
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                    MessageBox.Show(DiffToString(diffs, getLabel), "Alert", MessageBoxButtons.OK);
            };
            view.ExpandAll();
        }

        public string DiffToString<T>(List<Matching<T>> diff, Func<T, string> getLabel)
        {
            var s = "";
            diff.ForEach(x => s += getLabel(x.bas) + " -> " + getLabel(x.other) + "\n");
            return s;
        }

        public int cntNode(SyntaxNode n)
        {
            if (n.ChildNodes().Count() == 0)
                return 1;
            return n.ChildNodes().Sum(x => cntNode(x));
        }

        public Form1()
        {
            InitializeComponent();

#if true
            var baseSyntax = Examples.SmallBaseTree;
            var leftSyntax = Examples.SmallLeftTree;
            var rightSyntax = Examples.SmallRightTree;


            //var x = cntNode(Examples.smartAlgorithmTree.GetRoot());
            //var y = cntNode(Examples.flowAlgorithm.GetRoot());

            Func<SyntaxNode, string> getLabel = x => x.getLabel();
            Func<TreeDiff<SyntaxNode>.MergeTreeNode, string> getLabelMt = x => getLabel(x.value);

            var b = convert(baseSyntax.GetRoot());
            var l = convert(leftSyntax.GetRoot());
            var r = convert(rightSyntax.GetRoot());

            addTreeToView(baseTree, b, b, 0, getLabel, getLabelMt);
            addTreeToView(leftTree, b, l, 0, getLabel, getLabelMt);
            addTreeToView(rightTree, b, r, 0, getLabel, getLabelMt);
#else
            var b = new Tree<string>("A", new Tree<string>("B", "X"), new Tree<string>("B", "X"), new Tree<string>("C", "Y"));
            var l = new Tree<string>("A", new Tree<string>("C", "Y"), new Tree<string>("B", "X"));
            var r = new Tree<string>("A", new Tree<string>("B", "X"));

            Func<string, string> getLabel = x => x;
            Func<TreeDiff<string>.MergeTreeNode, string> getLabelMt = x => getLabel(x.value);


            addTreeToView(baseTree, b, b, 0, getLabel, getLabelMt);
            addTreeToView(leftTree, b, l, 0, getLabel, getLabelMt);
            addTreeToView(rightTree, b, r, 0, getLabel, getLabelMt);
#endif

        }
    }
}
