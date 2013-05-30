using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{
    public interface ISmartDiffInterface<T>
    {
        CodeTreeType getChildType(T sn);
        string getLabel(T n);
        T ConvertBack(Tree<Diff<T>> t);

        Tree<T> ConvertToTree(T n);
        List<string> LinesFromSyntax(T m);

        T SyntaxFromLines(List<string> l);
        List<T> Children(T n);

        T CreateClass(T n, List<T> l);
        T CreateCompilationUnitSyntax(T n, List<T> l);
        int? MemberCost(T n, T n2);
    }
}
