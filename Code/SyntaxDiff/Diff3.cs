using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff.Diff3
{
    public class Tests
    {
        private static bool HandleConflict(List<String> mergedfile, Chunk<String> chunck)
        {
            mergedfile.Add(">>> A");
            foreach (var line in chunck.A)
                mergedfile.Add(line);
            mergedfile.Add(">>> O");
            foreach (var line in chunck.O)
                mergedfile.Add(line);
            mergedfile.Add(">>> B");
            foreach (var line in chunck.B)
                mergedfile.Add(line);
            mergedfile.Add("<<<");

            return false;
        }
        public static void Test()
        {
#if true
            var fileA = new List<String>(new string[] { "1", "4", "5", "2", "3", "6" });
            var fileO = new List<String>(new string[] { "1", "2", "3", "4", "5", "6" });
            var fileB = new List<String>(new string[] { "1", "2", "4", "5", "3", "6" });
#else
            var fileA = new List<String>(new string[] { "1", "3", "5" });
            var fileO = new List<String>(new string[] { "1", "2", "5"});
            var fileB = new List<String>(new string[] { "1", "5" });
#endif
            var merge = Diff3<string>.Merge(fileA, fileO, fileB, (a, b) => a == b, HandleConflict);

            foreach (var l in merge)
            {
                Console.WriteLine(l);
            }
        }
    }

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
        public static bool ChunkEqual(List<T> x, List<T> y, Func<T, T, bool> comparer) { //TODO: This can be done in chunck generation
            if(x.Count == y.Count) {
                for (int i = 0; i < x.Count;i++ )
                {
                    if (!comparer(x[i], y[i]))
                        return false;
                }
                return true;
            }
            return false;

        }
    }

    
    public class Diff3<T> where T: class
    {

        public static List<T> Merge(List<T> A, List<T> O, List<T> B, Func<T, T, bool> comparer, Func<List<T>, Chunk<T>, bool> HandleConflict)
        {

            var Ma = NeedlemanWunsch<T>.Allignment(A, O, comparer);
            var Mb = NeedlemanWunsch<T>.Allignment(B, O, comparer);

            var totalMatch = NeedlemanWunsch<Tuple<T, T>>.Allignment(Ma, Mb, (a, b) => comparer(a.Item2, b.Item2)).Select(x => new SyntaxDiff.SmartDiff.Diff<T>(x.Item1, x.Item2)).ToList();
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

        private static List<Chunk<T>> getChunks(List<SyntaxDiff.SmartDiff.Diff<T>> totalMatch, Func<T, T, bool> comparer)
        {
            var chuncks = new List<Chunk<T>>();
            Chunk<T> chunk = new Chunk<T>(false);
            bool stableChunk = false;

            foreach (var m in totalMatch)
            {
                var isStable = comparer(m.A, m.O) && comparer( m.O, m.B);

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
