using System.Collections.Generic;
using DataStructures;
using Main;
using UnityEditor;
using UnityEngine;

namespace Geometry.Polygons
{
    public class Polygon
    {
        // Polygon vertices
        private CyclicalList<Vertex> _vertices;

        public Polygon()
        {
            _vertices = new CyclicalList<Vertex>();
        }

        public Polygon(Polygon polygon)
        {
            _vertices = new CyclicalList<Vertex>();

            foreach (var vertex in polygon._vertices)
            {
                AddPoint(vertex.position);
            }
        }

        public Polygon(IEnumerable<Vertex> vertices)
        {
            foreach (var vertex in vertices)
            {
                AddPoint(vertex.position);
            }
        }

        public void Clear()
        {
            _vertices.Clear();
        }

        // Add a vertex to the polygon
        public void AddPoint(Vector2 point)
        {
            _vertices.Add(new Vertex(point, GetVerticesCount()));
        }

        public Vector2 GetPoint(int i)
        {
            return _vertices[i].position;
        }

        public void RemovePoint(int i)
        {
            _vertices.RemoveAt(i);
        }

        public Vertex GetVertex(int i)
        {
            return _vertices[i];
        }

        // Get the list of vertices of the polygon
        public CyclicalList<Vertex> GetPoints()
        {
            return _vertices;
        }

        // Get the number of the vertices of the polygon
        public int GetVerticesCount()
        {
            return _vertices.Count;
        }


        public float GetArea()
        {
            return Mathf.Abs(GetSignedArea());
        }


        // Calculate the area of the polygon. If the area is negative then the polygon is counterclockwise
        float GetSignedArea()
        {
            // Get the areas.
            float area = 0f;
            if (GetVerticesCount() > 0)
                for (int i = 0; i < GetVerticesCount(); i++)
                {
                    area += (GetPoint(i + 1).x - GetPoint(i).x) *
                            (GetPoint(i + 1).y + GetPoint(i).y) / 2f;
                }

            // Return the result.
            return area;
        }

        // Determine the winding order 
        public WindingOrder DetermineWindingOrder()
        {
            return GetSignedArea() < 0 ? WindingOrder.CounterClockwise : WindingOrder.Clockwise;
        }

        // Ensures that a set of vertices are wound in the desired winding order
        public void EnsureWindingOrder(WindingOrder windingOrder)
        {
            if (!DetermineWindingOrder().Equals(windingOrder))
            {
                ReverseWindingOrder();
            }
        }

        // Reverses the winding order for the polygon vertices.
        public void ReverseWindingOrder()
        {
            CyclicalList<Vertex> reverseVertices = new CyclicalList<Vertex>();

            int index = 0;
            for (int i = GetVerticesCount() - 1; i >= 0; i--)
                reverseVertices.Add(new Vertex(GetPoint(i), index++));

            _vertices = reverseVertices;
        }

        // Smooth the polygon
        public void SmoothPolygon(float minDistance)
        {
            float minAngle = Properties.MinAngle;
            float maxAngle = Properties.MaxAngle;

            int index = 0;
            while (index < _vertices.Count)
            {
                // Remove the vertices if its angle is below the min threshold or more than the max threshold
                if (GeometryHelper.GetAngle(_vertices[index - 1].position, _vertices[index].position,
                        _vertices[index + 1].position) >= maxAngle)
                {
                    _vertices.RemoveAt(index);
                    index = 0;
                }

                if (GeometryHelper.GetAngle(_vertices[index - 1].position, _vertices[index].position,
                        _vertices[index + 1].position) <= minAngle)
                {
                    _vertices.RemoveAt(index);
                    index = 0;
                }
                else if (Vector2.Distance(_vertices[index - 1].position, _vertices[index + 1].position) <=
                         minDistance)
                {
                    _vertices.RemoveAt(index);
                    index = 0;
                }
                else
                    index++;
            }
        }


