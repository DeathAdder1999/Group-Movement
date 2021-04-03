using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Main;
using Managers;
using Pathfinding;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NPCs.GridBased
{
    public class GridMovement : MonoBehaviour, IMovement
    {
        [SerializeField] private bool _enableClick;
        [SerializeField] private Vector2Int _destination1;
        [SerializeField] private Vector2Int _destination2;
        [SerializeField] private bool _isPlayer;

        [SerializeField] private Vector2Int _currentNode;
        private Vector2Int _currentDestination;
        private Vector2 _currentPosition;
        private float _speed = 6.0f;
        private float _accum = 0;
        private Stack<Node> _path;
        private bool _initialized;
        private Vector2 _distanceToCenter;
        private Group _parentGroup;

        public Vector2 Position => new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
        private Node[][] Grid => GameManager.Instance.GridMap;

        private Node NextNode
        {
            get
            {
                if (_path == null || !_path.Any())
                {
                    _path = null;
                    return null;
                }

                var node = _path.Peek();

                if (Mathf.Approximately(Position.x, node.Center.x) && Mathf.Approximately(Position.y, node.Center.y))
                {
                    _path.Pop();
                    _currentNode = node.GridPosition;
                    _currentPosition = node.Center;

                    if (_path.Any())
                    {
                        node = _path.Peek();
                    }

                    LookAt2d(node.Center);
                    _accum = 0;
                }

                return node;
            }
        }

        public bool HasPath => _path != null;

        public Vector2Int CurrentNode => _currentNode;

        public Vector2Int Destination1 => _destination1;

        public Vector2Int Destination2 => _destination2;

        public List<Node> Path => _path?.ToList();

        public bool Enabled { get; set; }

        public Group ParentGroup
        {
            get => _parentGroup;
            set
            {
                _parentGroup = value;
                _parentGroup.GroupLimitReached += (sender, args) => _distanceToCenter = ParentGroup.Center - Position;
            }
        }

        void Awake()
        {
            EventManager.NpcDestinationSetEvent += OnDestinationSet;
            EventManager.GameMangerReadyEvent += OnGameManagerReady;
        }

        void Start()
        {
            if (_enableClick)
            {
                _initialized = true;
            }

            //_currentNode = _destination1;
            _currentPosition = new Vector2(transform.position.x, transform.position.y);
            _currentNode = GameManager.Instance.WorldToGrid(_currentPosition);
        }

        private void OnGameManagerReady(object sender, EventArgs e)
        {
            if (!_enableClick)
            {
                //StartCoroutine(SetPath());
            }
        }

        private IEnumerator SetPath()
        {
            yield return new WaitForSeconds(1.0f);
            _path = Pathfinder.FindPath(_destination1, _destination2, Grid, _isPlayer);
            _initialized = true;
        }

        void Update()
        {
            if (/*!_initialized ||*/ !Enabled)
            {
                return;
            }

            //TODO remove if
            if (_path != null)
            {
                var next = NextNode;

                if (next != null)
                {
                    _accum += Time.deltaTime * _speed;
                    transform.position = Vector2.Lerp(_currentPosition, next.Center, _accum);
                }
            }

            
        }

        private void OnDestinationSet(object sender, Vector2Int e)
        {
            if (!_enableClick)
            {
                return;
            }

            _accum = 0;
            _path = Pathfinder.FindPath(_currentNode, e, Grid, _isPlayer);

            if (_path != null)
            {
                Debug.Log($"Destination: {e}");
            }
        }

        //TODO remove
        public void RecalculatePath()
        {
            if (_initialized)
            {
                _path = Pathfinder.FindPath(_destination1, _destination2, Grid, _isPlayer);
            }
        }

        public void SetDestination(Vector2Int destinationGrid)
        {
            _path = Pathfinder.FindPath(_currentNode, destinationGrid, Grid, _isPlayer);
        }

        public void SetDestination(Vector2 destinationWorld)
        {
            destinationWorld += _distanceToCenter;
            var destination = GameManager.Instance.WorldToGrid(destinationWorld);
            Debug.Log($"Destination {destination}");
            SetDestination(destination);
        }

        public void SetPath(Stack<Node> path)
        {
            _path = path;
        }

        private void LookAt2d(Vector2 target)
        {
            transform.up = new Vector3(target.x, target.y, 0) - transform.position;
        }

        private void OnDrawGizmos()
        {
            if (HasPath && Path.Any())
            {
                var destination = Path.Last();
                Gizmos.color = Color.red;
                Gizmos.DrawCube(destination.Center, new Vector3(GameManager.Instance.NodeWidth, GameManager.Instance.NodeHeight, 0.1f));
            }
        }
    }
}
