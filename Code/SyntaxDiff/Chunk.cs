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

        public Chunk(bool stable, Diff<T> d) : this(stable)
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


        public static List<Chunk<T>> getChunks(List<Diff<T>> totalMatch, Func<T, T, bool> comparer)
        {
            var chuncks = new List<Chunk<T>>();
            Chunk<T> chunk = new Chunk<T>(false);
            bool stableChunk = false;

            foreach (var m in totalMatch)
            {
                var isStable = comparer(m.A, m.O) && comparer(m.O, m.B);

                if (!stableChunk && isStable || stableChunk && !isStable)
                {
                    stableChunk = !stableChunk;
                    if (!chuncks.Contains(chunk)) // TODO: Improve runtime
                        chuncks.Add(chunk);
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
            if (!chuncks.Contains(chunk)) // TODO: Improve runtime
                chuncks.Add(chunk);



            return chuncks;
        }
    }

    
}
