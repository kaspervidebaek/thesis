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
            public int pos;
            public MergeTreeNode(T value, T compliment, List<MergeTreeNode> children, int pos)
            {
                this.pos = pos;
                this.value = value;
                this.children = children;
                this.compliment = compliment;
            }
        }

        public static MergeTreeNode mergeTree(Tree<T> btree, Tree<T> otree, List<Matching<T>> diffs, Func<T, string> getLabel, ref int i)
        {
            var children = new List<MergeTreeNode>();
            var bChildren = btree.getChildren();
            var oChildren = otree.getChildren();

            var bCnt = bChildren.Count-1;
            var oCnt = oChildren.Count-1;

            while (oCnt >= 0 || bCnt >= 0)
            {
                Tree<T> bChild = bCnt >= 0 ? bChildren[bCnt] : null;
                Tree<T> oChild = oCnt >= 0 ? oChildren[oCnt] : null;

                MergeTreeNode cNode = null;
                if (oChild == null || bChild == null || getLabel(oChild.value) != getLabel(bChild.value))
                {
                    // Base and other is diffferent. This is an update, a deletion or insert.

                    // Find the corresponding diff.
                    var diff = getMatchingForNodePair(diffs, bChild, oChild);

                    if (diff.isDeletion())
                    {
                        cNode = mergeTree(bChild, null, diffs, getLabel, ref i);
                        bCnt--;
                        cNode.type = MergeType.Delete;
                    }
                    else if (diff.isInsertion())
                    {
                        cNode = mergeTree(null, oChild, diffs, getLabel, ref i);
                        oCnt--;
                        cNode.type = MergeType.Insert;
                    }
                    else
                    { // An update
                        cNode = mergeTree(bChild, oChild, diffs, getLabel, ref i);
                        bCnt--;
                        oCnt--;
                        cNode.type = MergeType.Update;
                    }
                }
                else
                {   // A copy!
                    oCnt--;
                    bCnt--;
                    cNode = mergeTree(bChild, oChild, diffs, getLabel, ref i);
                }

                children.Add(cNode);
            }
            children.Reverse();

            var oNode = otree != null ? otree.value : default(T);
            var bNode = btree != null ? btree.value : default(T);
            MergeTreeNode nnode;
            if (otree == null)
                nnode = new MergeTreeNode(btree.value, oNode, children, i);
            else
                nnode = new MergeTreeNode(otree.value, bNode, children, i);
            i++;
            return nnode;
        }

        // TODO: Performance trouble
        private static Matching<T> getMatchingForNodePair(List<Matching<T>> diffs, Tree<T> bChild, Tree<T> oChild)
        {
            if (oChild == null)
                return new Matching<T>(bChild.value, default(T));
            if (bChild == null)
                return new Matching<T>(default(T), oChild.value);

            var m1 = diffs.Where(x => oChild.value.Equals(x.other)).ToList();
            var m2 = diffs.Where(x => bChild.value.Equals(x.bas)).ToList();

            var matching = diffs.SingleOrDefault(x => oChild.value.Equals(x.other) && x.bas == null); // Insertion

            if (matching == null)
                matching = diffs.SingleOrDefault(x => bChild.value.Equals(x.bas) && x.other == null); // Deletion

            if (matching == null)
                matching = diffs.SingleOrDefault(x => bChild.value.Equals(x.bas) && oChild.value.Equals(x.other)); // Update or Copy

            if (matching == null)
                return new Matching<T>(default(T), default(T));
               // throw new Exception("Reordering exception");

            return matching;
        }

        public static List<Matching<T>> GetDiff(Tree<T> btree, Tree<T> otree, Func<T, string> getLabel)
        {
            Func<Tree<T>, IEnumerable<Tree<T>>> getChildren = x => x.children;
            return JavaMatching<T>.getMapping(btree, otree, getLabel);
        }
    }
}
