using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Graphviz;


namespace AlgoritmeTests
{
    class GraphAlgorithm
    {
        public static int cnt = 0;
        public class Node
        {
            public readonly string Name;
            public readonly bool a;

            public Node(string name, bool a)
            {
                this.Name = name;
                this.a = a;
            }

            public override string ToString()
            {
                return this.Name;
            }

            /*public override bool Equals(object obj)
            {
                if(!(obj is Node))
                    return false;
                var objC = obj as Node;
                return Name == objC.Name;
            }*/

            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }
        }

        public class PixelNode : Node
        {
            public readonly Pixel p;
            public PixelNode(Pixel p)
                : base("px" + p.ToString(), false)
            {
                this.p = p;
            }
        }

        public class ImageNode : Node
        {
            public readonly Image i;
            public ImageNode(Image i)
                : base("im" + i.average.ToString(), false)
            {
                this.i = i;
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
                return (this.Target is PixelNode && this.Source is ImageNode) ||
                        (this.Target is ImageNode && this.Source is PixelNode);
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

                var xNode = this.Source is PixelNode ? this.Source : this.Target;
                var yNode = this.Source is ImageNode ? this.Source : this.Target;

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

        public class Graph : AdjacencyGraph<Node, Edge>
        {
            public readonly List<PixelNode> xs;
            public readonly List<ImageNode> ys;

            public static readonly Pixel fakePixel = new Pixel(-1, -1, -1);

            public Graph(List<PixelNode> xs, List<ImageNode> ys)
                : base()
            {
                this.xs = xs;
                this.ys = ys;
            }

            public struct CostElement
            {
                public int cost;
                public Pixel cs;
                public Image pi;
                public CostElement(int c, Pixel cs_, Image pi_)
                {
                    cost = c;
                    cs = cs_;
                    pi = pi_;
                }
            }
            public static Graph CreateFromImageOrg(Image i, List<Image> il)
            {
                var xs = i.list.Select(x => new PixelNode(x)).ToList();
                var ys = il.Select(x => new ImageNode(x)).ToList();


                var graph = new Graph(xs, ys);

                foreach (var x in xs)
                {
                    foreach (var y in ys)
                    {
                        var edge = new Edge(x, y, x.p.CompareTo(y.i.average));
                        graph.AddVerticesAndEdge(edge);
                    }
                }

                var extraXs = new List<PixelNode>();
                while (xs.Count < ys.Count)
                {
                    var pn = new PixelNode(fakePixel);
                    extraXs.Add(pn);
                    xs.Add(pn);
                }

                foreach (var x in extraXs)
                {
                    foreach (var y in ys)
                    {
                        var edge = new Edge(x, y, 500);
                        graph.AddVerticesAndEdge(edge);
                    }
                }


                return graph;
            }
            public static Tuple<List<Image>, Graph> CreateFromImage(Image c, List<Image> p)
            {

                var MergedCSLists = new List<CostElement>();
                var cutOffCSLists = new Dictionary<Pixel, List<CostElement>>();

                foreach (var cs in c.list)
                {
                    cutOffCSLists.Add(cs, new List<CostElement>());
                    var sortedPI = new List<CostElement>();
                    foreach (var pi in p)
                    {
                        sortedPI.Add(new CostElement(cs.CompareTo(pi.average), cs, pi));
                    }
                    sortedPI = sortedPI.OrderBy(a => a.cost).Take(c.list.Count).ToList();

                    MergedCSLists.AddRange(sortedPI);
                }
                MergedCSLists = MergedCSLists.OrderBy(a => a.cost).ToList();

                HashSet<Image> usedPImages = new HashSet<Image>();
                Dictionary<Pixel, CostElement> usedCSImages = new Dictionary<Pixel, CostElement>();

                int counter = 0;

                while (usedCSImages.Keys.Count < c.list.Count)
                {
                    CostElement current = MergedCSLists[counter];
                    if (!usedCSImages.ContainsKey(current.cs) && !usedPImages.Contains(current.pi))
                    {
                        if (usedCSImages.ContainsKey(current.cs))
                            usedCSImages[current.cs] = current;
                        else
                        {
                            usedCSImages.Add(current.cs, current);
                            usedPImages.Add(current.pi);
                        }
                    }
                    cutOffCSLists[current.cs].Add(current);
                    counter++;
                }
                if (counter < MergedCSLists.Count)
                {
                    int cutOffCost = MergedCSLists[counter].cost;
                    int temp = 0;
                    foreach (var costEle in usedCSImages.Values)
                    {
                        temp += costEle.cost - cutOffCSLists[costEle.cs].First().cost;
                    }
                    cutOffCost += (temp / 2) + 1;
                    //var FakePixelMatches = new List<Image>();
                    while (counter < MergedCSLists.Count && MergedCSLists[counter].cost <= cutOffCost)
                    {
                        CostElement current = MergedCSLists[counter];
                        cutOffCSLists[current.cs].Add(current);
                        //FakePixelMatches.Add(current.pi);
                        counter++;
                    }
                }
                var OriginallySelectedPI = MergedCSLists.Select(a => a.pi).Distinct();



                var xs = c.list.Select(x => new PixelNode(x)).ToList();
                var ys = MergedCSLists.Take(counter).Select(a => a.pi).Distinct().Select(x => new ImageNode(x)).ToList();

                Dictionary<Pixel, PixelNode> pixelNodes = new Dictionary<Pixel, PixelNode>();
                Dictionary<Image, ImageNode> imageNodes = new Dictionary<Image, ImageNode>();
                foreach (var x in xs)
                    pixelNodes.Add(x.p, x);
                foreach (var y in ys)
                    imageNodes.Add(y.i, y);

                var graph = new Graph(xs, ys);


                foreach (var x in xs)
                {
                    var edges = new List<Edge>();
                    foreach (var y in cutOffCSLists[x.p])
                    {
                        edges.Add(new Edge(x, imageNodes[y.pi], y.cost));

                    }
                    graph.AddVerticesAndEdgeRange(edges);

                }
                var dontTakeforFakes = new List<Image>();
                foreach (var x in xs)
                    dontTakeforFakes.Add(cutOffCSLists[x.p].First().pi);
                var extraXs = new List<PixelNode>();
                while (xs.Count < ys.Count)
                {
                    var pn = new PixelNode(fakePixel);
                    extraXs.Add(pn);
                    xs.Add(pn);
                }



                foreach (var x in extraXs)
                {
                    foreach (var y in ys)
                    {
                        if (!dontTakeforFakes.Contains(y.i))
                        {
                            var edge = new Edge(x, y, 500);
                            graph.AddVerticesAndEdge(edge);
                        }
                    }
                }

                var n = c.list.Count();

                return new Tuple<List<Image>, Graph>(MergedCSLists.Take(counter).Select(a => a.pi).Distinct().ToList(), graph);
            }
            public static List<Image> GreedyAlgorithm(Image c, List<Image> p)
            {

                var MergedCSLists = new List<CostElement>();
                var cutOffCSLists = new Dictionary<Pixel, List<CostElement>>();

                foreach (var cs in c.list)
                {
                    cutOffCSLists.Add(cs, new List<CostElement>());
                    var sortedPI = new List<CostElement>();
                    foreach (var pi in p)
                    {
                        sortedPI.Add(new CostElement(cs.CompareTo(pi.average), cs, pi));
                    }
                    sortedPI = sortedPI.OrderBy(a => a.cost).Take(c.list.Count).ToList();

                    MergedCSLists.AddRange(sortedPI);
                }
                MergedCSLists = MergedCSLists.OrderBy(a => a.cost).ToList();

                HashSet<Image> usedPImages = new HashSet<Image>();
                Dictionary<Pixel, CostElement> usedCSImages = new Dictionary<Pixel, CostElement>();
                List<CostElement> assignmentList = new List<CostElement>();
                int counter = 0;
                var returnList = new List<Image>();
                while (usedCSImages.Keys.Count < c.list.Count)
                {
                    CostElement current = MergedCSLists[counter];
                    if (!usedCSImages.ContainsKey(current.cs) && !usedPImages.Contains(current.pi))
                    {
                        if (usedCSImages.ContainsKey(current.cs))
                            usedCSImages[current.cs] = current;
                        else
                        {
                            usedCSImages.Add(current.cs, current);
                            usedPImages.Add(current.pi);
                            assignmentList.Add(current);
                        }

                    }
                    cutOffCSLists[current.cs].Add(current);
                    counter++;

                }

                return assignmentList.OrderBy(x => c.list.IndexOf(x.cs)).Select(x => x.pi).ToList();
            }

            static Dictionary<Node, QuickGraph.Graphviz.Dot.GraphvizPoint> pd = new Dictionary<Node, QuickGraph.Graphviz.Dot.GraphvizPoint>();


            public void RenderToFile(string name, List<Edge> markEdges, Dictionary<Node, int> prices, Matching m)
            {
                if (!Program.writeGraphs)
                    return;


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
                        if (n is ImageNode)
                        {
                            x = 9;
                            y = yCounted++ * distanceBetweenPoints;
                        }
                        if (n is PixelNode)
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

                System.IO.File.WriteAllText(@"C:\Users\Kasper\Desktop\Algoritme design\" + name + ".gv", output);
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

        public class ResidualGraph : Graph
        {
            public readonly Node source;
            public readonly Node sink;

            public ResidualGraph(Graph g, Matching matching)
                : base(g.xs, g.ys)
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
                        if (e.Source is PixelNode && e.Target is ImageNode)
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


        static public int RunCount = 0;
        public static List<Image> MinAlgorithm(Image i, List<Image> il)
        {
            RunCount++;
            var tuple = Graph.CreateFromImage(i, il);

            var flowgraph = tuple.Item2;

            int nrOfEdges = flowgraph.Edges.Count();

            var M = new Matching();

            var p = new Dictionary<Node, int>();
            flowgraph.RenderToFile(RunCount + "it0", null, null, null);

            flowgraph.xs.ToList().ForEach(x => p.Add(x, 0));
            flowgraph.ys.ToList().ForEach(y => p.Add(y, int.MaxValue));
            flowgraph.Edges.ToList().ForEach(x => p[x.Target] = Math.Min(p[x.Target], x.Tag));
            int cnt = 1;
            while (M.Count != tuple.Item1.Count)
            {
                var Gm = new ResidualGraph(flowgraph, M);
                Gm.RenderToFile(RunCount + "it" + cnt++, null, p, M);

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
                else throw new Exception("no path");

            }
            var endGm = new ResidualGraph(flowgraph, M);
            endGm.RenderToFile(RunCount + "it" + cnt++, null, p, M);


            var rl = new Image[i.list.Count];
            foreach (var m in M.edges)
            {
                if (((PixelNode)m.Value.Source).p != Graph.fakePixel)
                    rl[i.list.IndexOf(((PixelNode)m.Value.Source).p)] = ((ImageNode)m.Value.Target).i;
            }


            System.Console.WriteLine("WE SAVED: " + (il.Count - tuple.Item1.Count) + " VERTICES <3 Caps");
            countSavedVertices += il.Count - tuple.Item1.Count;
            System.Console.WriteLine("WE SAVED: " + (Math.Pow(Program.noImages, 2) - nrOfEdges) + " EDGES!!");
            countSavedEdges += (int)Math.Pow(Program.noImages, 2) - nrOfEdges;
            return rl.ToList();
        }
        public static int countSavedVertices = 0;
        public static int countSavedEdges = 0;
        public static List<Image> MinAlgorithmOrg(Image i, List<Image> il)
        {
            RunCount++;


            var flowgraph = Graph.CreateFromImageOrg(i, il);

            var M = new Matching();

            var p = new Dictionary<Node, int>();
            flowgraph.RenderToFile(RunCount + "it0", null, null, null);

            flowgraph.xs.ToList().ForEach(x => p.Add(x, 0));
            flowgraph.ys.ToList().ForEach(y => p.Add(y, int.MaxValue));
            flowgraph.Edges.ToList().ForEach(x => p[x.Target] = Math.Min(p[x.Target], x.Tag));
            int cnt = 1;
            while (M.Count != il.Count)
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

            }

            var _Gm = new ResidualGraph(flowgraph, M);
            _Gm.RenderToFile(RunCount + "it" + cnt++, null, p, M);

            var rl = new Image[i.list.Count];
            foreach (var m in M.edges)
            {
                if (((PixelNode)m.Value.Source).p != Graph.fakePixel)
                    rl[i.list.IndexOf(((PixelNode)m.Value.Source).p)] = ((ImageNode)m.Value.Target).i;
            }

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