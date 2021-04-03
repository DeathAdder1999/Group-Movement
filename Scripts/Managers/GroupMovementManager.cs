using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Main;
using NPCs;
using NPCs.Steering;
using Pathfinding;
using UnityEngine;
using Utility;
using Node = Pathfinding.Node;

namespace Managers
{
    public class GroupMovementManager : AbstractSingleton<GroupMovementManager>
    {
        private Dictionary<string, Group> _groups;

        [SerializeField] private int _numGroups;
        [SerializeField] private bool _checkWholePath;
        [SerializeField] private float _maxRaycastDistance;
        private string[] _groupTags;
        
        [Header("Debug")]
        [SerializeField] private string _groupName;
        [SerializeField] private Vector2 _destination;
        [SerializeField] private bool _setDestination;
        [SerializeField] private bool _drawDebugCenter;
        [SerializeField] private bool _drawDebugPath;
        [SerializeField] private bool _drawBoundingBox;
        [SerializeField] private bool _drawMovementDirection;

        public int NumberOfGroups => _groups?.Count ?? 0;
        public bool DrawBoundingBox => _drawBoundingBox;
        public bool DrawMovementDirection => _drawMovementDirection;
        public float MaxRaycastDistance => _maxRaycastDistance;

        private void Start()
        {
            if (_maxRaycastDistance.Equals(0))
            {
                _maxRaycastDistance = 1.0f;
            }
        }

        void Update()
        {
            if (_setDestination)
            {
                SetDestination(_destination, _groupName);
                _setDestination = false;
            }
        }

        void FixedUpdate()
        {
            if (_groups != null)
            {
                foreach (var group in _groups.Values)
                {
                    group.Update();
                }
            }
        }

        protected override void Initialize()
        {
            _groupTags = new string[_numGroups];

            for (var i = 0; i < _numGroups; i++)
            {
                _groupTags[i] = $"G_{i}";
            }

            _groups = new Dictionary<string, Group>();
            EventManager.PlayersReadyEvent += OnPlayersReady;
        }

        private void OnPlayersReady(object sender, EventArgs e)
        {
            PopulateGroups();
        }

        public void SetDestination(Vector2 destination, string groupName)
        {
            _groups.TryGetValue(groupName, out var group);

            if (group == null)
            {
                throw new ArgumentException($"Group {groupName} does not exist");
            }

            SetDestination(destination, group);
        }

        public void SetDestination(Vector2 destination, Group group, GridRestriction gridRestriction = null)
        {
            //First find path from the center to the destination
            //Assumption is that all of the agents in groups are friendly npcs
            var start = GameManager.Instance.WorldToGrid(group.Center);

            var end = GameManager.Instance.WorldToGrid(destination);
            var destinationNode = GameManager.Instance.GridMap[end.x][end.y];
            var startNode = GameManager.Instance.GridMap[start.x][start.y];
            Path path = null;
            Stack<Node> potentialPath = null;

            //If the destination is already in a path of some other group change your final destination
            if (OtherGroupIsOnWayTo(destinationNode, group))
            {
                path = GetPathInDirection(startNode, destinationNode, group, GetVisitedMap(), gridRestriction);
            }

            if (path == null)
            {
                potentialPath = Pathfinder.FindPath(start, end, GameManager.Instance.GridMap, true);
            }

            //Debug.Log($"Group {groupName} Start {start} End {end}");

            if (path == null && potentialPath == null)
            {
                Debug.LogWarning("Path does not exist");
                return;
            }

            path = path ?? new Path(potentialPath);
            group.Path = path;

            MakeWayForGroup(group, gridRestriction);
        }

