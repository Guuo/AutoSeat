using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeatingOptimizer
{
    public class AdjacencySetGraph
    {
        private HashSet<Node> vertexSet;
        private int numVertices;

        public AdjacencySetGraph(int numVertices)
        {
            this.vertexSet = new HashSet<Node>();
            for (var i = 0; i < numVertices; i++)
            {
                vertexSet.Add(new Node(i));
            }
            this.numVertices = numVertices;
        }
        public AdjacencySetGraph(List<SeatRequest> seatRequests)
        {
            this.vertexSet = new HashSet<Node>();
            var UnappliedEdges = new List<Tuple<int, int>>();
            for (var i = 0; i < seatRequests.Count; i++)
            {
                Node node = new Node(i);
                foreach (int adjacentPerson in seatRequests[i].RequestedAdjacentsId)
                {
                    node.AddEdge(adjacentPerson);
                    UnappliedEdges.Add(new Tuple<int, int>(adjacentPerson, i));
                }
                vertexSet.Add(node);
            }
            this.numVertices = seatRequests.Count;

            foreach (var edge in UnappliedEdges)
                AddEdge(edge.Item1, edge.Item2);
        }
        public void AddEdge(int v1, int v2)
        {
            if (v1 >= this.numVertices || v2 >= this.numVertices || v1 < 0 || v2 < 0)
                throw new ArgumentOutOfRangeException("Vertices are out of bounds");

            this.vertexSet.ElementAt(v1).AddEdge(v2);

            //In an undirected graph all edges are bi-directional
            this.vertexSet.ElementAt(v2).AddEdge(v1);
        }

        public IEnumerable<int> GetAdjacentVertices(int v)
        {
            if (v < 0 || v >= this.numVertices) throw new ArgumentOutOfRangeException("Cannot access vertex");
            return this.vertexSet.ElementAt(v).GetAdjacentVertices();
        }


        public void DFS(int startVertex)
        {
            if (startVertex < 0 || startVertex >= this.numVertices)
                throw new ArgumentOutOfRangeException("Invalid start vertex");

            HashSet<int> visited = new HashSet<int>();
            DFSUtil(startVertex, visited);
        }

        private void DFSUtil(int vertex, HashSet<int> visited)
        {
            // Mark current vertex as visited and process it
            visited.Add(vertex);
            //Console.WriteLine($"Visited vertex: {vertex}");

            // Recursively visit all adjacent vertices
            foreach (int adjacentVertex in GetAdjacentVertices(vertex))
            {
                if (!visited.Contains(adjacentVertex))
                {
                    DFSUtil(adjacentVertex, visited);
                }
            }
        }

        // Find all clusters/connected components:
        public List<HashSet<int>> FindClusters()
        {
            List<HashSet<int>> clusters = new List<HashSet<int>>();
            HashSet<int> visited = new HashSet<int>();

            for (int vertex = 0; vertex < numVertices; vertex++)
            {
                if (!visited.Contains(vertex))
                {
                    HashSet<int> currentCluster = new HashSet<int>();
                    FindClustersUtil(vertex, visited, currentCluster);
                    clusters.Add(currentCluster);
                }
            }

            return clusters;
        }

        private void FindClustersUtil(int vertex, HashSet<int> visited, HashSet<int> currentCluster)
        {
            visited.Add(vertex);
            currentCluster.Add(vertex);

            foreach (int adjacentVertex in GetAdjacentVertices(vertex))
            {
                if (!visited.Contains(adjacentVertex))
                {
                    FindClustersUtil(adjacentVertex, visited, currentCluster);
                }
            }
        }
    }




    // A vertex in a graph
    public class Node
    {
        private readonly int VertexId;
        private readonly HashSet<int> AdjacencySet;

        public Node(int vertexId)
        {
            this.VertexId = vertexId;
            this.AdjacencySet = new HashSet<int>();
        }

        public void AddEdge(int v)
        {
            if (this.VertexId == v)
                throw new ArgumentException("The vertex cannot be adjacent to itself");
            this.AdjacencySet.Add(v);
        }

        public HashSet<int> GetAdjacentVertices()
        {
            return this.AdjacencySet;
        }
    }

}
