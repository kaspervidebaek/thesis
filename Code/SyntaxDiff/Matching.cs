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

        public bool isDeletion()
        {
            if (other == null)
                return true;
            return false;

        }
        public bool isInsertion()
        {
            if (bas == null)
                return true;
            return false;
        }

        public override string ToString()
        {
            if (bas is SyntaxNode)
            {
                var ba = bas as SyntaxNode;
                var ot = other as SyntaxNode;
                var b = ba == null ? "" : ba.GetText().ToString().Trim();
                var o = ot == null ? "" : ot.GetText().ToString().Trim();

                return b + "->" + o;
            }
            return base.ToString();
        }
    }

}
