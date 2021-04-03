using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace Pathfinding
{

    public class Path
    {
        private Stack<PathNode> _path;
        public bool IsEmpty => _path?.Count == 0;
        public int Length => IsEmpty ? 0 : _path.Count;

        public List<Node> Nodes
        {
            get
            {
                var list = _path?.ToList();
                var nodes = new List<Node>();

                if (list != null)
                {
                    foreach (var node in list)
                    {
                        nodes.Add(node.Node);
                    }
                }

                return nodes;
            }
        }

        public bool Contains(Node node)
        {
            if (IsEmpty)
            {
                return false;
            }

            foreach (var pathNode in _path)
            {
                if (pathNode.Node == node)
                {
                    return true;
                }
            }

            return false;
        }

        public Path([NotNull] Stack<Node> path)
        {
            _path = new Stack<PathNode>();
            var pathList = path.ToList();
            pathList.Reverse();

            var time = pathList.Count - 1;

            foreach (var node in pathList)
            {
                _path.Push(new PathNode(node, time));
                time --;
            }
        }

        public Node Pop()
        {
            if (IsEmpty)
            {
                return null;
            }

            var node = _path.Pop().Node;
            OnPop();

            return node;
        }

        private void OnPop()
        {
            foreach (var pathNode in _path)
            {
                pathNode.TimeStep--;

                //TODO delete
                Assert.IsTrue(pathNode.TimeStep >= 0);
            }
        }

        public Node Peek()
        {
            if (IsEmpty)
            {
                return null;
            }

            return _path.Peek().Node;
        }

        public bool HasIntersection(int time)
        {
            return GetNodeAtTime(time) != null;
        }

        public bool Intersects(Path otherPath)
        {
            var lastTimeStamp = Length > otherPath.Length ? Length : otherPath.Length;

            for (var i = 0; i <= lastTimeStamp; i++)
            {
                if (GetNodeAtTime(i) == otherPath.GetNodeAtTime(i))
                {
                    return true;
                }
            }

            return false;
        }

        public Node GetNodeAtTime(int time)
        {
            if (!IsEmpty)
            {
                foreach (var node in _path)
                {
                    if (node.TimeStep == time)
                    {
                        return node.Node;
                    }
                }
            }

            return null;
        }

        public Vector2 GetMovementDirection()
        {
            if (IsEmpty)
            {
                return new Vector2();
            }

            return _path.Last().Node.Center - _path.First().Node.Center;
        }

        public PathNode GetTheLastExpectedPosition()
        {
            if (IsEmpty)
            {
                return null;
            }

            return _path.Last();
        }

        public class PathNode
        {
            public Node Node { get; private set; }
            public int TimeStep { get; set; }

            public PathNode(Node node, int timeStep)
            {
                TimeStep = timeStep;
                Node = node;
            }
        }
    }
}
