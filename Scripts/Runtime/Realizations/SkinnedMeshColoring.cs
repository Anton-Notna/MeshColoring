using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace OmicronMeshColoring
{
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class SkinnedMeshColoring : MeshColoring
    {
        [SerializeField, HideInInspector]
        private SkinnedMeshRenderer _renderer;

        private Mesh _backedMesh;
        private NativeArray<float3> _backedPositions;

        protected override Mesh MeshHolderPointer
        {
            get => _renderer.sharedMesh;
            set => _renderer.sharedMesh = value;
        }

        protected override void OnIniting(Mesh generatedMesh)
        {
            _backedMesh = new Mesh();
            _backedPositions = new NativeArray<float3>(generatedMesh.vertexCount, Allocator.Persistent);
        }

        protected override void OnClearing()
        {
            if (_backedMesh != null)
            {
                Destroy(_backedMesh);
                _backedMesh = null;
            }

            if (_backedPositions.IsCreated)
                _backedPositions.Dispose();
        }

        protected override void OnBeforeJobStart()
        {
            _renderer.BakeMesh(_backedMesh, true);
            using (Mesh.MeshDataArray dataArray = Mesh.AcquireReadOnlyMeshData(_backedMesh))
            {
                Mesh.MeshData data = dataArray[0];
                data.GetVertices(_backedPositions.Reinterpret<Vector3>());
            }
        }

        protected override NativeArray<float3> GetLocalBackedPositions() => _backedPositions;

        private void OnValidate()
        {
            if (_renderer == null)
                _renderer = GetComponent<SkinnedMeshRenderer>();
        }
    }
}