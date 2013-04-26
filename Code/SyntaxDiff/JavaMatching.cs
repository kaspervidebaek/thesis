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
        private static LblTree JSyntaxNodeConvert(T node, Func<T, string> getLabel, Func<T, IEnumerable<T>> getChildren)
        {
            //var cnode = new Diffing.Tree(NodetToLabel(node), node, ListModule.OfSeq(childNodes));
            var cnode = new LblTree(getLabel(node), treeId++);
            cnode.setUserObject(node);

            foreach (var child in getChildren(node))
            {
                cnode.add(JSyntaxNodeConvert(child, getLabel, getChildren));
            }

            return cnode;
        }

        public static List<Matching<T>> getMapping(T bas, T mod, Func<T, string> getLabel, Func<T, IEnumerable<T>> getChildren)
        {
            var t1 = JSyntaxNodeConvert(bas, getLabel, getChildren);
            var t2 = JSyntaxNodeConvert(mod, getLabel, getChildren);

            RTED_InfoTree_Opt rted = new RTED_InfoTree_Opt(1, 1, 1);
            double dist = rted.nonNormalizedTreeDist(t1, t2);
            var t1E = java.util.Collections.list(t1.postorderEnumeration()).toArray().ToList().ConvertAll(x => (util.LblTree)x);
            var t2E = java.util.Collections.list(t2.postorderEnumeration()).toArray().ToList().ConvertAll(x => (util.LblTree)x);

            var mapping = rted.computeEditMapping().toArray().ToList().ConvertAll(x =>
            {
                var m = (int[])x;
                T element1 = m[0] != 0 ? (T)t1E[m[0] - 1].getUserObject() : default(T);
                T element2 = m[1] != 0 ? (T)t2E[m[1] - 1].getUserObject() : default(T);

                return new Matching<T>(element1, element2);
            }
            );

            return mapping;
        }

    }
}
