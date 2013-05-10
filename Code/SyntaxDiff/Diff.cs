using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{
    public class Diff<T>
    {
        public T A;
        public T O;
        public T B;

        public Diff(Tuple<T, T> Item1, Tuple<T, T> Item2)
        {
            A = Item1 == null ? default(T) : Item1.Item1;
            O = Item1 == null ? Item2.Item2 : Item1.Item2;
            B = Item2 == null ? default(T) : Item2.Item1;
        }

        public Diff(Matching<T> Item1, Matching<T> Item2)
        {
            A = Item1 == null ? default(T) : Item1.other;
            O = Item1 == null ? Item2.bas : Item1.bas;
            B = Item2 == null ? default(T) : Item2.other;
        }

        public override string ToString()
        {
            var a = A.Equals(default(T)) ? "-" : A.ToString();
            var o = O.Equals(default(T)) ? "-" : O.ToString();
            var b = B.Equals(default(T)) ? "-" : B.ToString();
            return "A:{" + a + "} O:{" + o + "} B:{" + b + "}";
        }
    }
}
