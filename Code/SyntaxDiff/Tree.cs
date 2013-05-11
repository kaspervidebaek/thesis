using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{
    public class Tree<T>
    {
        public T value;
        public List<Tree<T>> children;

        public Tree(T v, params Tree<T>[] children)
        {
            value = v;
            this.children = children.ToList();
        }

        public Tree(T v, params T[] children)
        {
            value = v;
            this.children = children.Select(x => new Tree<T>(x)).ToList();
        }

        public Tree(T v, params Object[] children)
        {
            value = v;
            this.children = children.Select(x =>
            {
                if (x is T)
                    return new Tree<T>((T)x);
                else if (x is Tree<T>)
                    return (Tree<T>)x;
                throw new Exception();
            }
                ).ToList();
        }


        public override string ToString()
        {
            return value.ToString();
        }

        public Tree<Y> Convert<Y>(Func<T, Y> c)
        {
            var newchildren = this.children.Select(x => x.Convert(c));
            return new Tree<Y>(c(value), newchildren.ToArray());
        }

        public List<T> PostOrderEnumeration()
        {
            var rv = new List<T>();

            foreach (var child in children.Reverse<Tree<T>>())
            {
                rv.AddRange(child.PostOrderEnumeration());
            }

            rv.Add(this.value);
            return rv;
        }

        public int Size()
        {
            return 1 + children.Select(x => x.Size()).Sum();
        }


        public class TreeWithMatching
        {
            public T tree;
            public Matching<T> matching;
            public override string ToString()
            {
                return "(" + tree.ToString() + ")";
                return "(" + tree.ToString() + "," + matching.ToString() + ")";
            }
        }


        public static Tree<T> Merge(Tree<T> A, Tree<T> O, Tree<T> B, Func<T, T, bool> equals)
        {
            var diff = Tree<T>.TreeWayMatch(A, O, B, equals);


            return MergeDiff(diff) ;
        }



        public static Tree<T> MergeDiff(Tree<Diff<T>> diffs)
        {
            var children = new List<Tree<T>>();
            foreach (var diff in diffs.children)
            {
                var merge = MergeDiff(diff);
                if (merge != null)
                    children.Add(merge);
            }

            if (diffs.value.O == null)
            {
                if (diffs.value.A != null)
                    return new Tree<T>(diffs.value.A, children.ToArray());
                if (diffs.value.B != null)
                    return new Tree<T>(diffs.value.B, children.ToArray());
            }
            else if(diffs.value.A != null && diffs.value.B != null )
            {
                return new Tree<T>(diffs.value.O, children.ToArray());
            }

            return null;
        }
        public static Tree<Diff<T>> TreeWayMatch(Tree<T> A, Tree<T> O, Tree<T> B, Func<T, T, bool> equals)
        {
            var ao = Match(O, A, equals);
            var ob = Match(O, B, equals);

            var match = Tree<Matching<T>>.Match(ao, ob, (x, y) => x.bas != null && y.bas != null && equals(x.bas, y.bas)).Convert(x => new Diff<T>(x.bas, x.other)); // TODO: Examine why we nede to write equals here instead of Object.referenceequals

            return match;
        }

        public static Tree<Matching<T>> Match(Tree<T> bas, Tree<T> mod, Func<T, T, bool> equals)
        {
            var basTreeConverted = bas.Convert(x => new TreeWithMatching { tree = x });
            var modTreeConverted = mod.Convert(x => new TreeWithMatching { tree = x });

            var basIt = basTreeConverted.PostOrderEnumeration();
            var modIt = modTreeConverted.PostOrderEnumeration();

            Func<TreeWithMatching, TreeWithMatching, bool> e = (x, y) =>
            {
                return equals(x.tree, y.tree);
            };

            var matches = NeedlemanWunsch<TreeWithMatching>.Allignment(basIt, modIt, e);

            foreach (var match in matches)
            {
                var b = match.Item1 == null ? default(T) : match.Item1.tree;
                var o = match.Item2 == null ? default(T) : match.Item2.tree;
                var matching = new Matching<T>(b, o);
                if (match.Item1 != null)
                    match.Item1.matching = matching;
                if (match.Item2 != null)
                    match.Item2.matching = matching;
            }

            return BuildMappingTree(basTreeConverted, modTreeConverted);
        }


        private static Tree<Matching<T>> BuildMappingTree(Tree<TreeWithMatching> bas, Tree<TreeWithMatching> other)
        {
            var bCnt = 0;
            var oCnt = 0;

            var bChildren = bas.getChildren();
            var oChildren = other.getChildren();

            var resultChildren = new List<Tree<Matching<T>>>();


            while (bCnt < bChildren.Count || oCnt < oChildren.Count)
            {
                var bChild = bCnt < bChildren.Count ? bChildren[bCnt] : null;
                var oChild = oCnt < oChildren.Count ? oChildren[oCnt] : null;

                var bChildV = bChild != null ? bChild.value : null;
                var oChildV = oChild != null ? oChild.value : null;

                var bChildM = bChildV != null ? bChildV.matching : null;
                var oChildM = oChildV != null ? oChildV.matching : null;

                if (oChild == null) // Deletion at end of sequence
                {
                    resultChildren.Add(bChild.Convert(x => x.matching));
                    bCnt++;
                }
                else if (bChild == null) // Insertion at end of sequence
                {
                    resultChildren.Add(oChild.Convert(x => x.matching));
                    oCnt++;
                }
                else if (bChildM == oChildM)
                {
                    resultChildren.Add(BuildMappingTree(bChild, oChild));
                    bCnt++;
                    oCnt++;
                }
                else
                {
                    if (bChildM != null && bChildM.other == null) // Deletion
                    {

                        resultChildren.Add(bChild.Convert(x => x.matching));
                        bCnt++;
                    }
                    else if (oChildM != null && oChildM.bas == null) // Insertion
                    {
                        resultChildren.Add(oChild.Convert(x => x.matching));
                        oCnt++;
                    }
                    else
                        throw new Exception("Should not happen");

                }
            }

            var mapping = bas == null || bas.value == null || bas.value.matching == null ? other.value.matching : bas.value.matching;

            // Take care of 10->2 here.
            var result = new Tree<Matching<T>>(mapping, resultChildren.ToArray());

            return result;
        }
    }

    public static class TreeExtensions
    {

        public static Tree<SyntaxNode> ConvertToTree(this SyntaxNode n)
        {
            var children = n.ChildNodes().Select(x => x.ConvertToTree()).ToArray();
            return new Tree<SyntaxNode>(n, children);
        }


        public static List<Tree<T>> getChildren<T>(this Tree<T> c)
        {
            if (c != null)
                return c.children;
            else return new List<Tree<T>>();
        }
    }
}
