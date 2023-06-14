using System.Collections.Generic;
using _2D_WorldGen.Scripts.GenerationTree.Core;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace _2D_WorldGen.Scripts.GenerationTree
{
    public abstract class GenerationAlgorithm : ScriptableObject
    {
        [Header("General Settings")]
        public int batchSize;
        public GenerationAlgorithm[] dependencies;

        private NativeArray<float> _results = new(1, Allocator.Persistent);
        private int _cycleHash;
        private bool _scheduled;
        private JobHandle _jobHandle;

        private void ValidateCycle(GenerationCycleData cycleData)
        {
            if (_cycleHash == cycleData.GetHashCode()) return;
            _cycleHash = cycleData.GetHashCode();
            _results.Dispose();
            _results = new NativeArray<float>(cycleData.ArrayLength, Allocator.Persistent);
            _scheduled = false;
        }

        public void ExecuteOne(int index, GenerationCycleData cycleData)
        {
            ValidateCycle(cycleData);
            var inputs = new List<NativeArray<float>>();
            foreach (var generationAlgorithm in dependencies)
            {
                generationAlgorithm.ExecuteOne(index, cycleData);
                inputs.Add(generationAlgorithm.GetResults());
            }
            Execute(index, cycleData, inputs, _results);
        }

        public JobHandle ScheduleAll(GenerationCycleData cycleData, JobHandle dependsOn = default)
        {
            ValidateCycle(cycleData);

            // execution would be redundant, results are already calculated -> exit
            if (_scheduled && _jobHandle.IsCompleted)
            {
                return _jobHandle;
            }

            // schedule dependent generation algorithms in parallel
            var depJobs = new NativeArray<JobHandle>(dependencies.Length, Allocator.TempJob);
            for (var i = 0; i < dependencies.Length; i++)
            {
                depJobs[i] = dependencies[i].ScheduleAll(cycleData);
            }

            // wait for all jobs to be completed
            JobHandle.CombineDependencies(depJobs).Complete();
            depJobs.Dispose();

            // use results of dependencies for further calculation
            var inputs = new List<NativeArray<float>>();
            foreach (var generationAlgorithm in dependencies)
            {
                inputs.Add(generationAlgorithm.GetResults());
            }
            
            // schedule this generation algorithm itself
            _jobHandle = Schedule(cycleData, inputs, _results, dependsOn);
            _scheduled = true;
            return _jobHandle;
        }

        public NativeArray<float> GetResults()
        {
            return _results;
        }

        protected abstract JobHandle Schedule(GenerationCycleData cycleData, List<NativeArray<float>> inputs,
            NativeArray<float> output, JobHandle dependsOn = default);

        protected abstract void Execute(int index, GenerationCycleData cycleData, List<NativeArray<float>> inputs,
            NativeArray<float> output);

        ~GenerationAlgorithm()
        {
            _results.Dispose();
        }
    }
}