using _2D_WorldGen.Scripts.GenerationTree;
using _2D_WorldGen.Scripts.GenerationTree.Core;
using Unity.Mathematics;
using UnityEngine;

namespace _2D_WorldGen.Scripts
{
    public class TestGenerationTree : MonoBehaviour
    {
        public CurveGraphAlg curveGraphAlg;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var cycleData = new GenerationCycleData(0, int2.zero, 2, 1);
                curveGraphAlg.ScheduleAll(cycleData).Complete();
                foreach (var value in curveGraphAlg.GetResults())
                {
                    Debug.Log(value);
                }
            }
        }
    }
}