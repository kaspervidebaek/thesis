using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SyntaxDiff;

namespace Tests
{
    /// <summary>
    /// Summary description for DiffTests
    /// </summary>
    [TestClass]
    public class DiffTests
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
}
