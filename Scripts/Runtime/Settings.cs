using UnityEngine;
using UnityEngine.Rendering;

namespace OmicronMeshColoring
{
    public static class Settings
    {
        private static int _threadsCount = 32;

        public static MeshUpdateFlags MeshUpdateFlags => MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds;

        public static int ThreadsCount
        {
            get => _threadsCount;
            set => _threadsCount = Mathf.Max(1, _threadsCount);
        }
    }
}