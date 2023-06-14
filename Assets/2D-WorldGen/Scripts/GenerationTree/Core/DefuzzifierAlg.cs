using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace _2D_WorldGen.Scripts.GenerationTree.Core
{
    [CreateAssetMenu(fileName = "Defuzzifier", menuName = "GenerationTree/Core/Defuzzifier", order = 0)]
    public class DefuzzifierAlg : GenerationAlgorithm
    {
        [Header("Defuzzifier Settings")] public float cutoff;
        
        [BurstCompile(CompileSynchronously = true)]
        private struct DefuzzifierCgJob : IJobFor
        {
            [ReadOnly] public NativeArray<float> Input;
            [ReadOnly] public float Cutoff;
            public NativeArray<float> Output;
            
            public void Execute(int i)
            {
                Output[i] = Input[i] < Cutoff ? 0 : 1;
            }
        }

        protected override JobHandle Schedule(GenerationCycleData cycleData, List<NativeArray<float>> inputs, NativeArray<float> output, JobHandle dependsOn = default)
        {
            var job = new DefuzzifierCgJob
            {
                Input = inputs.ToArray()[0],
                Cutoff = cutoff,
                Output = output
            };
            return job.ScheduleParallel(cycleData.ArrayLength, batchSize, dependsOn);
        }

        protected override void Execute(int index, GenerationCycleData cycleData, List<NativeArray<float>> inputs, NativeArray<float> output)
        {
            var job = new DefuzzifierCgJob
            {
                Input = inputs.ToArray()[0],
                Cutoff = cutoff,
                Output = output
            };
            job.Execute(index);
        }
    }
}