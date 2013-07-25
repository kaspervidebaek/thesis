using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{
    public class PriorityChunk2<T, U>
    {
        public Chunk2<T, U> chunk;
        public ChunkEqualType equal;
    }
    /*public class Chunk2<T> : Chunk2<T, T>
    {
    }*/
    public class Chunk2<T> : Chunk2<T, T>
    {
    }
    public class Chunk2<T, U>
    {
        public List<T> x = new List<T>();
        public List<U> y = new List<U>();
        public bool stable;

        public static List<Chunk2<T, U>> getChunks(List<Tuple<T, U>> match, Func<T, U, bool> comparer)
        {
            var cs = match.ChunkBy(m => comparer(m.Item1, m.Item2));

            var chuncks = new List<Chunk2<T, U>>();

            foreach (var c in cs)
            {
                Chunk2<T, U> chunk = new Chunk2<T, U> { stable = c.Key };

                foreach (var m in c)
                {
                    if (m.Item1 != null)
                        chunk.x.Add(m.Item1);
                    if (m.Item2 != null)
                        chunk.y.Add(m.Item2);
                }

                chuncks.Add(chunk);
            }

            return chuncks;
        }

        public static List<PriorityChunk2<T, U>> TwoWayDiffPriority(List<T> x, List<U> y, Func<T, U, bool> comparer, Func<T, U, bool> comparer2)
        {
            var matching = NeedlemanWunsch<T, U>.Allignment(x, y, comparer);
            var chunks = Chunk2<T, U>.getChunks(matching, comparer);

            var finalChunks = new List<PriorityChunk2<T, U>>();

            foreach (var chunk in chunks)
            {
                if (chunk.stable) // Stable Chunk
                {
                    finalChunks.Add(new PriorityChunk2<T, U> { equal = ChunkEqualType.PrimaryEqual, chunk = chunk });
                }
                else
                {
                    var innermatching = NeedlemanWunsch<T, U>.Allignment(chunk.x, chunk.y, comparer2);
                    var innerchunks = Chunk2<T, U>.getChunks(innermatching, comparer2);

                    foreach (var innerchunk in innerchunks)
                    {
                        if (innerchunk.stable)
                        {
                            finalChunks.Add(new PriorityChunk2<T, U> { equal = ChunkEqualType.SecondaryEqual, chunk = chunk });

                        }
                        else
                        {
                            finalChunks.Add(new PriorityChunk2<T, U> { equal = ChunkEqualType.NotEqual, chunk = chunk });
                        }

                    }
                }
            }
            return finalChunks;
        }
    }
}
