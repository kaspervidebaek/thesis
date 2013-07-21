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
        public string MergeTree(SyntaxNode A, SyntaxNode O, SyntaxNode B)
        {
            return MergeTree(Triplet<SyntaxNode>.Create(A, O, B));
        }
        public string MergeTree(Triplet<SyntaxNode> nodes)
        {
            if (nodes.O != null && nodes.A != null && nodes.B == null)
            {
                if(Equal(nodes.A, nodes.O)) {
                    return "";
                }
                return "/*Conflict: Deleted in B*/\r\n" + nodes.A;
            }
            else if(nodes.O != null && nodes.A == null && nodes.B != null)
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
                    return M.Apply(MergeTree);
                }
            }
            else if (nodes.Is<MethodDeclarationSyntax>())
            {
                var M = nodes.Cast<MethodDeclarationSyntax>();

                var name = M.Select(x => x.Identifier).Apply(MergeToken);
                var parm = M.Select(x => x.ParameterList).Apply(MergeTree);
                var body = M.Select(x => x.Body).Apply(MergeTree);
                var retv = M.Select(x => x.ReturnType).Apply(MergeType);

                return retv + " " + name + "(" + parm + ") \r\n" + body + "\r\n";
            }
            else if (nodes.Is<IfStatementSyntax>())
            {
                var i = nodes.Cast<IfStatementSyntax>();

                var expr = i.Select(x => x.Condition).Apply(MergeTree);
                var body = i.Select(x => x.Statement).Apply(MergeTree);
                var elses = i.Select(x => x.Else).ApplyIfExists(MergeTree);


                return "if( " + expr.ToString() + ")\r\n" + body + (elses != null ? " else " + elses : "");
            }
            else if (nodes.Is<ExpressionStatementSyntax>())
            {
                var e = nodes.Cast<ExpressionStatementSyntax>();
                return e.Select(x => x.Expression).Apply(MergeTree) + ";";
            }
            else if (nodes.Is<InvocationExpressionSyntax>())
            {
                var i = nodes.Cast<InvocationExpressionSyntax>();

                var arguments = i.Select(x => x.ArgumentList).Apply(MergeTree);
                var expression = i.Select(x => x.Expression).Apply(MergeTree);

                return expression + "(" + arguments + ")";
            }
            else if (nodes.Is<MemberAccessExpressionSyntax>())
            {
                var i = nodes.Cast<MemberAccessExpressionSyntax>();

                var expression = i.Select(x => x.Expression).Apply(MergeTree);
                var name = i.Select(x => x.Name).Apply(MergeTree);

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
                var i = nodes.Cast<ArgumentSyntax>().Select(x => x.Expression).Apply(MergeTree);

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


                Func<List<string>, Chunk<StatementSyntax>, bool> conflictHandler = (output, original) =>
                {
                    var chunks = Diff3<StatementSyntax>.ThreeWayDiffPriority(original.A, original.O, original.B, Equal, (x, y) => Similarity(x, y) > 0.6f);

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
                        else
                        {
                            // They are not equal. Try an uneven merge.
                            output.Add(MergeTreeUneven(chunk));
                        }
                    }
                    return false; 
                };

                Func<StatementSyntax, StatementSyntax, bool> comparer = (x, y) => x != null && y != null && x.ToString().Trim() == y.ToString().Trim();
                var merge = Diff3<StatementSyntax>.Merge<string>(statements.A, statements.O, statements.B, comparer, conflictHandler, x => x.ToString());
                return "{\r\n" + string.Join("\r\n", merge) + "\r\n}\r\n";
            }
            else if (nodes.Is<ExpressionStatementSyntax, WhileStatementSyntax>())
            {
                var n = nodes.Cast<ExpressionStatementSyntax, WhileStatementSyntax>();

                var merge = MergeTree(n.other.Statement, n.bas, n.even);

                return "while(" + n.other.Condition + ")\r\n" + merge;

            }
            else if (nodes.Is<ExpressionStatementSyntax, IfStatementSyntax>())
            {
                var n = nodes.Cast<ExpressionStatementSyntax, IfStatementSyntax>();

                var merge = MergeTree(n.other.Statement, n.bas, n.even);

                return "while(" + n.other.Condition + ")\r\n" + merge;
            }

            throw new NotImplementedException();
        }

        private string MergeChunk(PriorityChunk<StatementSyntax> chunk)
        {
            if (chunk.chunk.stable)
            {
                string output = "";

                for (int i = 0; i < chunk.chunk.A.Count; i++)
                {
                    output += MergeTree(chunk.chunk.A[i], chunk.chunk.O[i], chunk.chunk.B[i]) + "\r\n";
                }

                return output;
            }

            if (chunk.chunk.A.Count == 0 && chunk.chunk.O.Count != 0 && chunk.chunk.B.Count != 0)
            {
            }

            throw new NotImplementedException();

        }


        private string MergeTreeUneven(PriorityChunk<StatementSyntax> originals)
        {

            if (originals.chunk.A.First() is IfStatementSyntax)
            {
                var ifstatement =  (originals.chunk.A.First() as IfStatementSyntax);
                var substatements = (ifstatement.Statement as BlockSyntax).Statements.ToList();

                var chunks = Diff3<StatementSyntax>.ThreeWayDiffPriority(substatements, originals.chunk.O, originals.chunk.B, Equal, (x, y) => Similarity(x, y) > 0.6f);
                string output = "";

                if (chunks.First().chunk.stable && chunks.Last().chunk.stable)
                {
                    var mergedBlock = String.Join("\r\n", chunks.Select(MergeChunk).ToArray());
                    return "while(" + ifstatement.Condition.ToString() + ") {" + mergedBlock + "}";
                }

                //var chunks = Chunk<StatementSyntax>.getChunks(matches, Equal);

                

            }

            throw new NotImplementedException();
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
            else if (nodes.Is<ExpressionStatementSyntax, WhileStatementSyntax>())
            {

            }
            throw new NotImplementedException();

        }

        double Similarity(SyntaxNode x, SyntaxNode y)
        {
            return Similarity(Doublet<SyntaxNode>.Create(x, y));
        }

        double Similarity(Doublet<SyntaxNode> nodes)
        {
            if (nodes.Is<ExpressionStatementSyntax>())
            {
                return nodes.Cast<ExpressionStatementSyntax>().Select(x => x.Expression).Apply(Similarity); 
            }
            else if (nodes.Is<InvocationExpressionSyntax>())
            {
                var c = nodes.Cast<InvocationExpressionSyntax>();
                var argumentsSimlarity = c.Select(x => x.ArgumentList).Apply(Similarity);
                var expressionSimlarity = c.Select(x => x.Expression).Apply(Similarity);
                return argumentsSimlarity * 0.5 + expressionSimlarity + 0.5;
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

                return expression*0.5 + name*0.5;
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
            else if (nodes.Is<ExpressionStatementSyntax, WhileStatementSyntax>())
            {

            }
            else if (nodes.Is<WhileStatementSyntax, ExpressionStatementSyntax>())
            {
                return Similarity(((WhileStatementSyntax)nodes.X).Statement, nodes.Y);
            }
            else if (nodes.Is<IfStatementSyntax, ExpressionStatementSyntax>())
            {
                return Similarity(((IfStatementSyntax)nodes.X).Statement, nodes.Y);
            }

            return 0;
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
            var reordered = SmartDiff<Item>.FilterAndMerge(l.A, l.O, l.B, matches, MergeTree);

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
            if(n is CompilationUnitSyntax || n is ClassDeclarationSyntax)
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
