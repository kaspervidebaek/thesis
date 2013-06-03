using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{

    public class Diff3<T> where T: class
    {

        public static List<T> Merge(List<T> A, List<T> O, List<T> B, Func<T, T, bool> comparer, Func<List<T>, Chunk<T>, bool> HandleConflict)
        {
            var Ma = NeedlemanWunsch<T>.Allignment(A, O, comparer);
            var Mb = NeedlemanWunsch<T>.Allignment(B, O, comparer);

            var totalMatch = NeedlemanWunsch<Tuple<T, T>>.Allignment(Ma, Mb, (a, b) => comparer(a.Item2, b.Item2)).Select(x => new Diff<T>(x.Item1, x.Item2)).ToList();
            var chuncks = Chunk<T>.getChunks(totalMatch, comparer);

            var mergedfile = new List<T>();

            foreach (var chunck in chuncks)
            {
                if (chunck.stable)
                {
                    foreach (var line in chunck.O)
                        mergedfile.Add(line);
                    continue;
                }


                if(chunck.O.Count == 0 && chunck.A.Count == 0) // Added B
                    foreach (var line in chunck.B)
                        mergedfile.Add(line);
                else if (chunck.O.Count == 0 && chunck.B.Count == 0) // Added A
                    foreach (var line in chunck.A)
                        mergedfile.Add(line);
                else { // This is an update.
                    var AO = Chunk<T>.ChunkEqual(chunck.A, chunck.O, comparer);
                    var BO = Chunk<T>.ChunkEqual(chunck.B, chunck.O, comparer);
                    
                    if (AO && !BO)  // Updated B
                        foreach (var line in chunck.B)
                            mergedfile.Add(line);
                    else if (BO && !AO)  // Updated A
                        foreach (var line in chunck.A)
                            mergedfile.Add(line);
                    else
                    {
                        if (Chunk<T>.ChunkEqual(chunck.A, chunck.B, comparer)) // Both branches added the exact same thing. Just add one of them.
                        {
                            foreach (var line in chunck.A)
                                mergedfile.Add(line);
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

    }
}
