using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff.Diff3
{
    public class Chunk<T>
    {
        public List<T> A;
        public List<T> O;
        public List<T> B;
        public bool stable;

        public Chunk(bool stable)
        {
            this.stable = stable;
            A = new List<T>();
            O = new List<T>();
            B = new List<T>();
        }
    }

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

        public override string ToString()
        {
            return "A:" + A + " O:" + O + " B:" + B;
        }
    }
    public class Diff3<T>
    {
        public static List<T> Merge(List<T> A, List<T> O, List<T> B, Func<T, T, bool> comparer, Action<List<T>, Chunk<T>> HandleConflict)
        {

            var Ma = NeedlemanWunsch<T>.Allignment(A, O, comparer);
            var Mb = NeedlemanWunsch<T>.Allignment(B, O, comparer);

            var totalMatch = NeedlemanWunsch<Tuple<T, T>>.Allignment(Ma, Mb, (a, b) => comparer(a.Item2, b.Item2)).Select(x => new Diff<T>(x.Item1, x.Item2)).ToList();
            var chuncks = getChunks(totalMatch, comparer);

            var mergedfile = new List<T>();

            foreach (var chunck in chuncks)
            {
                if (chunck.stable)
                {
                    foreach (var line in chunck.O)
                        mergedfile.Add(line);
                    continue;
                }


                if(chunck.O.Count == 0 && chunck.A.Count == 0)
                    foreach (var line in chunck.B)
                        mergedfile.Add(line);
                else if (chunck.O.Count == 0 && chunck.B.Count == 0)
                    foreach (var line in chunck.A)
                        mergedfile.Add(line);
                else { // Conflict
                    HandleConflict(mergedfile, chunck);
                }
            }
            return mergedfile;
        }



        private static List<Chunk<T>> getChunks(List<Diff<T>> totalMatch, Func<T, T, bool> comparer)
        {
            var chuncks = new List<Chunk<T>>();
            {
                Chunk<T> chunk = new Chunk<T>(false);
                var stableChunk = false;

                foreach (var m in totalMatch)
                {
                    var isStable = comparer(m.A, m.O) && comparer( m.O, m.B);

                    if (!stableChunk && isStable || stableChunk && !isStable)
                    {
                        stableChunk = !stableChunk;
                        chunk = new Chunk<T>(stableChunk);
                        chuncks.Add(chunk);
                    }

                    if (m.A != null)
                        chunk.A.Add(m.A);
                    if (m.O != null)
                        chunk.O.Add(m.O);
                    if (m.B != null)
                        chunk.B.Add(m.B);
                }
            }
            return chuncks;
        }


    }
}
