using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyntaxDiff
{
    public class SyntaxNodeSmartDiff : ISmartDiffInterface<SyntaxNode>
    {
        public SyntaxNodeSmartDiff()
        {
            unorderedmerges = new List<UnorderedMergeType<SyntaxNode>> {
                new UnorderedMergeType<SyntaxNode> { type = typeof(ClassDeclarationSyntax), recreate = this.CreateClass, cost = this.ClassMemberCost },
                new UnorderedMergeType<SyntaxNode> { type = typeof(NamespaceDeclarationSyntax), recreate = this.CreateNamespace, cost = this.NamespaceMemberCost },
                new UnorderedMergeType<SyntaxNode> { type = typeof(CompilationUnitSyntax), recreate = this.CreateCLU, cost = this.NamespaceMemberCost },
            };

            var CompilationUnit = typeof(CompilationUnitSyntax);
            var NamespaceDeclaration = typeof(NamespaceDeclarationSyntax);


            treerewrites = new List<SyntaxRewriter<SyntaxNode>> {
                new SyntaxRewriter<SyntaxNode>(CompilationUnit, CompilationUnit, CompilationUnit,                   (a, o, b, children) => children),
                new SyntaxRewriter<SyntaxNode>(NamespaceDeclaration, NamespaceDeclaration, NamespaceDeclaration,    (a, o, b, children) =>  children),
                new SyntaxRewriter<SyntaxNode>(NamespaceDeclaration, NamespaceDeclaration, NamespaceDeclaration,    (a, o, b, children) => children)
            };
        }


        public List<UnorderedMergeType<SyntaxNode>> unorderedmerges { get; set; }
        public List<SyntaxRewriter<SyntaxNode>> treerewrites { get; set; }

        public string Cast<A, O, B>(SyntaxNode a, SyntaxNode o, SyntaxNode b, string child, Func<A, O, B, string, string> f)
            where A : SyntaxNode
            where O : SyntaxNode
            where B : SyntaxNode
        {
            return f((A)a, (O)o, (B)b, child);
        }



        private string StringMerge(string A, string O, string B)
        {
            if (A == O && O != B)
            {
                return B;
            }
            else if (A != O && O == B)
            {
                return A;
            }
            else if (A == O && O == B)
            {
                return O;
            }
            else if (A == B)
                return A;

            return A + "<-" + O + "->" + B;
            throw new NotImplementedException();
        }

        private string MergeToken(Triplet<SyntaxToken> n)
        {
            return StringMerge(n.A.ValueText, n.O.ValueText, n.B.ValueText);
        }

        private string MergeType(Triplet<TypeSyntax> n)
        {
            return StringMerge(n.A.ToString(), n.O.ToString(), n.B.ToString());
        }


        [DebuggerNonUserCode]
        public string MergeNode(SyntaxNode A, SyntaxNode O, SyntaxNode B)
        {
            return MergeNode(Triplet<SyntaxNode>.Create(A, O, B));
        }
        public string MergeNode(Triplet<SyntaxNode> nodes)
        {
            if (nodes.O != null && nodes.A != null && nodes.B == null)
            {
                if (Equal(nodes.A, nodes.O))
                {
                    return "";
                }
                return "/*Conflict: Deleted in B*/\r\n" + nodes.A;
            }
            else if (nodes.O != null && nodes.A == null && nodes.B != null)
            {
                if (Equal(nodes.B, nodes.O))
                {
                    return "";
                }
                return "/*Conflict: Deleted in A*/\r\n" + nodes.B;
            }
            if (nodes.O == null && nodes.A != null && nodes.B == null)
            {
                return nodes.A.ToString();
            }
            else if (nodes.O == null && nodes.A == null && nodes.B != null)
            {
                return nodes.B.ToString();
            }
            else if (nodes.Is<CompilationUnitSyntax>())
            {
                var c = nodes.Cast<CompilationUnitSyntax>().Select(x => x.Members);
                if (c.Select(x => x.Count).Apply((x, y, z) => x != 0 && y != 0 && z != 0))
                {
                    var M = c.Select(x => x.First());
                    return M.Apply(MergeNode);
                }
            }
            else if (nodes.Is<MethodDeclarationSyntax>())
            {
                var M = nodes.Cast<MethodDeclarationSyntax>();

                var name = M.Select(x => x.Identifier).Apply(MergeToken);
                var parm = M.Select(x => x.ParameterList).Apply(MergeNode);
                var body = M.Select(x => x.Body).Apply(MergeNode);
                var retv = M.Select(x => x.ReturnType).Apply(MergeType);

                return retv + " " + name + "(" + parm + ") \r\n" + body + "\r\n";
            }
            else if (nodes.Is<IfStatementSyntax>())
            {
                var i = nodes.Cast<IfStatementSyntax>();

                var expr = i.Select(x => x.Condition).Apply(MergeNode);
                var body = i.Select(x => x.Statement).Apply(MergeNode);
                var elses = i.Select(x => x.Else).ApplyIfExists(MergeNode);


                return "if( " + expr.ToString() + ")\r\n" + body + (elses != null ? " else " + elses : "");
            }
            else if (nodes.Is<ExpressionStatementSyntax>())
            {
                var e = nodes.Cast<ExpressionStatementSyntax>();
                return e.Select(x => x.Expression).Apply(MergeNode) + ";";
            }
            else if (nodes.Is<InvocationExpressionSyntax>())
            {
                var i = nodes.Cast<InvocationExpressionSyntax>();

                var arguments = i.Select(x => x.ArgumentList).Apply(MergeNode);
                var expression = i.Select(x => x.Expression).Apply(MergeNode);

                return expression + "(" + arguments + ")";
            }
            else if (nodes.Is<MemberAccessExpressionSyntax>())
            {
                var i = nodes.Cast<MemberAccessExpressionSyntax>();

                var expression = i.Select(x => x.Expression).Apply(MergeNode);
                var name = i.Select(x => x.Name).Apply(MergeNode);

                return expression + "." + name;
            }

            else if (nodes.Is<LiteralExpressionSyntax>())
            {
                var i = nodes.Cast<LiteralExpressionSyntax>().Select(x => x.Token).Apply(MergeToken);

                return i;
            }
            else if (nodes.Is<IdentifierNameSyntax>())
            {
                var i = nodes.Cast<IdentifierNameSyntax>().Select(x => x.Identifier).Apply(MergeToken);

                return i;
            }
            else if (nodes.Is<ArgumentSyntax>())
            {
                var i = nodes.Cast<ArgumentSyntax>().Select(x => x.Expression).Apply(MergeNode);

                return i;
            }
            else if (nodes.Is<ParameterListSyntax>())
            {
                return ListMerger<ParameterListSyntax, ParameterSyntax>(nodes, x => x.Parameters, ClassMemberCost);
            }
            else if (nodes.Is<ArgumentListSyntax>())
            {
                return ListMerger<ArgumentListSyntax, ArgumentSyntax>(nodes, x => x.Arguments, ExpressionCost);
            }
            else if (nodes.Is<BlockSyntax>())
            {
                var b = nodes.Cast<BlockSyntax>();

                var statements = b.Select(x => x.Statements.ToList());

                return "{\r\n" + MergeStatementList(statements.A, statements.O, statements.B) + "\r\n}\r\n";
            }
            else if (nodes.Is<ExpressionStatementSyntax, WhileStatementSyntax>())
            {
                var n = nodes.Cast<ExpressionStatementSyntax, WhileStatementSyntax>();

                var merge = MergeNode(n.other.Statement, n.bas, n.even);

                return "while(" + n.other.Condition + ")\r\n" + merge;

            }
            else if (nodes.Is<ExpressionStatementSyntax, IfStatementSyntax>())
            {
                var n = nodes.Cast<ExpressionStatementSyntax, IfStatementSyntax>();

                var merge = MergeNode(n.other.Statement, n.bas, n.even);

                return "while(" + n.other.Condition + ")\r\n" + merge;
            }

            throw new NotImplementedException();
        }

        private string MergeStatementList(List<StatementSyntax> A, List<StatementSyntax> O, List<StatementSyntax> B)
        {
            Func<List<string>, Chunk<StatementSyntax>, bool> conflictHandler = (output, original) =>
            {
                var chunks = Diff3<StatementSyntax>.ThreeWayDiffPriority(original.A, original.O, original.B, Equal, Similar);

                foreach (var chunk in chunks)
                {
                    if (chunk.equalType == ChunkEqualType.PrimaryEqual)
                    {
                        // They are completely equal. Just add O.
                        foreach (var m in chunk.chunk.O)
                            output.Add(m.ToString());
                    }
                    else if (chunk.equalType == ChunkEqualType.SecondaryEqual)
                    {
                        output.Add(MergeChunk(chunk));
                    }
                    else if (chunk.chunk.A.Count == 0 || chunk.chunk.O.Count == 0 || chunk.chunk.B.Count == 0) // It seems its a deletion or insertion
                    {
                        output.Add(MergeChunk(chunk));
                    }
                    else
                    {
                        // They are not equal, and is not a pure insertion or deletion. Try unevenness
                        output.Add(MergeTreeUneven(chunk.chunk));
                    }
                }
                return false;
            };

            Func<StatementSyntax, StatementSyntax, bool> comparer = (x, y) => x != null && y != null && x.ToString().Trim() == y.ToString().Trim();
            var merge = Diff3<StatementSyntax>.Merge<string>(A, O, B, comparer, conflictHandler, x => x.ToString());
            return string.Join("\r\n", merge);
        }

        private string MergeChunk(PriorityChunk<StatementSyntax> chunk)
        {
            if (chunk.equalType == ChunkEqualType.PrimaryEqual)
            {
                string output = "";
                for (int i = 0; i < chunk.chunk.A.Count; i++) // Since it is a stable chunk, the length of A and O and B are equal.
                {
                    output += chunk.chunk.O[i].ToString();
                    if (i != chunk.chunk.A.Count - 1)
                        output += "\r\n";
                }
                return output;
            }

            return MergeChunk(chunk.chunk); // This is not completely equal. Lets merge.
        }

        private string MergeChunk(Chunk<StatementSyntax> chunk)
        {
            if (chunk.stable)
            {
                string output = "";
                {
                    for (int i = 0; i < chunk.A.Count; i++) // Since it is a stable chunk, the length of A and O and B are equal.
                    {
                        output += MergeNode(chunk.A[i], chunk.O[i], chunk.B[i]);
                        if (i != chunk.A.Count - 1)
                            output += "\r\n";
                    }
                }

                return output;
            }

            return MergeChunk(chunk.A, chunk.O, chunk.B);
        }

        private string MergeChunk(List<StatementSyntax> A, List<StatementSyntax> O, List<StatementSyntax> B)
        {

            // Deletion of A, do nothing.
            if (A.Count == 0 && O.Count != 0 && B.Count != 0)
            {
                return null;
            }
            // Deletion of B, do nothing.
            if (A.Count != 0 && O.Count != 0 && B.Count == 0)
            {
                return null;
            }
            // Isertion of A, return A
            if (A.Count != 0 && O.Count == 0 && B.Count == 0)
            {
                return String.Join("\r\n", A);
            }
            // Isertion of B, return B
            if (A.Count == 0 && O.Count == 0 && B.Count != 0)
            {
                return String.Join("\r\n", B);
            }
            // Deletion in both A and B
            if (A.Count == 0 && O.Count != 0 && B.Count == 0)
            {
                return null;
            }
             // Nothing passed in.
            if (A.Count == 0 && O.Count == 0 && B.Count == 0)
                return null;

            throw new NotImplementedException();

        }


        private string MergeTreeBlockToStatement(PriorityChunk<StatementSyntax> originals)
        {
            if (originals.chunk.A.First() is IfStatementSyntax && originals.chunk.O.First() is IfStatementSyntax && originals.chunk.B.First() is IfStatementSyntax)
            {
                var A = (originals.chunk.A.First() as IfStatementSyntax);
                var O = (originals.chunk.O.First() as IfStatementSyntax);
                var B = (originals.chunk.B.First() as IfStatementSyntax);

                var As = A.Statement;
                var Os = O.Statement;
                var Bs = B.Statement;

                var expression = MergeNode(A.Condition, O.Condition, B.Condition);

                return "if(" + expression + ") \r\n" + ChangeBetweenBlockAndStatement(As, Os, Bs);
            }

            throw new NotImplementedException();
        }

        // This function will test if an unstable chunk is simply due to insertion of a node and handling on a deeper level.
        // Will also handle how Statements can be chagned into blocks.
        private string MergeTreeUneven(Chunk<StatementSyntax> originals)
        {

            Func<StatementSyntax, bool> hasSub = x => x is IfStatementSyntax || x is BlockSyntax || x is WhileStatementSyntax;
            Func<StatementSyntax, StatementSyntax> subStatement = x =>
            {
                if (x is IfStatementSyntax)
                    return (x as IfStatementSyntax).Statement;
                if (x is WhileStatementSyntax)
                    return (x as IfStatementSyntax).Statement;
                if (x is BlockSyntax)
                    return x;
                if (x is ExpressionStatementSyntax)
                    return x;

                throw new NotImplementedException();
            };
            Func<StatementSyntax, string> getStr = x =>
            {
                if (x is IfStatementSyntax)
                    return "if(" + (x as IfStatementSyntax).Condition.ToString() + ")" ;

                throw new NotImplementedException();
            };


            var aParent = originals.A.First().Parent;
            var oParent = originals.A.First().Parent;
            var bParent = originals.A.First().Parent;

            // Flatten all substatements out
            var aStatementsFlattened = originals.A.SelectMany(x => hasSub(x) ? GetSubstatementList(subStatement(x)).Select(y => Tuple.Create(x, y)) : new List<Tuple<StatementSyntax, StatementSyntax>> { Tuple.Create((StatementSyntax)null, x) }).ToList();
            var oStatementsFlattened = originals.O.SelectMany(x => hasSub(x) ? GetSubstatementList(subStatement(x)).Select(y => Tuple.Create(x, y)) : new List<Tuple<StatementSyntax, StatementSyntax>> { Tuple.Create((StatementSyntax)null, x) }).ToList();
            var bStatementsFlattened = originals.B.SelectMany(x => hasSub(x) ? GetSubstatementList(subStatement(x)).Select(y => Tuple.Create(x, y)) : new List<Tuple<StatementSyntax, StatementSyntax>> { Tuple.Create((StatementSyntax)null, x) }).ToList();

            var flattenedMatch = Diff3<Tuple<StatementSyntax, StatementSyntax>>.ThreeWayDiffPriority(aStatementsFlattened, oStatementsFlattened, bStatementsFlattened, (x, y) => x != null && y != null && Equal(x.Item2, y.Item2), (x, y) => x != null && y != null && Similar(x.Item2, y.Item2));

            StatementSyntax lastAItem = null;
            StatementSyntax lastOItem = null;
            StatementSyntax lastBItem = null;

            var output = new List<String>();

            foreach (var match in flattenedMatch)
            {
                if (new List<StatementSyntax> { lastAItem, lastOItem, lastBItem }.Where(x => x != null).Count() > 1)
                    throw new Exception("Conflict");

                if (match.chunk.stable)
                {
                    var zipped = match.chunk.A.Zip(match.chunk.O.Zip(match.chunk.B, (o, b) => new { o = o, b = b }), (a, o) => new { a = a, o = o.o, b = o.b });

                    var merged = new List<string>();
                    foreach (var item in zipped)
                    {
                        var aOpen = item.a.Item1 != null;
                        var oOpen = item.o.Item1 != null;
                        var bOpen = item.b.Item1 != null;
                        var lastAOpen = lastAItem != null;
                        var lastOOpen = lastOItem != null;
                        var lastBOpen = lastBItem != null;

                        if (lastAItem != null && item.a.Item1 != null && lastAItem != item.a.Item1)
                        {

                        }
                        else if (lastBItem != null && item.b.Item1 != null && lastBItem != item.b.Item1)
                        {

                        }

                        if ((!aOpen && lastAOpen) || (!bOpen && lastBOpen))
                        {
                            merged.Add("}");
                        }
                        if (aOpen && !lastAOpen)
                            merged.Add("if(" + ((IfStatementSyntax)item.a.Item1).Condition + ") {");
                        if (oOpen && !lastOOpen)
                            merged.Add("if(" + ((IfStatementSyntax)item.o.Item1).Condition + ") {");
                        if (bOpen && !lastBOpen)
                            merged.Add("if(" + ((IfStatementSyntax)item.b.Item1).Condition + ") {");

                        merged.Add(MergeNode(item.a.Item2, item.o.Item2, item.b.Item2));

                        lastAItem = item.a.Item1;
                        lastOItem = item.o.Item1;
                        lastBItem = item.b.Item1;
                    }

                    output.Add(String.Join("\r\n", merged));

                }
                else
                {
                    if (match.chunk.A.Count == 0 && match.chunk.O.Count == 0 && match.chunk.B.Count != 0)
                    {
                        var merged = new List<string>();
                        foreach (var item in match.chunk.B)
                        {
                            var bOpen = item.Item1 != null;
                            var lastBOpen = lastBItem != null;

                            if (!bOpen && lastBOpen)
                            {
                                merged.Add("}");
                            }
                            if ((bOpen && !lastBOpen))
                                merged.Add("if() {");
                            else if(item.Item1 != lastBItem)
                                merged.Add("}\r\nif() {");


                            merged.Add(MergeNode(null, null, item.Item2));

                            lastBItem = item.Item1;
                        }

                        output.Add(String.Join("\r\n", merged));
                    }
                    else if (match.chunk.A.Count != 0 && match.chunk.O.Count == 0 && match.chunk.B.Count == 0)
                    {
                        var merged = new List<string>();
                        foreach (var item in match.chunk.A)
                        {
                            var aOpen = item.Item1 != null;
                            var lastAOpen = lastAItem != null;

                            if (!aOpen && lastAOpen)
                            {
                                merged.Add("}");
                            }
                            if (aOpen && !lastAOpen)
                                merged.Add("if() {");

                            merged.Add(MergeNode(null, null, item.Item2));

                            lastAItem = item.Item1;
                        }

                        output.Add(String.Join("\r\n", merged));

                    }
                    else if (match.chunk.A.Count == 0 && match.chunk.O.Count != 0 && match.chunk.B.Count == 0);
                    else
                        throw new NotImplementedException();
                };
            }

            if (lastAItem != null || lastOItem != null || lastBItem != null)
                output.Add("}");

/*

        // OLD METHOD. Lets try something new.
        var aBlocks = originals.A.Where(hasSub).Select(x => new { statement = x, sub = subStatement(x) });
        var bBlocks = originals.B.Where(hasSub).Select(x => new { statement = x, sub = subStatement(x) });

        var aMatches = aBlocks.Select(x => new { parents = x, chunks = IdentifyChunkRests(originals, x.sub, SubstatementPosition.A) }).ToList();
        var bMatches = bBlocks.Select(x => new { parents = x, chunks = IdentifyChunkRests(originals, x.sub, SubstatementPosition.B) }).ToList();

        var aStatements = aMatches.SelectMany(x => x.chunks).SelectMany(x => x.chunk.O).ToList();
        var bStatements = bMatches.SelectMany(x => x.chunks).SelectMany(x => x.chunk.O).ToList();


        var overlap = aStatements.Any(x => bStatements.Contains(x));

        if (overlap)
            throw new Exception("Conflcit");

            
        var aCnt = 0;
        var oCnt = 0;
        var bCnt = 0;

        var chunks = new List<Chunk<StatementSyntax>>();

        var output = new List<String>();

        while (aCnt < originals.A.Count && oCnt < originals.O.Count && bCnt < originals.B.Count)
        {
            var aItem = originals.A[aCnt];
            var oItem = originals.O[oCnt];
            var bItem = originals.B[bCnt];
            if (hasSub(aItem))
            {
                var match = aMatches.Single(x => x.parents.statement == aItem);

                var newoCnt = oCnt;
                while(originals.O[newoCnt] != match.chunks.First().chunk.O.First())
                    newoCnt++;

                var newbCnt = bCnt;
                while (originals.B[newbCnt] != match.chunks.First().chunk.B.First())
                    newbCnt++;
                    
                var restMerge = MergeChunk(new List<StatementSyntax>(), originals.O.TakeRange(oCnt, newoCnt).ToList(), originals.B.TakeRange(bCnt, newbCnt).ToList());
                output.Add(restMerge);

                output.Add(getStr(aItem));
                if (match.parents.sub is BlockSyntax)
                    output.Add("{");
                foreach (var chunk in match.chunks)
                {
                    var mergeChunk = MergeChunk(chunk);
                    output.Add(mergeChunk);
                }
                if (match.parents.sub is BlockSyntax)
                    output.Add("}");

                aCnt ++;
                oCnt = newoCnt + match.chunks.Sum(x => x.chunk.O.Count);
                bCnt = newbCnt + match.chunks.Sum(x => x.chunk.B.Count);
            }
            else if (hasSub(bItem))
            {
                var match = bMatches.Single(x => x.parents.statement == bItem);

                var newoCnt = oCnt;
                while (originals.O[newoCnt] != match.chunks.First().chunk.O.First())
                    newoCnt++;

                var newaCnt = aCnt;
                while (originals.A[newaCnt] != match.chunks.First().chunk.A.First())
                    newaCnt++;

                var restMerge = MergeChunk(originals.A.TakeRange(aCnt, newaCnt).ToList(), originals.O.TakeRange(oCnt, newoCnt).ToList(), new List<StatementSyntax>());

                output.Add(getStr(bItem));
                if(match.parents.sub is BlockSyntax)
                    output.Add("{");
                foreach(var chunk in match.chunks) {
                    var mergeChunk = MergeChunk(chunk);
                    output.Add(mergeChunk);
                }
                if (match.parents.sub is BlockSyntax)
                    output.Add("}");

                aCnt += match.chunks.Sum(x => x.chunk.A.Count);
                oCnt += match.chunks.Sum(x => x.chunk.O.Count);
                bCnt++;
            }
            else
            {
                throw new Exception("This should not happen");
            }
        }


        output.Add(MergeChunk(originals.A.Skip(aCnt).ToList(), originals.O.Skip(oCnt).ToList(), originals.B.Skip(bCnt).ToList()));

        */
            return string.Join("\r\n", output.Where(x => x != null).ToArray());
            //throw new NotImplementedException();
        }

        public bool Similar(StatementSyntax x, StatementSyntax y)
        {
            return Similarity(x, y) > 0.6f;
        }
        enum SubstatementPosition {
            A, O, B
        }

        private List<PriorityChunk<StatementSyntax>> IdentifyChunkRests(Chunk<StatementSyntax> originals, StatementSyntax substatement, SubstatementPosition pos)
        {
            var Subs = GetSubstatementList(substatement);

            var A = pos == SubstatementPosition.A ? Subs : originals.A;
            var O = pos == SubstatementPosition.O ? Subs : originals.O;
            var B = pos == SubstatementPosition.B ? Subs : originals.B;

            return ExtractRelevantChunks(A, O, B);

        }

        private static List<StatementSyntax> GetSubstatementList(StatementSyntax substatement)
        {
            List<StatementSyntax> Subs;

            if (substatement is ExpressionStatementSyntax)
            {
                Subs = new List<StatementSyntax> { (substatement as ExpressionStatementSyntax) };

            }
            else if (substatement is BlockSyntax)
            {
                Subs = (substatement as BlockSyntax).Statements.Select(x => x).ToList();
            }
            else
                throw new NotImplementedException();
            return Subs;
        }

        private List<PriorityChunk<StatementSyntax>> ExtractRelevantChunks(List<StatementSyntax> A, List<StatementSyntax> O, List<StatementSyntax> B)
        {
            var chunks = Diff3<StatementSyntax>
                .ThreeWayDiffPriority(A, O, B, (x, y) => x != null && y != null && Equal(x, y), (x, y) => x != null && y != null && Similar(x, y));

            /*var chunkAO = Chunk2<StatementSyntax>
                .TwoWayDiffPriority(A, O, (x, y) => x != null && y != null && Equal(x, y), (x, y) => x != null && y != null && Similar(x, y));
            var chunkBO = Chunk2<StatementSyntax>
                .TwoWayDiffPriority(B, O, (x, y) => x != null && y != null && Equal(x, y), (x, y) => x != null && y != null && Similar(x, y));*/


            var firstStableChunk = chunks.First(x => x.chunk.stable);
            var lastStableChunk = chunks.Last(x => x.chunk.stable);

            var rv = new List<PriorityChunk<StatementSyntax>>();

            var recordToSequence = false;
            foreach (var chunk in chunks)
            {
                if (chunk == firstStableChunk)
                    recordToSequence = true;

                if (!recordToSequence)
                    continue;

                rv.Add(chunk);

                if (chunk == lastStableChunk)
                    break;
            }

            return rv;
        }
        /*
        private List<Tuple<StatementSyntax, List<Tuple<StatementSyntax, double>>> ChunkPrices(StatementSyntax other, List<StatementSyntax> bas) {
            if (other is ExpressionStatementSyntax)
            {
                return bas.Select(x => Tuple.Create(x, Similarity(x, other))).ToList();
            }
            if (other is BlockSyntax)
            {
                var block = (other as BlockSyntax).Statements.ToList();;
                return bas.Select(x => Tuple.Create(x, block.Select(y => Tuple.Create((StatementSyntax)y, Similarity(x, y)).ToList()))).ToList();
            }
            throw new NotImplementedException();
        }*/
        private string ChangeBetweenBlockAndStatement(StatementSyntax A, StatementSyntax O, StatementSyntax B)
        {
            // Changed into block, when others were expressions
            if ((A is ExpressionStatementSyntax && O is ExpressionStatementSyntax && B is BlockSyntax) ||
                (A is BlockSyntax && O is ExpressionStatementSyntax && B is ExpressionStatementSyntax))
                return "{\r\n" + TestForBlockAndStatement(A, O, B) + "\r\n}";

            // Changed into expression when others were blocks
            if ((A is ExpressionStatementSyntax && O is BlockSyntax && B is BlockSyntax) ||
                (A is BlockSyntax && O is BlockSyntax && B is ExpressionStatementSyntax))
                return TestForBlockAndStatement(A, O, B);

            throw new Exception();
        }

        private List<StatementSyntax> getStatementList(StatementSyntax a)
        {
            if (a is ExpressionStatementSyntax)
                return new List<StatementSyntax> { a };
            else if (a is BlockSyntax)
                return (a as BlockSyntax).Statements.ToList();
            throw new NotImplementedException();
        }


        private string TestForBlockAndStatement(StatementSyntax A, StatementSyntax O, StatementSyntax B)
        {
            var a = getStatementList(A);
            var o = getStatementList(O);
            var b = getStatementList(B);

            var statements = MergeStatementList(a, o, b);
            return statements;
        }

        // Match a statement with two chunks.
        private string CreateInnerBlock(StatementSyntax pA, List<StatementSyntax> pO, List<StatementSyntax> pB)
        {
            // Check if the statement inside an if-statement is a block. If it is, try to merge with the current chunk. TODO: When does it suceed?
            if (pA is BlockSyntax)
            {
                var substatements = (pA as BlockSyntax).Statements.ToList();
                var mergedBlock = MergeStatementList(substatements, pO, pB);
                return "{\r\n" + mergedBlock + "}";
            }

            // Check if the statement inside an if-statement is a single expressionstatement. if it is, merge with the others.
            if (pA is ExpressionStatementSyntax)
            {
                if (pO.Count != 1 || pB.Count != 1)
                    throw new Exception("Conflict!");

                var A = (ExpressionStatementSyntax)pA;
                var O = pO.First();
                var B = pB.First();

                if (Similar(A, O) && Similar(O, B))
                {
                    return MergeNode(A, O, B);
                }
                else
                    throw new Exception("Conflict!");
            }
            throw new Exception("Conflict!");
        }

        bool Equal(SyntaxNode x, SyntaxNode y)
        {
            return Equal(Doublet<SyntaxNode>.Create(x, y));
        }

        bool Equal(Doublet<SyntaxNode> nodes)
        {
            if (nodes.X == null || nodes.Y == null)
                return false;

            if (nodes.X.GetType() != nodes.Y.GetType())
                return false;

            if (nodes.Is<ExpressionStatementSyntax>())
            {
                return nodes.Cast<ExpressionStatementSyntax>().Select(x => x.Expression).Apply(Equal);
            }
            else if (nodes.Is<InvocationExpressionSyntax>())
            {
                var c = nodes.Cast<InvocationExpressionSyntax>();
                var argumentsSimlarity = c.Select(x => x.ArgumentList).Apply(Equal);
                var expressionSimlarity = c.Select(x => x.Expression).Apply(Equal);
                return argumentsSimlarity && expressionSimlarity;
            }
            else if (nodes.Is<ArgumentListSyntax>())
            {
                var c = nodes.Cast<ArgumentListSyntax>().Select(x => x.Arguments.ToList());
                if (c.X.Count == c.Y.Count)
                {
                    for (int i = 0; i < c.Y.Count; i++)
                        if (!Equal(c.X[i], c.Y[i]))
                            return false;
                    return true;
                }
                return false;
            }
            else if (nodes.Is<MemberAccessExpressionSyntax>())
            {
                var c = nodes.Cast<MemberAccessExpressionSyntax>();

                var expression = c.Select(x => x.Expression).Apply(Equal);
                var name = c.Select(x => x.Name).Apply(Equal);

                return expression && name;
            }
            else if (nodes.Is<IdentifierNameSyntax>())
            {
                return nodes.Cast<IdentifierNameSyntax>().Select(x => x.Identifier.ToString()).Apply((x, y) => x == y);
            }
            else if (nodes.Is<IfStatementSyntax>())
            {
                var i = nodes.Cast<IfStatementSyntax>();

                var expr = i.Select(x => x.Condition).Apply(Equal);
                var body = i.Select(x => x.Statement).Apply(Equal);
                var elses = i.Select(x => x.Else).ApplyIfExists(Equal);

                return expr && body && elses;
            }
            else if (nodes.Is<LiteralExpressionSyntax>())
            {
                return nodes.Cast<LiteralExpressionSyntax>().Select(x => x.Token.ToString()).Apply((x, y) => x == y);
            }
            else if (nodes.Is<ArgumentSyntax>())
            {
                return nodes.Cast<ArgumentSyntax>().Select(x => x.Expression).Apply(Equal);
            }
            else if (nodes.Is<BlockSyntax>())
            {
                var statements = nodes.Cast<BlockSyntax>().Select(x => x.Statements);
                if (statements.X.Count == statements.Y.Count)
                {
                    for (int i = 0; i < statements.X.Count; i++)
                    {
                        if (!Equal(statements.X[i], statements.Y[i]))
                            return false;
                    }
                    return true;
                }
                else
                    return false;


            }
            throw new NotImplementedException();
        }

        double Similarity(SyntaxNode x, SyntaxNode y)
        {
            return Similarity(Doublet<SyntaxNode>.Create(x, y));
        }

        double Similarity(Doublet<SyntaxNode> nodes)
        {
            if (nodes.X == null || nodes.Y == null)
                return 0.0f;

            if (nodes.X.GetType() != nodes.Y.GetType())
                return 0.0f;

            if (nodes.Is<ExpressionStatementSyntax>())
            {
                return nodes.Cast<ExpressionStatementSyntax>().Select(x => x.Expression).Apply(Similarity);
            }
            else if (nodes.Is<InvocationExpressionSyntax>())
            {
                var c = nodes.Cast<InvocationExpressionSyntax>();
                var argumentsSimlarity = c.Select(x => x.ArgumentList).Apply(Similarity);
                var expressionSimlarity = c.Select(x => x.Expression).Apply(Similarity);
                return argumentsSimlarity * 0.5 + expressionSimlarity * 0.5;
            }
            else if (nodes.Is<ArgumentListSyntax>())
            {
                var c = nodes.Cast<ArgumentListSyntax>().Select(x => x.Arguments.ToList());

                var matches = GraphMatching<ArgumentSyntax, ArgumentSyntax>.Match(c.X, c.Y, ExpressionCost);
                var cost = matches.Select(x => x.Item1 == null || x.Item2 == null ? 0 : 1);

                if (cost.Count() == 0)
                    return 1;

                return cost.Average();
            }
            else if (nodes.Is<MemberAccessExpressionSyntax>())
            {
                var c = nodes.Cast<MemberAccessExpressionSyntax>();

                var expression = c.Select(x => x.Expression).Apply(Similarity);
                var name = c.Select(x => x.Name).Apply(Similarity);

                return expression * 0.5 + name * 0.5;
            }
            else if (nodes.Is<IdentifierNameSyntax>())
            {
                return nodes.Cast<IdentifierNameSyntax>().Select(x => x.Identifier.ToString()).Apply(StringSimilarity);
            }
            else if (nodes.Is<IfStatementSyntax>())
            {
                var i = nodes.Cast<IfStatementSyntax>();

                var expr = i.Select(x => x.Condition).Apply(Similarity);
                var body = i.Select(x => x.Statement).Apply(Similarity);
                var elses = i.Select(x => x.Else).ApplyIfExists(Similarity);

                return 0.33 * expr + 0.33 * body + 0.33 * elses;
            }
            else if (nodes.Is<LiteralExpressionSyntax>())
            {
                return nodes.Cast<LiteralExpressionSyntax>().Select(x => x.Token.ToString()).Apply(StringSimilarity);
            }
            else if (nodes.Is<BlockSyntax>())
            {
                var c = nodes.Cast<BlockSyntax>().Select(x => x.Statements.ToList());

                var chunks = Chunk2<StatementSyntax>.TwoWayDiffPriority(c.X, c.Y, Equal, Similar);

                var statements = 0;
                var statementsTotal = 0D;

                foreach (var chunk in chunks)
                {
                    if (chunk.equal == ChunkEqualType.PrimaryEqual)
                    {
                        statements += chunk.chunk.x.Count;
                        statementsTotal += chunk.chunk.x.Count;
                    }
                    else if (chunk.equal == ChunkEqualType.SecondaryEqual)
                    {
                        statements += chunk.chunk.x.Count;
                        for (int i = 0; i < chunk.chunk.x.Count; i++)
                            statementsTotal += Similarity(chunk.chunk.x[i], chunk.chunk.y[i]);
                    }
                    else
                    {
                        statements += chunk.chunk.x.Count + chunk.chunk.y.Count;
                    }
                }
                var avg = statementsTotal / statements;
                throw new NotImplementedException("We need to tests thissssSSS");
                //                return avg;
            }
            throw new NotImplementedException();
        }


        double StringSimilarity(string x, string y)
        {
            return x == y ? 1 : 0;
        }


        private string ListMerger<List, Item>(Triplet<SyntaxNode> nodes, Func<List, IEnumerable<Item>> c, GraphMatching<Item, Item>.Cost cost)
            where List : SyntaxNode
            where Item : SyntaxNode
        {
            var l = nodes.Cast<List>()
                .Select(x => c(x).ToList());

            var matches = SmartDiff<Item>.GetThreeWayUnorderedMatch(l.A, l.O, l.B, cost);
            var reordered = SmartDiff<Item>.FilterAndMerge(l.A, l.O, l.B, matches, MergeNode);

            return string.Join(", ", reordered);
        }

        public string CreateCLU(SyntaxNode O, string inside)
        {
            return inside;
        }

        private string CreateNamespace(SyntaxNode node, string members)
        {
            var IDENTIFIER = getIdentifier(node);

            return "namespace " + IDENTIFIER + " {\n" +
                        members + "\n" +
                    "}";

        }


        public string CreateClass(SyntaxNode O, string inside)
        {
            var IDENTIFIER = getIdentifier(O);

            return "class " + IDENTIFIER + " {\n" +
                        inside + "\n" +
                    "}";
        }

        public int? ExpressionCost(SyntaxNode x, SyntaxNode y)
        {
            int cost = 4;

            if (x.GetType() == y.GetType())
                cost--;

            if (x.ToString().Trim() == y.ToString().Trim())
                cost -= 2;

            if (cost == 4)
                return null;

            return cost;
        }

        public int? ClassMemberCost(SyntaxNode x, SyntaxNode y)
        {
            int cost = 4;

            /*if (x.GetType() == y.GetType())
                cost--;*/

            if (getIdentifier(x) == getIdentifier(y))
                cost -= 2;

            if (cost == 4)
                return null;

            return cost;
        }

        public int? NamespaceMemberCost(SyntaxNode x, SyntaxNode y)
        {
            int cost = 4;

            if (x.GetType() == y.GetType())
                cost--;

            if (getIdentifier(x) == getIdentifier(y))
                cost -= 2;

            if (cost == 4)
                return null;

            return cost;
        }

        public string getIdentifier(SyntaxNode n)
        {
            if (n is IdentifierNameSyntax)
                return n.ToString();
            else if (n is ClassDeclarationSyntax)
                return ((ClassDeclarationSyntax)n).Identifier.ToString();
            else if (n is MethodDeclarationSyntax)
                return ((MethodDeclarationSyntax)n).Identifier.ToString(); // TODO: Implement heristic to also indicate closeness in body, in parameter list and identifiers.
            else if (n is UsingDirectiveSyntax)
                return n.ChildNodes().First().ToString();
            else if (n is NamespaceDeclarationSyntax)
                return ((NamespaceDeclarationSyntax)n).Name.ToString();

            throw new NotImplementedException();
        }

        public CodeNodeType getChildType(SyntaxNode sn)
        {
            if (sn is CompilationUnitSyntax)
                return CodeNodeType.Unordered;
            else if (sn is NamespaceDeclarationSyntax)
                return CodeNodeType.Unordered;
            else if (sn is ClassDeclarationSyntax)
                return CodeNodeType.Unordered;
            return CodeNodeType.Ordered;
        }

        public SyntaxNode SyntaxFromLines(string code)
        {
            return SyntaxTree.ParseText(code).GetRoot();
        }

        public List<string> LinesFromSyntax(SyntaxNode m)
        {
            if (m == null)
                return new List<string>();
            return m.GetText().Lines.Select(x => x.ToString()).ToList();
        }

        public List<SyntaxNode> Children(SyntaxNode n)
        {
            if (n is CompilationUnitSyntax || n is ClassDeclarationSyntax)
                return n.ChildNodes().ToList();
            if (n is NamespaceDeclarationSyntax)
                return n.ChildNodes().Skip(1).Take(1).ToList();

            throw new NotImplementedException();
        }

        public Tree<SyntaxNode> ConvertToTree(SyntaxNode n)
        {
            var children = n.ChildNodes().Select(x => ConvertToTree(x)).ToArray();
            return new Tree<SyntaxNode>(n, children);
        }

        public string getSyntaxString<T>(SyntaxNode node, Func<T, string> f) where T : SyntaxNode
        {
            if (node is T)
            {
                var c = node as T;
                return "[" + f(c) + "]";
            }
            return "";
        }

        public string getLabel(SyntaxNode t)
        {
            if (t == null)
                return "empty";

            string s = t.GetType().ToString().Substring(24);

            s += getSyntaxString<IdentifierNameSyntax>(t, x => x.Identifier.ToString());
            s += getSyntaxString<ClassDeclarationSyntax>(t, x => x.Identifier.ToString());
            s += getSyntaxString<LiteralExpressionSyntax>(t, x => x.Token.ToString());
            s += getSyntaxString<ArrayTypeSyntax>(t, x => x.ElementType.ToString());
            s += getSyntaxString<PredefinedTypeSyntax>(t, x => x.ToString());
            s += getSyntaxString<ParameterSyntax>(t, x => x.Identifier.ToString());

            return s;
        }
    }
}
