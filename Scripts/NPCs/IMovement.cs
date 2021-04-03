using System.Collections.Generic;
using Managers;
using Pathfinding;
using UnityEngine;

namespace NPCs
{
    public interface IMovement
    {
        Group ParentGroup { get; set; }
        bool Enabled { get; set; }
        Vector2 Position { get; }
        void SetDestination(Vector2Int destination);
        void SetDestination(Vector2 destination);
        void SetPath(Stack<Node> path);
    }
}
