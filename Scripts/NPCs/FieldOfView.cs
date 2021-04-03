using System;
using System.Collections;
using System.Collections.Generic;
using Main;
using UnityEngine;

namespace NPCs
{
    public class FieldOfView : MonoBehaviour
    {
        // 
        private int edgeResolveIterations = 1;
        private float edgeDstThreshold = 100f;

        private int meshResolution = 1;

        private float m_ViewAngle;
        private float m_ViewRadius;

        // Mesh for rendering the field of view
        private MeshFilter m_ViewMeshFilter;
        private Mesh m_ViewMesh;
        private MeshRenderer m_MeshRenderer;
        private Material m_Material;

        // Vertices of the field of view mesh
        private List<Vector3> m_ViewPoints = new List<Vector3>();

        // Obstacle Layer
        private LayerMask m_ObstacleMask;

        void Awake()
        {
            // Assign the references to the mesh
            m_ViewMesh = new Mesh();
            m_ViewMeshFilter = gameObject.AddComponent<MeshFilter>();
            m_MeshRenderer = GetComponent<MeshRenderer>();

            m_ViewMeshFilter.mesh = m_ViewMesh;

            // Set the layer to intersect vision
            m_ObstacleMask = LayerMask.GetMask("Wall");

            m_ViewAngle = Properties.ViewAngle;
            m_ViewRadius = Properties.ViewRadius;
        }

        void Update()
        {
            CastFieldOfView();
        }

        // Cast the Vision
        public void CastFieldOfView()
        {
            // The list of points the ray either hit or reach
            m_ViewPoints.Clear();

            var oldViewCast = new ViewCastInfo();

            // NPC position and Y-rotation
            var npcTransform = transform;
            var source = npcTransform.position;
            var yRotation = -npcTransform.eulerAngles.z;

            m_ViewPoints.Add(source);

            // Calculate field of view
            for (var i = 0; i <= Mathf.RoundToInt(m_ViewAngle * meshResolution); i++)
            {
                var angle = yRotation - m_ViewAngle / 2 +
                              (m_ViewAngle / Mathf.RoundToInt(m_ViewAngle * meshResolution)) * i;

                var newViewCast = ViewCast(source, angle, m_ViewRadius);

                if (i > 0)
                {
                    if (oldViewCast.hit != newViewCast.hit ||
                        (oldViewCast.hit && newViewCast.hit &&
                         Mathf.Abs(oldViewCast.distance - newViewCast.distance) > edgeDstThreshold))
                    {
                        var edge = FindEdge(source, oldViewCast, newViewCast, m_ViewRadius);
                        if (edge.PointA != Vector3.zero)
                        {
                            m_ViewPoints.Add(edge.PointA);
                        }

                        if (edge.PointB != Vector3.zero)
                        {
                            m_ViewPoints.Add(edge.PointB);
                        }
                    }
                }


                m_ViewPoints.Add(newViewCast.point);
                oldViewCast = newViewCast;
            }

            // if (m_ViewAngle >= 360f)
            // {
            //     m_ViewPoints.RemoveAt(0);
            //     m_ViewPoints.RemoveAt(m_ViewPoints.Count - 1);
            // }

            // Draw mesh
            var vertexCount = m_ViewPoints.Count;
            var vertices = new Vector3[vertexCount];
            var triangles = new int[(vertexCount - 2) * 3];

            vertices[0] = Vector3.zero;
            for (var i = 0; i < vertexCount - 1; i++)
            {
                vertices[i + 1] = transform.InverseTransformPoint(m_ViewPoints[i]);

                if (i < vertexCount - 2)
                {
                    triangles[i * 3] = 0;
                    triangles[i * 3 + 1] = i + 1;
                    triangles[i * 3 + 2] = i + 2;
                }
            }

            m_ViewMesh.Clear();

            m_ViewMesh.vertices = vertices;
            m_ViewMesh.triangles = triangles;
            m_ViewMesh.RecalculateNormals();
        }

        public List<Vector3> GetFovVertices()
        {
            return m_ViewPoints;
        }

        // Get the direction from the angle
        private Vector3 DirFromAngle(float angleInDegrees)
        {
            return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
        }

        // Shoot the ray
        ViewCastInfo ViewCast(Vector3 source, float globalAngle, float viewRadius)
        {
            var dir = DirFromAngle(globalAngle);

            Physics.autoSyncTransforms = false;

            var hit = Physics2D.Raycast(source, dir, viewRadius, m_ObstacleMask);
            if (hit)
            {
                Physics.autoSyncTransforms = true;
                return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
            }

            // In case the ray was not obstructed
            Physics.autoSyncTransforms = true;
            return new ViewCastInfo(false, source + dir * viewRadius, viewRadius, globalAngle);
        }

        EdgeInfo FindEdge(Vector3 source, ViewCastInfo minViewCast, ViewCastInfo maxViewCast, float viewRadius)
        {
            var minAngle = minViewCast.angle;
            var maxAngle = maxViewCast.angle;
            var minPoint = Vector3.zero;
            var maxPoint = Vector3.zero;
            
            for (var i = 0; i < edgeResolveIterations; i++)
            {
                var angle = (minAngle + maxAngle) / 2;
                var newViewCast = ViewCast(source, angle, viewRadius);

                var edgeDstThresholdExceeded =
                    Mathf.Abs(minViewCast.distance - newViewCast.distance) > edgeDstThreshold;
                if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
                {
                    minAngle = angle;
                    minPoint = newViewCast.point;
                }
                else
                {
                    maxAngle = angle;
                    maxPoint = newViewCast.point;
                }
            }

            return new EdgeInfo(minPoint, maxPoint);
        }
    }

// Information about the edge
    public struct EdgeInfo
    {
        public Vector3 PointA;
        public Vector3 PointB;

        public EdgeInfo(Vector3 pointA, Vector3 pointB)
        {
            PointA = pointA;
            PointB = pointB;
        }
    }


// Contains information about the result of ray cast behaviour.
    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;

        public ViewCastInfo(bool hit, Vector3 point, float distance, float angle)
        {
            this.hit = hit;
            this.point = point;
            this.distance = distance;
            this.angle = angle;
        }
    }
}