using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace OmicronMeshColoring
{
    internal static class MeshExtensions
    {
        public static void SetData(this Mesh mesh, NativeArray<float4> data, AttributeType attributeType)
        {
            if (attributeType == AttributeType.VertexColor)
            {
                mesh.SetColors(data, 0, data.Length, Settings.MeshUpdateFlags);
                return;
            }

            int uvIndex = (int)attributeType;
            mesh.SetUVs(uvIndex, data, 0, data.Length, Settings.MeshUpdateFlags);
        }
    }
}