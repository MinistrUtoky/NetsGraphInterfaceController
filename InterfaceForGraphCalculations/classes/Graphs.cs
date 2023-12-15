using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using static InterfaceForGraphCalculations.MainWindow.GraphPoint;
using static InterfaceForGraphCalculations.MainWindow.GraphBranch;
using System.Windows.Shapes;
//using MathNet.Numerics.Distributions;

namespace InterfaceForGraphCalculations.classes
{
    public class Graph
    {
        public class Vertex
        {
            private float xCoordinate;
            private float yCoordinate;
            private string name;
            private float dataPassthroughModifier;
            private int index;
            private MainWindow.GraphPoint visualPoint;

            public Vertex(float xCoordinate, float yCoordinate, float dataPassthroughModifier = 1, string name = "", int index = -1, MainWindow.GraphPoint visualPoint = null)
            {
                this.dataPassthroughModifier = dataPassthroughModifier;
                this.xCoordinate = xCoordinate;
                this.yCoordinate = yCoordinate;
                this.name = name;
                this.index = index;
                this.visualPoint = visualPoint;
            }
            public float GetXCoordinate() => xCoordinate;
            public float GetYCoordinate() => yCoordinate;
            public void SetXCoordinate(float xCoordinate) => this.xCoordinate = xCoordinate;
            public void SetTCoordinate(float yCoordinate) => this.yCoordinate = yCoordinate;
            
            public void SetCoordinates(int xCoordinate, int yCoordinate)
            {
                this.xCoordinate = xCoordinate;
                this.yCoordinate = yCoordinate;
            }

            public string GetName() => name;
            public void SetName(string name) => this.name = name;
            public void SetDataPassthroughModifier(float dataPassthroughModifier) => this.dataPassthroughModifier = dataPassthroughModifier;
            public float GetDataPassthroughModifier() => dataPassthroughModifier;
            public double GetDistance(Vertex otherVertex) => Math.Sqrt(Math.Pow((otherVertex.GetXCoordinate() - this.GetXCoordinate()), 2) + Math.Pow((otherVertex.GetYCoordinate() - this.GetYCoordinate()), 2));
            public int GetIndex() => index;
            public void SetIndex(int index) => this.index = index;           
            public MainWindow.GraphPoint GetVisualVertex() => visualPoint;
            public void SetVisualVertex(MainWindow.GraphPoint gp) => visualPoint = gp;
        }
        public class Edge
        {
            // bandwidth в мегабитах/с
            private double bandwidth;
            private double currentFlow;
            private Vertex startVertex;
            private Vertex endVertex;
            private MainWindow.GraphBranch visualEdge;
            private double length; //в километрах

            private bool isDirected;
            private bool toSecond=true;

            public bool ToSecond=>toSecond;

            public Vertex GetStartVertex() => startVertex;
            public Vertex GetEndVertex() => endVertex;
            public void SetBandwidth(double maxLoad) => this.bandwidth = maxLoad;
            public double GetBandwidth() => bandwidth;
            public void AddFlow(double addedLoad)
            {
                if (currentFlow + addedLoad <= bandwidth)
                    currentFlow += addedLoad;
            }
            public void RemoveFlow(double removableLoad)
            {
                if (currentFlow >= removableLoad)
                    currentFlow -= removableLoad;
            }
            public double GetCurrentFlow() => currentFlow;
            public bool GetIsDirected() => isDirected;
            public void SetIsDirected(bool isDirected) => this.isDirected = isDirected;
            public void SetToSecond(bool toSecond) => this.toSecond = toSecond;         
            public MainWindow.GraphBranch GetVisualEdge() => visualEdge;
            public void SetVisualEdge(MainWindow.GraphBranch ge) => visualEdge = ge;
            public double GetDelay(double messageLength)
            {
                return (double)(messageLength / 2 * (1 / (bandwidth * 1048576) + 1 / ((bandwidth * 1048576) - (currentFlow * 1048576))));
            }
            public double GetPrice(Dictionary<double, double> prices, double constructionPrice)
            {
                return length * constructionPrice + prices[bandwidth];
            }
            public Edge(Vertex startVertex, Vertex endVertex, double bandwidth = 0, double currentFlow = 0, 
                        bool isDirected = false, MainWindow.GraphBranch graphBranch = null)
            {
                this.isDirected = isDirected;
                this.startVertex = startVertex;
                this.endVertex = endVertex;
                this.currentFlow = currentFlow;
                this.bandwidth = bandwidth;
            }
        }

