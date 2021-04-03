using System.Collections.Generic;
using Main;
using Managers;
using UnityEngine;
using UnityEngine.Animations;

namespace Pathfinding
{
    public class Node : MonoBehaviour
    {
        [SerializeField] private Vector2Int _gridPosition;

        private BoxCollider2D _collider;
        private Vector2 _center;
        private bool _intialized;
        private bool _isPlayerWalkable = true;

        public bool IsGuardWalkable { get; set; } = true;

        //Used for making a way
        public bool IsVisited { get; set; }

        public bool IsPlayerWalkable
        {
            get => _isPlayerWalkable;
            set
            {
                _isPlayerWalkable = value;
                _collider.enabled = value;
            }
        }
        public int GCost;
        public int HCost;
        public int FCost;
       
        public Node Parent = null;

        public Vector2 Center
        {
            get => _center;
            set
            {
                _center = value;
                gameObject.transform.position = new Vector3(_center.x, _center.y, 0);
            }
        }

        public Vector2Int GridPosition
        {
            get => _gridPosition;
            set => _gridPosition = value;
        }

        public float Size { get; private set; }

        void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
            _collider.size = new Vector2(GameManager.Instance.NodeWidth, GameManager.Instance.NodeHeight);
            gameObject.layer = LayerMask.NameToLayer("GridNode");
        }

        public void CalculateFCost()
        {
            FCost = GCost + HCost;
        }

        public void Initialize(Vector2 center, Vector2Int gridPosition)
        {
            GridPosition = gridPosition;
            Center = center;
            _intialized = true;
        }

        public void SetColliderActive(bool value)
        {
            _collider.enabled = value;
        }

        public List<Node> GetWalkableNeighbours()
        {
            var neighbours = new List<Node>();
            var grid = GameManager.Instance.GridMap;
            var gridWidth = grid[0].Length;
            var gridHeight = grid.Length;

            //top
            if (GridPosition.x - 1 >= 0)
            {
                var n = grid[GridPosition.x - 1][GridPosition.y];

                if (n.IsPlayerWalkable)
                {
                    neighbours.Add(grid[GridPosition.x - 1][GridPosition.y]);
                }
            }

            //bottom
            if (GridPosition.x + 1 < gridHeight)
            {
                var n = grid[GridPosition.x + 1][GridPosition.y];

                if (n.IsPlayerWalkable)
                {
                    neighbours.Add(grid[GridPosition.x + 1][GridPosition.y]);
                }
            }

            //left
            if (GridPosition.y - 1 >= 0)
            {
                var n = grid[GridPosition.x][GridPosition.y - 1];

                if (n.IsPlayerWalkable)
                {
                    neighbours.Add(grid[GridPosition.x][GridPosition.y - 1]);
                }
            }

            //right
            if (GridPosition.y + 1 < gridWidth)
            {
                var n = grid[GridPosition.x][GridPosition.y + 1];

                if (n.IsPlayerWalkable)
                {
                    neighbours.Add(grid[GridPosition.x][GridPosition.y + 1]);
                }
            }

            //Top Left
            if (GridPosition.y - 1 >= 0 && GridPosition.x - 1 >= 0)
            {
                var n = grid[GridPosition.x - 1][GridPosition.y - 1];

                if (n.IsPlayerWalkable)
                {
                    neighbours.Add(grid[GridPosition.x - 1][GridPosition.y - 1]);
                }
            }

            //Bottom Left
            if (GridPosition.x + 1 < gridHeight && GridPosition.y - 1 >= 0)
            {
                var n = grid[GridPosition.x + 1][GridPosition.y - 1];

                if (n.IsPlayerWalkable)
                {
                    neighbours.Add(grid[GridPosition.x + 1][GridPosition.y - 1]);
                }
            }

            //Top Right
            if (GridPosition.x - 1 >= 0 && GridPosition.y + 1 < gridWidth)
            {
                var n = grid[GridPosition.x - 1][GridPosition.y + 1];

                if (n.IsPlayerWalkable)
                {
                    neighbours.Add(grid[GridPosition.x - 1][GridPosition.y + 1]);
                }
            }

            //Top Bottom
            if (GridPosition.x + 1 < gridHeight && GridPosition.y + 1 < gridWidth)
            {
                var n = grid[GridPosition.x + 1][GridPosition.y + 1];

                if (n.IsPlayerWalkable)
                {
                    neighbours.Add(grid[GridPosition.x + 1][GridPosition.y + 1]);
                }
            }

            return neighbours;
        }

        public void Reset()
        {
            IsGuardWalkable = true;
            IsPlayerWalkable = true;
            _collider.enabled = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("PolygonLine"))
            {
                IsGuardWalkable = false;
                IsPlayerWalkable = false;
                _collider.enabled = false;
            }

            if (other.CompareTag("FieldOfView"))
            {
                IsPlayerWalkable = false;
                _collider.enabled = false;
            }
        }
    }
}
