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
            Action<List<String>, Chunk<String>> a = (output, chunk) => { output.AddRange(chunk.O.Select(x => "Conflict: " + x)); };
            
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
                Func<MemberDeclarationSyntax, MemberDeclarationSyntax, bool> comparer = (x, y) => x != null && y != null && x.getMemberDeclerationIdentifier() == y.getMemberDeclerationIdentifier();

                var Ma = NeedlemanWunsch<MemberDeclarationSyntax>.Allignment(Ac, Oc, comparer);
                var Mb = NeedlemanWunsch<MemberDeclarationSyntax>.Allignment(Bc, Oc, comparer);

                var totalMatch = NeedlemanWunsch<Tuple<MemberDeclarationSyntax, MemberDeclarationSyntax>>.Allignment(Ma, Mb, (a, b) => comparer(a.Item2, b.Item2))
                    .Select(x => new {
                        A = x.Item1 == null ? null : x.Item1.Item1,
                        O = x.Item1 == null ? x.Item2.Item2 : x.Item1.Item2,
                        B = x.Item2 == null ? null : x.Item2.Item2
                    }).ToList();

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

            throw new NotImplementedException();
        }
    }
}
