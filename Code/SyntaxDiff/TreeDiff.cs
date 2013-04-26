using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using Microsoft.FSharp.Collections;

namespace SyntaxDiff
{

    public enum MergeType
    {
        None, Delete, Insert, Update
    }

    public class TreeDiff<T>
    {
        public class MergeTreeNode
        {
            public MergeType type;
            public List<MergeTreeNode> children;
            public T value;
            public T compliment;
            public MergeTreeNode(T value, T compliment, List<MergeTreeNode> children)
            {
                this.value = value;
                this.children = children;
                this.compliment = compliment;
            }
        }

        public static MergeTreeNode mergeTree(Tree<T> btree, Tree<T> otree, List<Matching<T>> diffs, Func<T, string> getLabel)
        {
            var children = new List<MergeTreeNode>();
            var bChildren = btree.getChildren();
            var oChildren = otree.getChildren();

            var bCnt = 0;
            var oCnt = 0;

            while (oCnt < oChildren.Count || bCnt < bChildren.Count)
            {
                Tree<T> bChild = bCnt < bChildren.Count ? bChildren[bCnt] : null;
                Tree<T> oChild = oCnt < oChildren.Count ? oChildren[oCnt] : null;

                MergeTreeNode cNode = null;
                if (oChild == null || bChild == null || getLabel(oChild.value) != getLabel(bChild.value))
                {
                    // Base and other is diffferent. This is an update, a deletion or insert.

                    // Find the corresponding diff.
                    var diff = getMatchingForNodePair(diffs, bChild, oChild);

                    if (diff.isDeletion())
                    {
                        cNode = mergeTree(bChild, null, diffs, getLabel);
                        bCnt++;
                        cNode.type = MergeType.Delete;
                    }
                    else if (diff.isInsertion())
                    {
                        cNode = mergeTree(null, oChild, diffs, getLabel);
                        oCnt++;
                        cNode.type = MergeType.Insert;
                    }
                    else
                    { // An update
                        cNode = mergeTree(bChild, oChild, diffs, getLabel);
                        bCnt++;
                        oCnt++;
                        cNode.type = MergeType.Update;
                    }
                }
                else
                {   // A copy!
                    oCnt++;
                    bCnt++;
                    cNode = mergeTree(bChild, oChild, diffs, getLabel);
                }

                children.Add(cNode);
            }
            var oNode = otree != null ? otree.value : default(T);
            MergeTreeNode nnode;
            if (otree == null)
                nnode = new MergeTreeNode(btree.value, oNode, children);
            else
                nnode = new MergeTreeNode(otree.value, oNode, children);

            return nnode;
        }

        // TODO: Performance trouble
        private static Matching<T> getMatchingForNodePair(List<Matching<T>> diffs, Tree<T> bChild, Tree<T> oChild)
        {
            if (oChild == null)
                return new Matching<T>(bChild.value, default(T));
            if (bChild == null)
                return new Matching<T>(default(T), oChild.value);

            var matching = diffs.SingleOrDefault(x => oChild.value.Equals(x.other) && x.bas == null); // Insertion

            if (matching == null)
                matching = diffs.SingleOrDefault(x => bChild.value.Equals(x.bas) && x.other == null); // Deletion

            if (matching == null)
                matching = diffs.SingleOrDefault(x => bChild.value.Equals(x.bas) && oChild.value.Equals(x.other)); // Update or Copy

            if (matching == null)
                return new Matching<T>(default(T), default(T));

            return matching;
        }

        public static List<Matching<T>> GetDiff(Tree<T> btree, Tree<T> otree, Func<T, string> getLabel)
        {
            Func<Tree<T>, IEnumerable<Tree<T>>> getChildren = x => x.children;
            return JavaMatching<T>.getMapping(btree, otree, getLabel);
        }
    }
}
