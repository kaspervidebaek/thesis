using distance;
using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using util;

namespace SyntaxDiff
{
    public class JavaMatching<T>
    {


        static int treeId = 0;
        private static LblTree JSyntaxNodeConvert(Tree<T> node, Func<T, string> getLabel)
        {
            //var cnode = new Diffing.Tree(NodetToLabel(node), node, ListModule.OfSeq(childNodes));
            var cnode = new LblTree(getLabel(node.value), treeId++);
            cnode.setUserObject(node.value);

            foreach (var child in node.children)
            {
                cnode.add(JSyntaxNodeConvert(child, getLabel));
            }

            return cnode;
        }

        public static List<Matching<T>> getMapping(Tree<T> bas, Tree<T> mod, Func<T, string> getLabel)
        {
            var t1 = JSyntaxNodeConvert(bas, getLabel);
            var t2 = JSyntaxNodeConvert(mod, getLabel);

            RTED_InfoTree_Opt rted = new RTED_InfoTree_Opt(1, 1, 1);

            double dist = rted.nonNormalizedTreeDist(t1, t2);


            var t1E = java.util.Collections.list(t1.postorderEnumeration()).toArray().ToList().ConvertAll(x => (util.LblTree)x);
            var t2E = java.util.Collections.list(t2.postorderEnumeration()).toArray().ToList().ConvertAll(x => (util.LblTree)x);

            var map = rted.computeEditMapping().toArray().ToList();
            var mapping = map.ConvertAll(x =>
                {
                    var m = (int[])x;
                    T element1 = m[0] != 0 ? (T)t1E[m[0] - 1].getUserObject() : default(T);
                    T element2 = m[1] != 0 ? (T)t2E[m[1] - 1].getUserObject() : default(T);

                    return new Matching<T>(element1, element2);
                }
            );


            return mapping;
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

        public static Tree<Matching<T>> getMappingTree(Tree<T> bas, Tree<T> mod, Func<T, string> getLabelT)
        {
            var basTreeConverted = bas.Convert(x => new TreeWithMatching { tree = x });
            var modTreeConverted = mod.Convert(x => new TreeWithMatching { tree = x });

            var basIt = basTreeConverted.PostOrderEnumeration();
            var modIt = modTreeConverted.PostOrderEnumeration();

            Func<TreeWithMatching, string> getLabel = x => getLabelT(x.tree);

            var mapping = JavaMatching<TreeWithMatching>.getMapping(basTreeConverted, modTreeConverted, getLabel);

            foreach (var map in mapping)
            {
                var b = map.bas == null ? default(T) : map.bas.tree;
                var o = map.other == null ? default(T) : map.other.tree;
                var matching = new Matching<T>(b, o);
                if(map.bas != null)
                    map.bas.matching = matching;
                if (map.other != null)
                    map.other.matching = matching;
            }

            var result = BuildMappingTree(basTreeConverted, modTreeConverted);

            return result;
        }



        public static Tree<Matching<T>> BuildMappingTree(Tree<TreeWithMatching> bas, Tree<TreeWithMatching> other)
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
}
