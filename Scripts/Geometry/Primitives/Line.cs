using System;
using Main;
using NPCs.Steering;
using UnityEngine;

namespace Geometry.Primitives
{
    public class Line : MonoBehaviour
    {
        private BoxCollider2D _collider;
        private LineRenderer _lineRenderer;

        [SerializeField] private BoxCollider2D _obstacleCollider;
        [SerializeField] private Vector2 _startPoint;
        [SerializeField] private Vector2 _endPoint;

        private bool _initialized;

        public Vector2 StartPoint
        {
            get => _startPoint;
            set
            {
                _startPoint = value;
                AdjustColliders();
            }
        }

        public Vector2 EndPoint
        {
            get => _endPoint;
            set
            {
                _endPoint = value;
                AdjustColliders();
            }
        }

        public bool IsRendering
        {
            get => _lineRenderer.enabled;
            set => _lineRenderer.enabled = value; 
        }

        public float LineWidth { get; set; } = Properties.LineStartWidth;

        public bool EnableCollider { get; set; } = true;

        private Vector2 Center => new Vector2((EndPoint.x + StartPoint.x) / 2, (EndPoint.y + StartPoint.y) / 2);
        private float Slope => (EndPoint.y - StartPoint.y) / (EndPoint.x - StartPoint.x);

        void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
            _lineRenderer = GetComponent<LineRenderer>();
            tag = "PolygonLine";
        }

        void Update()
        {
            if (!_initialized)
            {
                return;
            }

            Draw();
        }

        private void Draw()
        {
            _lineRenderer.startWidth = LineWidth;
            _lineRenderer.endWidth = LineWidth;
            _lineRenderer.SetPosition(0, new Vector3(StartPoint.x, StartPoint.y, 0));
            _lineRenderer.SetPosition(1, new Vector3(EndPoint.x, EndPoint.y, 0));
        }

        public void Rotate(float angle, float length)
        {
           var radians = angle * Mathf.Deg2Rad;
           var x = Mathf.Sin(radians) * length;
           var y = Mathf.Cos(radians) * length;

           EndPoint = new Vector2(StartPoint.x + x, StartPoint.y + y);
        }

        private void AdjustColliders()
        {
            if (!_initialized || !EnableCollider)
            {
                return;
            }

            var center = Center;
            var angle = 90 - Mathf.Atan(Slope) * Mathf.Rad2Deg;
            gameObject.transform.Rotate(0, 0, -angle);
            gameObject.transform.position = new Vector3(center.x, center.y, 0);

            var yAxis = Mathf.Abs(EndPoint.y - StartPoint.y);
            var xAxis = Mathf.Abs(EndPoint.x - StartPoint.x);
            var height = Mathf.Sqrt(yAxis * yAxis + xAxis * xAxis);

            _collider.size = new Vector2(Properties.LineStartWidth + Properties.LineColliderOffset, height);
            _obstacleCollider.size = new Vector2(Properties.LineStartWidth + SteeringProperties.LineObstacleWidth, height);
        }

        public void Initialize(Vector2 startPoint, Vector2 endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            _initialized = true;
            AdjustColliders();
        }
    }
}
