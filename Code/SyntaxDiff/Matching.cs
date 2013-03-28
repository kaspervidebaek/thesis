using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{

    public class Matching
    {
        public SyntaxNode bas;
        public SyntaxNode other;
        public Matching(SyntaxNode bas, SyntaxNode other)
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
    }

}
