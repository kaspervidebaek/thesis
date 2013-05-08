using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyntaxDiff.Diff3;

namespace SyntaxDiff
{
    public class Reorder<T> where T : class
    {
        public static List<T> OrderLists(List<T> A, List<T> O, List<T> B)
        {
            List<Tuple<int, int>> o;
            return OrderLists(A, O, B, out o);
        }
        public static List<T> OrderLists(List<T> A, List<T> O, List<T> B, out List<Tuple<int, int>> oconflicts)
        {


            var ao = NeedlemanWunsch<T>.Allignment(A, O, (x, y) => Object.ReferenceEquals(x, y));
            var bo = NeedlemanWunsch<T>.Allignment(B, O, (x, y) => Object.ReferenceEquals(x, y));
            
            var matches = NeedlemanWunsch<Tuple<T, T>>.Allignment(ao, bo, (x, y) => x.Item2 != null && y.Item2 != null && object.ReferenceEquals(x.Item2, y.Item2))
                .Select(x => new SmartDiff.Diff<T>(x.Item1, x.Item2));

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
                if(match.A == default(T) && match.O == default(T) && match.B != default(T))
                {
                    var oContained = O.Contains(match.B);
                    var aContained = A.Contains(match.B);
                    if ((oContained && aContained) || !oContained) // TODO: Performance
                    {
                        add(match.B);
                    }
                }
                else if (match.A != default(T) && match.O == default(T) && match.B == default(T))
                {

                    var oContained = O.Contains(match.A);
                    var bContained = B.Contains(match.A);
                    if ((oContained && bContained) || !oContained) // TODO: Performance
                    {
                        add(match.A);
                    }
                }
                else if (match.A != default(T) && match.O != default(T) && match.B != default(T))
                {
                    add(match.O);
                }
            }

            oconflicts = conflicts;

            return rv;

        }
    }
}
