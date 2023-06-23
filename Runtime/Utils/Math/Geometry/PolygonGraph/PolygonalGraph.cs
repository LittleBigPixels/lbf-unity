using System;
using System.Collections.Generic;
using System.Linq;
using LBF.Helpers;
using UnityEngine;
using UnityEngine.Profiling;

namespace LBF.Math.Geometry.PolygonGraph
{
    [Serializable]
    public struct Vector2Data
    {
        public float X;
        public float Y;
        public Vector2Data(Vector2 v) { X = v.x; Y = v.y; }
        public Vector2 ToVector2() { return new Vector2(X, Y); }
    }

    [Serializable]
    public struct EdgeData
    {
        public int Index;
        public Vector2Data A;
        public Vector2Data B;
        public int Polygon1;
        public int Polygon2;
    }

    [Serializable]
    public struct PolygonData
    {
        public int Index;
        public Vector2Data Center;
        public int[] Borders;
        public int[] Neighbours;
    }

    [Serializable]
    public class PolygonGraphData
    {
        public EdgeData[] Edges;
        public PolygonData[] Polygons;
    }

    [Serializable]
    public class Edge
    {
        public int Index;
        public Vector2 A;
        public Vector2 B;
        public Polygon Polygon1;
        public Polygon Polygon2;
    }

    [Serializable]
    public class Polygon
    {
        public int Index;
        public Vector2 Center;
        public List<Edge> Borders;
        public List<Polygon> Neighbours;
    }

    [Serializable]
    public class Triangle
    {
        public Polygon[] Vertices;
    }

    public class PolygonalGraph
    {
        List<Polygon> m_polygons;
        public List<Polygon> Polygons {
            get { return m_polygons; }
        }

        List<Edge> m_edges;
        public List<Edge> Edges {
            get { return m_edges; }
        }

        List<Triangle> m_delaunayTriangles;
        public List<Triangle> DelaunayTriangles {
            get { return m_delaunayTriangles; }
        }

        public float Width;
        public float Height;

        public int PolygonCount {
            get { if (m_polygons == null) return 0; else return m_polygons.Count; }
        }

        public PolygonalGraph()
        {
        }

        public void Generate(float width, float height, int polyCount, int seed)
        {
            Width = width;
            Height = height;

            m_polygons = new List<Polygon>(polyCount);
            m_edges = new List<Edge>();

            for (int i = 0; i < polyCount; i++)
            {
                Polygons.Add(new Polygon()
                {
                    Index = i,
                    Center = Vector2.zero,
                    Borders = new List<Edge>(),
                    Neighbours = new List<Polygon>()
                });
            }

            GenerateVoronoi2(width, height, seed, 4);
        }

        public void RelaxPoints(double[] xArray, double[] yArray, float width, float height)
        {
            var voronoi = new Voronoi.Voronoi(0.01f);
            var graph = voronoi.generateVoronoi(xArray, yArray, -width * 0.5f, width * 0.5f, -height * 0.5f, height * 0.5f);

            for (int i = 0; i < xArray.Length; i++)
            {
                xArray[i] = 0;
                yArray[i] = 0;
            }

            var nCorner = new int[xArray.Length];
            for (int i = 0; i < graph.Count; i++)
            {
                var graphEdge = graph[i];
                nCorner[graphEdge.site1] += 2;
                xArray[graphEdge.site1] += graphEdge.x1;
                xArray[graphEdge.site1] += graphEdge.x2;
                yArray[graphEdge.site1] += graphEdge.y1;
                yArray[graphEdge.site1] += graphEdge.y2;

                nCorner[graphEdge.site2] += 2;
                xArray[graphEdge.site2] += graphEdge.x1;
                xArray[graphEdge.site2] += graphEdge.x2;
                yArray[graphEdge.site2] += graphEdge.y1;
                yArray[graphEdge.site2] += graphEdge.y2;
            }

            for (int i = 0; i < xArray.Length; i++)
            {
                xArray[i] = xArray[i] / nCorner[i];
                yArray[i] = yArray[i] / nCorner[i];
            }
        }

        public void GenerateVoronoi2(float width, float height, int seed, int relaxationSteps = 4)
        {
            var random = new System.Random(seed);

            Profiler.BeginSample("PolygonGraph.GenerateVoronoi2 - Create random points");
            var nPoints = m_polygons.Count;
            var nSize = (int)System.Math.Sqrt(nPoints) + 1;
            double[] xArray = new double[nPoints];
            double[] yArray = new double[nPoints];

            for (int i = 0; i < nPoints; i++)
            {
                xArray[i] = random.NextFloat(-width * 0.5f, width * 0.5f);
                yArray[i] = random.NextFloat(-height * 0.5f, height * 0.5f);
            }
            Profiler.EndSample();

            Profiler.BeginSample("PolygonGraph.GenerateVoronoi2 - Relaxation");
            for (int i = 0; i < relaxationSteps; i++)
                RelaxPoints(xArray, yArray, width, height);
            Profiler.EndSample();

            Profiler.BeginSample("PolygonGraph.GenerateVoronoi2 - Voronoi");
            Voronoi.Voronoi voronoi = new Voronoi.Voronoi(0.1f);
            var graph = voronoi.generateVoronoi(xArray, yArray, -width * 0.5f, width * 0.5f, -height * 0.5f, height * 0.5f);
            Profiler.EndSample();

            Profiler.BeginSample("PolygonGraph.GenerateVoronoi2 - Setup Graph");
            for (int i = 0; i < nPoints; i++)
            {
                m_polygons[i].Center = new Vector2((float)xArray[i], (float)yArray[i]);
            }

            for (int i = 0; i < graph.Count; i++)
            {
                var graphEdge = graph[i];

                var edgeA = new Vector2((float)graphEdge.x1, (float)graphEdge.y1);
                var edgeB = new Vector2((float)graphEdge.x2, (float)graphEdge.y2);

                if (edgeA == edgeB) continue;

                var edge = new Edge() { Index = m_edges.Count, A = edgeA, B = edgeB, Polygon1 = m_polygons[graphEdge.site1], Polygon2 = m_polygons[graphEdge.site2] };
                m_polygons[graphEdge.site1].Neighbours.Add(m_polygons[graphEdge.site2]);
                m_polygons[graphEdge.site1].Borders.Add(edge);
                m_polygons[graphEdge.site2].Neighbours.Add(m_polygons[graphEdge.site1]);
                m_polygons[graphEdge.site2].Borders.Add(edge);

                m_edges.Add(edge);
            }
            Profiler.EndSample();

            Profiler.BeginSample("PolygonGraph.GenerateVoronoi2 - Delaunay");
            GenerateDelaunayTriangulation();
            Profiler.EndSample();
        }

