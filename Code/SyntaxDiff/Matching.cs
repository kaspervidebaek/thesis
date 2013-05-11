using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{

    public class Matching<T>
    {
        public T bas;
        public T other;
        public Matching(T bas, T other)
        {
            this.bas = bas;
            this.other = other;
        }

        public override string ToString()
        {
            var b = bas == null ? "" : bas.ToString().Trim();
            var o = other == null ? "" : other.ToString().Trim();
            if (bas is SyntaxNode)
            {
                var ba = bas as SyntaxNode;
                var ot = other as SyntaxNode;
                b = ba == null ? "" : ba.GetText().ToString().Trim();
                o = ot == null ? "" : ot.GetText().ToString().Trim();
            }
            return b + "->" + o;
        }
    }

}
