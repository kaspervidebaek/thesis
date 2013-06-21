using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{

    public class SyntaxRewriter<T>
    {
        public delegate string Handler(T A, T O, T B, string children );
        public Type A, O, B;
        public Handler handler;

        public SyntaxRewriter(Type A, Type O, Type B, Handler handler)
        {
            this.A = A;
            this.B = B;
            this.O = O;
            this.handler = handler;

        }
    }

    public enum CodeNodeType
    {
        Unordered, Ordered
    }

    public class UnorderedMergeType<T>
    {
        public delegate string Recreate(T node, string members);

        public Type type;
        public GraphMatching<T, T>.Cost cost;
        public Recreate recreate;
    }
    public interface ISmartDiffInterface<T>
    {
        List<UnorderedMergeType<T>> unorderedmerges { get; set; }
        List<SyntaxRewriter<T>> treerewrites { get; set; }

        CodeNodeType getChildType(T sn);
        string getLabel(T n);
        List<string> LinesFromSyntax(T m);

        string MergeTree(T a, T o, T b);

        T SyntaxFromLines(string l);
        List<T> Children(T n);
    }
}
