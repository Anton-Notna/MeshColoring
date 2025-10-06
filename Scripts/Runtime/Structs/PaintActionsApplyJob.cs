using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace OmicronMeshColoring
{
    [BurstCompile]
    public struct PaintActionsApplyJob : IJobParallelFor
    {
        public NativeArray<float4> Colors;

        [ReadOnly]
        public NativeArray<float3> LocalPositions;
        [ReadOnly]
        public NativeList<JobPaintAction> Modifications;

        public void Execute(int index)
        {
            float4 color = Colors[index];
            int successModifications = 0;

            for (int i = 0; i < Modifications.Length; i++) 
            {
                JobPaintAction modification = Modifications[i];

                float3 vertexPosition = LocalPositions[index];
                float3 modificationPosition = modification.LocalPosition;
                float distanceSq = math.lengthsq(vertexPosition - modificationPosition);
                if (distanceSq > modification.MaxRadius * modification.MaxRadius)
                    continue;

                successModifications += 1;
                float distance = math.sqrt(distanceSq);
                float distanceIntensity = math.remap(modification.MinRadius, modification.MaxRadius, 1f, 0f, distance);
                distanceIntensity = math.clamp(distanceIntensity, 0f, 1f);
                float totalIntensity = distanceIntensity * modification.Intensity;

                if (modification.MixType == ColorMixType.Additive)
                {
                    float4 delta = modification.Delta * totalIntensity;
                    color += delta;
                }
                else
                {
                    color = math.lerp(color, modification.Delta, totalIntensity);
                }
            }

            if (successModifications > 0)
                color = math.clamp(color, float4.zero, new float4(1f, 1f, 1f, 1f));

            Colors[index] = color;
        }
    }
}