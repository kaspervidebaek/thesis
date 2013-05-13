using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Graphviz;


namespace SyntaxDiff
{
    public class GraphMatching<X, Y>
    {
        public static int cnt = 0;
        public class Node
        {
            public readonly bool fakeNode = false;
            public readonly string Name;
            public readonly bool a;

            public Node(string name, bool a)
            {
                this.Name = name;
                this.a = a;
            }
            public Node()
                : this("fake", false)
            {
                fakeNode = true;
            }

            public override string ToString()
            {
                return this.Name;
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }
        }

        public class yNode : Node
        {
            public readonly Y item;
            public yNode(Y item)
                : base("Y: "/* + item.ToString()*/, false)
            {
                this.item = item;
            }
            public yNode()
                : base()
            {
            }
        }

        public class xNode : Node
        {
            public readonly X item;
            public xNode(X item)
                : base("X: "/* + item.ToString()*/, false)
            {
                this.item = item;
            }
            public xNode()
                : base()
            {
            }
        }

        public class Edge : TaggedEdge<Node, int>
        {
            public Edge(Node source, Node target, int cost)
                : base(source, target, cost)
            {
            }

            public override string ToString()
            {
                return this.Source.ToString() + "->" + this.Target.ToString();
            }

            public bool isSourceEdge()
            {
                return (this.Target is yNode && this.Source is xNode) ||
                        (this.Target is xNode && this.Source is yNode);
            }

            public bool hasSameNodes(Edge e)
            {
                if (e.Source == Source && e.Target == Target || e.Target == Source && e.Source == Target)
                    return true;
                return false;
            }


            public bool isSame(Edge e)
            {
                if (e.Source == Source && e.Target == Target)
                    return true;
                return false;
            }

            public int TagPrice(Dictionary<Node, int> TagPrice)
            {
                if (Target.a || Source.a)
                    return 0;

                var xNode = this.Source is yNode ? this.Source : this.Target;
                var yNode = this.Source is xNode ? this.Source : this.Target;

                return TagPrice[xNode] + this.Tag - TagPrice[yNode];
            }
            public override bool Equals(object obj)
            {
                Edge e = (Edge)obj;
                if (e.Source == Source && e.Target == Target)
                    return true;
                return false;
            }
            public override int GetHashCode()
            {
                return Source.GetHashCode() + Target.GetHashCode();
            }

        }

        public class BipartiteGraph : AdjacencyGraph<Node, Edge>
        {
            public readonly List<xNode> xs;
            public readonly List<yNode> ys;
            public readonly List<Node> extras;

            public BipartiteGraph(List<xNode> xs, List<yNode> ys, List<Node> extras)
                : base()
            {
                this.xs = xs;
                this.ys = ys;
                this.extras = extras;
            }

            public struct CostElement
            {
                public int cost;
                public X xs;
                public Y ys;
                public CostElement(int c, X xs, Y ys)
                {
                    cost = c;
                    this.xs = xs;
                    this.ys = ys;
                }
            }
            public static BipartiteGraph CreateFromSets(List<X> xs_, List<Y> ys_, Func<X, Y, int?> cost)
            {
                var xs = xs_.Select(x => new xNode(x)).ToList();
                var ys = ys_.Select(y => new yNode(y)).ToList();

                var extras = new List<Node>();
                var graph = new BipartiteGraph(xs, ys, extras);

                foreach (var x in xs)
                {
                    foreach (var y in ys)
                    {
                        var c = cost(x.item, y.item);
                        if (c.HasValue)
                        {
                            var edge = new Edge(x, y, c.Value);
                            graph.AddVerticesAndEdge(edge);
                        }
                    }
                }

                xs.ForEach(x => graph.AddVertex(x));
                ys.ForEach(y => graph.AddVertex(y));

                if (xs.Count < ys.Count)
                {
                    while (xs.Count < ys.Count)
                    {
                        var pn = new xNode();
                        extras.Add(pn);
                        xs.Add(pn);
                    }

                    foreach (var x in extras)
                    {
                        foreach (var y in ys)
                        {
                            var edge = new Edge(x, y, 500);
                            graph.AddVerticesAndEdge(edge);
                        }
                    }
                }

                if (ys.Count < xs.Count)
                {
                    while (ys.Count < xs.Count)
                    {
                        var pn = new yNode();
                        extras.Add(pn);
                        ys.Add(pn);
                    }

                    foreach (var x in xs)
                    {
                        foreach (var y in extras)
                        {
                            var edge = new Edge(x, y, 500);
                            graph.AddVerticesAndEdge(edge);
                        }
                    }
                }

                return graph;
            }

