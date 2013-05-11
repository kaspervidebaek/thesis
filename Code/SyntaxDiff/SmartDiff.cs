using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{
    public class SmartDiff
    {
        public static List<String> Merge(SyntaxNode A, SyntaxNode O, SyntaxNode B)
        {
            if (!(A.getChildType() == O.getChildType() && O.getChildType() == B.getChildType()))
                throw new Exception("This is bad!");

            if (O.getChildType() == CodeTreeType.Set)
            {
                return SetMerge (A, O, B);

            }
            else if (O.getChildType() == CodeTreeType.Sequence)
            {
                return SequenceMerge(A, O, B);
            }

            return null;
        }

        private static SyntaxNode SyntaxNodeMerge(SyntaxNode A, SyntaxNode O, SyntaxNode B)
        {
            throw new NotImplementedException();
        }

        private static List<string> LinesFromSyntax(SyntaxNode m)
        {
            if (m == null)
                return new List<string>();
            return m.GetText().Lines.Select(x => x.ToString()).ToList();
        }

        private static List<string> SequenceMerge(SyntaxNode A, SyntaxNode O, SyntaxNode B)
        {
            // TODO: If there are conflicts - use syntax tree merge.
#if false
            Func<List<string>, Chunk<string>, bool> conflictHandler = (output, chunk) =>
            {
                var mergedTree = TreeMerge.Merge(A, O, B);
                output.Clear();
                output.AddRange(LinesFromSyntax(mergedTree));

                return true;
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

            var merged = Diff3<string>.Merge(LinesFromSyntax(A), LinesFromSyntax(O), LinesFromSyntax(B), equality, conflictHandler);
            return merged.Select(x => x.ToString()).ToList();
        }


        private static List<string> SetMerge(SyntaxNode A, SyntaxNode O, SyntaxNode B)
        {
            if (A is ClassDeclarationSyntax && O is ClassDeclarationSyntax && B is ClassDeclarationSyntax)
            {
                var Or = O as ClassDeclarationSyntax;

                var Ac = A.ChildNodes().Select(x => (MemberDeclarationSyntax)x).ToList();
                var Oc = O.ChildNodes().Select(x => (MemberDeclarationSyntax)x).ToList();
                var Bc = B.ChildNodes().Select(x => (MemberDeclarationSyntax)x).ToList();

                Func<MemberDeclarationSyntax, MemberDeclarationSyntax, int?> cost = (x, y) =>
                { // TODO: Implement heristic to also indicate closeness in body, in parameter list and identifiers.
                    var xI = x.getMemberDeclerationIdentifier();
                    var yI = y.getMemberDeclerationIdentifier();
                    if (xI == yI)
                        return 1;
                    return null;
                };

                var Ma = GraphMatching<MemberDeclarationSyntax, MemberDeclarationSyntax>.Match(Ac, Oc, cost);
                var Mb = GraphMatching<MemberDeclarationSyntax, MemberDeclarationSyntax>.Match(Bc, Oc, cost);

                var totalMatch = GraphMatching<
                                            Tuple<MemberDeclarationSyntax, MemberDeclarationSyntax>,
                                            Tuple<MemberDeclarationSyntax, MemberDeclarationSyntax>
                                            >.Match(Ma, Mb,
                                                (x, y) =>
                                                {
                                                    if (x.Item2 == y.Item2 && y.Item2 != null)
                                                        return 1;
                                                    return null;
                                                }).Select(u => new Diff<MemberDeclarationSyntax>(u.Item1, u.Item2)).ToList() ;

                var members = new List<Tuple< Diff<MemberDeclarationSyntax>, MemberDeclarationSyntax>>();


                foreach (var m in totalMatch)
                {
                    if (m.A != null && m.B != null && m.O != null) // Function exists in all revisions
                    {
                        var functionName = m.O.getMemberDeclerationIdentifier();

                        var merge = Merge(m.A, m.O, m.B);
                        var tree = SyntaxTree.ParseText(String.Join("\n", merge));

                        var child = (MemberDeclarationSyntax)tree.GetRoot().ChildNodes().First();

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

                var reordered = Reorder<MemberDeclarationSyntax>.OrderLists(newAc, newOc, newBc);
                var memberList = new SyntaxList<MemberDeclarationSyntax>().Add(reordered.ToArray());

                // TODO: Merge all other class identifeirs too.
                var rv = Syntax.ClassDeclaration(Or.AttributeLists, Or.Modifiers, Or.Identifier, Or.TypeParameterList, Or.BaseList, Or.ConstraintClauses, memberList);

                return LinesFromSyntax(rv);
            }

            throw new NotImplementedException();
        }

        public static List<string> OrderedMerge(SyntaxNode A, SyntaxNode O, SyntaxNode B)
        {
            if (A is ClassDeclarationSyntax && O is ClassDeclarationSyntax && B is ClassDeclarationSyntax)
            {

                var Ac = A.ChildNodes().Select(x => (MemberDeclarationSyntax)x).ToList();
                var Oc = O.ChildNodes().Select(x => (MemberDeclarationSyntax)x).ToList();
                var Bc = B.ChildNodes().Select(x => (MemberDeclarationSyntax)x).ToList();
                Func<MemberDeclarationSyntax, MemberDeclarationSyntax, bool> comparer = (x, y) => x != null && y != null && x.getMemberDeclerationIdentifier() == y.getMemberDeclerationIdentifier();

                var Ma = NeedlemanWunsch<MemberDeclarationSyntax>.Allignment(Ac, Oc, comparer);
                var Mb = NeedlemanWunsch<MemberDeclarationSyntax>.Allignment(Bc, Oc, comparer);

                var totalMatch = NeedlemanWunsch<Tuple<MemberDeclarationSyntax, MemberDeclarationSyntax>>.Allignment(Ma, Mb, (a, b) => comparer(a.Item2, b.Item2))
                    .Select(x => new Diff<MemberDeclarationSyntax>(x.Item1, x.Item2)).ToList();


                var matchesWithBase = new List<Diff<MemberDeclarationSyntax>>();
                var matchesWithoutBases = new List<Diff<MemberDeclarationSyntax>>();

                var merged = O;
                foreach (var m in totalMatch)
                {
                    if (m.A != null && m.B != null && m.O != null)
                    {
                        var functionName = m.O.getMemberDeclerationIdentifier();

                        var merge = Merge(m.A, m.O, m.B);
                        var tree = SyntaxTree.ParseText(String.Join("\n", merge));
                        var child = (MemberDeclarationSyntax)tree.GetRoot().ChildNodes().First();

                        merged = merged.ReplaceNode(m.O, child);
                    }
                }
                return LinesFromSyntax(merged);
            }
            return null;
        }

    }
}
