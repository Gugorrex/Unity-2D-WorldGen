using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace _2D_WorldGen.Scripts.GenerationTree.Core
{
    [CreateAssetMenu(fileName = "BiSubtraction", menuName = "GenerationTree/Core/BiSubtraction", order = 0)]
    public class BiSubtractionAlg : GenerationAlgorithm
    {
        [Header("Bi Subtraction Settings")] public bool clamp01;
        
        [BurstCompile(CompileSynchronously = true)]
        private struct BiSubtractionCgJob : IJobFor
        {
            [ReadOnly] public bool Clamp01;
            [ReadOnly] public NativeArray<float> InputA;
            [ReadOnly] public NativeArray<float> InputB;
            /*[WriteOnly]*/ public NativeArray<float> Output;

            public void Execute(int i)
            {
                var sub = InputA[i] - InputB[i];
                Output[i] = Clamp01 ? Mathf.Clamp01(sub) : sub;
            }
        }

        protected override JobHandle Schedule(GenerationCycleData cycleData, List<NativeArray<float>> inputs, NativeArray<float> output, JobHandle dependsOn = default)
        {
            var inputsArr = inputs.ToArray();
            var job = new BiSubtractionCgJob
            {
                Clamp01 = clamp01,
                InputA = inputsArr[0],
                InputB = inputsArr[1],
                Output = output
            };
            return job.ScheduleParallel(cycleData.ArrayLength, batchSize, dependsOn);
        }

        protected override void Execute(int index, GenerationCycleData cycleData, List<NativeArray<float>> inputs, NativeArray<float> output)
        {
            var inputsArr = inputs.ToArray();
            var job = new BiSubtractionCgJob
            {
                Clamp01 = clamp01,
                InputA = inputsArr[0],
                InputB = inputsArr[1],
                Output = output
            };
            job.Execute(index);
        }
    }
}