using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace _2D_WorldGen.Scripts.Pathfinding
{
    [BurstCompile(CompileSynchronously = true)]
    public struct AStarJob : IJob
    {
        public Node StartNode;
        public Node TargetNode;
        [ReadOnly] public int Safeguard; // max amount of nodes in a list -> low value can lead to failing pathfinding
        // also necessary in an open world because otherwise it will find an infinite amount of new nodes
        // TODO: another possible constraint (not implemented): do not allow finding nodes beyond loaded chunks
        
        [ReadOnly] public NativeHashSet<int2> Obstacles;
        [ReadOnly] public NativeHashMap<int2, float> SparseCostsMap;
        // save only non-default costs and non-obstacles to save memory and better performance while iterating
        
        private NativeHashMap<int2, Node> _openList;
        private NativeHashMap<int2, Node> _closedList;


        public struct Node
        {
            public int2 Coords;
            public int2 Parent;
            public int GScore;
            public int HScore;
        }

        public void Init()
        {
            _openList = new NativeHashMap<int2, Node>(Safeguard, Allocator.TempJob);
            _closedList = new NativeHashMap<int2, Node>(Safeguard, Allocator.TempJob);
        }

        public void TearDown()
        {
            _openList.Dispose();
            _closedList.Dispose();
        }

        public Stack<int2> GetPath()
        {
            var path = new Stack<int2>();

            if (!_closedList.ContainsKey(TargetNode.Coords)) return path;
            var currentCoords = TargetNode.Coords;

            while (!currentCoords.Equals(StartNode.Coords))
            {
                path.Push(currentCoords);
                currentCoords = _closedList[currentCoords].Parent;
            }

            return path;
        }

        public int2[] GetClosedNodes()
        {
            var closedValues = _closedList.GetValueArray(Allocator.Temp);
            var nodes = new int2[closedValues.Length];
            for (var i = 0; i < closedValues.Length; i++)
            {
                nodes[i] = closedValues[i].Coords;
            }
            closedValues.Dispose();
            return nodes;
        }

        public int2[] GetOpenNodes()
        {
            var openValues = _openList.GetValueArray(Allocator.Temp);
            var nodes = new int2[openValues.Length];
            for (var i = 0; i < openValues.Length; i++)
            {
                nodes[i] = openValues[i].Coords;
            }
            openValues.Dispose();
            return nodes;
        }

        private static int SqDistance(int2 coordsA, int2 coordsB)
        {
            var distX = coordsA.x - coordsB.x;
            var distY = coordsA.y - coordsB.y;
            return distX * distX + distY * distY;
        }

        public void Execute()
        {
            StartNode.GScore = 0;
            StartNode.HScore = SqDistance(StartNode.Coords, TargetNode.Coords);
            _openList.TryAdd(StartNode.Coords, StartNode);
            
            var successorOffsets = new NativeArray<int2>(8, Allocator.Temp);
            successorOffsets[0] = new int2(-1, 1);
            successorOffsets[1] = new int2(0, 1);
            successorOffsets[2] = new int2(1, 1);
            successorOffsets[3] = new int2(-1, 0);
            successorOffsets[4] = new int2(1, 0);
            successorOffsets[5] = new int2(-1, -1);
            successorOffsets[6] = new int2(0, -1);
            successorOffsets[7] = new int2(1, -1);

            var counter = 0;
            while (!_openList.IsEmpty && !(counter > Safeguard))
            {
                var currentNode = MinF();
                _closedList.TryAdd(currentNode.Coords, currentNode);

                if (currentNode.Coords.Equals(TargetNode.Coords)) break; // PathFound
                ExpandNode(currentNode, successorOffsets);
                counter++;
            }
            
            if (counter > Safeguard)
            {
                Debug.Log("AStarJob terminated due to safeguard");
            }

            successorOffsets.Dispose();
            // NoPathFound
        }

        private void ExpandNode(Node currentNode, NativeArray<int2> successorOffsets)
        {
            foreach (var offset in successorOffsets)
            {
                var successorCoords = currentNode.Coords + offset;
                if (Obstacles.Contains(successorCoords) || _closedList.ContainsKey(successorCoords)) continue;

                var successorCost = SparseCostsMap.TryGetValue(successorCoords, out var cost) ? cost : 1f;
                var tentativeG = currentNode.GScore +
                                 (int)(SqDistance(currentNode.Coords, successorCoords) * successorCost);
                var successor = new Node
                {
                    Coords = successorCoords,
                    Parent = currentNode.Coords,
                    GScore = tentativeG,
                    HScore = SqDistance(successorCoords, TargetNode.Coords)
                };

                if (_openList.ContainsKey(successorCoords) && tentativeG >= successor.GScore) continue;

                if (_openList.ContainsKey(successorCoords))
                {
                    _openList[successorCoords] = successor;
                } 
                else
                {
                    _openList.Add(successorCoords, successor);
                }
            }
        }

        private Node MinF()
        {
            var openListValues = _openList.GetValueArray(Allocator.Temp);
            var min = openListValues[0];
            var minFScore = int.MaxValue;
            foreach (var node in openListValues)
            {
                var fScore = node.GScore + node.HScore;
                if (fScore >= minFScore) continue;
                min = node;
                minFScore = fScore;
            }
            openListValues.Dispose();
            _openList.Remove(min.Coords);
            return min;
        }
    }
}