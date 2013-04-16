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
            return m.GetText().Lines.Select(x => x.ToString()).ToList();
        }

        private static List<string> SequenceMerge(SyntaxNode A, SyntaxNode O, SyntaxNode B)
        {
            //TODO: What happens with conflicts?
            // TODO: actually test this.
            // Todo: make this into lists of lines.
            Action<List<String>, Chunk<String>> a = (output, chunk) => { output.AddRange(chunk.O.Select(x => "Conflict: " + x)); };
            
            var merged = Diff3.Diff3<String>.Merge(LinesFromFunction(A), LinesFromFunction(O), LinesFromFunction(B), (x, y) => x != null && y != null && x.Trim() == y.Trim(), a);
            return merged.Select(x => x.ToString()).ToList();
        }

        private static List<string> SetMerge(SyntaxNode A, SyntaxNode O, SyntaxNode B)
        {

            throw new NotImplementedException();
        }
    }
}
