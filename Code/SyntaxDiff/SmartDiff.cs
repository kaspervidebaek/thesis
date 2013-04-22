using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using SyntaxDiff.Diff3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{
    public class SmartDiff
    {
        public class Diff<T>
        {
            public T A;
            public T O;
            public T B;

            public Diff(Tuple<T, T> Item1, Tuple<T, T> Item2)
            {
                A = Item1 == null ? default(T) : Item1.Item1;
                O = Item1 == null ? Item2.Item2 : Item1.Item2;
                B = Item2 == null ? default(T) : Item2.Item1;
            }

            public override string ToString()
            {
                return "A:" + A + " O:" + O + " B:" + B;
            }
        }

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

        private static List<string> LinesFromFunction(SyntaxNode m)
        {
            if (m == null)
                return new List<string>();
            return m.GetText().Lines.Select(x => x.ToString()).ToList();
        }

        private static List<string> SequenceMerge(SyntaxNode A, SyntaxNode O, SyntaxNode B)
        {
            // TODO: What happens with conflicts?
            // TODO: actually test this.
            // Todo: make this into lists of lines.
            Action<List<String>, Chunk<String>> a = (output, chunk) => { output.AddRange(chunk.O.Select(x => "/*Conflict*/" + x)); };
            
            var merged = Diff3.Diff3<String>.Merge(LinesFromFunction(A), LinesFromFunction(O), LinesFromFunction(B), (x, y) => x != null && y != null && x.Trim() == y.Trim(), a);
            return merged.Select(x => x.ToString()).ToList();
        }



        private static List<string> SetMerge(SyntaxNode A, SyntaxNode O, SyntaxNode B)
        {
            if (A is ClassDeclarationSyntax && O is ClassDeclarationSyntax && B is ClassDeclarationSyntax)
            {
                var Ac = A.ChildNodes().Select(x => (MemberDeclarationSyntax)x).ToList();
                var Oc = O.ChildNodes().Select(x => (MemberDeclarationSyntax)x).ToList();
                var Bc = B.ChildNodes().Select(x => (MemberDeclarationSyntax)x).ToList();

                Func<MemberDeclarationSyntax, MemberDeclarationSyntax, int?> cost = (x, y) =>
                {
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

                var newO = O;
                foreach (var m in totalMatch)
                {
                    if (m.A != null && m.B != null && m.O != null)
                    {
                        var member = Merge(m.A, m.O, m.B);
                        var tree = SyntaxTree.ParseText(String.Join("\n", member));
                        var child = (MemberDeclarationSyntax)tree.GetRoot().ChildNodes().First();
                        newO = newO.ReplaceNode(m.O, child);
                    }
                }
                return LinesFromFunction(newO);

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

                var newO = O;
                foreach (var m in totalMatch)
                {
                    if (m.A != null && m.B != null && m.O != null)
                    {
                        var member = Merge(m.A, m.O, m.B);
                        var tree = (MemberDeclarationSyntax)SyntaxTree.ParseText(String.Join("\n", member)).GetRoot().ChildNodes().First();
                        newO = newO.ReplaceNode(m.O, tree);
                    }
                }
                return LinesFromFunction(newO);
            }
            return null;
        }

    }
}
