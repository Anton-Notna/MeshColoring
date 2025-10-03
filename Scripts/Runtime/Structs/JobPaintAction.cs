using Unity.Mathematics;
using UnityEngine;

namespace OmicronMeshColoring
{
    public struct JobPaintAction
    {
        public float4 Delta;
        public float3 LocalPosition;
        public float MaxRadius;
        public float MinRadius;
        public ColorMixType MixType;

        public static JobPaintAction FromColorModification(PaintAction modification, Transform relativeTo)
        {
            Vector3 scale = relativeTo.lossyScale;
            float averageScale = (scale.x + scale.y + scale.z) / 3f;
            averageScale = Mathf.Max(Mathf.Epsilon, averageScale);

            return new JobPaintAction()
            {
                Delta = modification.Delta,
                LocalPosition = relativeTo.InverseTransformPoint(modification.WorldPosition),
                MaxRadius = modification.MaxRadius / averageScale,
                MinRadius = modification.MinRadius / averageScale,
                MixType = modification.MixType,
            };
        }
    }
}