        //Done to demonstrate the naive approach
        public void SetDestinationGrid(Vector2 destination, string groupName)
        {
            _groups.TryGetValue(groupName, out var group);

            if (group == null)
            {
                throw new ArgumentException($"Group {groupName} does not exist");
            }

            var startGrid = group.CurrentNode;
            var destinationGrid = GameManager.Instance.WorldToGrid(destination);
            var path = Pathfinder.FindPath(startGrid, destinationGrid, GameManager.Instance.GridMap, true);

            if (path == null)
            {
                Debug.LogWarning("Path does not exist");
                return;
            }

            foreach (var otherGroup in _groups.Values)
            {
                if (otherGroup == group)
                {
                    continue;
                }

                foreach (var node in path)
                {
                    if (otherGroup.BoundingBoxContains(node.Center))
                    {
                        if (otherGroup.IgnoreMoveGroupOutTheWay)
                        {
                            continue;
                        }

                        otherGroup.IgnoreMoveGroupOutTheWay = true;
                        var centroid = GetCentroid();
                        var movementDirection = path.Last().Center - path.First().Center;
                        var direction = movementDirection - centroid;

                        //default will size of the vector will be 2
                        direction.Normalize();
                        direction *= 2;
                        var otherDestination = group.Center + direction;
                        SetDestinationGrid(otherDestination, otherGroup.Name);
                        otherGroup.IgnoreMoveGroupOutTheWay = false;
                    }
                }
            }

            group.SetDestination(destination);
        }

        private bool OtherGroupIsOnWayTo(Node node, Group thisGroup)
        {
            foreach (var group in _groups.Values)
            {
                if (group == thisGroup)
                {
                    continue;
                }

                if (!group.IsAt(node) && group.Path != null && group.Path.Contains(node))
                {
                    return true;
                }
            }

            return false;
        }

        public bool[][] GetVisitedMap()
        {
            var map = new bool[GameManager.Instance.GridMap.Length][];

            for (var i = 0; i < GameManager.Instance.GridMap.Length; i++)
            {
               map[i] = new bool[GameManager.Instance.GridMap[0].Length];
            }

            return map;
        }

        public bool ContainsGroupTag(string groupTag)
        {
            return _groupTags.Contains(groupTag);
        }

        public IEnumerable<Group> GetGroups()
        {
            return _groups.Values.ToList();
        }

        public string GetGroupTag(int index)
        {
            return _groupTags[index];
        }

        public Group GetGroup(string name)
        {
            if (_groups.TryGetValue(name, out var group))
            {
                return group;
            }

            return null;
        }

        private void MakeWayForGroup(Group group, GridRestriction gridRestriction)
        {
            if (group.Path == null)
            {
                return;
            }

            foreach (var kvp in _groups)
            {
                var otherGroup = kvp.Value;

                if (group != otherGroup)
                {
                    var intersectionNode = group.FindIntersection(otherGroup);

                    if (intersectionNode != null)
                    {
                        group.IgnoreMoveGroupOutTheWay = true;
                        MoveGroupOutTheWay(otherGroup, group.Path.GetMovementDirection(), gridRestriction);
                        group.IgnoreMoveGroupOutTheWay = false;
                    }
                }
            }
        }

        private Vector2 GetCentroid()
        {
            var center = new Vector2();

            foreach (var group in _groups.Values)
            {
                center += group.Center;
            }

            center /= _groups.Count;

            return center;
        }

        public void MoveGroupOutTheWay(Group group, Vector2 movementDirection, GridRestriction gridRestriction)
        {
            if (group.IgnoreMoveGroupOutTheWay || group.Path != null)
            {
                return;
            }

            //Find the largest density of the groups on the map
            var centroid = GetCentroid();
            var direction = movementDirection - centroid;

            //default will size of the vector will be 2
            direction.Normalize();
            direction *= 2;
            var destination = group.Center + direction;
            var startGrid = GameManager.Instance.WorldToGrid(group.Center);
            var endGrid = GameManager.Instance.WorldToGrid(destination);
            var grid = GameManager.Instance.GridMap;

            //if()
            //var path = Pathfinder.FindPath(startGrid, endGrid, grid, true);
            Stack<Node> path = null;

            //if centroid method fails move to an empty cell
            if (path == null)
            {
                var neighbours = group.GetNeighbouringCells();
                var startNode = GameManager.Instance.GridMap[startGrid.x][startGrid.y];

                foreach (var neighbour in neighbours)
                {
                    var potentialPath = GetPathInDirection(startNode, neighbour, group, GetVisitedMap(), gridRestriction);

                    if (potentialPath != null)
                    {
                        group.Path = potentialPath;
                        break;
                    }
                }
            }
            else
            {
                group.Path = new Path(path);
            }

            MakeWayForGroup(group, gridRestriction);
        }

