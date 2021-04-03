using System;
using System.Collections.Generic;
using System.Linq;
using Main;
using Managers;
using UnityEngine;

namespace Pathfinding
{
    public static class Pathfinder
    {
        private static int StraightCost = 10;
        private static int DiagonalCost = 14;

        public static Stack<Node> FindPath(Vector2Int start, Vector2Int end, Node[][] grid, bool player)
        {
            var startNode = grid[start.x][start.y];
            var destinationNode = grid[end.x][end.y];
            var openList = new List<Node>() { startNode };
            var closedList = new List<Node>();

            foreach (var row in grid)
            {
                foreach (var node in row)
                {
                    node.GCost = int.MaxValue;
                    node.CalculateFCost();
                    node.Parent = null;
                }
            }

            startNode.GCost = 0;
            startNode.HCost = CalculateDistanceCost(startNode, destinationNode);
            startNode.CalculateFCost();

            while (openList.Any())
            {
                var currentNode = GetLowestFCostNode(openList);

                if (currentNode == destinationNode)
                {
                    return CalculatePath(destinationNode);
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                var neighbours = GetNeighbours(currentNode, grid);

                foreach (var neigbourNode in neighbours)
                {
                    if (closedList.Contains(neigbourNode))
                    {
                        continue;
                    }

                    var isNeighbourWalkable = player ? neigbourNode.IsPlayerWalkable : neigbourNode.IsGuardWalkable;

                    if (!isNeighbourWalkable)
                    {
                        closedList.Add(neigbourNode);
                        continue;
                    }

                    var tentativeGCost = currentNode.GCost + CalculateDistanceCost(currentNode, neigbourNode);

                    if (tentativeGCost < neigbourNode.GCost)
                    {
                        neigbourNode.Parent = currentNode;
                        neigbourNode.GCost = tentativeGCost;
                        neigbourNode.HCost = CalculateDistanceCost(neigbourNode, destinationNode);
                        neigbourNode.CalculateFCost();

                        if (!openList.Contains(neigbourNode))
                        {
                            openList.Add(neigbourNode);
                        }
                    }
                }
            }

            return null;
        }

        private static Stack<Node> CalculatePath(Node destinationNode)
        {
            var list = new List<Node>() { destinationNode };
            var currentNode = destinationNode;

            while (currentNode.Parent != null)
            {
                list.Add(currentNode);
                currentNode = currentNode.Parent;
            }

            //list.Reverse();
            var path = new Stack<Node>(list);
            return path;
        }

        private static Node GetLowestFCostNode(List<Node> nodeList)
        {
            if (!nodeList.Any())
            {
                return null;
            }

            var lowest = nodeList[0];

            foreach (var node in nodeList)
            {
                if (node.FCost < lowest.FCost)
                {
                    lowest = node;
                }
            }

            return lowest;
        }

        private static List<Node> GetNeighbours(Node node, Node[][] grid)
        {
            var neighbours = new List<Node>();
            var nodePosition = node.GridPosition;
            var gridWidth = grid[0].Length;
            var gridHeight = grid.Length;

            //top
            if (nodePosition.x - 1 >= 0)
            {
                neighbours.Add(grid[nodePosition.x - 1][nodePosition.y]);
            }

            //bottom
            if (nodePosition.x + 1 < gridHeight)
            {
                neighbours.Add(grid[nodePosition.x + 1][nodePosition.y]);
            }

            //left
            if (nodePosition.y - 1 >= 0)
            {
                neighbours.Add(grid[nodePosition.x][nodePosition.y - 1]);
            }

            //right
            if (nodePosition.y + 1 < gridWidth)
            {
                neighbours.Add(grid[nodePosition.x][nodePosition.y + 1]);
            }

            //Top Left
            if (nodePosition.y - 1 >= 0 && nodePosition.x - 1 >= 0)
            {
                neighbours.Add(grid[nodePosition.x - 1][nodePosition.y - 1]);
            }

            //Bottom Left
            if (nodePosition.x + 1 < gridHeight && nodePosition.y - 1 >= 0)
            {
                neighbours.Add(grid[nodePosition.x + 1][nodePosition.y - 1]);
            }

            //Top Right
            if (nodePosition.x - 1 >= 0 && nodePosition.y + 1 < gridWidth)
            {
                neighbours.Add(grid[nodePosition.x - 1][nodePosition.y + 1]);
            }

            //Top Bottom
            if (nodePosition.x + 1 < gridHeight && nodePosition.y + 1 < gridWidth)
            {
                neighbours.Add(grid[nodePosition.x + 1][nodePosition.y + 1]);
            }

            return neighbours;
        }

        private static bool IsValidPosition(Vector2Int position, Node[][] grid)
        {
            //Assuming square grid
            return position.x >= 0 && position.y >= 0 && position.x < grid.Length && position.y < grid[0].Length;
        }

        private static int Heuristic(Node from, Node to)
        {
            return Math.Abs(from.GridPosition.x - to.GridPosition.x) +
                   Math.Abs(from.GridPosition.y - to.GridPosition.y);
        }

        private static int CalculateDistanceCost(Node from, Node to)
        {
            var xDistance = Math.Abs(from.GridPosition.x - to.GridPosition.x);
            var yDistance = Math.Abs(from.GridPosition.y - to.GridPosition.y);
            var remaining = Math.Abs(xDistance - yDistance);
            return DiagonalCost * Math.Min(xDistance, yDistance) + StraightCost * remaining;
        }
        
    }
}
