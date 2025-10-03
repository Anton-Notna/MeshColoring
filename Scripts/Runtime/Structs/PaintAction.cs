using Unity.Mathematics;
using UnityEngine;

namespace OmicronMeshColoring
{
    public struct PaintAction
    {
        public float4 Delta;
        public float3 WorldPosition;
        public float MaxRadius;
        public float MinRadius;
        public ColorMixType MixType;

        public static PaintAction FromPoint(Color deltaColor, Vector3 position, ColorMixType mixType = ColorMixType.Additive)
        {
            return new PaintAction()
            {
                Delta = new float4(deltaColor.r, deltaColor.g, deltaColor.b, deltaColor.a),
                WorldPosition = position,
                MaxRadius = 0f,
                MinRadius = 0f,
                MixType = mixType,
            };
        }

        public static PaintAction FromSphere(Color deltaColor, Vector3 position, float radius, ColorMixType mixType = ColorMixType.Additive)
        {
            radius = Mathf.Max(0f, radius);

            return new PaintAction()
            {
                Delta = new float4(deltaColor.r, deltaColor.g, deltaColor.b, deltaColor.a),
                WorldPosition = position,
                MaxRadius = radius,
                MinRadius = radius,
                MixType = mixType,
            };
        }

        public PaintAction WithSmooth(float smoothDistance)
        {
            smoothDistance = Mathf.Max(0f, smoothDistance);
            float maxRadius = this.MinRadius + smoothDistance;

            return new PaintAction()
            {
                Delta = this.Delta,
                WorldPosition = this.WorldPosition,
                MaxRadius = maxRadius,
                MinRadius = this.MinRadius,
                MixType = this.MixType,
            };
        }

        public PaintAction SubstractColor()
        {
            return new PaintAction()
            {
                Delta = this.Delta * -1f,
                WorldPosition = this.WorldPosition,
                MaxRadius = this.MaxRadius,
                MinRadius = this.MinRadius,
                MixType = this.MixType,
            };
        }

        public PaintAction WithIntensity(float intensity)
        {
            intensity = Mathf.Max(0f, intensity);
            return new PaintAction()
            {
                Delta = this.Delta * intensity,
                WorldPosition = this.WorldPosition,
                MaxRadius = this.MaxRadius,
                MinRadius = this.MinRadius,
                MixType = this.MixType,
            };
        }
    }
}