using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{
    public class Reorder<T> where T : class
    {
        public static List<T> OrderLists(List<T> A, List<T> O, List<T> B, HashSet<object> conflicts)
        {
            List<Tuple<int, int>> o;
            return OrderLists(A, O, B, out o, conflicts);
        }
        public static List<T> OrderLists(List<T> A, List<T> O, List<T> B, out List<Tuple<int, int>> oconflicts, HashSet<object> outerConflicts)
        {
            var matches = Diff3<T>.ThreeWayDiff(A, O, B, (x, y) => Object.ReferenceEquals(x, y));

            var alreadyAdded = new Dictionary<T, int>();

            var rv = new List<T>();
            var conflicts = new List<Tuple<int, int>>();

            Action<T> add = (item) =>
            {
                int i1;

                if (alreadyAdded.TryGetValue(item, out i1))
                {
                    conflicts.Add(Tuple.Create(i1, rv.Count));
                }
                else
                    alreadyAdded.Add(item, rv.Count);
                rv.Add(item);
            }; 
            
            foreach (var match in matches)
            {
                if(match.A == default(T) && match.O == default(T) && match.B != default(T)) // Insertion from B
                {
                    var oContained = O.Contains(match.B);
                    var aContained = A.Contains(match.B);
                    if ((oContained && aContained) || !oContained) // TODO: Performance
                    {
                        add(match.B);
                    }
                    else
                    {

                    }
                }
                else if (match.A != default(T) && match.O == default(T) && match.B == default(T)) // Insertion from A
                {
                    var oContained = O.Contains(match.A);
                    var bContained = B.Contains(match.A);
                    if ((oContained && bContained) || !oContained) // TODO: Performance
                        // !oContained because that means this is a plain insertion.
                        // (oContained && bContained) because that means it was not deleted in the A-sequence, and therefore actually should be inserted.
                    {
                        add(match.A);
                    }
                    else
                    {

                    }
                }
                else if (match.A != default(T) && match.O != default(T) && match.B != default(T))  // Exists in all.
                {
                    add(match.O);
                }
                else if(outerConflicts.Contains(match.O))
                    add(match.O);
            }

            oconflicts = conflicts;

            return rv;

        }
    }
}
