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
        public TreeNode buildTree(MergeTreeNode node)
        {
            var children = new List<TreeNode>();
            foreach (var child in node.children)
            {
                var vchild = buildTree(child);

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

            return new TreeNode(node.node.getLabel(), children.ToArray());
        }

        public void addTreeToView(TreeView view, SyntaxTree btree, SyntaxTree otree, int pos)
        {
            List<Matching> diffs = null;
            if (btree != otree)
                diffs = Differ.GetDiff(btree, otree);

            var tree = buildTree(Differ.mergeTree(btree.GetRoot(), otree.GetRoot(), diffs));

            view.Nodes.Add(tree);
            view.Click += (x, y) =>
            {
                var e = (MouseEventArgs)y;
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                    MessageBox.Show(DiffToString(diffs), "Alert", MessageBoxButtons.OK);
            };
            view.ExpandAll();
        }

        public string DiffToString(List<Matching> diff)
        {
            var s = "";
            diff.ForEach(x => s += x.bas.getLabel() + " -> " + x.other.getLabel() + "\n");
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

            var baseSyntax = Examples.baseTree;
            var leftSyntax = Examples.leftTree;
            var rightSyntax = Examples.rightTree;


            var x = cntNode(Examples.smartAlgorithmTree.GetRoot());
            var y = cntNode(Examples.flowAlgorithm.GetRoot());

            addTreeToView(baseTree, baseSyntax, baseSyntax, 0);
            addTreeToView(leftTree, baseSyntax, leftSyntax, 0);
            addTreeToView(rightTree, baseSyntax, rightSyntax, 0);
        }
    }
}
