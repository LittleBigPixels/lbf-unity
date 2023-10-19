using System.Linq;
using LBF.Geometry.Polygons;
using LBF.Structures;
using UnityEngine;

namespace LBF.Geometry.PolygonMaps
{
    public interface IBoundaryQuery
    {
        public Vector2[] Vertices { get; }
        public Bounds2D BoundingBox { get; }

        public bool Contains(Vector2 point);
        public Helpers.BoundaryIntersection Raycast(Ray2D ray);
    }

    public class AcceleratedBoundaryQuery : IBoundaryQuery
    {
        enum GridContainsType
        {
            Outside,
            Test,
            Inside
        }

        public Bounds2D BoundingBox => m_polygon.Bounds;
        public Vector2[] Vertices { get; }
        private SpatialGrid<GridContainsType> m_accelerationGrid;
        private Polygon m_polygon;

        public AcceleratedBoundaryQuery(Vector2[] vertices)
        {
            Vertices = vertices;
            m_polygon = new Polygon(vertices);
            m_accelerationGrid = new SpatialGrid<GridContainsType>(
                m_polygon.Bounds.Min, m_polygon.Bounds.Max,
                20, 20);

            foreach (var idx in m_accelerationGrid.Indices)
            {
                var cellCenter = m_accelerationGrid.GetCellCenter(idx);
                var cellBoundPolygon = new Polygon(new Vector2[]
                {
                    cellCenter
                    - m_accelerationGrid.CellSize.x * Vector2.right
                    - m_accelerationGrid.CellSize.y * Vector2.up,
                    cellCenter
                    + m_accelerationGrid.CellSize.x * Vector2.right
                    - m_accelerationGrid.CellSize.y * Vector2.up,
                    cellCenter
                    + m_accelerationGrid.CellSize.x * Vector2.right
                    + m_accelerationGrid.CellSize.y * Vector2.up,
                    cellCenter
                    - m_accelerationGrid.CellSize.x * Vector2.right
                    + m_accelerationGrid.CellSize.y * Vector2.up,
                });
                m_accelerationGrid[idx] = GridContainsType.Outside;
                if (PolygonHelper.Intersects(m_polygon, cellBoundPolygon))
                {
                    if (cellBoundPolygon.Points.Any(p => m_polygon.Contains(p) == false))
                        m_accelerationGrid[idx] = GridContainsType.Test;
                    else
                        m_accelerationGrid[idx] = GridContainsType.Inside;
                }
            }
        }

        public bool Contains(Vector2 point)
        {
            if (m_accelerationGrid.Contains(point) == false) return false;
            var containType = m_accelerationGrid[point];
            if (containType == GridContainsType.Outside) return false;
            if (containType == GridContainsType.Inside) return true;
            return m_polygon.Contains(point);
        }

        public Helpers.BoundaryIntersection Raycast(Ray2D ray)
        {
            return Helpers.RaycastBoundary(Vertices, ray);
        }
    }
}