using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace _2D_WorldGen.Scripts.GenerationTree.Core
{
    [CreateAssetMenu(fileName = "CurveGraph", menuName = "2D World Gen/GenerationTree/Core/CurveGraph", order = 0)]
    public class CurveGraphAlg : GenerationAlgorithm
    {
        [Header("Curve Graph Settings")] 
        public float strength;
        public float cutoff;
        public float amplitude;
        public float yOffset;
        
        [BurstCompile(CompileSynchronously = true)]
        private struct CurveGraphJob : IJobFor
        {
            [ReadOnly] public NativeArray<float> Input;
            [ReadOnly] public float Strength;
            [ReadOnly] public float Cutoff;
            [ReadOnly] public float Amplitude;
            [ReadOnly] public float YOffset;
            
            /*[WriteOnly]*/
            public NativeArray<float> Output;
            
            public void Execute(int i)
            {
                Output[i] = Amplitude * CurveGraph(Input[i], Strength, Cutoff / (1 - Cutoff)) + YOffset;
            }
            
            private static float CurveGraph(float value, float strength, float cutoff)
            {
                var pow = Mathf.Pow(value, strength);
                return pow / (pow + Mathf.Pow(cutoff * (1 - value), strength));
            }
        }
        
        protected override JobHandle Schedule(GenerationCycleData cycleData, List<NativeArray<float>> inputs, NativeArray<float> output, JobHandle dependsOn = default)
        {
            var job = new CurveGraphJob
            {
                Input = inputs.ToArray()[0],
                Cutoff = cutoff,
                Strength = strength,
                Amplitude = amplitude,
                YOffset = yOffset,
                Output = output
            };
            return job.ScheduleParallel(cycleData.ArrayLength, batchSize, dependsOn);
        }

        protected override void Execute(int index, GenerationCycleData cycleData, List<NativeArray<float>> inputs, NativeArray<float> output)
        {
            var job = new CurveGraphJob
            {
                Input = inputs.ToArray()[0],
                Cutoff = cutoff,
                Strength = strength,
                Amplitude = amplitude,
                YOffset = yOffset,
                Output = output
            };
            job.Execute(index);
        }
    }
}