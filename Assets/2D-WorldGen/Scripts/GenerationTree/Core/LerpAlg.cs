using System.Collections.Generic;
using System.ComponentModel;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace _2D_WorldGen.Scripts.GenerationTree.Core
{
    [CreateAssetMenu(fileName = "Lerp", menuName = "2D World Gen/GenerationTree/Core/Lerp", order = 0)]
    public class LerpAlg : GenerationAlgorithm
    {
        [Description("description")]
        
        [BurstCompile(CompileSynchronously = true)]
        private struct LerpJob : IJobFor
        {
            [Unity.Collections.ReadOnly] public NativeArray<float> Input0;
            [Unity.Collections.ReadOnly] public NativeArray<float> Input1;
            [Unity.Collections.ReadOnly] public NativeArray<float> Lerp;
            public NativeArray<float> Output;

            public void Execute(int i)
            {
                Output[i] = math.lerp(Input0[i], Input1[i], Lerp[i]);
            }
        }
        
        protected override JobHandle Schedule(GenerationCycleData cycleData, List<NativeArray<float>> inputs, NativeArray<float> output, JobHandle dependsOn = default)
        {
            var job = new LerpJob
            {
                Input0 = inputs[0],
                Input1 = inputs[1],
                Lerp = inputs[2],
                Output = output
            };
            return job.ScheduleParallel(cycleData.ArrayLength, batchSize, dependsOn);
        }

        protected override void Execute(int index, GenerationCycleData cycleData, List<NativeArray<float>> inputs, NativeArray<float> output)
        {
            var job = new LerpJob
            {
                Input0 = inputs[0],
                Input1 = inputs[1],
                Lerp = inputs[2],
                Output = output
            };
            job.Execute(index);
        }
    }
}