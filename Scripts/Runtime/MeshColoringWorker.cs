using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace OmicronMeshColoring
{
    internal class MeshColoringWorker : IDisposable
    {
        private readonly AttributeType _attributeType;
        private readonly Queue<JobPaintAction> _cashedModifications = new Queue<JobPaintAction>();

        private NativeArray<float4> _colors;
        private NativeList<JobPaintAction> _modifications;

        public AttributeType AttributeType => _attributeType;

        public bool Dirty => _cashedModifications.Count > 0;

        public MeshColoringWorker(int vertexCount, AttributeType attributeType)
        {
            _attributeType = attributeType;
            _colors = new NativeArray<float4>(vertexCount, Allocator.Persistent);
            _modifications = new NativeList<JobPaintAction>(Allocator.Persistent);
        }

        public void Paint(JobPaintAction jobPaintAction) => _cashedModifications.Enqueue(jobPaintAction);

        public void ResetToDefaultColors(Mesh mesh, NativeArray<float4> defaultColors)
        {
            _colors.CopyFrom(defaultColors);
            ApplyModifications(mesh);
        }

        public (JobHandle handle, AttributeType attributeType) StartJob(NativeArray<float3> localPositions)
        {
            _modifications.Clear();
            while (_cashedModifications.Count > 0)
                _modifications.Add(_cashedModifications.Dequeue());

            var job = new PaintActionsApplyJob()
            {
                Colors = _colors,
                LocalPositions = localPositions,
                Modifications = _modifications,
            };

            return (job.Schedule(_colors.Length, Settings.ThreadsCount), _attributeType);
        }

        public void ApplyModifications(Mesh mesh)
        {
            mesh.SetData(_colors, _attributeType);
        }

        public void Dispose()
        {
            if (_colors.IsCreated)
                _colors.Dispose();

            if (_modifications.IsCreated)
                _modifications.Dispose();
        }
    }
}