        public void AddToGroup(string groupName, IMovement movement)
        {
            var group = GetOrCreate(groupName);
            group.Add(movement);
        }

        public Path GetPathInDirection(Node fromNode, Node toNode, Group group, bool[][] visitedMap, GridRestriction gridRestriction = null)
        {
            Path path = null;
            var queue = new Queue<Node>();

            queue.Enqueue(toNode);

            while (queue.Any())
            {
                //If path found stop search
                if (path != null)
                {
                    break;
                }

                var node = queue.Dequeue();

                //If already visited the node skip to the next one
                if (visitedMap[node.GridPosition.x][node.GridPosition.y])
                {
                    continue;
                }


                if (node.IsPlayerWalkable && !Intersects(group, node))
                {
                    var potentialPath = Pathfinder.FindPath(fromNode.GridPosition, node.GridPosition,
                        GameManager.Instance.GridMap, true);

                    if (potentialPath != null && (gridRestriction == null || gridRestriction.IsValid(node)))
                    {
                        path = new Path(potentialPath);
                    }
                }

                visitedMap[node.GridPosition.x][node.GridPosition.y] = true;

                var neighbours = node.GetWalkableNeighbours();

                foreach (var neighbour in neighbours)
                {
                    queue.Enqueue(neighbour);
                }
            }

            return path;
        }

        private bool Intersects(Group group, Node node)
        {
            //Heuristic approach to get the time stamp
            var minDistance = Utils.GetMinimumDistance(group.CurrentNode, node.GridPosition);            
            var maxDistance = Utils.GetMaximumDistance(group.CurrentNode, node.GridPosition);            

            foreach (var otherGroup in _groups.Values)
            {
                if (group == otherGroup)
                {
                    continue;
                }

                //The group is stationary
                if ((otherGroup.Path == null || otherGroup.Path.IsEmpty) && otherGroup.IsAt(node))
                {
                    return true;
                }

                /*
                //The group contains it on it's path
                if (otherGroup.Path != null)
                {
                    for (var i = minDistance - 1; i < maxDistance; i++)
                    {
                        if (otherGroup.Path.GetNodeAtTime(i) == node)
                        {
                            return true;
                        }
                    }
                }*/

                var intersects = false;

                if(otherGroup.Path != null)
                {
                    intersects = _checkWholePath ? otherGroup.Path.Contains(node) : otherGroup.Path.GetTheLastExpectedPosition().Node == node;
                }
                
                if (intersects)
                {
                    return true;
                }
            }

            return false;
        }

        private Group GetOrCreate(string groupName)
        {
            if (!_groups.TryGetValue(groupName, out var group))
            {
                group = new Group(groupName);
                _groups.Add(groupName, group);
            }

            return group;
        }

        //TODO can be optimized by having a "Taken" flag in a Node class
        private bool IsTaken(Node node)
        {
            foreach (var group in _groups.Values)
            {
                if (group.CurrentNode == node.GridPosition)
                {
                    return true;
                }
            }

            return false;
        }

        private void PopulateGroups()
        {
            foreach (var groupTag in _groupTags)
            {
                var group = GameObject.FindGameObjectsWithTag(groupTag);
                var movementGroup = new Group(groupTag) {MaxDistanceToCenter = 4f};

                foreach (var member in group)
                {
                    if (!member.activeSelf)
                    {
                        continue;
                    }

                    var movement = member.GetComponent<IMovement>();

                    if (movement == null)
                    {
                        Debug.Log($"MOVEMENT IS NULL {member.name}");
                    }

                    movement.ParentGroup = movementGroup;
                    movementGroup.Add(movement);
                }

                _groups.Add(groupTag, movementGroup);
            }
        }

