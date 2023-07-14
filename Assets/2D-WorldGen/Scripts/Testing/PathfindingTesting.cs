using System.Collections.Generic;
using _2D_WorldGen.Scripts.Manager.Addons;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace _2D_WorldGen.Scripts.Testing
{
    public class PathfindingTesting : MonoBehaviour
    {
        public Transform startTransform;
        public Transform targetTransform;
        public PathfindingManager pathfindingManager;
        public KeyCode startPathfindingKey = KeyCode.None;

        private void Update()
        {
            if (Input.GetKeyDown(startPathfindingKey))
            {
                Debug.Log("Start Pathfinding...");
                
                int2 startCoords = new int2((int)startTransform.position.x, (int)startTransform.position.y);
                int2 targetCoords = new int2((int)targetTransform.position.x, (int)targetTransform.position.y);
                var job = pathfindingManager.CreateAStarJob(startCoords, targetCoords, 100000);
                var handle = job.Schedule();
                handle.Complete();
                
                Color black = Color.black;
                black.a = 0.75f;
                Color gray = Color.gray;
                gray.a = 0.75f;
                
                DrawNodes(job.GetClosedNodes(), black);
                DrawNodes(job.GetOpenNodes(), gray);
                DrawPath(job.GetPath(), Color.blue);
                job.TearDown();
                
                Debug.Log("Pathfinding complete");
            }
        }
        
        private void DrawPath(Stack<int2> path, Color color)
        {
            if (path.Count <= 0) return;
            var previous = path.Pop();
            while (path.Count > 0)
            {
                var pos = path.Pop();
                Debug.DrawLine(new Vector3(previous.x+.5f, previous.y+.5f, 0), 
                    new Vector3(pos.x+.5f, pos.y+.5f, 0), 
                    color, 5, false);
                previous = pos;
            }
        }

        private void DrawNodes(int2[] nodes, Color color)
        {
            foreach (var n in nodes)
            {
                Debug.DrawLine(new Vector3(n.x, n.y, 0f), new Vector3(n.x+1, n.y+1, 0f), 
                    color, 5, false);
                Debug.DrawLine(new Vector3(n.x+1, n.y, 0f), new Vector3(n.x, n.y+1, 0f), 
                    color, 5, false);
            }
        }
    }
}