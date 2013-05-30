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

        public List<String> Merge(T A, T O, T B)
        {
            if (!(i.getChildType(A) == i.getChildType(O) && i.getChildType(O) == i.getChildType(B)))
                throw new Exception("This is bad!");

            if (i.getChildType(O) == CodeTreeType.Set)
            {
                return SetMerge(A, O, B);
            }
            else if (i.getChildType(O) == CodeTreeType.Sequence)
            {
                return SequenceMerge(A, O, B);
            }

            return null;
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

        private List<string> SimilarityMerge<PT, CT>(PT A, PT O, PT B, Func<CT, CT, int?> cost, Func<PT, List<CT>, PT> recreate)
            where PT : class, T
            where CT : class, T
        {
            var Ac = i.Children(A).Select(x => (CT)x).ToList();
            var Oc = i.Children(O).Select(x => (CT)x).ToList();
            var Bc = i.Children(B).Select(x => (CT)x).ToList();

            var Ma = GraphMatching<CT, CT>.Match(Ac, Oc, cost);
            var Mb = GraphMatching<CT, CT>.Match(Bc, Oc, cost);

            var totalMatch = GraphMatching<
                                        Tuple<CT, CT>,
                                        Tuple<CT, CT>
                                        >.Match(Ma, Mb,
                                            (x, y) =>
                                            {
                                                if (x.Item2 == y.Item2 && y.Item2 != null)
                                                    return 1;
                                                return null;
                                            }).Select(u => new Diff<CT>(u.Item1, u.Item2)).ToList();

            var members = new List<Tuple<Diff<CT>, CT>>();


            foreach (var m in totalMatch)
            {
                if (m.A != null && m.B != null && m.O != null) // Function exists in all revisions
                {
                    var merge = Merge(m.A, m.O, m.B);
                    var tree = i.SyntaxFromLines(merge);

                    var child = (CT)tree;

                    members.Add(Tuple.Create(m, child));
                }
                else if (m.A == null && m.O == null && m.B != null) // Function only exists in B - Inserted
                {
                    members.Add(Tuple.Create(m, m.B));
                }
                else if (m.A != null && m.O == null && m.B == null) // Function only exists in A - Inserted
                {
                    members.Add(Tuple.Create(m, m.A));
                } 
                else
                    throw new NotImplementedException();
            }

            var newAc = Ac.Select(a => members.First(x => x.Item1.A == a).Item2).ToList(); // TODO: Performance
            var newOc = Oc.Select(o => members.First(x => x.Item1.O == o).Item2).ToList();
            var newBc = Bc.Select(b => members.First(x => x.Item1.B == b).Item2).ToList();

            var reordered = Reorder<CT>.OrderLists(newAc, newOc, newBc);

            // TODO: Merge all other class identifeirs too.

            return i.LinesFromSyntax(recreate((PT)O, reordered));
        }


        private List<string> SetMerge(T A, T O, T B)
        {
            if (A is ClassDeclarationSyntax && O is ClassDeclarationSyntax && B is ClassDeclarationSyntax)
            {
                return SimilarityMerge<T, T>(A, O, B, i.MemberCost, i.CreateClass);
            }
            else if (A is CompilationUnitSyntax && O is CompilationUnitSyntax && B is CompilationUnitSyntax)
            {
                return SimilarityMerge<T, T>(A, O, B, i.MemberCost, i.CreateCompilationUnitSyntax);
            }


            throw new NotImplementedException();
        }


    }
}
