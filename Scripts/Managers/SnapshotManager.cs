using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Main;
using Pathfinding;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Grid = Pathfinding.Grid;

namespace Managers
{
    public class SnapshotManager : AbstractSingleton<SnapshotManager>
    {
        //Trigger to generate the end map
        [SerializeField] private bool _mergeSnapshots;
        //Trigger to clear all the existing data
        [SerializeField] private bool _clearSnapshots;

        //Contains snapshots of the grid
        private List<bool[][]> _snapShots;

        private void OnGameManagerReady(object sender, EventArgs e)
        {
            Initialize();
        }

        void Update()
        {
            if (_mergeSnapshots)
            {
                MergeSnapshots();
                _mergeSnapshots = false;
            }
        }

        private void MergeSnapshots()
        {
            if (!_snapShots.Any())
            {
                return;
            }

            foreach (var snapshot in _snapShots)
            {
                MergeSnapshot(snapshot);
            }

            EventManager.RaiseSnapshotMergedEvent(Instance, EventArgs.Empty);
        }

        private void MergeSnapshot(bool[][] snapshot)
        {
            var map = GameManager.Instance.GridMap;
            var columnLength = map[0].Length;

            for (var row = 0; row < map.Length; row++)
            {
                for (var column = 0; column < columnLength; column++)
                {
                    GameManager.Instance.GridMap[row][column].IsPlayerWalkable =
                        GameManager.Instance.GridMap[row][column].IsPlayerWalkable && snapshot[row][column];
                }
            }
        }

        protected override void Initialize()
        {
            _snapShots = new List<bool[][]>();
            EventManager.MapPassabilityChangedEvent += OnMapPassabilityChanged;
            EventManager.GameMangerReadyEvent += OnGameManagerReady;
        }

        private void OnMapPassabilityChanged(object sender, EventArgs e)
        {
            var snapShot = new bool[Properties.GridRows][];

            for(var row = 0; row < Properties.GridRows; row++)
            {
                snapShot[row] = new bool[Properties.GridColumns];
            }

            for (var row = 0; row < Properties.GridRows; row++)
            {
                for (var column = 0; column < Properties.GridColumns; column++)
                {
                    snapShot[row][column] = GameManager.Instance.GridMap[row][column].IsPlayerWalkable;
                }
            }

            _snapShots.Add(snapShot);
            Debug.Log("Adding snapshot");
        }
    }
}