        private List<Vertex> vertices;
        private List<Edge> edges;
        private double[][] adjacencyMatrix;
        private double[][] loadMatrix;
        private double[][] fastestPathsMatrix;
        private double[][] dist;
        private int[][] prev;
        private double[][] tempFlows;
        private string name;
        private double messageLength;
        private double constructionPrice;
        private List<double> possibleBandwidths = new List<double>{
            10, 100, 1024, 10240
        };
        private Dictionary<double, double> prices = new Dictionary<double, double>(){
            {10, 6500}, {100, 12500}, {1024, 40000}, {10240, 90000}
        };

        public Graph()
        {
            vertices = new List<Vertex>();
            edges = new List<Edge>();
            adjacencyMatrix = new double[0][];
            loadMatrix = new double[0][];
            dist = new double[0][];
            prev = new int[0][];
            tempFlows = new double[0][];
        }
        public Graph(List<Vertex> vertices, List<Edge> edges, double[][] loadMatrix, double messageLength, string name = "", double constructionPrice = 17500)
        {
            this.vertices = vertices;
            this.edges = edges;
            this.name = name;
            this.messageLength = messageLength;
            adjacencyMatrix = new double[vertices.Count()][];
            this.loadMatrix = new double[vertices.Count()][];
            dist = new double[vertices.Count()][];
            prev = new int[vertices.Count()][];
            tempFlows = new double[vertices.Count()][];

            for (int i = 0; i < vertices.Count(); i++)
            {
                fastestPathsMatrix[i] = new double[vertices.Count()];
                adjacencyMatrix[i] = new double[vertices.Count()];
                this.loadMatrix[i] = new double[vertices.Count()];
                tempFlows[i] = new double[vertices.Count()];
                dist[i] = new double[vertices.Count()];
                prev[i] = new int[vertices.Count()];
                for (int j = 0; j < vertices.Count(); j++)
                {
                    this.loadMatrix[i][j] = loadMatrix[i][j];
                    adjacencyMatrix[i][j] = 0;
                    tempFlows[i][j] = 0;
                    prev[i][j] = -1;
                    dist[i][j] = Double.PositiveInfinity;
                }

            }
            foreach (Edge i in edges)
                adjacencyMatrix[vertices.IndexOf(i.GetStartVertex())][vertices.IndexOf(i.GetEndVertex())] = 1;
            GenerateAllRoutes(); NewTempFlowsBasedOnLoadMatrix();
            this.name = name;
            this.messageLength = messageLength;
            this.constructionPrice = constructionPrice;
        }
        public void AddEdge(Vertex Vertex1, Vertex Vertex2, float maxLoad, bool isDirected, float currentLoad = 0)
        {
            edges.Add(new Edge(Vertex1, Vertex2, maxLoad, currentLoad, isDirected));
            ReformAdjacencyMatrix();
            GenerateAllRoutes(); NewTempFlowsBasedOnLoadMatrix();
        }
        public void AddEdge(Edge edge)
        {
            edges.Add(edge);
            ReformAdjacencyMatrix();
            GenerateAllRoutes();
        }
        public void AddVertex(Vertex newVertex)
        {
            vertices.Add(newVertex);
            SyncVertexIndex(); 
        }