        private void GenerateDelaunayTriangulation()
        {
            HashSet<Triangle> triangles = new HashSet<Triangle>();
            foreach (var polygon in m_polygons)
            {
                var sortedNeightbours = polygon.Neighbours.OrderBy(
                        n => Vector2.SignedAngle(Vector2.right, n.Center - polygon.Center)).ToList();

                for (int iNeighbour = 0; iNeighbour < sortedNeightbours.Count; iNeighbour++)
                {
                    var p1 = polygon;
                    var p2 = sortedNeightbours[iNeighbour];
                    var p3 = sortedNeightbours[(iNeighbour + 1) % sortedNeightbours.Count];

                    var points = new Polygon[3] { p1, p2, p3 };
                    triangles.Add(new Triangle() { Vertices = points });// points.OrderBy(p => p.Index).ToArray() });
                }
            }

            m_delaunayTriangles = triangles.ToList();
        }

        public PolygonGraphData Save()
        {
            var edges = new EdgeData[m_edges.Count];
            for (int i = 0; i < edges.Length; i++)
                edges[i] = new EdgeData()
                {
                    Index = m_edges[i].Index,
                    A = new Vector2Data(m_edges[i].A),
                    B = new Vector2Data(m_edges[i].B),
                    Polygon1 = m_edges[i].Polygon1.Index,
                    Polygon2 = m_edges[i].Polygon2.Index,
                };

            var polys = new PolygonData[m_polygons.Count];
            for (int i = 0; i < polys.Length; i++)
            {
                var borderIndexes = new int[m_polygons[i].Borders.Count];
                for (int iBorder = 0; iBorder < borderIndexes.Length; iBorder++)
                    borderIndexes[iBorder] = m_polygons[i].Borders[iBorder].Index;

                var neighbourIndexes = new int[m_polygons[i].Neighbours.Count];
                for (int iNeighbour = 0; iNeighbour < neighbourIndexes.Length; iNeighbour++)
                    neighbourIndexes[iNeighbour] = m_polygons[i].Neighbours[iNeighbour].Index;

                polys[i] = new PolygonData()
                {
                    Index = m_polygons[i].Index,
                    Center = new Vector2Data(m_polygons[i].Center),
                    Borders = borderIndexes,
                    Neighbours = neighbourIndexes,
                };
            }

            var graphData = new PolygonGraphData() { Edges = edges, Polygons = polys };
            return graphData;
        }

        public void Load(PolygonGraphData graphData)
        {
            //Create Edges and Polygons
            m_edges = new List<Edge>(graphData.Edges.Length);
            for (int iEdge = 0; iEdge < graphData.Edges.Length; iEdge++)
                m_edges.Add(new Edge()
                {
                    Index = graphData.Edges[iEdge].Index,
                    A = graphData.Edges[iEdge].A.ToVector2(),
                    B = graphData.Edges[iEdge].B.ToVector2(),
                });

            m_polygons = new List<Polygon>(graphData.Polygons.Length);
            for (int iPoly = 0; iPoly < graphData.Polygons.Length; iPoly++)
                m_polygons.Add(new Polygon()
                {
                    Index = graphData.Polygons[iPoly].Index,
                    Center = graphData.Polygons[iPoly].Center.ToVector2(),
                });

            //Fix up references
            for (int iEdge = 0; iEdge < graphData.Edges.Length; iEdge++)
            {
                m_edges[iEdge].Polygon1 = m_polygons[graphData.Edges[iEdge].Polygon1];
                m_edges[iEdge].Polygon2 = m_polygons[graphData.Edges[iEdge].Polygon2];
            }

            for (int iPoly = 0; iPoly < graphData.Polygons.Length; iPoly++)
            {
                m_polygons[iPoly].Borders = new List<Edge>(graphData.Polygons[iPoly].Borders.Length);
                for (int iEdge = 0; iEdge < graphData.Polygons[iPoly].Borders.Length; iEdge++)
                    m_polygons[iPoly].Borders.Add(m_edges[graphData.Polygons[iPoly].Borders[iEdge]]);

                m_polygons[iPoly].Neighbours = new List<Polygon>(graphData.Polygons[iPoly].Neighbours.Length);
                for (int iNeighbour = 0; iNeighbour < graphData.Polygons[iPoly].Neighbours.Length; iNeighbour++)
                    m_polygons[iPoly].Neighbours.Add(m_polygons[graphData.Polygons[iPoly].Neighbours[iNeighbour]]);
            }

            GenerateDelaunayTriangulation();
        }
    }
}