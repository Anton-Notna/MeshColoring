using System;

namespace OmicronMeshColoring
{
    [Flags]
    public enum ExecuteFlags : byte
    {
        ManualOnly = 0,
        Update = 1 << 0,
        LateUpdate = 1 << 1,
    }
}