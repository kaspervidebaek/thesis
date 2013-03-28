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
    class JavaMatching
    {
        static int treeId = 0;
        private static LblTree JSyntaxNodeConvert(SyntaxNode node)
        {
            //var cnode = new Diffing.Tree(NodetToLabel(node), node, ListModule.OfSeq(childNodes));
            var cnode = new LblTree(node.getLabel(), treeId++);
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

        public static List<Matching> GetDiff(SyntaxTree bas, SyntaxTree mod)
        {
            var t1 = JSyntaxTreeConvert(bas);
            var t2 = JSyntaxTreeConvert(mod);

            RTED_InfoTree_Opt rted = new RTED_InfoTree_Opt(1, 1, 1);
            double dist = rted.nonNormalizedTreeDist(t1, t2);
            var t1E = java.util.Collections.list(t1.postorderEnumeration()).toArray().ToList().ConvertAll(x => (util.LblTree)x);
            var t2E = java.util.Collections.list(t2.postorderEnumeration()).toArray().ToList().ConvertAll(x => (util.LblTree)x);

            var mapping = rted.computeEditMapping().toArray().ToList().ConvertAll(x =>
            {
                var m = (int[])x;
                SyntaxNode element1 = m[0] != 0 ? (SyntaxNode)t1E[m[0] - 1].getUserObject() : null;
                SyntaxNode element2 = m[1] != 0 ? (SyntaxNode)t2E[m[1] - 1].getUserObject() : null;

                return new Matching(element1, element2);
            }
            );

            return mapping;
        }

    }
}