            static Dictionary<Node, QuickGraph.Graphviz.Dot.GraphvizPoint> pd = new Dictionary<Node, QuickGraph.Graphviz.Dot.GraphvizPoint>();


            public void RenderToFile(string name, List<Edge> markEdges, Dictionary<Node, int> prices, Matching m)
            {

                var graphviz = new GraphvizAlgorithm<Node, Edge>(this);

                graphviz.FormatEdge += (sender, edge) =>
                {
                    if (m != null)
                    {
                        if (m.Exists(edge.Edge))
                        {
                            edge.EdgeFormatter.Style = QuickGraph.Graphviz.Dot.GraphvizEdgeStyle.Solid;
                        }
                        else
                        {
                            edge.EdgeFormatter.Style = QuickGraph.Graphviz.Dot.GraphvizEdgeStyle.Dashed;
                        }
                    }

                    edge.EdgeFormatter.Label.Value = edge.Edge.Tag.ToString();
                    if (prices != null)
                    {
                        try
                        {
                            edge.EdgeFormatter.Label.Value += " P:" + edge.Edge.TagPrice(prices);
                        }
                        catch
                        {
                        }
                    }

                    if (markEdges != null)
                    {
                        var o = markEdges.SingleOrDefault(x => x == edge.Edge);
                        if (o != null)
                        {
                            edge.EdgeFormatter.Style = QuickGraph.Graphviz.Dot.GraphvizEdgeStyle.Bold;
                        }
                    }
                };
                var distanceBetweenPoints = 2;
                var maxPoints = ys.Count;
                var maxY = maxPoints * (distanceBetweenPoints);
                var maxX = xs.Count * (distanceBetweenPoints);
                var yCounted = 0;
                var xCounted = 0;

                graphviz.FormatVertex += (sender, node) =>
                {
                    node.VertexFormatter.Label = node.Vertex.ToString();
                    int p;
                    if (prices != null && prices.TryGetValue(node.Vertex, out p))
                        node.VertexFormatter.Label += " P:" + p;

                    int x = 0, y = 0;


                    var n = node.Vertex;
                    QuickGraph.Graphviz.Dot.GraphvizPoint point;
                    if (!pd.TryGetValue(n, out point))
                    {
                        if (n.Name == "source")
                        {
                            x = 1;
                            y = maxY / 2;
                        }
                        if (n.Name == "sink")
                        {
                            x = 12;
                            y = maxY / 2;
                        }
                        if (n is yNode)
                        {
                            x = 9;
                            y = yCounted++ * distanceBetweenPoints;
                        }
                        if (n is xNode)
                        {
                            x = 4;

                            y = (xCounted++ * distanceBetweenPoints) - (xs.Count - ys.Count) * distanceBetweenPoints / 2;
                        }

                        point = new QuickGraph.Graphviz.Dot.GraphvizPoint(x, y);

                        pd[n] = point;
                    }

                    node.VertexFormatter.Position = point;
                };

                string output = graphviz.Generate();
                //Console.WriteLine(output);

                System.IO.File.WriteAllText(@"C:\Users\Kasper\Desktop\Algorihtms\GV\" + name + ".gv", output);
            }

        }

        public class Matching
        {
            public HashSet<Node> nodes = new HashSet<Node>();
            public Dictionary<Node, Edge> edges = new Dictionary<Node, Edge>();

            public Matching Clone()
            {
                var ms = new Matching();

                foreach (var m in edges)
                {
                    ms.Add(m.Value);
                }

                return ms;

            }
            public int Count
            {
                get
                {
                    return edges.Count;
                }
            }
            public void Add(Edge e)
            {
                edges.Add(e.Source, e);
                nodes.Add(e.Source);
                nodes.Add(e.Target);
            }
            public bool Exists(Node e)
            {
                return nodes.Contains(e);
            }
            public bool Exists(Edge e)
            {
                Edge a;
                if (!edges.TryGetValue(e.Source, out a))
                    return false;
                return a.Target == e.Target;
            }


            public bool ExistsInSource(Edge e)
            {
                Edge a;
                return edges.TryGetValue(e.Source, out a);
            }
            public void RemoveFromSource(Edge e)
            {
                edges.Remove(e.Source);

                nodes.Remove(e.Source);
                nodes.Remove(e.Target);
            }
        }

        public class ResidualGraph : BipartiteGraph
        {
            public readonly Node source;
            public readonly Node sink;

