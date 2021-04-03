using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main;
using Managers;
using UnityEngine;

namespace Pathfinding
{
    public class Grid : MonoBehaviour
    {
        [SerializeField] private GameObject _nodePrefab;
        [SerializeField] private bool _disableColliders;
        [SerializeField] private int _rows;
        [SerializeField] private int _columns;

        //Top left corner of the grid
        [SerializeField] private Vector2 _startPoint = new Vector2(-8, 5);
        private bool _collidersDisabled;

        public Node[][] GridMap { get; private set; }

        void Awake()
        { 
            GridMap = new Node[_rows][];

            for (var i = 0; i < _rows; i++)
            {
                GridMap[i] = new Node[_columns];
            }

            EventManager.ShadowVolumeChangedEvent += OnShadowVolumeChanged;
        }

        private void OnShadowVolumeChanged(object sender, EventArgs e)
        {
            ResetNodes();
            StartCoroutine(RaiseMapPassabilityChangedEvent());
        }

        void Start()
        {
            GenerateGrid();
        }

        void Update()
        {
            if (!_collidersDisabled && _disableColliders)
            {
                SetCollidersActive(false);
            }

            if (_collidersDisabled && !_disableColliders)
            {
                SetCollidersActive(true);
            }
        }

        public void DrawPath(Stack<Node> path)
        {
            foreach (var row in GridMap)
            {
                foreach (var node in row)
                {
                    if (!path.Contains(node))
                    {
                        node.SetColliderActive(false);
                    }
                }
            }
        }

        private void SetCollidersActive(bool value)
        {
            foreach (var row in GridMap)
            {
                foreach (var node in row)
                {
                    node.SetColliderActive(value);
                }
            }

            _collidersDisabled = !value;
        }

        private void ResetNodes()
        {
            foreach (var row in GridMap)
            {
                foreach (var node in row)
                {
                   node.Reset();
                }
            }
        }

        private IEnumerator RaiseMapPassabilityChangedEvent()
        {
            yield return new WaitForSeconds(.5f);

            EventManager.RaiseMapPassabilityChangedEvent(this, EventArgs.Empty);
        }

        private void GenerateGrid()
        {
            var center = _startPoint;
            var gridPosition = new Vector2Int();
            var nodeWidth = GameManager.Instance.NodeWidth;
            var nodeHeight = GameManager.Instance.NodeHeight;

            for (var row = 0; row < _rows; row++)
            {
                for (var column = 0; column < _columns; column++)
                {
                    var gridNode = Instantiate(_nodePrefab, transform);
                    center.x = _startPoint.x + (column) * (nodeWidth);
                    center.y = _startPoint.y - (row) * (nodeHeight);

                    gridPosition.x = row;
                    gridPosition.y = column;
                    GridMap[row][column] = gridNode.GetComponent<Node>();
                    GridMap[row][column].Initialize(center, gridPosition);
                }
            }

            EventManager.RaiseGridGeneratedEvent(this, EventArgs.Empty);
        }

        public Vector2 GridToWorld(Vector2Int gridPosition)
        {
            var node = GridMap[gridPosition.x][gridPosition.y];
            return node.Center;
        }

        public Vector2Int WorldToGrid(Vector2 worldPosition)
        {
            var positionToCenter = worldPosition - _startPoint;
            var position = new Vector2Int()
            {
                x = (int) Math.Round(Math.Abs((positionToCenter.y / GameManager.Instance.NodeWidth))),
                y = (int) Math.Round(Math.Abs((positionToCenter.x / GameManager.Instance.NodeHeight)))
            };

            if (worldPosition == new Vector2(5, 0))
            {
                Debug.Log($"worldPosition {worldPosition} gridPosition{position}");
            }

            return position;
        }

        public Vector2 GetXDimensions()
        {
            return new Vector2(_startPoint.x, _startPoint.x + _columns * GameManager.Instance.NodeWidth);
        }

        public Vector2 GetYDimensions()
        {
            return new Vector2(_startPoint.y - _rows * GameManager.Instance.NodeHeight, _startPoint.y);
        }
    }
}
