using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace _2D_WorldGen.Scripts.GenerationTree.Core
{
    [CreateAssetMenu(fileName = "BiCurveGraph", menuName = "GenerationTree/Core/BiCurveGraph", order = 0)]
    public class BiCurveGraphAlg : GenerationAlgorithm
    {
        [Header("Bi Curve Graph Settings")] 
        public float strength;
        public float cutoff;
        public float shift;
        public float amplitude;
        
        [BurstCompile(CompileSynchronously = true)]
        private struct BiCurveGraphCgJob : IJobFor
        {
            [ReadOnly] public NativeArray<float> Input;
            [ReadOnly] public float Strength;
            [ReadOnly] public float Cutoff;
            [ReadOnly] public float Shift;
            [ReadOnly] public float Amplitude;
            
            public NativeArray<float> Output;
            
            public void Execute(int i)
            {
                var halfAndShiftNumerator = Input[i] - Shift - 0.5f;
                
                if (Input[i] <= 0.5f)
                {
                    halfAndShiftNumerator = -halfAndShiftNumerator;
                }

                var modifiedValue = halfAndShiftNumerator / 0.5f;
                Output[i] = Amplitude * CurveGraph(modifiedValue, Strength, Cutoff);
            }
            
            private static float CurveGraph(float value, float strength, float cutoff)
            {
                var pow = Mathf.Pow(value, strength);
                return pow / (pow + Mathf.Pow(cutoff * (1 - value), strength));
            }
        }

        protected override JobHandle Schedule(GenerationCycleData cycleData, List<NativeArray<float>> inputs, NativeArray<float> output, JobHandle dependsOn = default)
        {
            var job = new BiCurveGraphCgJob
            {
                Input = inputs.ToArray()[0],
                Cutoff = cutoff,
                Strength = strength,
                Shift = shift,
                Amplitude = amplitude,
                Output = output
            };
            return job.ScheduleParallel(cycleData.ArrayLength, batchSize, dependsOn);
        }

        protected override void Execute(int index, GenerationCycleData cycleData, List<NativeArray<float>> inputs, NativeArray<float> output)
        {
            var job = new BiCurveGraphCgJob
            {
                Input = inputs.ToArray()[0],
                Cutoff = cutoff,
                Strength = strength,
                Shift = shift,
                Amplitude = amplitude,
                Output = output
            };
            job.Execute(index);
        }
    }
}