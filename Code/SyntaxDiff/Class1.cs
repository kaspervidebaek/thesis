using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using Microsoft.FSharp.Collections;
using util;
using distance;

namespace SyntaxDiff
{
    public class Differ
    {

        public static SyntaxTree QSbaseTree = SyntaxTree.ParseFile(@"CodeSnippets\QuickSortBase.cs");
        public static SyntaxTree QSleftTree =  SyntaxTree.ParseFile(@"CodeSnippets\QuickSortLeft.cs");
        public static SyntaxTree QSrightTree = SyntaxTree.ParseFile(@"CodeSnippets\QuickSortRight.cs");

        public static SyntaxTree baseTree = SyntaxTree.ParseText(@"using System;
                using System2.Linq;
                namespace HelloWorld
                {
                    class Program
                    {
                        static void Main()
                        {
                            Console.WriteLine(""Hello, new World!"");
                        }
                    }
                }");

        public static SyntaxTree leftTree = SyntaxTree.ParseText(@"using System;
                namespace HelloWorld2
                {
                    class Program
                    {
                        static void Main()
                        {
                            Console.WriteLine(""Hello, new World!"");
                        }   
                    }
                }");
        public static SyntaxTree rightTree = SyntaxTree.ParseText(@"using System;
                using System.Linq;
                namespace HelloWorld
                {
                    class Program2
                    {
                        static void Main2(string args)
                        {
                            Console.WriteLine(""Hello, new right World!"");
                        }
                    }
                }");


        static int treeId = 0;
        private static LblTree JSyntaxNodeConvert(SyntaxNode node)
        {
            //var cnode = new Diffing.Tree(NodetToLabel(node), node, ListModule.OfSeq(childNodes));
            var cnode = new LblTree(getType (node), treeId++);
            cnode.setUserObject(node);

            foreach (var child in node.ChildNodes())
            {
                cnode.add(JSyntaxNodeConvert(child));
            }

            return cnode;
        }

        private static LblTree JSyntaxTreeConvert(SyntaxTree tree)
        {
            return JSyntaxNodeConvert(tree.GetRoot());
        }



        public static List<Tuple<LblTree, LblTree>> GetDiff(SyntaxTree bas, SyntaxTree mod)
        {
            var t1 = JSyntaxTreeConvert(bas);
            var t2 = JSyntaxTreeConvert(mod);

            RTED_InfoTree_Opt rted = new RTED_InfoTree_Opt(1, 1, 1);
            double dist = rted.nonNormalizedTreeDist(t1, t2);
            var t1E = java.util.Collections.list(t1.postorderEnumeration()).toArray().ToList().ConvertAll(x => (util.LblTree)x);
            var t2E = java.util.Collections.list(t2.postorderEnumeration()).toArray().ToList().ConvertAll(x => (util.LblTree)x);

            var mapping = rted.computeEditMapping().toArray().ToList().ConvertAll(x => {
                    var m = (int[])x;
                    LblTree element1 = m[0] != 0 ? t1E[m[0] - 1] : null;
                    LblTree element2 = m[1] != 0 ? t2E[m[1] - 1] : null;

                    return Tuple.Create(element1, element2);
                }
            );
        
            return mapping;
        }

        public static string getSyntaxString<T>(SyntaxNode node, Func<T, string> f) where T : SyntaxNode
        {
            if(node is T) {
                var c = node as T;
                return "[" + f(c) + "]";
            }
            return "";
        }

        public static string getType(LblTree t)
        {
            if (t == null)
                return "empty";
            return getType((SyntaxNode)t.getUserObject());
        }

        public static string getType(SyntaxNode t)
        {
            if (t == null)
                return "empty";

            string s = t.GetType().ToString().Substring(24);

            s += getSyntaxString<IdentifierNameSyntax>(t, x => x.Identifier.ToString());
            s += getSyntaxString<ClassDeclarationSyntax>(t, x => x.Identifier.ToString());
            s += getSyntaxString<LiteralExpressionSyntax>(t, x => x.Token.ToString());
            s += getSyntaxString<ArrayTypeSyntax>(t, x => x.ElementType.ToString());
            s += getSyntaxString<PredefinedTypeSyntax>(t, x => x.ToString());

            return s;
        }


        public static List<SyntaxNode> getChildrenEnum(SyntaxNode n)
        {
            var childEnum = (n == null ? new List<SyntaxNode>() : n.ChildNodes()).ToList();
            return childEnum;
        }

        public  class MergeTreeNode
        {
            public enum MergeType {
                None, Delete, Insert, Update
            }

            public MergeType type = MergeType.None;
            public SyntaxNode node;
            public List<MergeTreeNode> children;

            public MergeTreeNode(SyntaxNode node, List<MergeTreeNode> children)
            {
                this.node = node;
                this.children = children;
            }
        }

        public static MergeTreeNode mergeTree(SyntaxNode btree, SyntaxNode otree, List<Tuple<LblTree, LblTree>> diffs)
        {
            var children = new List<MergeTreeNode>();
            var bChildren = getChildrenEnum(btree);
            var oChildren = getChildrenEnum(otree);

            var bCnt = 0;
            var oCnt = 0;

            while (oCnt < oChildren.Count || bCnt < bChildren.Count)
            {
                SyntaxNode bChild = bCnt < bChildren.Count ? bChildren[bCnt] : null;
                SyntaxNode oChild = oCnt < oChildren.Count ? oChildren[oCnt] : null;

                MergeTreeNode cNode = null;
                if (bChild == null) // The base is depleted print the remaining "other", as added nodes.
                {
                    oCnt++;
                    cNode = mergeTree(bChild, oChild, diffs);
                    cNode.type = MergeTreeNode.MergeType.Insert;
                }
                else if (oChild == null) // Other is depleted, print the remaining base, as deleted nodes.
                {
                    bCnt++;
                    cNode = mergeTree(bChild, oChild, diffs);
                    cNode.type = MergeTreeNode.MergeType.Delete;
                }
                else if (SyntaxDiff.Differ.getType(oChild) != SyntaxDiff.Differ.getType(bChild)) // Base and other is diffferent. This is an update, a deletion or insert.
                {
                    // Find the corresponding "other child" diff.
                    var diff = diffs.Single(x => x.Item2 != null && x.Item2.getUserObject() == oChild);

                    if (diff.Item1 == null)
                    {// If Item1 is null, then this is an insertion
                        cNode = mergeTree(null, oChild, diffs);
                        oCnt++;
                        cNode.type = MergeTreeNode.MergeType.Insert;
                    }
                    else
                    {
                        // Find the corresponding base child
                        diff = diffs.Single(x => x.Item1 != null && x.Item1.getUserObject() == bChild);

                        if (diff.Item2 == null)
                        {// If Item2 is null, then this is a deletion
                            cNode = mergeTree(bChild, null, diffs);
                            bCnt++;
                            cNode.type = MergeTreeNode.MergeType.Delete;
                        }
                        else // Last case: An Update
                        {
                            cNode = mergeTree(bChild, oChild, diffs);
                            bCnt++;
                            oCnt++;
                            cNode.type = MergeTreeNode.MergeType.Update;
                        }
                    }

                }
                else
                {
                    oCnt++;
                    bCnt++;
                    cNode = mergeTree(bChild, oChild, diffs);
                }

                if (cNode != null)
                    children.Add(cNode);
            }

            MergeTreeNode nnode;
            if (otree == null)
                nnode = new MergeTreeNode(btree, children);
            else
                nnode = new MergeTreeNode(otree, children);

            return nnode;
        }
    }
}