        private void OnDrawGizmos()
        {
            if (_groups == null)
            {
                return;
            }

            Gizmos.color = Color.black;
            foreach (var group in _groups.Values)
            {
                if (group.IsEmpty)
                {
                    continue;
                }

                if (_drawDebugCenter)
                {
                    Gizmos.DrawCube(new Vector3(group.Center.x, group.Center.y, 0), new Vector3(0.15f, 0.15f, 0.1f));
                }

                if (group.Path != null && _drawDebugPath)
                {
                    Gizmos.color = Color.red;
                    foreach (var node in group.Path.Nodes)
                    {
                        Gizmos.DrawCube(node.Center, new Vector3(GameManager.Instance.NodeWidth, GameManager.Instance.NodeHeight, 0.1f));
                    }                   
                }

                group.OnDrawGizmos();
            }
        }
    }

    public class Group
    {
        private List<IMovement> _movements;
        private Vector2 _center;
        private Vector2? _target;
        private Vector2Int _currentNode;
        private float _minX = float.MaxValue;
        private float _maxX = float.MinValue;
        private float _minY = float.MaxValue;
        private float _maxY = float.MinValue;
        private Bounds _bounds;
        private bool _enableMovement = true;

        //For debug purposes only
        public string Name { get; private set; }
        public int PathLength => Path?.Length ?? 0;
        public bool IgnoreMoveGroupOutTheWay { get; set; }

        public Vector2 Center => new Vector2(_center.x, _center.y);
        public bool IsEmpty => _movements?.Count == 0;
        public float MaxDistanceToCenter { get; set; }

        public EventHandler TargetReached;
        public EventHandler GroupLimitReached;
        public Bounds Bounds => _bounds;

        public Vector2Int CurrentNode => GameManager.Instance.WorldToGrid(Center);

        public bool EnableMovement
        {
            get => _enableMovement;
            set
            {
                foreach (var movement in _movements)
                {
                    movement.Enabled = value;
                }

                _enableMovement = value;
            }
        }

        private Path _path;

        public Path Path
        {
            get => _path;
            set
            {
                if (_path != value)
                {
                    GameManager.Instance.DEBUGPATH(Name);
                }

                _path = value;
                
            }
        }

        public Group(IEnumerable<IMovement> movements)
        {
            _movements = new List<IMovement>(movements);
            CalculateBounds();
            GroupLimitReached?.Invoke(this, EventArgs.Empty);
        }

        public Group(string name)
        {
            Name = name;
            _movements = new List<IMovement>();
        }

        public void Add(IMovement movement)
        {
            _movements.Add(movement);
            CalculateBounds();

            if (_movements.Count == Properties.GroupSize)
            {
                GroupLimitReached?.Invoke(this, EventArgs.Empty);
            }
        }

        public List<Node> GetNeighbouringCells()
        {
            var grid = GameManager.Instance.GridMap;
            var currentNode = grid[_currentNode.x][_currentNode.y];
            return currentNode.GetWalkableNeighbours();
        }

        public bool IsAt(Node node)
        {
            return _currentNode == node.GridPosition;
        }

        private void CalculateBounds()
        {
            _minX = float.MaxValue;
            _maxX = float.MinValue;
            _minY = float.MaxValue;
            _maxY = float.MinValue;

            foreach (var movement in _movements)
            {
                var position = movement.Position;

                if (_minX > position.x)
                {
                    _minX = position.x;
                }

                if (_minY > position.y)
                {
                    _minY = position.y;
                }

                if (position.x > _maxX)
                {
                    _maxX = position.x;
                }

                if (position.y > _maxY)
                {
                    _maxY = position.y;
                }
            }
            
            _center = new Vector2((_minX + _maxX) / 2.000f, (_minY + _maxY) / 2.000f);
            _currentNode = GameManager.Instance.WorldToGrid(_center);
            _bounds = new Bounds(_center, new Vector3(_maxX - _minX, _maxY - _minY));
        }

        public void Update()
        {
            if (!IsEmpty)
            {
                CalculateBounds();
            }

            if (EnableMovement)
            {
                FollowPath();
            }
        }

