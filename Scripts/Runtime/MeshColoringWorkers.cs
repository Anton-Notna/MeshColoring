using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace OmicronMeshColoring
{
    internal class MeshColoringWorkers : IDisposable
    {
        private readonly Dictionary<AttributeType, MeshColoringWorker> _workersDictionary = new Dictionary<AttributeType, MeshColoringWorker>();
        private readonly List<MeshColoringWorker> _workersList = new List<MeshColoringWorker>();
        private readonly List<AttributeType> _runningJobs = new List<AttributeType>();

        public MeshColoringWorkers(int vertexCount, IReadOnlyList<AttributeType> additionalAttributes)
        {
            CreateWorker(vertexCount, AttributeType.VertexColor);

            if (additionalAttributes == null)
                return;

            for (int i = 0; i < additionalAttributes.Count; i++)
                CreateWorker(vertexCount, additionalAttributes[i]);
        }

        public bool Paint(PaintAction modification, Transform transform)
        {
            if (_workersDictionary.TryGetValue(modification.AttributeType, out var worker) == false)
                return false;

            worker.Paint(JobPaintAction.FromColorModification(modification, transform));
            return true;
        }

        public void ResetToDefaultColors(Mesh mesh, NativeArray<float4> defaultColors)
        {
            for (int i = 0; i < _workersList.Count; i++)
                _workersList[i].ResetToDefaultColors(mesh, defaultColors);
        }

        public bool ComputeDirtiness()
        {
            for (int i = 0; i < _workersList.Count; i++)
            {
                if (_workersList[i].Dirty)
                    return true;
            }

            return false;
        }

        public JobHandle StartJob(NativeArray<float3> localPositions)
        {
            JobHandle? result = null;

            for (int i = 0; i < _workersList.Count; i++)
            {
                var worker = _workersList[i];
                if (worker.Dirty == false)
                    continue;

                var job = worker.StartJob(localPositions);
                _runningJobs.Add(job.attributeType);

                if (result.HasValue)
                    result = JobHandle.CombineDependencies(result.Value, job.handle);
                else
                    result = job.handle;

            }

            return result.Value;
        }

        public void ApplyModifications(Mesh mesh)
        {
            for (int i = 0; i < _runningJobs.Count; i++)
                _workersDictionary[_runningJobs[i]].ApplyModifications(mesh);

            _runningJobs.Clear();
        }

        public void Dispose()
        {
            for (int i = 0; i < _workersList.Count; i++)
                _workersList[i].Dispose();
        }

        private void CreateWorker(int vertexCount, AttributeType attributeType)
        {
            if (_workersDictionary.ContainsKey(attributeType))
                return;

            MeshColoringWorker worker = new MeshColoringWorker(vertexCount, attributeType);
            _workersDictionary.Add(attributeType, worker);
            _workersList.Add(worker);
        }
    }
}