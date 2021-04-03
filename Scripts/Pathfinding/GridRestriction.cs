using UnityEngine;

namespace Pathfinding
{
    public class GridRestriction
    {
        public Vector2Int XRestriction { get; private set; }
        public Vector2Int YRestriction { get; private set; }

        public GridRestriction(Vector2Int xRestriction, Vector2Int yRestriction)
        {
            XRestriction = xRestriction;
            YRestriction = yRestriction;
        }

        public bool IsValid(Node node)
        {
            var nodePosition = node.GridPosition;
            return nodePosition.x <= XRestriction.y && nodePosition.x >= XRestriction.x &&
                   nodePosition.y <= YRestriction.y && nodePosition.y >= YRestriction.x;
        }
    }
}
