using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{
    public static class SmartDiffSyntaxTreeExtensions
    {
        public static CodeTreeType getChildType(this SyntaxNode sn)
        {
            if (sn is CompilationUnitSyntax)
                return CodeTreeType.Set;
            else if (sn is NamespaceDeclarationSyntax)
                return CodeTreeType.Set;
            else if (sn is ClassDeclarationSyntax)
                return CodeTreeType.Set;
            return CodeTreeType.Sequence;
        }
    }

    public class SmartDiff
    {



        public static List<String> Merge(SyntaxNode A, SyntaxNode O, SyntaxNode B)
        {
            if (!(A.getChildType() == O.getChildType() && O.getChildType() == B.getChildType()))
                throw new Exception("This is bad!");



            if (O.getChildType() == CodeTreeType.Set)
            {
                return SetMerge(A, O, B);
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
#if true
            Func<List<string>, Chunk<string>, bool> conflictHandler = (output, chunk) =>
            {
                var aC = A.ConvertToTree();
                var oC = O.ConvertToTree();
                var bC = B.ConvertToTree();

                var mergedTree = Tree<SyntaxNode>.Merge(aC, oC, bC, (x, y) => x.getLabel() == y.getLabel());

                output.Clear();
                output.AddRange(LinesFromSyntax(TreeToSyntax.convertNode(mergedTree)));

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

        private static List<string> SimilarityMerge<T, CT>(T A, T O, T B, Func<CT, CT, int?> cost, Func<List<CT>, SyntaxNode> recreate)
            where T : SyntaxNode
            where CT : SyntaxNode
        {
            var Ac = A.ChildNodes().Select(x => (CT)x).ToList();
            var Oc = O.ChildNodes().Select(x => (CT)x).ToList();
            var Bc = B.ChildNodes().Select(x => (CT)x).ToList();

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
                    var tree = SyntaxTree.ParseText(String.Join("\n", merge));

                    var child = (CT)tree.GetRoot().ChildNodes().First();

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

            return LinesFromSyntax(recreate(reordered));
        }


        private static List<string> SetMerge(SyntaxNode A, SyntaxNode O, SyntaxNode B)
        {
            if (A is ClassDeclarationSyntax && O is ClassDeclarationSyntax && B is ClassDeclarationSyntax)
            {
                var Or = O as ClassDeclarationSyntax;

                Func<MemberDeclarationSyntax, MemberDeclarationSyntax, int?> cost = (x, y) =>
                { // TODO: Implement heristic to also indicate closeness in body, in parameter list and identifiers.
                    var xI = x.getMemberDeclerationIdentifier();
                    var yI = y.getMemberDeclerationIdentifier();
                    if (xI == yI)
                        return 1;
                    return null;
                };
                Func<List<MemberDeclarationSyntax>, SyntaxNode> recreate = (newmembers) =>
                {
                    var memberList = new SyntaxList<MemberDeclarationSyntax>().Add(newmembers.ToArray());
                    return Syntax.ClassDeclaration(Or.AttributeLists, Or.Modifiers, Or.Identifier, Or.TypeParameterList, Or.BaseList, Or.ConstraintClauses, memberList);
                };

                return SimilarityMerge<ClassDeclarationSyntax, MemberDeclarationSyntax>((ClassDeclarationSyntax)A, (ClassDeclarationSyntax)O, (ClassDeclarationSyntax)B, cost, recreate);
            }
            else if (A is CompilationUnitSyntax && O is CompilationUnitSyntax && B is CompilationUnitSyntax)
            {
                var Or = O as CompilationUnitSyntax;

                Func<MemberDeclarationSyntax, MemberDeclarationSyntax, int?> cost = (x, y) =>
                { // TODO: Implement heristic to also indicate closeness in body, in parameter list and identifiers.
                    var xI = x.getMemberDeclerationIdentifier();
                    var yI = y.getMemberDeclerationIdentifier();
                    if (xI == yI)
                        return 1;
                    return null;
                };
                Func<List<MemberDeclarationSyntax>, SyntaxNode> recreate = (newmembers) =>
                {
                    var memberList = new SyntaxList<MemberDeclarationSyntax>().Add(newmembers.ToArray());
                    return Syntax.CompilationUnit(Or.Externs, Or.Usings, Or.AttributeLists, memberList);
                };

                return SimilarityMerge<CompilationUnitSyntax, MemberDeclarationSyntax>((CompilationUnitSyntax)A, (CompilationUnitSyntax)O, (CompilationUnitSyntax)B, cost, recreate);
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
            throw new Exception("Not implemented!");
        }

    }
}
