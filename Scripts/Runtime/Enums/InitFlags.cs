using System;

namespace OmicronMeshColoring
{
    [Flags]
    public enum InitFlags : byte
    {
        ManualOnly = 0,
        OnEnableOnDisable = 1 << 0,
        AwakeOnDestroy = 1 << 1,
    }
}