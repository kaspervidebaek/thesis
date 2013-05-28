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
    }
}