        private void FollowPath()
        {
            if (Path != null && !Path.IsEmpty)
            {
                _target = _target ?? Path.Peek().Center;

                if (Utils.IsCloserThan(_center, _target.Value, SteeringProperties.PathFollowingRadius))
                {
                    _target = Path.Pop().Center;
                }
            }
            else
            {
                if (_target != null)
                {
                    if (!Utils.IsCloserThan(_center, _target.Value, SteeringProperties.MinimumStopDistance))
                    {
                        //Do not stop till the destination is reached
                        return;
                    }

                    TargetReached?.Invoke(this, EventArgs.Empty);
                }
                
                _target = null;
                Path = null;
            }

            if (_target != null)
            {
                SetDestination(_target.Value);
            }
        }

        public Node GetPositionAt(int time)
        {
            //Get latest
            var latest = Path?.GetTheLastExpectedPosition();

            if (latest == null)
            {
                return GameManager.Instance.GridMap[_currentNode.x][_currentNode.y];
            }

            var node = latest.Node;

            if (latest.TimeStep > time)
            {
                node = Path.GetNodeAtTime(time);
            }

            return node;
        }

        public bool BoundingBoxContains(Vector2 point)
        {
            return _minX < point.x && _maxX > point.x && _minY < point.y && _maxY > point.y;
        }

        public Path.PathNode FindIntersection(Group other)
        {
            var count = other.PathLength > PathLength ? other.PathLength : PathLength;

            for (var t = 0; t < count; t++)
            {
                var position = GetPositionAt(t);
                var otherPosition = other.GetPositionAt(t);

                if (position != null && position == otherPosition)
                {
                    return new Path.PathNode(position, t);
                }
            }

            return null;
        }

        public void SetDestination(Vector2 destination)
        {
            foreach (var movement in _movements)
            {
                movement.SetDestination(destination);
            }
        }

        public List<Collider2D> Raycast(Vector2 direction)
        {
            var point1 = new Vector2(_maxX, _maxY);
            var point2 = new Vector2(_minX, _minY);
            var list = new List<Collider2D>();

            var hit = Physics2D.Raycast(_center, direction, 100, LayerMask.NameToLayer("Player"));

            if (hit.collider != null && !list.Contains(hit.collider))
            {
                list.Add(hit.collider);
            }

            hit = Physics2D.Raycast(point1, direction, 100, LayerMask.NameToLayer("Player"));

            if (hit.collider != null && !list.Contains(hit.collider))
            {
                list.Add(hit.collider);
            }

            hit = Physics2D.Raycast(point2, direction, 100, LayerMask.NameToLayer("Player"));

            if (hit.collider != null && !list.Contains(hit.collider))
            {
                list.Add(hit.collider);
            }

            return list;
        }

        public List<Group> RaycastGroups(Vector2 direction)
        {
            var point1 = new Vector2(_maxX, _maxY);
            var point2 = new Vector2(_minX, _minY);
            var groups = new List<Group>();

            //TODO remove direction
            direction = _path.GetMovementDirection();

            var rayFromCenter = new Ray(_center, direction);
            var rayFromPoint1 = new Ray(point1, direction);
            var rayFromPoint2 = new Ray(point2, direction);

            foreach(var group in GroupMovementManager.Instance.GetGroups())
            {
                if (group == this || (group.Center - Center).sqrMagnitude > GroupMovementManager.Instance.MaxRaycastDistance)
                {      
                    continue;
                }

                if (group.Bounds.IntersectRay(rayFromCenter) || group.Bounds.IntersectRay(rayFromPoint1) || group.Bounds.IntersectRay(rayFromPoint2))
                {
                    groups.Add(group);
                }
            }

            return groups;
        }

        public void OnDrawGizmos()
        {
            if (_target != null)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawCube(_target.Value, new Vector3(GameManager.Instance.NodeWidth, GameManager.Instance.NodeHeight, 0.01f));
            }

            if (GroupMovementManager.Instance.DrawBoundingBox)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawCube(Center, new Vector3(_maxX - _minX, _maxY - _minY, 0.01f));
            }

            if (GroupMovementManager.Instance.DrawMovementDirection && _path != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(new Ray(_center, _path.GetMovementDirection()));
            }
        }
    }
}