        public List<Vertex> GetAdjacentVertices(int vertexNumber)
        {
            if (vertexNumber < 0 || vertexNumber >= this.vertices.Count()) throw new ArgumentOutOfRangeException("Cannot access vertex");

            List<Vertex> adjacentVertices = new List<Vertex>();
            for (int i = 0; i < this.vertices.Count(); i++)
            {
                if (this.adjacencyMatrix[vertexNumber][i] > 0)
                    adjacentVertices.Add(vertices[i]);
            }
            return adjacentVertices;
        }
        public double GetShortestPathLength(Vertex vert1, Vertex vert2) => fastestPathsMatrix[vertices.IndexOf(vert1)][vertices.IndexOf(vert2)];
        private void GenerateAllRoutes()
        {
            for (int i = 0; i < vertices.Count(); i++)
                for (int j = 0; j < vertices.Count(); j++)
                {
                    if (adjacencyMatrix[i][j] != 0)
                    {
                        dist[i][j] = adjacencyMatrix[i][j];
                        prev[i][j] = i;
                    }
                    if (i == j)
                    {
                        dist[i][j] = 0;
                        prev[i][j] = i;
                    }
                }
            for (int k = 0; k < vertices.Count(); k++)
                for (int u = 0; u < vertices.Count(); u++)
                    for (int v = 0; v < vertices.Count(); v++)
                        if (dist[u][v] > dist[u][k] + dist[k][v])
                        {
                            dist[u][v] = dist[u][k] + dist[k][v];
                            prev[u][v] = prev[k][v];
                        }
        }
        public List<Vertex> GetPath(Vertex vert1, Vertex vert2)
        {
            List<Vertex> path = new List<Vertex>();
            GenerateAllRoutes();
            path.Add(vert1);
            int v = vert1.GetIndex(), u = vert2.GetIndex();
            if (prev[u][v] == -1)
                return path;
            while (u != v)
            {
                v = prev[u][v];
                if (v == -1) return new List<Vertex>();
                path.Add(vertices[v]);
            }
            path.Reverse();
            return path;
        }
        public void Infinitizebandwidths() => edges.ForEach(edge => edge.SetBandwidth(possibleBandwidths.Max() + 1));

        public void NewTempFlowsBasedOnLoadMatrix() => NewTempFlowsBasedOnLoadMatrix(loadMatrix);
        
