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
        public enum MergeType
        {
            None, Delete, Insert, Update, WTF
        }

        public static TreeNode buildDiffTree<T>(Tree<Diff<T>> node, Func<T, string> getLabel)
        {
            var children = new List<TreeNode>();
            foreach (var child in node.children)
            {
                var vchild = buildDiffTree(child, getLabel);
                var matching = child.value;
                children.Add(vchild);
            }
            var n = new TreeNode(getLabel(node.value.A) + " <- " + getLabel(node.value.O) + "->" + getLabel(node.value.B), children.ToArray());
            n.Tag = node;
            return n;
        }

        public static TreeNode buildTree<T>(Tree<Matching<T>> node, Func<T, string> getLabel)
        {
            var children = new List<TreeNode>();
            foreach (var child in node.children)
            {
                var vchild = buildTree(child, getLabel);

                var matching = child.value;

                switch (getMergeType(matching, getLabel))
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
                    case MergeType.WTF:
                        vchild.ForeColor = Color.Cyan;
                        break;
                }

                children.Add(vchild);
            }
            var val = node.value.bas != null ? node.value.bas : node.value.other;
            var n = new TreeNode(getLabel(val), children.ToArray());
            n.Tag = node;
            return n;
        }

        private static MergeType getMergeType<T>(Matching<T> matching, Func<T, string> getLabel)
        {
            if (matching.bas == null && matching.other != null)
            {
                return MergeType.Insert;
            }
            else if (matching.bas != null && matching.other == null)
            {
                return MergeType.Delete;
            }
            else if (getLabel(matching.bas) != getLabel(matching.other))
            {
                return MergeType.Update;
            }
            return MergeType.None;
        }

        public void addTreeToView<T>(TreeView view, Tree<T> btree, Tree<T> otree, Func<T, string> getLabel)
        {
            Func<T, T, bool> equals = (x, y) => getLabel(x) == getLabel(y);
            

            var mTree = Tree<T>.Match(btree, otree, equals);
            var tree = buildTree<T>(mTree, getLabel);

            view.Nodes.Add(tree);
            view.ExpandAll();
        }

        public string DiffToString<T>(List<Matching<T>> diff, Func<T, string> getLabel)
        {
            var s = "bas->other\n";
            diff.ForEach(x => s += getLabel(x.bas) + " -> " + getLabel(x.other) + "\n");
            return s;
        }

        public int cntNode(SyntaxNode n)
        {
            if (n.ChildNodes().Count() == 0)
                return 1;
            return n.ChildNodes().Sum(x => cntNode(x));
        }

        public void addDiffTreeToView<T>(TreeView view, Tree<Diff<T>> mTree, Func<T, string> getLabel)
        {
            var tree = buildDiffTree<T>(mTree, getLabel);
            view.Nodes.Add(tree);
            view.ExpandAll();
        }

        public Form1()
        {
            InitializeComponent();

            var i = new SyntaxDiff.SyntaxNodeSmartDiff();


            var baseSyntax = Examples.ConflictingBaseTree;
            var leftSyntax = Examples.ConflictingLeftTree;
            var rightSyntax = Examples.ConflictingRightTree;


            Func<SyntaxNode, string> getLabel = x => i.getLabel(x);

            var b = i.ConvertToTree(baseSyntax.GetRoot());
            var l = i.ConvertToTree(leftSyntax.GetRoot());
            var r = i.ConvertToTree(rightSyntax.GetRoot());

            addTreeToView(leftTree, b, l, getLabel);
            addTreeToView(baseTree, b, b, getLabel);
            addTreeToView(rightTree, b, r, getLabel);

            var mergedTree = Tree<SyntaxNode>.ThreeWayMatch(l, b, r, (x, y) => i.getLabel(x) == i.getLabel(y));

            addDiffTreeToView(bottomTree, mergedTree, getLabel);
        }

        private void leftTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Determine by checking the Text property.
            //MessageBox.Show(((SyntaxNode)e.Node.Tag).ToString());
        }
    }
}
