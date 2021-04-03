using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPCs
{
    public interface IAgentController
    {
        void SetDestination(Vector2Int destinationGrid);
        void SetDestination(Vector2 destinationWorld);
    }
}
