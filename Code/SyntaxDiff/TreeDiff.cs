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

    public class MergeTreeNode
    {
        public MergeType type = MergeType.None;
        public SyntaxNode node;
        public List<MergeTreeNode> children;

        public MergeTreeNode(SyntaxNode node, List<MergeTreeNode> children)
        {
            this.node = node;
            this.children = children;
        }
    }

    public class Differ
    {
        public static MergeTreeNode mergeTreeOld(SyntaxNode btree, SyntaxNode otree, List<Matching> diffs)
        {
            var children = new List<MergeTreeNode>();
            var bChildren = btree.getChildrenEnum();
            var oChildren = otree.getChildrenEnum();

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
                    cNode = mergeTreeOld(bChild, oChild, diffs);
                    cNode.type = MergeType.Insert;
                }
                else if (oChild == null) // Other is depleted, print the remaining base, as deleted nodes.
                {
                    bCnt++;
                    cNode = mergeTreeOld(bChild, oChild, diffs);
                    cNode.type = MergeType.Delete;
                }
                else if (oChild.getLabel() != bChild.getLabel()) // Base and other is diffferent. This is an update, a deletion or insert.
                {
                    // Find the corresponding "other child" diff.
                    var diff = diffs.Single(x => x.other == oChild);

                    if (diff.bas == null)
                    {// If Item1 is null, then this is an insertion
                        cNode = mergeTreeOld(null, oChild, diffs);
                        oCnt++;
                        cNode.type = MergeType.Insert;
                    }
                    else
                    {
                        // Find the corresponding base child
                        diff = diffs.Single(x => x.bas == bChild);

                        if (diff.other == null)
                        {// If Item2 is null, then this is a deletion
                            cNode = mergeTreeOld(bChild, null, diffs);
                            bCnt++;
                            cNode.type = MergeType.Delete;
                        }
                        else // Last case: An Update
                        {
                            cNode = mergeTreeOld(bChild, oChild, diffs);
                            bCnt++;
                            oCnt++;
                            cNode.type = MergeType.Update;
                        }
                    }

                }
                else
                {
                    oCnt++;
                    bCnt++;
                    cNode = mergeTreeOld(bChild, oChild, diffs);
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


        public static MergeTreeNode mergeTree(SyntaxNode btree, SyntaxNode otree, List<Matching> diffs)
        {
            var children = new List<MergeTreeNode>();
            var bChildren = btree.getChildrenEnum();
            var oChildren = otree.getChildrenEnum();

            var bCnt = 0;
            var oCnt = 0;

            while (oCnt < oChildren.Count || bCnt < bChildren.Count)
            {
                SyntaxNode bChild = bCnt < bChildren.Count ? bChildren[bCnt] : null;
                SyntaxNode oChild = oCnt < oChildren.Count ? oChildren[oCnt] : null;

                MergeTreeNode cNode = null;
                if (oChild.getLabel() != bChild.getLabel()) 
                {
                    // Base and other is diffferent. This is an update, a deletion or insert.

                    // Find the corresponding diff.
                    var diff = getMatchingForNodePair(diffs, bChild, oChild);

                    if (diff.isDeletion())
                    {
                        cNode = mergeTree(bChild, null, diffs);
                        bCnt++;
                        cNode.type = MergeType.Delete;
                    }
                    else if (diff.isInsertion())
                    {
                        cNode = mergeTree(null, oChild, diffs);
                        oCnt++;
                        cNode.type = MergeType.Insert;
                    }
                    else
                    { // An update
                        cNode = mergeTree(bChild, oChild, diffs);
                        bCnt++;
                        oCnt++;
                        cNode.type = MergeType.Update;
                    }
                }
                else
                {   // A copy!
                    oCnt++;
                    bCnt++;
                    cNode = mergeTree(bChild, oChild, diffs);
                }

                children.Add(cNode);
            }

            MergeTreeNode nnode;
            if (otree == null)
                nnode = new MergeTreeNode(btree, children);
            else
                nnode = new MergeTreeNode(otree, children);

            return nnode;
        }

        public class MergeArguments
        {
            public SyntaxNode btree;
            public SyntaxNode ltree;
            public SyntaxNode rtree;
            public List<Matching> ldiffs;
            public List<Matching> rdiffs;

            public MergeArguments(SyntaxNode btree, SyntaxNode ltree, SyntaxNode rtree, List<Matching> ldiffs, List<Matching> rdiffs)
            {
                this.btree = btree;
                this.ltree = ltree;
                this.rtree = rtree;
                this.ldiffs = ldiffs;
                this.rdiffs = rdiffs;
            }
        }

        public static MergeTreeNode merge3Tree(MergeArguments a)
        {
            var children = new List<MergeTreeNode>();
            var bChildren = a.btree.getChildrenEnum();
            var lChildren = a.ltree.getChildrenEnum();
            var rChildren = a.rtree.getChildrenEnum();

            var bCnt = 0;
            var lCnt = 0;
            var rCnt = 0;

            while (rCnt < rChildren.Count || lCnt < lChildren.Count || bCnt < bChildren.Count)
            {
                SyntaxNode bChild = bCnt < bChildren.Count ? bChildren[bCnt] : null;
                SyntaxNode lChild = lCnt < lChildren.Count ? lChildren[lCnt] : null;
                SyntaxNode rChild = rCnt < rChildren.Count ? rChildren[rCnt] : null;

                MergeTreeNode cNode = null;

                var leftToBaseEquals = lChild.getLabel() == bChild.getLabel();
                var rightToBaseEquals = lChild.getLabel() == bChild.getLabel();

                if (leftToBaseEquals && rightToBaseEquals)
                {
                    bCnt++; rCnt++; lCnt++;
                    children.Add(merge3Tree(new MergeArguments(bChild, lChild, rChild, a.ldiffs, a.rdiffs)));
                }
                else if (leftToBaseEquals && !rightToBaseEquals)
                {
                    var diff = getMatchingForNodePair(a.ldiffs, bChild, lChild);

                    UseMatching(diff, a, children, ref bCnt, ref lCnt, ref rCnt);
                }


                if (cNode != null)
                    children.Add(cNode);
            }
            /*
            MergeTreeNode nnode;
            if (otree == null)
                nnode = new MergeTreeNode(btree, children);
            else
                nnode = new MergeTreeNode(otree, children);

            return nnode;*/
            return null;
        }

        private static void UseMatching(Matching m, MergeArguments ma, List<MergeTreeNode> children, ref int bCnt, ref int thisCnt, ref int otherCnt)
        {
            if (m.isDeletion())
            {
                bCnt++;
                otherCnt++;
            }
            else if (m.isInsertion())
            {
                thisCnt++;
                children.Add(merge3Tree(ma));
            }
        }



        private static Matching getMatchingForNodePair(List<Matching> diffs, SyntaxNode bChild, SyntaxNode oChild)
        {
            if (oChild == null)
                return new Matching(bChild, null);
            if (bChild == null)
                return new Matching(null, oChild);

            var matching = diffs.SingleOrDefault(x => x.other == oChild && x.bas == null); // Insertion
            
            if(matching == null)
                matching = diffs.SingleOrDefault(x => x.bas == bChild && x.other == null); // Deletion

            if (matching == null)
                matching = diffs.SingleOrDefault(x => x.bas == bChild && x.other == oChild); // Update or Copy

            if (matching == null)
                throw new Exception("This is wrong!");

            return matching;
        }

        public static List<Matching> GetDiff(SyntaxTree btree, SyntaxTree otree)
        {
            return JavaMatching.GetDiff(btree, otree);
        }
    }
}
