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

namespace InterfaceForGraphCalculations.classes
{
    internal class Graph
    {
        public class Vertex
        {
            private float xCoordinate;
            private float yCoordinate;
            private string name;
            private float dataPassthroughModifier;
            private int index;

            public Vertex(float xCoordinate, float yCoordinate, float dataPassthroughModifier = 1, string name = "", int index = -1)
            {
                this.dataPassthroughModifier = dataPassthroughModifier;
                this.xCoordinate = xCoordinate;
                this.yCoordinate = yCoordinate;
                this.name = name;
                this.index = index;
            }
            public float GetXCoordinate()
            {
                return xCoordinate;
            }
            public float GetYCoordinate()
            {
                return yCoordinate;
            }
            public void SetXCoordinate(float xCoordinate)
            {
                this.xCoordinate = xCoordinate;
            }
            public void SetTCoordinate(float yCoordinate)
            {
                this.yCoordinate = yCoordinate;
            }
            public void SetCoordinates(int xCoordinate, int yCoordinate)
            {
                this.xCoordinate = xCoordinate;
                this.yCoordinate = yCoordinate;
            }

            public string GetName()
            {
                return name;
            }
            public void SetName(string name)
            {
                this.name = name;
            }
            public void SetDataPassthroughModifier(float dataPassthroughModifier)
            {
                this.dataPassthroughModifier = dataPassthroughModifier;
            }
            public float GetDataPassthroughModifier()
            {
                return dataPassthroughModifier;
            }
            public double GetDistance(Vertex otherVertex)
            {
                return Math.Sqrt(Math.Pow((otherVertex.GetXCoordinate() - this.GetXCoordinate()), 2) + Math.Pow((otherVertex.GetYCoordinate() - this.GetYCoordinate()), 2));
            }
            public int GetIndex()
            {
                return index;
            }
            public void SetIndex(int index)
            {
                this.index = index;
            }

        }
        public class Edge
        {
            // Bandwidth в мегабитах/с
            private double Bandwidth;
            private double currentFlow;
            private Vertex startVertex;
            private Vertex endVertex;
            
            private bool isDirected;

