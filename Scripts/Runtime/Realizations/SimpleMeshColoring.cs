using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace OmicronMeshColoring
{
    public class SimpleMeshColoring : MeshColoring
    {
        [SerializeField, HideInInspector]
        private MeshFilter _filter;

        private NativeArray<float3> _backedPositions;

        protected override Mesh MeshHolderPointer
        {
            get => _filter.sharedMesh;
            set => _filter.sharedMesh = value;
        }

        protected override NativeArray<float3> GetLocalBackedPositions() => _backedPositions;

        protected override void OnBeforeJobStart() { }

        protected override void OnClearing()
        {
            if (_backedPositions.IsCreated)
                _backedPositions.Dispose();
        }

        protected override void OnIniting(Mesh generatedMesh)
        {
            _backedPositions = new NativeArray<float3>(generatedMesh.vertexCount, Allocator.Persistent);
            using (Mesh.MeshDataArray dataArray = Mesh.AcquireReadOnlyMeshData(generatedMesh))
            {
                Mesh.MeshData data = dataArray[0];
                data.GetVertices(_backedPositions.Reinterpret<Vector3>());
            }
        }

        private void OnValidate()
        {
            if (_filter == null)
                _filter = GetComponent<MeshFilter>();
        }
    }
}