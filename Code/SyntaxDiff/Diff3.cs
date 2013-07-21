using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{
    public enum ChunkEqualType { NotEqual, PrimaryEqual, SecondaryEqual };
    public class PriorityChunk<T>
    {
        public ChunkEqualType equalType;
        public Chunk<T> chunk;
    }

    public class Diff3<T> where T : class
    {
        public static List<T> Merge(List<T> A, List<T> O, List<T> B, Func<T, T, bool> comparer, Func<List<T>, Chunk<T>, bool> HandleConflict)
        {
            return Merge<T>(A, O, B, comparer, HandleConflict, x => x);
        }

        public static List<Y> Merge<Y>(List<T> A, List<T> O, List<T> B, Func<T, T, bool> comparer, Func<List<Y>, Chunk<T>, bool> HandleConflict, Func<T, Y> converter)
        {
            var chuncks = ChunksFromLists(A, O, B, comparer);

            var mergedfile = new List<Y>();

            foreach (var chunck in chuncks)
            {
                if (chunck.stable)
                {
                    foreach (var line in chunck.O)
                        mergedfile.Add(converter(line));
                    continue;
                }


                if(chunck.O.Count == 0 && chunck.A.Count == 0) // Added B
                    foreach (var line in chunck.B)
                        mergedfile.Add(converter(line));
                else if (chunck.O.Count == 0 && chunck.B.Count == 0) // Added A
                    foreach (var line in chunck.A)
                        mergedfile.Add(converter(line));
                else { // This is an update.
                    var AO = Chunk<T>.ChunkEqual(chunck.A, chunck.O, comparer);
                    var BO = Chunk<T>.ChunkEqual(chunck.B, chunck.O, comparer);
                    
                    if (AO && !BO)  // Updated B
                        foreach (var line in chunck.B)
                            mergedfile.Add(converter(line));
                    else if (BO && !AO)  // Updated A
                        foreach (var line in chunck.A)
                            mergedfile.Add(converter(line));
                    else
                    {
                        if (Chunk<T>.ChunkEqual(chunck.A, chunck.B, comparer)) // Both branches added the exact same thing. Just add one of them.
                        {
                            foreach (var line in chunck.A)
                                mergedfile.Add(converter(line));
                        }
                        else
                        {
                            // Conflict
                            if(HandleConflict(mergedfile, chunck))
                                break;
                        }
                    }
                }
            }
            return mergedfile;
        }

        public static List<Chunk<T>> ChunksFromLists(List<T> A, List<T> O, List<T> B, Func<T, T, bool> comparer)
        {
            var totalMatch = ThreeWayDiff(A, O, B, comparer);
            var chuncks = Chunk<T>.getChunks(totalMatch, comparer);
            return chuncks;
        }

        public static List<Diff<T>> ThreeWayDiff(List<T> A, List<T> O, List<T> B, Func<T, T, bool> comparer)
        {
            var Ma = NeedlemanWunsch<T>.Allignment(A, O, comparer);
            var Mb = NeedlemanWunsch<T>.Allignment(B, O, comparer);

            var totalMatch = NeedlemanWunsch<Tuple<T, T>>.Allignment(Ma, Mb, (a, b) => a.Item2 != null && b.Item2!= null && Object.ReferenceEquals(a.Item2, b.Item2)).Select(x => new Diff<T>(x.Item1, x.Item2)).ToList();
            return totalMatch;
        }



        public static List<PriorityChunk<T>> ThreeWayDiffPriority(List<T> A, List<T> O, List<T> B, Func<T, T, bool> comparer, Func<T, T, bool> comparer2)
        {
            var outChunks = new List<PriorityChunk<T>>();

            var matches = ThreeWayDiff(A, O, B, comparer);
            var chunks = Chunk<T>.getChunks(matches, comparer);

            foreach (var chunk in chunks)
            {
                if (chunk.A.Count == 0 && chunk.B.Count == 0 && chunk.O.Count == 0)
                    continue;

                if (chunk.stable)
                    outChunks.Add(new PriorityChunk<T>() { chunk = chunk, equalType = ChunkEqualType.PrimaryEqual });

                else
                {
                    var innermatches = ThreeWayDiff(chunk.A, chunk.O, chunk.B, comparer2);
                    var innerchunks = Chunk<T>.getChunks(innermatches, comparer2);

                    foreach (var innerchunk in innerchunks)
                    {
                        if (chunk.A.Count == 0 && chunk.B.Count == 0 && chunk.O.Count == 0)
                            continue;

                        if (chunk.stable)
                            outChunks.Add(new PriorityChunk<T>() { chunk = chunk, equalType = ChunkEqualType.SecondaryEqual });
                        else
                            outChunks.Add(new PriorityChunk<T>() { chunk = chunk, equalType = ChunkEqualType.NotEqual });

                    }



                }

            }
            

            return outChunks;
        }
    }
}