        public void NewTempFlowsBasedOnLoadMatrix(double[][] newLoadMatrix)
        {
            Infinitizebandwidths();
            foreach (Edge edge in edges)
                edge.RemoveFlow(edge.GetCurrentFlow());
            for (int i = 0; i < newLoadMatrix.Length; i++)
                for (int j = 0; j < newLoadMatrix[i].Length; j++) 
                    AddFlow(newLoadMatrix[i][j], vertices[i], vertices[j]);
            SuggestMinimalbandwidthsBasedOnTempLoads();
        }
        private void SyncVertexIndex()
        {
            int i = 0;
            foreach (Vertex v in vertices)
            {
                v.SetIndex(i);
                i++;
            }
            ReformAdjacencyMatrix();
            GenerateAllRoutes(); NewTempFlowsBasedOnLoadMatrix();
        }
        public Edge GetEdge(Vertex vert1, Vertex vert2)
        {
            foreach (Edge edge in edges)
            {
                if ((edge.GetStartVertex() == vert2 & edge.GetEndVertex() == vert1)
                     || (edge.GetStartVertex() == vert1 & edge.GetEndVertex() == vert2)) return edge;
            }
            return null;
        }
        public void RemoveVertex(Vertex vert)
        {
            vertices.Remove(vert);
            foreach (Vertex i in vertices)
                if (GetEdge(vert, i) != null)
                {
                    edges.Remove(GetEdge(vert, i));
                    if (GetEdge(i, vert) != null)
                        edges.Remove(GetEdge(i, vert));
                }
            SyncVertexIndex();
        }
        public void RemoveEdge(Edge edge)
        {
            edges.Remove(edge);
            ReformAdjacencyMatrix();
            GenerateAllRoutes(); NewTempFlowsBasedOnLoadMatrix();
        }
        public void RemoveEdge(Vertex vert1, Vertex vert2)
        {
            edges.Remove(GetEdge(vert1, vert2));
            ReformAdjacencyMatrix();
            GenerateAllRoutes(); NewTempFlowsBasedOnLoadMatrix();
            SuggestMinimalbandwidthsBasedOnTempLoads();
        }
        public void ReformAdjacencyMatrix()
        {
            if (vertices.Count() != 0)
            {
                dist = new double[vertices.Count()][];
                prev = new int[vertices.Count()][];
                adjacencyMatrix = new double[vertices.Count()][];
                for (int i = 0; i < vertices.Count(); i++)
                {
                    dist[i] = new double[vertices.Count()];
                    prev[i] = new int[vertices.Count()];
                    adjacencyMatrix[i] = new double[vertices.Count()];
                    for (int j = 0; j < vertices.Count(); j++)
                    {
                        adjacencyMatrix[i][j] = 0;
                        dist[i][j] = Double.PositiveInfinity;
                        prev[i][j] = -1;
                    }

                }
                foreach (Edge i in edges)
                {
                    if (!i.GetIsDirected())
                    {
                        adjacencyMatrix[vertices.IndexOf(i.GetStartVertex())][vertices.IndexOf(i.GetEndVertex())] = 1;
                        adjacencyMatrix[vertices.IndexOf(i.GetEndVertex())][vertices.IndexOf(i.GetStartVertex())] = 1;
                    }
                    else if (!i.ToSecond)
                        adjacencyMatrix[vertices.IndexOf(i.GetStartVertex())][vertices.IndexOf(i.GetEndVertex())] = 1;
                    else
                        adjacencyMatrix[vertices.IndexOf(i.GetEndVertex())][vertices.IndexOf(i.GetStartVertex())] = 1;
                }
            }
            else
            {
                adjacencyMatrix = new double[0][];
                dist = new double[0][];
                prev = new int[0][];
                return;
            }

        }
        private void SuggestMinimalbandwidthsBasedOnTempLoads()
        {
            for (int i = 0; i < vertices.Count(); i++)
                for (int j = 0; j < vertices.Count(); j++)
                    foreach (int bandwidth in possibleBandwidths)
                        if (GetEdge(vertices[i], vertices[j]) != null) 
                            if (bandwidth >= GetEdge(vertices[i], vertices[j]).GetCurrentFlow())
                            {
                                GetEdge(vertices[i], vertices[j]).SetBandwidth(bandwidth);
                                break;
                            }
            tempFlows = new double[0][];
        }
        public void AddbandwidthsToList(double newStandart)
        {
            possibleBandwidths.Add(newStandart);
        }
        public void RemovebandwidthsFromList(double oldStandart)
        {
            possibleBandwidths.Remove(oldStandart);
        }
        public void Changebandwidth(Edge edge, double newbandwidth)
        {
            edges[edges.IndexOf(edge)].SetBandwidth(newbandwidth);
        }
        public void AddFlow(Edge edge, double additionalLoad)
        {
            edge.AddFlow(additionalLoad);
        }
        public void AddFlow(double extraFlow, Vertex vert1, Vertex vert2)
        {
            GenerateAllRoutes();
            List<Vertex> path = GetPath(vert1, vert2);
            for (int i = 0; i < path.Count() - 1; i++)
            {
                if (GetEdge(path[i], path[i + 1]) == null) 
                    throw new Exception("No edge between points: (" + path[i].GetXCoordinate() + ", " + path[i].GetYCoordinate() 
                                        + ") and (" + path[i+1].GetXCoordinate() + ", " + path[i + 1].GetYCoordinate() + ")");
                GetEdge(path[i], path[i + 1]).AddFlow(extraFlow);
            }
        }
        public void ClearGraph()
        {
            foreach (Vertex v in vertices)
                RemoveVertex(v);
            ReformAdjacencyMatrix();
            loadMatrix = new double[0][];
            tempFlows = new double[0][];
        }
        public double GetAverageDelay()
        {
            double delaySum = 0.0;
            foreach (Edge e in edges)
            {
                delaySum += e.GetDelay(messageLength);
            }
            return delaySum / edges.Count;
        }
        public double GetMaxDelay()
        {
            double maxDelay = GetMaxDelayEdge().GetDelay(messageLength);
            return maxDelay;
        }
        public Edge GetMaxDelayEdge()
        {
            double maxDelay = 0.0;
            Edge maxEdge = null;
            foreach (Edge e in edges)
            {
                if (e.GetDelay(messageLength) > maxDelay)
                {
                    maxDelay = e.GetDelay(messageLength);
                    maxEdge = e;
                }
            }
            if (maxEdge == null) throw new Exception("No load matrix passage");
            return maxEdge;
        }
        public double GetPathDelay (Vertex vert1, Vertex vert2)
        {
            GenerateAllRoutes();
            double pathDelaySum = 0.0;
            List<Vertex> path = GetPath(vert1, vert2);
            for (int i = 0; i < path.Count() - 1; i++)
            {
                if (GetEdge(path[i], path[i + 1]) == null) throw new Exception("No edge between points: (" + path[i].GetXCoordinate() + ", " + path[i].GetYCoordinate()
                                                                               + ") and (" + path[i + 1].GetXCoordinate() + ", " + path[i + 1].GetYCoordinate() + ")");
                pathDelaySum += GetEdge(path[i], path[i + 1]).GetDelay(messageLength);
            }
            return pathDelaySum;
        }
        public Edge GetMaxPathDelayEdge(Vertex vert1, Vertex vert2)
        {
            GenerateAllRoutes();
            double maxPathDelay = 0.0;
            Edge maxDelayEdge= null;
            List<Vertex> path = GetPath(vert1, vert2);
            for (int i = 0; i < path.Count() - 1; i++)
            {
                if (GetEdge(path[i], path[i + 1]) == null) throw new Exception("No edge between points: (" + path[i].GetXCoordinate() + ", " + path[i].GetYCoordinate()
                                                                               + ") and (" + path[i + 1].GetXCoordinate() + ", " + path[i + 1].GetYCoordinate() + ")");
                if (GetEdge(path[i], path[i + 1]).GetDelay(messageLength) > maxPathDelay)
                {
                    maxPathDelay = GetEdge(path[i], path[i + 1]).GetDelay(messageLength);
                    maxDelayEdge = GetEdge(path[i], path[i + 1]);
                }
            }
            return (maxDelayEdge);
        }
        public double GetMaxPathDelay(Vertex vert1, Vertex vert2)
        {
            GenerateAllRoutes();
            double maxPathDelay = 0.0;
            Edge maxDelayEdge = null;
            List<Vertex> path = GetPath(vert1, vert2);
            for (int i = 0; i < path.Count() - 1; i++)
            {
                if (GetEdge(path[i], path[i + 1]) == null) throw new Exception("No edge between points: (" + path[i].GetXCoordinate() + ", " + path[i].GetYCoordinate()
                                                                               + ") and (" + path[i + 1].GetXCoordinate() + ", " + path[i + 1].GetYCoordinate() + ")");
                if (GetEdge(path[i], path[i + 1]).GetDelay(messageLength) > maxPathDelay)
                {
                    maxPathDelay = GetEdge(path[i], path[i + 1]).GetDelay(messageLength);
                    maxDelayEdge = GetEdge(path[i], path[i + 1]);
                }
            }
            if (maxDelayEdge == null) throw new Exception("No max delay edge");
            return (maxDelayEdge.GetDelay(messageLength));
        }
        public double GetAveragePathDelay(Vertex vert1, Vertex vert2)
        {
            GenerateAllRoutes();
            List<Vertex> path = GetPath(vert1, vert2);
            return (GetPathDelay(vert1,vert2) / path.Count);
        }
        public List<Vertex> GetMaxMaxDelayPath()
        {
            GenerateAllRoutes();
            Tuple<int, int> maxDelayPath = new Tuple<int, int>(-1, -1);
            double maxMaxDelay = 0.0;
            List<Tuple<int, int>> paths = new List<Tuple<int, int>>();
            for (int i = 0; i < loadMatrix.Length; i++)
            {
                for (int j = 0; j < loadMatrix.Length; j++)
                {
                    if (loadMatrix[i][j] != 0)
                    {
                        paths.Add(new Tuple<int, int>(i, j));
                    }
                }
            }
            if (paths == new List<Tuple<int, int>>()) throw new Exception("There are no valid paths");
            foreach (var path in paths)
            {
                if (GetMaxPathDelayEdge(vertices[path.Item1], vertices[path.Item2]).GetDelay(messageLength) > maxMaxDelay)
                {
                    maxMaxDelay = GetMaxPathDelayEdge(vertices[path.Item1], vertices[path.Item2]).GetDelay(messageLength);
                    maxDelayPath = new Tuple<int, int>(path.Item1, path.Item2);
                }
            }
            if (maxDelayPath.Item1 == -1 || maxDelayPath.Item2 == -1) throw new Exception("There is no valid max max delay path");
            return new List<Vertex> { vertices[maxDelayPath.Item1], vertices[maxDelayPath.Item2] };
        }
        public List<Vertex> GetMaxAverageDelayPath()
        {
            GenerateAllRoutes();
            Tuple<int, int> maxAverageDelayPath = new Tuple<int, int>(-1, -1);
            double maxAverageDelay = 0.0;
            List<Tuple<int, int>> paths = new List<Tuple<int, int>>();
            for (int i = 0; i < loadMatrix.Length; i++)
            {
                for (int j = 0; j < loadMatrix.Length; j++)
                {
                    if (loadMatrix[i][j] != 0)
                    {
                        paths.Add(new Tuple<int, int>(i, j));
                    }
                }
            }
            if (paths == new List<Tuple<int, int>>()) throw new Exception("There are no valid paths");
            foreach (var path in paths)
            {
                if (GetAveragePathDelay(vertices[path.Item1], vertices[path.Item2]) > maxAverageDelay)
                {
                    maxAverageDelay = GetAveragePathDelay(vertices[path.Item1], vertices[path.Item2]);
                    maxAverageDelayPath = new Tuple<int, int>(path.Item1, path.Item2);
                }
            }
            if (maxAverageDelayPath.Item1 == -1 || maxAverageDelayPath.Item2 == -1) throw new Exception("There is no valid max average delay path");
            return new List<Vertex> { vertices[maxAverageDelayPath.Item1], vertices[maxAverageDelayPath.Item2] }; 
        }
        public List<Vertex> GetPathWithMaxDelay(Vertex vert1, Vertex vert2)
        {
            GenerateAllRoutes();
            Tuple<int, int> maxDelayPath = new Tuple<int, int>(-1, -1);
            List<Tuple<int, int>> paths = new List<Tuple<int, int>>();
            for (int i = 0; i < loadMatrix.Length; i++)
            {
                for (int j = 0; j < loadMatrix.Length; j++)
                {
                    if (loadMatrix[i][j] != 0)
                    {
                        paths.Add(new Tuple<int, int>(i, j));
                    }
                }
            }
            if (paths == new List<Tuple<int, int>>()) throw new Exception("There are no valid paths");
            double maxDelay = 0.0;
            foreach (var path in paths)
            {
                if (GetPathDelay(vertices[path.Item1], vertices[path.Item2]) > maxDelay)
                {
                    maxDelay = GetPathDelay(vertices[path.Item1], vertices[path.Item2]);
                    maxDelayPath = new Tuple<int, int>(path.Item1, path.Item2);
                }
            }
            return new List<Vertex> { vertices[maxDelayPath.Item1], vertices[maxDelayPath.Item2] };
        }
        public void SetPrice(double key, double newPrice)
        {
            prices[key] = newPrice;
        }
        public void SetPrice(Edge e, double newPrice)
        {
            prices[e.GetBandwidth()] = newPrice;
        }
        public double GetPrice(double key)
        {
            return prices[key];
        }
        public double GetPrice(Edge e)
        {
            return e.GetPrice(prices, constructionPrice);
        }
        public void AddNewBandwidth(double bandwidth, double price)
        {
            prices.Add(bandwidth, price);
        }
        public void RemoveBandwidth(double bandwidth)
        {
            possibleBandwidths.Remove(bandwidth);
            prices.Remove(bandwidth);
        }
        public void SetConstructionPrice(double newPrice)
        {
            constructionPrice = newPrice;
        }
        public double GetConstructionPrice()
        {
            return constructionPrice;
        }
        public double GetTotalPrice()
        {
            double totalPrice = 0.0;
            foreach (Edge e in edges)
            {
                totalPrice += e.GetPrice(prices, constructionPrice);
            }
            return totalPrice;
        }
        public double GetMessageLength() => messageLength;
        public void SetMessageLength(double length) => messageLength = length;
    }
}