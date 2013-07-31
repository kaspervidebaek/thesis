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


        public string MergeCodeLines(string A, string O, string B)
        {
            Func<List<string>, Chunk<string>, bool> conflictHandler = (output, chunk) =>
            {
                var aC = i.SyntaxFromLines(A);
                var oC = i.SyntaxFromLines(O);
                var bC = i.SyntaxFromLines(B);

                output.Clear();
                output.AddRange(Merge(aC, oC, bC).Split('\n'));

                return true; // We should terminate. This will do the entire merging.
            };

            Func<string, string, bool> equality = (x, y) => x != null && y != null && x.ToString().Trim() == y.ToString().Trim();

            var merged = Diff3<string>.Merge(A.Split('\n').ToList(), O.Split('\n').ToList(), B.Split('\n').ToList(), equality, conflictHandler);
            return string.Join("\n", merged);
        }


        public string Merge(T A, T O, T B)
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

        private string SequenceMerge(T A, T O, T B)
        {
            // TODO: If there are conflicts - use syntax tree merge.
#if true
            Func<List<string>, Chunk<string>, bool> conflictHandler = (output, chunk) =>
            {
                /*var aC = i.ConvertToTree(A);
                var oC = i.ConvertToTree(O);
                var bC = i.ConvertToTree(B);

                var matching = Tree<T>.ThreeWayMatch(aC, oC, bC, (x, y) => i.getLabel(x) == i.getLabel(y));

                output.Clear();
                output.AddRange(i.LinesFromSyntax(i.ConvertBack(matching)));*/

                //var mergedTree = i.MergeTree(A, O, B);
                output.Clear();
                output.Add(i.MergeNode(A, O, B));


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
            return string.Join("\n", merged);
        }


        private string SetMerge(T A, T O, T B)
        {

            foreach (var merge in i.unorderedmerges)
            {
                if (A.GetType() == merge.type && O.GetType() == merge.type && B.GetType() == merge.type)
                {
                    var Ac = i.Children(A);
                    var Oc = i.Children(O);
                    var Bc = i.Children(B);
                    var totalMatch = GetThreeWayUnorderedMatch(Ac, Oc, Bc, merge.cost);
                    List<Tuple<int, int>> conflicts;
                    var reordered = FilterAndMerge(Ac, Oc, Bc, totalMatch, Merge, out conflicts);

                    // TODO: Merge all other class identifeirs too.

                    var str = merge.recreate((T)O, string.Join("\n", reordered));
                    return str;
                }
            }

            throw new NotImplementedException();
        }

        public static List<string> FilterAndMerge(List<T> Ac, List<T> Oc, List<T> Bc, List<Diff<T>> totalMatch, Func<T, T, T, string> merge, out List<Tuple<int, int>> oconflicts)
        {

            var members = new List<Tuple<Diff<T>, string>>();

            var conflicts = new HashSet<object>();

            foreach (var m in totalMatch)
            {
                if (m.A != null && m.B != null && m.O != null) // Function exists in all revisions
                {
                    var tree = merge(m.A, m.O, m.B);
                    members.Add(Tuple.Create(m, tree));
                }
                else if (m.A == null && m.O == null && m.B != null) // Function only exists in B - Inserted
                {
                    members.Add(Tuple.Create(m, m.B.ToString()));
                }
                else if (m.A != null && m.O == null && m.B == null) // Function only exists in A - Inserted
                {
                    members.Add(Tuple.Create(m, m.A.ToString()));
                }
                else if (m.A == null && m.O != null && m.B == null) // Function only exists in O - deleted in both
                {
                }
                else if (m.A != null && m.O != null && m.B == null) // Deleted in B
                {
                    if (m.A.ToString() != m.O.ToString())
                    {
                        var s = m.A + " /*deleted in B*/";
                        members.Add(Tuple.Create(m, s));
                        conflicts.Add(s);
                    }
                }
                else if (m.A == null && m.O != null && m.B != null) // Deleted in A
                {
                    if (m.B.ToString() != m.O.ToString()) {
                        var s =  m.B + " /*deleted in A*/";
                        members.Add(Tuple.Create(m, s));
                        conflicts.Add(s);
                    }
                }
                else
                    throw new NotImplementedException();
            }

            var newAc = Ac.Select(a => members.SingleOrDefault(x => x.Item1.A == a)).Where(x => x != null).Select(x => x.Item2).ToList(); // TODO: Performance
            var newOc = Oc.Select(o => members.SingleOrDefault(x => x.Item1.O == o)).Where(x => x != null).Select(x => x.Item2).ToList();
            var newBc = Bc.Select(b => members.SingleOrDefault(x => x.Item1.B == b)).Where(x => x != null).Select(x => x.Item2).ToList();
            
            var reordered = Reorder<string>.OrderLists(newAc, newOc, newBc, out oconflicts, conflicts);
            return reordered;
        }

        public static List<Diff<T>> GetThreeWayUnorderedMatch(List<T> Ac, List<T> Oc, List<T> Bc, GraphMatching<T, T>.Cost cost)
        {
            var Ma = GraphMatching<T, T>.Match(Ac, Oc, cost);
            var Mb = GraphMatching<T, T>.Match(Bc, Oc, cost);

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
            return totalMatch;
        }


    }
}
