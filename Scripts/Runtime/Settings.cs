using UnityEngine;

namespace OmicronMeshColoring
{
    public static class Settings
    {
        private static int _threadsCount = 32;

        public static int ThreadsCount
        {
            get => _threadsCount;
            set => _threadsCount = Mathf.Max(1, _threadsCount);
        }
    }
}