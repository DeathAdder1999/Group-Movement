using System.Collections;
using System.Collections.Generic;
using NPCs.GridBased;
using Pathfinding;
using UnityEngine;
using Grid = UnityEngine.Grid;

namespace NPCs
{
    public class PatrolBehaviour : MonoBehaviour
    {
        private GridMovement _gridMovement;

        void Awake()
        {
            _gridMovement = GetComponent<GridMovement>();
        }

        void Update()
        {
            if (_gridMovement.HasPath)
            {
                UpdateDestination();
            }
        }

        private void UpdateDestination()
        {
            Debug.Log($"Updating Destination CurrentNode: {_gridMovement.CurrentNode}");
            if (_gridMovement.CurrentNode == _gridMovement.Destination2)
            {
                Debug.Log("Setting path to destination1");
                _gridMovement.SetDestination(_gridMovement.Destination1);
            }
            else if (_gridMovement.CurrentNode == _gridMovement.Destination1)
            {
                Debug.Log("Setting path to dest2");
                _gridMovement.SetDestination(_gridMovement.Destination2);
            }
        }
    }
}
