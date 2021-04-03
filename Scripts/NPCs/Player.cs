using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using NPCs.GridBased;
using UnityEngine;

namespace NPCs
{

    public class Player : MonoBehaviour, IAgentController
    {
        private IMovement _movement;
        [SerializeField] private GameObject _avoidanceCollider;

        void Awake()
        {
            _movement = GetComponent<IMovement>();
            _movement.Enabled = true;
            gameObject.layer = LayerMask.NameToLayer("Player");
            _avoidanceCollider.gameObject.layer = LayerMask.NameToLayer("Player");
        }

        void Start()
        {
            EventManager.GameMangerReadyEvent += OnGameManagerReady;
            EventManager.SnapshotMergedEvent += OnSnapshotMergedEvent;
        }

        private void OnSnapshotMergedEvent(object sender, EventArgs e)
        {
            _movement.Enabled = true;
        }

        private void OnGameManagerReady(object sender, EventArgs e)
        {
            NPCManager.Instance.AddPlayer(this);
        }

        public void SetDestination(Vector2Int destinationGrid)
        {
            _movement.SetDestination(destinationGrid);
        }

        public void SetDestination(Vector2 destinationWorld)
        {
            _movement.SetDestination(destinationWorld);
        }
    }
}
