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
            // Bandwidth в мегабитах/с
            private double Bandwidth;
            private double currentFlow;
            private Vertex startVertex;
            private Vertex endVertex;
            private MainWindow.GraphBranch visualEdge;
            private double mu=1; //Needed for the delay calculations later, better ask Gostev
            private double messageIntensity=1; //Per second

            private bool isDirected;
            private bool toSecond=true;

            public bool ToSecond=>toSecond;

            public Vertex GetStartVertex() => startVertex;
            public Vertex GetEndVertex() => endVertex;
            public void SetBandwidth(double maxLoad) => this.Bandwidth = maxLoad;
            public double GetBandwidth() => Bandwidth;
            public void AddFlow(double addedLoad)
            {
                if (currentFlow + addedLoad <= Bandwidth)
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
            public double GetDelay()
            {
                return (double)(1 / mu * ((Bandwidth * 1048576) - (messageIntensity / mu)));
            }
            public Edge(Vertex startVertex, Vertex endVertex, double Bandwidth = 0, double currentFlow = 0, 
                        bool isDirected = false, MainWindow.GraphBranch graphBranch = null)
            {
                this.isDirected = isDirected;
                this.startVertex = startVertex;
                this.endVertex = endVertex;
                this.currentFlow = currentFlow;
                this.Bandwidth = Bandwidth;
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
        private List<double> possibleBandwidths = new List<double>{ 2560, 3686, 4096, 5120, 8192, 10240, 10557, 13107, 13967, 16384,
            20480, 24576,25221, 25600,26214, 32768, 40960, 42240, 49152,51200, 54886, 55848, 81920, 98304, 102400,
            126720, 163348, 167567, 204800, 307200, 409600, 614400 };

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
        public Graph(List<Vertex> vertices, List<Edge> edges, double[][] loadMatrix, string name = "")
        {
            this.vertices = vertices;
            this.edges = edges;
            this.name = name;
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
        public void InfinitizeBandwidths() => edges.ForEach(edge => edge.SetBandwidth(possibleBandwidths.Max() + 1));

        public void NewTempFlowsBasedOnLoadMatrix() => NewTempFlowsBasedOnLoadMatrix(loadMatrix);
        
        public void NewTempFlowsBasedOnLoadMatrix(double[][] newLoadMatrix)
        {
            InfinitizeBandwidths();
            foreach (Edge edge in edges)
                edge.RemoveFlow(edge.GetCurrentFlow());
            for (int i = 0; i < newLoadMatrix.Length; i++)
                for (int j = 0; j < newLoadMatrix[i].Length; j++) 
                    AddFlow(newLoadMatrix[i][j], vertices[i], vertices[j]);
            SuggestMinimalBandwidthsBasedOnTempLoads();
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
            SuggestMinimalBandwidthsBasedOnTempLoads();
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
        private void SuggestMinimalBandwidthsBasedOnTempLoads()
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
        public void AddBandwidthsToList(double newStandart)
        {
            possibleBandwidths.Add(newStandart);
        }
        public void RemoveBandwidthsFromList(double oldStandart)
        {
            possibleBandwidths.Remove(oldStandart);
        }
        public void ChangeBandwidth(Edge edge, double newBandwidth)
        {
            edges[edges.IndexOf(edge)].SetBandwidth(newBandwidth);
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
                if (GetEdge(path[i], path[i + 1]) == null) throw new Exception("No edge between points: (" + path[i].GetXCoordinate() + ", " + path[i].GetYCoordinate() 
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
            double delaySum=0.0;
            foreach(Edge e in edges)
            {
                delaySum += e.GetDelay();
            }
            return delaySum/edges.Count;
        }
        public double GetMaxDelay()
        {
            double maxDelay = 0.0;
            foreach (Edge e in edges)
            {
                if (e.GetDelay() > maxDelay)
                {
                    maxDelay = e.GetDelay();
                }
            }
            return maxDelay;
        }
    }
}