﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff.Diff3
{
    public class Tests
    {
        private static void HandleConflict(List<String> mergedfile, Chunk<String> chunck)
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
    }

    
    public class Diff3<T>
    {
        public static List<T> Merge(List<T> A, List<T> O, List<T> B, Func<T, T, bool> comparer, Action<List<T>, Chunk<T>> HandleConflict)
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



        private static List<Chunk<T>> getChunks(List<SyntaxDiff.SmartDiff.Diff<T>> totalMatch, Func<T, T, bool> comparer)
        {
            var chuncks = new List<Chunk<T>>();
            Chunk<T> chunk = new Chunk<T>(false);
            bool stableChunk = false;
            chuncks.Add(chunk);

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
            if (!chuncks.Contains(chunk)) // TODO: Improve runtime
                chuncks.Add(chunk);
            return chuncks;
        }


    }
}
