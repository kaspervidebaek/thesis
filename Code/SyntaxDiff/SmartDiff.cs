using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{
    public class SmartDiff<T> where T : class
    {
        public SmartDiff(ISmartDiffInterface<T> i)
        {
            this.i = i;
        }
        private ISmartDiffInterface<T> i;


        public List<String> Merge(List<string> A, List<string> O, List<string> B)
        {
            Func<List<string>, Chunk<string>, bool> conflictHandler = (output, chunk) =>
            {
                var aC = i.SyntaxFromLines(A);
                var oC = i.SyntaxFromLines(O);
                var bC = i.SyntaxFromLines(B);

                output.Clear();
                output.AddRange(MergeTree(aC, oC, bC));

                return true; // We should terminate. This will do the entire merging.
            };

            Func<string, string, bool> equality = (x, y) => x != null && y != null && x.ToString().Trim() == y.ToString().Trim();

            var merged = Diff3<string>.Merge(A, O, B, equality, conflictHandler);
            return merged.Select(x => x.ToString()).ToList();
        }


        public List<String> MergeTree(T A, T O, T B)
        {
            if (!(i.getChildType(A) == i.getChildType(O) && i.getChildType(O) == i.getChildType(B)))
                throw new Exception("This is bad!");

            if (i.getChildType(O) == CodeNodeType.Unordered)
            {
                return SetMerge(A, O, B);
            }
            else if (i.getChildType(O) == CodeNodeType.Ordered)
            {
                return SequenceMerge(A, O, B);
            }

            throw new NotImplementedException();
        }


        private List<string> SequenceMerge(T A, T O, T B)
        {
            // TODO: If there are conflicts - use syntax tree merge.
#if true
            Func<List<string>, Chunk<string>, bool> conflictHandler = (output, chunk) =>
            {
                var aC = i.ConvertToTree(A);
                var oC = i.ConvertToTree(O);
                var bC = i.ConvertToTree(B);

                var matching = Tree<T>.ThreeWayMatch(aC, oC, bC, (x, y) => i.getLabel(x) == i.getLabel(y));

                output.Clear();
                output.AddRange(i.LinesFromSyntax(i.ConvertBack(matching)));

                return true; // We should terminate. This will do the entire merging.
            };
#else
            Func<List<String>, Chunk<String>, bool> conflictHandler = (output, chunk) =>
            {
                output.Add("// Conflict. Base: ---------");
                output.AddRange(chunk.O);
                output.Add("/* A: ----------------------");
                output.AddRange(chunk.A);
                output.Add("-- B: ----------------------");
                output.AddRange(chunk.B);
                output.Add("*/");
                return false;
            };
#endif

            Func<string, string, bool> equality = (x, y) => x != null && y != null && x.ToString().Trim() == y.ToString().Trim();

            var merged = Diff3<string>.Merge(i.LinesFromSyntax(A), i.LinesFromSyntax(O), i.LinesFromSyntax(B), equality, conflictHandler);
            return merged.Select(x => x.ToString()).ToList();
        }

        private List<string> SetMerge(T A, T O, T B)
        {

            foreach (var merge in i.unorderedmerges)
            {
                if (A.GetType() == merge.type && O.GetType() == merge.type && B.GetType() == merge.type)
                {
                    var Ac = i.Children(A);
                    var Oc = i.Children(O);
                    var Bc = i.Children(B);

                    var Ma = GraphMatching<T, T>.Match(Ac, Oc, merge.cost);
                    var Mb = GraphMatching<T, T>.Match(Bc, Oc, merge.cost);

                    var totalMatch = GraphMatching<
                                                Tuple<T, T>,
                                                Tuple<T, T>
                                                >.Match(Ma, Mb,
                                                    (x, y) =>
                                                    {
                                                        if (x.Item2 == y.Item2 && y.Item2 != null)
                                                            return 1;
                                                        return null;
                                                    }).Select(u => new Diff<T>(u.Item1, u.Item2)).ToList();

                    var members = new List<Tuple<Diff<T>, T>>();

                    foreach (var m in totalMatch)
                    {
                        if (m.A != null && m.B != null && m.O != null) // Function exists in all revisions
                        {
                            var tree = i.SyntaxFromLines(MergeTree(m.A, m.O, m.B));
                            members.Add(Tuple.Create(m, tree));
                        }
                        else if (m.A == null && m.O == null && m.B != null) // Function only exists in B - Inserted
                        {
                            members.Add(Tuple.Create(m, m.B));
                        }
                        else if (m.A != null && m.O == null && m.B == null) // Function only exists in A - Inserted
                        {
                            members.Add(Tuple.Create(m, m.A));
                        }
                        else if (m.A == null && m.O != null && m.B == null) // Function only exists in O - deleted in both
                        {
                        }
                        else
                            throw new NotImplementedException();
                    }

                    var newAc = Ac.Select(a => members.First(x => x.Item1.A == a).Item2).ToList(); // TODO: Performance
                    var newOc = Oc.Select(o => members.First(x => x.Item1.O == o).Item2).ToList();
                    var newBc = Bc.Select(b => members.First(x => x.Item1.B == b).Item2).ToList();

                    var reordered = Reorder<T>.OrderLists(newAc, newOc, newBc);

                    // TODO: Merge all other class identifeirs too.

                    return i.LinesFromSyntax(merge.recreate((T)O, reordered));
                }
            }

            throw new NotImplementedException();
        }


    }
}
