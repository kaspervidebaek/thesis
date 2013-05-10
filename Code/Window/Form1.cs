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


        public static Tree<SyntaxNode> convert(SyntaxNode n)
        {
            var children = n.ChildNodes().Select(x => convert(x)).ToArray();
            return new Tree<SyntaxNode>(n, children);
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
            var n = new TreeNode(getLabel(node.value.bas), children.ToArray());
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

        public void addTreeToView<T>(TreeView view, Tree<T> btree, Tree<T> otree, int pos, Func<T, string> getLabel)
        {
            
            List<Matching<T>> diffs = null;
            Func<T, T, bool> equals = (x, y) => getLabel(x) == getLabel(y);
            

            var mTree = Tree<T>.Match(btree, otree, equals);
            var tree = buildTree<T>(mTree, getLabel);

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

            var b = convert(baseSyntax.GetRoot());
            var l = convert(leftSyntax.GetRoot());
            var r = convert(rightSyntax.GetRoot());

            //            addTreeToView(baseTree, b, b, 0, getLabel, getLabelMt);
            addTreeToView(leftTree, b, l, 0, getLabel);
            //          addTreeToView(rightTree, b, r, 0, getLabel, getLabelMt);
            addTreeToView(baseTree, b, b, 0, getLabel);
            addTreeToView(rightTree, l, l, 0, getLabel);
#else
            var b = new Tree<String>("Class",
                    new Tree<string>("Function",
                        new Tree<string>("Arguments"),
                        new Tree<string>("Block",
                            new Tree<string>("LocalDec",
                                new Tree<string>("Variable",
                                    new Tree<String>("Identifier"),
                                    new Tree<string>("VariableD",
                                        new Tree<string>("equals", new Tree<string>( "LiteralA"))))),
                            new Tree<string>("Expression",
                                new Tree<string>("Invocation",
                                    new Tree<string>("MemberAccess",
                                        new Tree<string>("Identifier1"),
                                        new Tree<string>("Identifier2"))),
                                new Tree<string>("ArgumentList",
                                    new Tree<string>("Argument",
                                        new Tree<string>("LiteralB")
                                        ))),
                            new Tree<string>("Expression",
                                new Tree<string>("Invocation",
                                    new Tree<string>("MemberAccess",
                                        new Tree<string>("Identifier1"),
                                        new Tree<string>("Identifier2"))),
                                new Tree<string>("ArgumentList",
                                    new Tree<string>("Argument",
                                        new Tree<string>("LiteralB")
                                        )))
                                )));

            var l = new Tree<String>("Class",
                    new Tree<string>("Function",
                        new Tree<string>("Arguments"),
                        new Tree<string>("Block",
                            new Tree<string>("Expression",
                                new Tree<string>("Invocation",
                                    new Tree<string>("MemberAccess",
                                        new Tree<string>("Identifier1"),
                                        new Tree<string>("Identifier2"))),
                                new Tree<string>("ArgumentList",
                                    new Tree<string>("Argument",
                                        new Tree<string>("LiteralB")
                                        ))),
                            new Tree<string>("LocalDec",
                                new Tree<string>("Variable",
                                    new Tree<String>("Identifier"),
                                    new Tree<string>("VariableD",
                                        new Tree<string>("equals", new Tree<string>("LiteralA")))))
                                )));
            Func<string, string> getLabel = x => x;
            Func<TreeDiff<string>.MergeTreeNode, string> getLabelMt = x => getLabel(x.value);


            addTreeToView(baseTree, b, b, 0, getLabel);
            addTreeToView(leftTree, b, l, 0, getLabel);
            addTreeToView(rightTree, l, l, 0, getLabel);
#endif

        }

        private void leftTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Determine by checking the Text property.
            //MessageBox.Show(((SyntaxNode)e.Node.Tag).ToString());
        }
    }
}
