using Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Pathfinding;
using UnityEngine;
using Group = Managers.Group;

namespace Scenario
{
    public class CrowdedScenario : AbstractScenario
    {
        [Header("Scenario Parameters")]
        [SerializeField] private float _waitTime;
        [SerializeField] private float _maxWaitTime;
        [SerializeField] private bool _useWait;
        [SerializeField] private bool _useTargetPoint;

        [Header("Scenario Destination")]
        [SerializeField] private bool _center;
        [SerializeField] private bool _topLeftCorner;
        [SerializeField] private bool _random;

        [Header("Scenario Distance")]
        [SerializeField] private bool _euclidian;
        [SerializeField] private bool _manhattan;
        [SerializeField] private bool _chebyshev;

        [Header("Scenario Map")]
        [SerializeField] private Vector2Int _gridXConstraints;
        [SerializeField] private Vector2Int _gridYConstraints;


        void Update()
        {
            if (_center)
            {
                _topLeftCorner = false;
                _random = false;
            }

            if (_random)
            {
                _topLeftCorner = false;
                _center = false;
            }

            if (_topLeftCorner)
            {
                _random = false;
                _center = false;
            }
        }

        public override void Run()
        {
            var target = new Vector2();

            if (_center)
            {
                var pos = new Vector2Int((_gridXConstraints.x + _gridXConstraints.y) / 2, (_gridYConstraints.x + _gridYConstraints.y) / 2);
                target = GameManager.Instance.GridMap[pos.x][pos.y].Center;
            }

            if (_topLeftCorner)
            {
                var pos = new Vector2Int(_gridXConstraints.y, _gridYConstraints.y);
                target = GameManager.Instance.GridMap[pos.x][pos.y].Center;
            }

            if (_random)
            {
                target = Utils.GetRandomVector(new Vector2(1.85f, 7), new Vector2(4, -4));
            }

            if (_useTargetPoint)
            {
                StartCoroutine(SetTarget(target));                
            }
            else
            {
                var groups = GroupMovementManager.Instance.GetGroups().ToArray();
                var targetMap = GetTargetMap();
                var groupsCenter = new Vector2();

                //Find the centroid of the group
                foreach (var group in groups)
                {
                    groupsCenter += group.Center;
                }

                groupsCenter /= groups.Length;

                //find the closest node to the centroid
                var closestNode = targetMap[0][0];

                foreach (var row in targetMap)
                {
                    foreach (var node in row)
                    {
                        if ((node.Center - groupsCenter).sqrMagnitude <
                            (closestNode.Center - groupsCenter).sqrMagnitude)
                        {
                            closestNode = node;
                        }
                    }
                }

                StartCoroutine(SetGroupsDestination(closestNode, groups, closestNode.Center - groupsCenter));
            }
        }

        private float CalculateDistance(Vector2 target, Group g2)
        {
            var targetGrid = GameManager.Instance.WorldToGrid(target);
            var g2Grid = g2.CurrentNode;

            if (_euclidian)
            {
                return (target - g2.Center).sqrMagnitude;
            }

            if (_manhattan)
            {
                return Mathf.Abs(targetGrid.x - g2Grid.x) + Mathf.Abs(targetGrid.y - g2Grid.y);
            }

            if (_chebyshev)
            {
                return Mathf.Max(Mathf.Abs(targetGrid.x - g2Grid.x), Mathf.Abs(targetGrid.y - g2Grid.y));
            }


            throw new ArgumentException("Distance type is not set!");
        }

        private IEnumerator SetTarget(Vector2 target)
        {
            var groups = GroupMovementManager.Instance.GetGroups();

            //Sort the closest to the target
            groups = groups.OrderBy(g => CalculateDistance(target, g)).ToArray();

            foreach (var group in groups)
            {
                GroupMovementManager.Instance.SetDestination(target, group.Name);
                yield return _useWait ? new WaitForSeconds(_waitTime) : null;
            }
        }

