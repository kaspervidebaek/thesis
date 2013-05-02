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

        class TreeWithMatching
        {
           public T tree;
           public Matching<T> matching;
           public override string ToString()
           {

               return "(" + tree.ToString() +  "," + matching.ToString() + ")";
           }
        }

        public static Tree<Matching<T>> getMappingTree(Tree<T> bas, Tree<T> mod, Func<T, string> getLabel)
        {
            var basTreeConverted = bas.Convert(x => new TreeWithMatching { tree = x });
            var modTreeConverted = mod.Convert(x => new TreeWithMatching { tree = x });

            var basIt = basTreeConverted.PostOrderEnumeration();
            var modIt = modTreeConverted.PostOrderEnumeration();

            var mapping = JavaMatching<T>.getMapping(bas, mod, getLabel);

            var bCnt = 0;
            var oCnt = 0;

            foreach (var map in mapping)
            {

                if (map.other == null)
                {
                    modIt[oCnt].matching = map;
                    oCnt++;
                }
                else if (map.bas == null)
                {
                    basIt[bCnt].matching = map;
                    bCnt++;
                }
                else
                {
                    basIt[bCnt].matching = map;
                    modIt[oCnt].matching = map;
                    oCnt++;
                    bCnt++;
                }
            }



            return null;
        }

    }
}
