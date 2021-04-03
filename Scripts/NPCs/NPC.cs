using System;
using System.Collections;
using System.Collections.Generic;
using Geometry.Polygons;
using Geometry.Primitives;
using Main;
using Managers;
using UnityEngine;

namespace NPCs
{
    public class NPC : MonoBehaviour, IAgentController
    {
        [SerializeField] private GameObject _linePrefab;
        [SerializeField] private FieldOfView _fov;
        [SerializeField] private bool _drawFieldOfView;
        [SerializeField] private Vector2Int _spawnPosition;

        private Line _lineLeft;
        private Line _lineRight;
        private const float LineWidth = 0.01f;

        public Polygon FovPolygon
        {
            get
            {
                var vertices = _fov.GetFovVertices();
                var polygon = new Polygon();

                foreach (var vertex in vertices)
                {
                    polygon.AddPoint(vertex);
                }

                return polygon;
            }
        }

        void Awake()
        {
            EventManager.GameMangerReadyEvent += OnGameManagerReady;
        }

        private void OnGameManagerReady(object sender, EventArgs e)
        {
            var spawnPoint = GameManager.Instance.GridToWorld(_spawnPosition);
            transform.position = new Vector3(spawnPoint.x, spawnPoint.y, 0);
            transform.Rotate(0, 0, -90);
            CreateView();
            NPCManager.Instance.AddNpc(this);
        }

        void Update()
        {
            if (!_drawFieldOfView && _lineRight != null && _lineLeft != null)
            {
                _lineRight.enabled = false;
                _lineLeft.enabled = false;
            }

           DrawView();
        }

        private void DrawView()
        {
            if (_lineLeft == null || _lineRight == null)
            {
                return;
            }

            var endPoint = transform.position + transform.up * Properties.ViewRadius;
            var angleLeft = Properties.ViewAngle / 2;
            var angleRight = 360 - angleLeft;

            _lineLeft.enabled = true;
            _lineRight.enabled = true;
            _lineLeft.StartPoint = transform.position;
            _lineRight.StartPoint = transform.position;
            _lineLeft.EndPoint = endPoint;
            _lineRight.EndPoint = endPoint;
            _lineLeft.Rotate(angleLeft, Properties.ViewRadius);
            _lineRight.Rotate(angleRight, Properties.ViewRadius);
        }

        private void CreateView()
        {
            var endPoint3 = transform.position + transform.up * Properties.ViewRadius;
            var endPoint = new Vector2(endPoint3.x, endPoint3.y);
            var startPoint = new Vector2(transform.position.x, transform.position.y);

            _lineLeft = Instantiate(_linePrefab, transform).GetComponent<Line>();
            _lineRight = Instantiate(_linePrefab, transform).GetComponent<Line>();

            _lineLeft.LineWidth = LineWidth;
            _lineRight.LineWidth = LineWidth;
            _lineLeft.EnableCollider = false;
            _lineRight.EnableCollider = false;
            _lineLeft.Initialize(startPoint, endPoint);
            _lineRight.Initialize(startPoint, endPoint);
        }
        
        void OnDestroyed()
        {
            EventManager.RaiseNpcDestroyedEvent(this, EventArgs.Empty);
        }

        public void SetDestination(Vector2Int destinationGrid)
        {
           
        }

        public void SetDestination(Vector2 destinationWorld)
        {
            throw new NotImplementedException();
        }
    }
}
