using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
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

        public Chunk(bool stable, Diff<T> d)
            : this(stable)
        {
            A.Add(d.A);
            O.Add(d.O);
            B.Add(d.B);
        }

        public static bool ChunkEqual(List<T> x, List<T> y, Func<T, T, bool> comparer)
        { //TODO: This can be done in chunck generation
            if (x.Count == y.Count)
            {
                for (int i = 0; i < x.Count; i++)
                {
                    if (!comparer(x[i], y[i]))
                        return false;
                }
                return true;
            }
            return false;

        }

        public static List<Chunk<T>> getChunks(List<Diff<T>> match, Func<T, T, bool> comparer)
        {
            var cs = match.ChunkBy(m => comparer(m.A, m.O) && comparer(m.O, m.B));

            var chuncks = new List<Chunk<T>>();

            foreach(var c in cs) {
                Chunk<T> chunk = new Chunk<T>(c.Key);

                foreach(var m in c) {
                    if (m.A != null)
                        chunk.A.Add(m.A);
                    if (m.O != null)
                        chunk.O.Add(m.O);
                    if (m.B != null)
                        chunk.B.Add(m.B);
                }

                chuncks.Add(chunk);
            }

            return chuncks;
        }

    }


}