            public Edge(Vertex startVertex, Vertex endVertex, double Bandwidth=0, double currentFlow=0, bool isDirected=false) 
            {
                this.isDirected = isDirected;
                this.startVertex = startVertex;
                this.endVertex = endVertex;
                this.currentFlow = currentFlow;
                this.Bandwidth = Bandwidth;
            }
            public Vertex GetStartVertex()
            {
                return startVertex;
            }
            public Vertex GetEndVertex()
            {
                return endVertex;
            }
            public void SetStartVertexNumber(Vertex startVertex)
            {
                this.startVertex = startVertex;
            }
            public void SetEndVertexNumber(Vertex endVertex)
            {
                this.endVertex = endVertex;
            }
            public void SetBandwidth(double maxLoad)
            {
                this.Bandwidth = maxLoad;
            }
            public double GetBandwidth()
            {
                return Bandwidth;
            }
            public void AddFlow(double addedLoad)
            {
                if (currentFlow + addedLoad <= Bandwidth) 
                {
                    currentFlow += addedLoad;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Out of range");
                }
            }
            public void RemoveFlow(double removableLoad)
            {
                if (currentFlow >= removableLoad)
                {
                    currentFlow-=removableLoad;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Out of range");
                }
            }
            public double GetCurrentFlow()
            {
                return currentFlow;
            }
            public bool GetDirection()
            {
                return isDirected;
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
        private List<double> possibleBandwidths= new List<double>{ 2560, 3686, 4096, 5120, 8192, 10240, 10557, 13107, 13967, 16384,
            20480, 24576,25221, 25600,26214, 32768,40960, 42240, 49152,51200, 54886, 55848, 81920, 98304, 102400,
            126720, 163348, 167567, 204800,307200, 409600,614400 };

        public Graph() {
            vertices = new List<Vertex>();
            edges = new List<Edge>();
        }
        public Graph(List<Vertex> vertices=null, double[][]loadMatrix=null, List<Edge> edges= null, string name = "")
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
                this.loadMatrix[i]=new double[vertices.Count()];
                tempFlows[i]= new double[vertices.Count()];
                dist[i]= new double[vertices.Count()];
                prev[i]=new int[vertices.Count()];
                for (int j = 0; j < vertices.Count(); j++)
                {
                    this.loadMatrix[i][j] =loadMatrix[i][j];
                    adjacencyMatrix[i][j] = 0;
                    tempFlows[i][j] = 0;
                    prev[i][j] = 0;
                    dist[i][j] = Double.PositiveInfinity;
                }
                
            }
            foreach (Edge i in edges)
            {
                adjacencyMatrix[vertices.IndexOf(i.GetStartVertex())][vertices.IndexOf(i.GetEndVertex())] = 1;
                if (!i.GetDirection())
                {
                    adjacencyMatrix[vertices.IndexOf(i.GetEndVertex())][vertices.IndexOf(i.GetStartVertex())] =1;
                }
            }
            GenerateAllRoutes();
            this.name = name;
        }
        public void AddEdge(Vertex Vertex1, Vertex Vertex2, float maxLoad, bool isDirected,float currentLoad=0)
        {
            edges.Append(new Edge(Vertex1, Vertex2,maxLoad, currentLoad,isDirected));
            ReformAdjacencyMatrix();
            GenerateAllRoutes();
        }
        public double[][] GetAdjacencyMatrix()
        {
            return adjacencyMatrix;
        }
        public double[] GetAdjecencyMatrix(int numberOfPoint)
        {
            return adjacencyMatrix[numberOfPoint];
        }
        public void AddVertex(Vertex newVertex)
        {
            vertices.Append(newVertex);
            SyncVertexIndex();
        }
        public List<Vertex> GetVertices()
        {
            return vertices;
        }
        public Vertex GetVertices(int number)
        {
            return vertices[number];
        }
        public List<Edge> GetEdges()
        {
            return edges;
        }
        public Edge GetEdge(int number)
        {
            return edges[number];
        }
        public void SetVertexCoordinates(int PointNumber, int x, int y)
        {
            vertices[PointNumber].SetCoordinates(x, y);
        }
        public IEnumerable<Vertex> GetAdjacentVertices(int vertexNumber)
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
        public string GetName()
        {
            return name;
        }
        public void SetName(string name)
        {
            this.name = name;
        }
        
        public double GetShortestPathLength(Vertex vert1, Vertex vert2)
        {
            return fastestPathsMatrix[vertices.IndexOf(vert1)][vertices.IndexOf(vert2)];
        }
        

        private void GenerateAllRoutes()
        {
            for(int i = 0; i < vertices.Count(); i++)
            {
                for (int j =0; j < vertices.Count(); j++)
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
            }
            for(int k = 0; k < vertices.Count(); k++)
            {
                for(int u = 0; u < vertices.Count(); u++)
                {
                    for(int v=0; v < vertices.Count(); v++)
                    {
                        if (dist[u][v] > dist[u][k] + dist[k][v])
                        {
                            dist[u][v] = dist[u][k] + dist[k][v];
                            prev[u][v] = prev[k][v];
                        }
                    }
                }
            }
        }
        public List<int> GetPath(Vertex vert1, Vertex vert2)
        {
            List<int> path = new List<int>();
            if (prev[vert1.GetIndex()][vert2.GetIndex()]==0)
            {
                return null;
            }
            int v=vert1.GetIndex(), u=vert2.GetIndex();
            while (u != v)
            {
                v = prev[u][v];
                path.Prepend(v);
            }
            
            return path;
        }


        public void CreateTempFlowsBasedOnPaths()
        {
            foreach(Edge e in edges)
            {
                for (int i = 0; i < GetPath(e.GetStartVertex(), e.GetEndVertex()).Count()-1; i++)
                {
                    tempFlows[i][i + 1] +=loadMatrix[i][i+1];
                    if (!e.GetDirection())
                    {
                        tempFlows[i + 1][i] += loadMatrix[i][i + 1];
                    }
                }
            }
        }

        private void SyncVertexIndex()
        {
            int i = 0;
            foreach(Vertex v in vertices)
            {
                v.SetIndex(i);
                i++;
            }
            ReformAdjacencyMatrix();
            GenerateAllRoutes();
        }

