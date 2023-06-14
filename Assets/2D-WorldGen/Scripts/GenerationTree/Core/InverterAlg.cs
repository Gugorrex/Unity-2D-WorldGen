using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace _2D_WorldGen.Scripts.GenerationTree.Core
{
    [CreateAssetMenu(fileName = "Inverter", menuName = "GenerationTree/Core/Inverter", order = 0)]
    public class InverterAlg : GenerationAlgorithm
    {
        [BurstCompile(CompileSynchronously = true)]
        private struct InverterCgJob : IJobFor
        {
            [ReadOnly] public NativeArray<float> Input;
            public NativeArray<float> Output;

            public void Execute(int i)
            {
                Output[i] = 1 - Input[i];
            }
        }
        
        protected override JobHandle Schedule(GenerationCycleData cycleData, List<NativeArray<float>> inputs, NativeArray<float> output, JobHandle dependsOn = default)
        {
            var job = new InverterCgJob
            {
                Input = inputs.ToArray()[0],
                Output = output
            };
            return job.ScheduleParallel(cycleData.ArrayLength, batchSize, dependsOn);
        }

        protected override void Execute(int index, GenerationCycleData cycleData, List<NativeArray<float>> inputs, NativeArray<float> output)
        {
            var job = new InverterCgJob
            {
                Input = inputs.ToArray()[0],
                Output = output
            };
            job.Execute(index);
        }
    }
}