using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{
    public enum CodeNodeType
    {
        Unordered, Ordered
    }

    public class UnorderedMergeType<T>
    {
        public Type type;
        public Func<T, T, int?> cost;
        public Func<T, List<T>, T> recreate;
    }
    public interface ISmartDiffInterface<T>
    {
        List<UnorderedMergeType<T>> unorderedmerges { get; set; }

        CodeNodeType getChildType(T sn);
        string getLabel(T n);
        T ConvertBack(Tree<Diff<T>> t);

        Tree<T> ConvertToTree(T n);
        List<string> LinesFromSyntax(T m);

        T SyntaxFromLines(List<string> l);
        List<T> Children(T n);
    }
}
