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
using util;
using SyntaxDiff;

namespace WindowApp
{
    public partial class Form1 : Form
    {
        public TreeNode buildTree(Differ.MergeTreeNode node)
        {
            var children = new List<TreeNode>();
            foreach (var child in node.children)
            {
                var vchild = buildTree(child);

                switch (child.type)
                {
                    case Differ.MergeTreeNode.MergeType.Delete:
                        vchild.ForeColor = Color.Red;
                        break;
                    case Differ.MergeTreeNode.MergeType.Insert:
                        vchild.ForeColor = Color.Blue;
                        break;
                    case Differ.MergeTreeNode.MergeType.Update:
                        vchild.ForeColor = Color.Green;
                        break;
                }

                children.Add(vchild);
            }

            return new TreeNode(Differ.getType(node.node), children.ToArray());
        }

        public void addTreeToView(TreeView view, SyntaxTree btree, SyntaxTree otree, List<Tuple<LblTree, LblTree>> diffs, int pos)
        {
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

        public string DiffToString(List<Tuple<LblTree, LblTree>> diff)
        {
            var s = "";
            diff.ForEach(x => s += SyntaxDiff.Differ.getType(x.Item1) + " -> " + SyntaxDiff.Differ.getType(x.Item2) + "\n");
            return s;
        }

        public Form1()
        {
            InitializeComponent();

            var baseSyntax = SyntaxDiff.Differ.baseTree;
            var leftSyntax = SyntaxDiff.Differ.leftTree;
            var rightSyntax = SyntaxDiff.Differ.rightTree;



            addTreeToView(baseTree, baseSyntax, baseSyntax, null, 0);
            addTreeToView(leftTree, baseSyntax, leftSyntax, SyntaxDiff.Differ.GetDiff(baseSyntax, leftSyntax), 0);
            addTreeToView(rightTree, baseSyntax, rightSyntax, SyntaxDiff.Differ.GetDiff(baseSyntax, rightSyntax), 0);
        }

        private void leftTree_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }
    }
}
