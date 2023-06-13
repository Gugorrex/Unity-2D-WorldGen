using _2D_WorldGen.Scripts.GenerationTree.Core;
using Unity.Mathematics;
using UnityEngine;

namespace _2D_WorldGen.Scripts
{
    public class TestGenerationTree : MonoBehaviour
    {
        public CurveGraphAlg curveGraphAlg;

        private void Start()
        {
            var cycleData = new GenerationCycleData(0, int2.zero, 2);
            curveGraphAlg.ScheduleAll(cycleData).Complete();
            foreach (var value in curveGraphAlg.GetResults())
            {
                Debug.Log(value);
            }
        }
    }
}