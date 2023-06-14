using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace _2D_WorldGen.Scripts.GenerationTree.Core
{
    [CreateAssetMenu(fileName = "SmoothExtremaCutoff", menuName = "GenerationTree/Core/SmoothExtremaCutoff", order = 0)]
    public class SmoothExtremaCutoffAlg : GenerationAlgorithm
    {
        [Header("Smooth Extrema Cutoff Settings")] public float yCutoff;
        
        [BurstCompile(CompileSynchronously = true)]
        private struct SmoothExtremaCutoffCgJob : IJobFor
        {
            [ReadOnly] public NativeArray<float> Input;
            [ReadOnly] public float YCutoff;
            
            public NativeArray<float> Output;
            
            public void Execute(int i)
            {
                Output[i] = CutoffLine(UnitSin(Input[i]), YCutoff);
            }

            private static float CutoffLine(float value, float yCutoff)
            {
                return value * (1 - 2 * yCutoff) + yCutoff;
            }

            private static float UnitSin(float value)
            {
                return 0.5f * Mathf.Sin(Mathf.PI * (value - 0.5f)) + 0.5f;
            }
        }

        protected override JobHandle Schedule(GenerationCycleData cycleData, List<NativeArray<float>> inputs, NativeArray<float> output, JobHandle dependsOn = default)
        {
            var job = new SmoothExtremaCutoffCgJob
            {
                Input = inputs.ToArray()[0],
                YCutoff = yCutoff,
                Output = output
            };
            return job.ScheduleParallel(cycleData.ArrayLength, batchSize, dependsOn);
        }

        protected override void Execute(int index, GenerationCycleData cycleData, List<NativeArray<float>> inputs, NativeArray<float> output)
        {
            var job = new SmoothExtremaCutoffCgJob
            {
                Input = inputs.ToArray()[0],
                YCutoff = yCutoff,
                Output = output
            };
            job.Execute(index);
        }
    }
}