        public Edge GetEdge(Vertex vert1,Vertex vert2)
        {
            foreach (Edge edge in edges)
            {
                if ((!edge.GetDirection()&&(edge.GetStartVertex()==vert1&&edge.GetEndVertex()==vert2
                    || edge.GetStartVertex() == vert2 && edge.GetEndVertex() == vert1))
                    ||(edge.GetDirection()&&edge.GetStartVertex()==vert1&&edge.GetEndVertex()==vert2))
                {
                    return edge;
                }
            }
            return null;
        }
        public void RemoveVertex(Vertex vert)
        {
            vertices.Remove(vert);
            foreach(Vertex i in vertices)
            {
                if (GetEdge(vert, i)!=null)
                {
                    edges.Remove(GetEdge(vert, i));
                    if (GetEdge(i, vert) != null)
                    {
                        edges.Remove(GetEdge(i,vert));
                    }
                }
            }
            SyncVertexIndex();
        }
        public void RemoveEdge(Edge edge)
        {
            edges.Remove(edge);
            ReformAdjacencyMatrix();
            GenerateAllRoutes();
        }
        public void RemoveEdge(Vertex vert1, Vertex vert2)
        {
            edges.Remove(GetEdge(vert1, vert2));
            ReformAdjacencyMatrix();
            //GenerateFastestRoutes();
            SuggestMinimalBandwidthsBasedOnTempLoads();
        }
        private void ReformAdjacencyMatrix()
        {
            if (vertices.Count() != 0)
            {
                dist = new double[vertices.Count()][];
                prev = new int[vertices.Count()][];
                adjacencyMatrix = new double[vertices.Count()][];
                for (int i = 0; i < vertices.Count(); i++)
                {
                    dist[i] = new double[vertices.Count()];
                    prev[i]= new int[vertices.Count()];
                    adjacencyMatrix[i] = new double[vertices.Count()];
                    for (int j = 0; j < vertices.Count(); j++)
                    {
                        adjacencyMatrix[i][j] = 0;
                        dist[i][j] = Double.PositiveInfinity;
                        prev[i][j] = 0;
                    }

                }
                foreach (Edge i in edges)
                {
                    adjacencyMatrix[vertices.IndexOf(i.GetStartVertex())][vertices.IndexOf(i.GetEndVertex())] = 1;
                    if (!i.GetDirection())
                    {
                        adjacencyMatrix[vertices.IndexOf(i.GetEndVertex())][vertices.IndexOf(i.GetStartVertex())] = 1;
                    }
                }
            }
            else
            {
                adjacencyMatrix = null;
                dist = null;
                prev= null;
                return;
            }
                
        }
        private void SuggestMinimalBandwidthsBasedOnTempLoads()
        {
            for (int i = 0; i < vertices.Count(); i++)
            {
                for(int j = 0;j < vertices.Count();j++)
                {
                    foreach (int bandwidth in possibleBandwidths)
                    {
                        if (bandwidth >= tempFlows[i][j])
                        {
                            GetEdge(vertices[i], vertices[j]).SetBandwidth(bandwidth);
                        }
                    }
                }
            }
            tempFlows = null;
        }
        public void AddBandwidthsToList(double newStandart )
        {
            possibleBandwidths.Add(newStandart);
        }
        public void RemoveBandwidthsFromList(double oldStandart)
        {
            possibleBandwidths.Remove(oldStandart);
        }
        public void ChangeBandwidth(Edge edge, double newBandwidth)
        {
            edge.SetBandwidth(newBandwidth);
        }
        public void AddFlow(Edge edge,double additionalLoad)
        {
            edge.AddFlow(additionalLoad);
        }
        public void AddFlow(double extraFlow,Vertex vert1,Vertex vert2)
        {
            List<int>path=GetPath(vert1, vert2);
            for(int i = 0; i < path.Count() - 1; i++)
            {
                GetEdge(vertices[i], vertices[i + 1]).AddFlow(extraFlow);
            }
        }
        public void ClearGraph()
        {
            foreach (Vertex v in vertices)
            {
                RemoveVertex(v);   
            }
            ReformAdjacencyMatrix();
            loadMatrix = null;
            tempFlows = null;
        }
        public Vertex GetVertex(int index)
        {
            return vertices[index];
        }
    }
}