        // Get the bounding box of polygon
        public void BoundingBox(out float minX, out float maxX, out float minY, out float maxY)
        {
            minX = Mathf.Infinity;
            minY = Mathf.Infinity;
            maxX = Mathf.NegativeInfinity;
            maxY = Mathf.NegativeInfinity;

            for (int i = 0; i < GetVerticesCount(); i++)
            {
                Vector2 p = GetPoint(i);
                if (minX > p.x)
                    minX = p.x;
                if (maxX < p.x)
                    maxX = p.x;
                if (minY > p.y)
                    minY = p.y;
                if (maxY < p.y)
                    maxY = p.y;
            }
        }

        // Check if a point is in polygon 
        public bool IsPointInPolygon(Vector2 point, bool includeBorders)
        {
            bool inside = false;
            for (int i = 0, j = GetVerticesCount() - 1; i < GetVerticesCount(); j = i++)
            {
                if (includeBorders)
                    if (point.Equals(GetPoint(i)))
                    {
                        return true;
                    }

                if ((GetPoint(i).y > point.y) != (GetPoint(j).y > point.y) && point.x <
                    (GetPoint(j).x - GetPoint(i).x) * (point.y - GetPoint(i).y) / (GetPoint(j).y - GetPoint(i).y) +
                    GetPoint(i).x)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        // Get a random position inside the polygon
        public Vector2 GetRandomPosition()
        {
            BoundingBox(out float minX, out float maxX, out float minY, out float maxY);

            while (true)
            {
                float xPos = Random.Range(minX, maxX);
                float yPos = Random.Range(minY, maxY);

                Vector2 possiblePoint = new Vector2(xPos, yPos);

                if (IsPointInPolygon(possiblePoint, false))
                    return possiblePoint;
            }
        }

        public bool IsPolygonInside(Polygon inPoly, bool includeBorder)
        {
            for (int i = 0; i < inPoly.GetVerticesCount(); i++)
            {
                if (!IsPointInPolygon(inPoly.GetPoint(i), includeBorder))
                    return false;
            }

            return true;
        }


        // Draw the polygon
        public virtual void DrawGizmos(string label)
        {
            for (int i = 0; i < GetVerticesCount(); i++)
            {
                Gizmos.DrawLine(GetPoint(i), GetPoint(i + 1));
                // Handles.Label(GetPoint(i), i.ToString());
            }

            Handles.Label(GetCentroidPosition(), label);
        }

        // Get the centroid position of the polygon
        public Vector2 GetCentroidPosition()
        {
            float x = 0f;
            float y = 0f;

            foreach (Vertex v in _vertices)
            {
                x += v.position.x;
                y += v.position.y;
            }

            return new Vector2(x / _vertices.Count, y / _vertices.Count);
        }

        // Enlarge or shrink the polygon based on its Winding order
        public void Enlarge(float displacementAmount)
        {
            List<Vector2> displacementDirections = new List<Vector2>();

            for (int i = 0; i < GetVerticesCount(); i++)
            {
                displacementDirections.Add(GeometryHelper.GetNormal(GetPoint(i - 1), GetPoint(i), GetPoint(i + 1)));
            }

            for (int i = 0; i < GetVerticesCount(); i++)
            {
                if (GeometryHelper.IsReflex(GetPoint(i + 1), GetPoint(i), GetPoint(i - 1)))
                    _vertices[i].position += displacementDirections[i] * displacementAmount;
                else
                    _vertices[i].position -= displacementDirections[i] * displacementAmount;
            }
        }

        //TODO make sure that if polygon is changed lines are changed as well
        public List<(Vector2, Vector2)> GetLines()
        {
            var list = new List<(Vector2, Vector2)>();

            for(var i = 0; i < _vertices.Count; i++)
            {
                list.Add((_vertices[i].position, _vertices[i+1].position));
            }

            return list;
        }
    }

// Winding order of a polygon vertices
    public enum WindingOrder
    {
        Clockwise,
        CounterClockwise
    }
}