            public ResidualGraph(BipartiteGraph g, Matching matching)
                : base(g.xs, g.ys, g.extras)
            {
                source = new Node("source", true);
                sink = new Node("sink", true);
                this.AddVertex(source);
                this.AddVertex(sink);
                this.AddVertexRange(xs);
                this.AddVertexRange(ys);

                foreach (var x in xs)
                {
                    if (!matching.Exists(x))
                        this.AddVerticesAndEdge(new Edge(source, x, 0));
                }

                foreach (var y in ys)
                {
                    if (!matching.Exists(y))
                        this.AddVerticesAndEdge(new Edge(y, sink, 0));
                }


                this.AddVertexRange(g.Vertices);

                foreach (var edge in g.Edges)
                {
                    if (matching.Exists(edge))
                    {
                        this.AddEdge(new Edge(edge.Target, edge.Source, edge.Tag));
                    }
                    else
                    {
                        this.AddEdge(new Edge(edge.Source, edge.Target, edge.Tag));
                    }
                }
            }

            public Matching Augment(Matching m_, List<Edge> p)
            {
                var m = m_.Clone();

                int b = p.Min(x => x.Tag);

                foreach (var e in p)
                {
                    if (e.isSourceEdge())
                    {
                        if (e.Source is xNode && e.Target is yNode)
                        {
                            if (m.Exists(e.Source))
                            {
                                m.RemoveFromSource(e);
                            }
                            m.Add(e);
                        }

                    }

                }
                return m;
            }
        }
        public static int RunCount = 0;

        public static List<Tuple<X, Y>> Match(List<X> xs, List<Y> ys, Func<X, Y, int?> cost)
        {
            RunCount++;

            var flowgraph = BipartiteGraph.CreateFromSets(xs, ys, cost);

            var M = new Matching();

            var p = new Dictionary<Node, int>();
            flowgraph.RenderToFile(RunCount + "it0", null, null, null);

            flowgraph.xs.ToList().ForEach(x => p.Add(x, 0));
            flowgraph.ys.ToList().ForEach(y => p.Add(y, int.MaxValue));
            flowgraph.Edges.ToList().ForEach(x => p[x.Target] = Math.Min(p[x.Target], x.Tag));

            var maxCount = ys.Count > xs.Count ? ys.Count : xs.Count;

            int cnt = 1;
            while (M.Count != maxCount)
            {
                var Gm = new ResidualGraph(flowgraph, M);

                var dijkstra = Gm.ShortestPathsDijkstra(x => x.TagPrice(p), Gm.source);
                var target = Gm.sink;
                IEnumerable<Edge> path;
                if (dijkstra(target, out path))
                {
                    var M_ = Gm.Augment(M, path.ToList());
                    Gm.RenderToFile(RunCount + "it" + cnt++, path.ToList(), p, M);
                    p = getNewPrices(dijkstra, p);
                    M = M_;
                }
                else
                    break; // KV: No path found. This means that we have finished our matching, however there are still nodes left in the graph.

            }

            var _Gm = new ResidualGraph(flowgraph, M);
            _Gm.RenderToFile(RunCount + "it" + cnt++, null, p, M);

            var rl = new List<Tuple<X, Y>>();
            foreach (var m in M.edges)
            {
                var x = ((xNode)m.Value.Source).item;
                var y = ((yNode)m.Value.Target).item;
                rl.Add(Tuple.Create(x, y));
            }

            var notMatchedXs = xs.Where(x => rl.SingleOrDefault(i => i.Item1 != null && i.Item1.Equals(x)) == null).ToList();
            var notMatchedYs = ys.Where(y => rl.SingleOrDefault(i => i.Item2 != null && i.Item2.Equals(y)) == null).ToList();

            notMatchedXs.ForEach(x => rl.Add(Tuple.Create(x, default(Y))));
            notMatchedYs.ForEach(y => rl.Add(Tuple.Create(default(X), y)));

            return rl.ToList();
        }

        private static Dictionary<Node, int> getNewPrices(TryFunc<Node, IEnumerable<Edge>> dijkstra, Dictionary<Node, int> p)
        {
            var p_ = new Dictionary<Node, int>();
            foreach (var kvp in p)
            {
                IEnumerable<Edge> path;
                if (dijkstra(kvp.Key, out path))
                {
                    p_.Add(kvp.Key, kvp.Value + path.Sum(x => x.TagPrice(p)));
                }
                //else throw new Exception("shit!");
            }

            return p_;
        }
    }
}
