using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace _2D_WorldGen.Scripts.GenerationTree.Core
{
    [CreateAssetMenu(fileName = "OneValue", menuName = "2D World Gen/GenerationTree/Core/OneValue", order = 0)]
    public class OneValueAlg : GenerationAlgorithm
    {
        [Header("One Value Settings")] 
        public float value;

        [BurstCompile(CompileSynchronously = true)]
        private struct OneValueJob : IJobFor
        {
            [ReadOnly] public float Value;
            public NativeArray<float> Output;

            public void Execute(int i)
            {
                Output[i] = Value;
            }
        }
        
        protected override JobHandle Schedule(GenerationCycleData cycleData, List<NativeArray<float>> inputs, NativeArray<float> output, JobHandle dependsOn = default)
        {
            var job = new OneValueJob
            {
                Value = value,
                Output = output
            };
            return job.ScheduleParallel(cycleData.ArrayLength, batchSize, dependsOn);
        }

        protected override void Execute(int index, GenerationCycleData cycleData, List<NativeArray<float>> inputs, NativeArray<float> output)
        {
            var job = new OneValueJob
            {
                Value = value,
                Output = output
            };
            job.Execute(index);
        }
    }
}