        //Target section of a map
        private Node[][] GetTargetMap()
        {
            var numRows = _gridXConstraints.y - _gridXConstraints.x;
            var numColumns = _gridYConstraints.y - _gridYConstraints.x;
            var targetMap = new Node[numRows][];

            for (var i = 0; i < numRows; i++)
            {
                targetMap[i] = new Node[numColumns];

                for (var j = 0; j < numColumns; j++)
                {
                    targetMap[i][j] = GameManager.Instance.GridMap[_gridXConstraints.x + i][_gridYConstraints.x + j];
                }
            }

            return targetMap;
        }

        /// <summary>
        /// Moves all groups to the target map
        /// </summary>
        /// <param name="node"> Closest node to the group</param>
        private IEnumerator SetGroupsDestination(Node node, IEnumerable<Group> groups, Vector2 direction)
        {
            //Sort the closest to the target
            var groupsArray = groups.OrderBy(g => CalculateDistance(node.Center, g)).ToArray();
            var groupsByDistance = groupsArray.ToList();

            /*
            //The groups will be grouped in larger groups to make sure that the movement is as smooth as possible
            var movementGroups = CreateMovementGroups(direction, groupsByDistance);
            */

            groupsByDistance.Reverse();
            foreach (var group in groupsByDistance)
            {
                group.EnableMovement = false;
                GroupMovementManager.Instance.SetDestination(node.Center, group, new GridRestriction(_gridXConstraints, _gridYConstraints));
            }

            //The groups will be grouped in larger groups to make sure that the movement is as smooth as possible
            groupsByDistance.Reverse();
            var movementGroups = CreateMovementGroups(direction, groupsByDistance);
            groupsByDistance.Reverse();

            for (var i = 0; i < movementGroups.Count; i++)
            {
                var waitTime = i == 0 ? 0 : 1/GetDistanceBetweenMovementGroups(movementGroups[i], movementGroups[i - 1]);
                waitTime = waitTime < _waitTime ? _waitTime : waitTime;

                foreach (var group in movementGroups[i])
                {
                    //Initiate movement
                    group.EnableMovement = true;
                }

                var totalWait = _waitTime * waitTime;
                totalWait = totalWait > _maxWaitTime ? _maxWaitTime : totalWait;
                
                yield return _useWait ? new WaitForSeconds(totalWait) : null;
            }
        }

        private float GetDistanceBetweenMovementGroups(List<Group> group1, List<Group> group2)
        {
            var group1Centroid = new Vector2();
            var group2Centroid = new Vector2();

            foreach (var group in group1)
            {
                group1Centroid += group.Center;
            }

            foreach (var group in group2)
            {
                group2Centroid += group.Center;
            }

            group1Centroid /= group1.Count;
            group2Centroid /= group2.Count;

            return (group1Centroid - group2Centroid).magnitude;
        }

        private List<List<Group>> CreateMovementGroups(Vector2 direction, List<Group> groups)
        {
            var movementGroups = new List<List<Group>>();

            foreach (var group in groups)
            {
                //raycast a group from 3 directions, center and sides in the direction
                var raycastResult = group.RaycastGroups(direction);

                //If nobody in front => first group
                if (raycastResult.Count == 0)
                {
                    if (movementGroups.Count == 0)
                    {
                        movementGroups.Add(new List<Group>());
                    }

                    movementGroups[0].Add(group);
                }
                else
                {
                    //TODO find a better strategy
                    //Use one result only
                    for (var i = 0; i < movementGroups.Count; i++)
                    {
                        if (movementGroups[i].Contains(raycastResult[0]) && movementGroups.Count > i + 1)
                        {
                            movementGroups[i + 1].Add(group);
                            break;
                        }
                        else if (movementGroups[i].Contains(raycastResult[0]))
                        {
                            movementGroups.Add(new List<Group>());
                            movementGroups[i + 1].Add(group);
                            break;
                        }
                    }
                }
            }

            Debug.Log($"Movement groups {movementGroups.Count}");
            return movementGroups;
        }
    }
}
