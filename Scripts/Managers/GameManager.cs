using System;
using System.Collections;
using System.Collections.Generic;
using FileIO;
using Geometry.Polygons;
using NPCs;
using Pathfinding;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Grid = Pathfinding.Grid;

namespace Managers
{
    public class GameManager : AbstractSingleton<GameManager>
    {
        [SerializeField] private Text _loadingText;
        [SerializeField] private Grid _grid;
        [SerializeField] private GameObject _npcPrefab;

        [Header("Game Settings")]
        [SerializeField] private Map _map;
        [SerializeField] private float _nodeWidth;
        [SerializeField] private float _nodeHeight;

        private bool _mapGenerated;
        private bool _gridGenerated;
        private bool _ready;

        public bool Ready
        {
            get => _ready;
            set
            {
                _ready = value;
                if (_ready)
                {
                    _loadingText.enabled = false;
                    EventManager.RaiseGameManagerReadyEvent(this, EventArgs.Empty);
                }
            }
        }

        public float NodeWidth => _nodeWidth;
        public float NodeHeight => _nodeHeight;

        public Node[][] GridMap => _grid.GridMap;

        public List<Polygon> MapData { get; private set; }

        public string CurrentMap => _map.ToString();

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                var position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var npcDestination = new Vector2(position.x, position.y);
                var pos = WorldToGrid(npcDestination);
                Debug.Log($"Click {pos}");
                EventManager.RaiseNpcDestinationSetEvent(this, WorldToGrid(npcDestination));
            }
        }

        public Vector2 GridToWorld(Vector2Int gridPosition)
        {
            return _grid.GridToWorld(gridPosition);
        }

        public Vector2 GetGridXDimensions()
        {
            return _grid.GetXDimensions();
        }

        public Vector2 GetGridYDimensions()
        {
            return _grid.GetYDimensions();
        }

        public void ResetGridVisited()
        {
            foreach (var row in GridMap)
            {
                foreach (var node in row)
                {
                    node.IsVisited = false;
                }
            }
        }

        public float GetMapScale()
        {
            switch (_map)
            {
                case Map.Test:
                    return 1;
                case Map.AlienIsolation:
                    return 2;
                case Map.Arkham:
                    return 2;
                case Map.MsgDock:
                    return 2;
                default:
                    return 1;
            }
        }

        public Vector2Int WorldToGrid(Vector2 worldPosition)
        {
            return _grid.WorldToGrid(worldPosition);
        }

        public void DrawPath(Stack<Node> path)
        {
            _grid.DrawPath(path);
        }

        //TODO delete
        public void DEBUGPATH(string groupName)
        {

        }

        protected override void Initialize()
        {
            EventManager.GridGeneratedEvent += OnGridGenerated;
            EventManager.MapGeneratedEvent += OnMapGenerated;
            CreateMapData();
        }

        private void OnMapGenerated(object sender, EventArgs e)
        {
            _mapGenerated = true;
            CheckIfReady();
        }

        private void OnGridGenerated(object sender, EventArgs e)
        {
            _gridGenerated = true;
            CheckIfReady();
            
            /*
            var gridPosition = WorldToGrid(new Vector2(-5.75f, -0.1f));
            Debug.Log($"GridPosition of (-5.75, -0.1) {gridPosition}");
            Debug.Log($"World position of {gridPosition} {GridToWorld(gridPosition)}");
            Debug.Log("---------------");
            gridPosition = WorldToGrid(new Vector2(-5.9f, 5));
            Debug.Log($"GridPosition of (-5.9, 5) {gridPosition}");
            Debug.Log($"World position of {gridPosition} {GridToWorld(gridPosition)}");
            Debug.Log("---------------");
            gridPosition = WorldToGrid(new Vector2(-7.1f, 2.45f));
            Debug.Log($"GridPosition of (-7.1, 2.45) {gridPosition}");
            Debug.Log($"World position of {gridPosition} {GridToWorld(gridPosition)}");
            Debug.Log("---------------");
            gridPosition = WorldToGrid(new Vector2(-1.25f, -0.1f));
            Debug.Log($"GridPosition of (-1.25, -0.1) {gridPosition}");
            Debug.Log($"World position of {gridPosition} {GridToWorld(gridPosition)}");
            Debug.Log("---------------");
            gridPosition = WorldToGrid(new Vector2(5.2f, -2.05f));
            Debug.Log($"GridPosition of (-5.75, -0.1) {gridPosition}");
            Debug.Log($"World position of {gridPosition} {GridToWorld(gridPosition)}");
            Debug.Log("---------------");
            gridPosition = WorldToGrid(new Vector2(-5.75f, -0.1f));
            Debug.Log($"GridPosition of (5.2, -2.05) {gridPosition}");
            Debug.Log($"World position of {gridPosition} {GridToWorld(gridPosition)}");
            Debug.Log("---------------");
            gridPosition = WorldToGrid(new Vector2(-4.85f, -3.55f));
            Debug.Log($"GridPosition of (-4.85, -3.55) {gridPosition}");
            Debug.Log($"World position of {gridPosition} {GridToWorld(gridPosition)}");
            Debug.Log("---------------");*/
            
        }

        private void CheckIfReady()
        {
            Ready = _mapGenerated && _gridGenerated;
        }

        private void CreateMapData()
        {
            var polygonReader = new PolygonDataReader();
            MapData = polygonReader.Polygons;
        }

        public enum Map
        {
            AlienIsolation,
            Arkham,
            MsgDock,
            Test,
            Test_Diagonal,
            Demo,
            Small_Linear,
            FourCorners,
        }
    }
}
