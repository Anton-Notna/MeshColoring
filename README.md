# Omicron Mesh Coloring
https://github.com/user-attachments/assets/7fa560af-8238-4a2d-8e7f-51333dd277ed
- Change vertex colors of meshes at runtime.
- Supports `MeshFilter` and `SkinnedMeshRenderer`.
- Fast, Zero-Allocation, simple to use.

# Installation
Omicron Mesh Coloring is the upm package, so the installation is similar to other upm packages:
1. Open `Window/PackageManager`
2. Click `+` in the right corner and select `Add package from git url...`
3. Paste the link to this package `https://github.com/Anton-Notna/MeshColoring.git` and click `Add`

# Usage
## Requirements
Mesh must be `isReadable == true`!

## Quick start
- Let's create a simple class to figure out how it all works:
```csharp
using UnityEngine;
using OmicronMeshColoring;

public class MeshColoringExample : MonoBehaviour
{
    [SerializeField]
    private MeshColoring _meshColoring; // Remember to set MeshColoring reference in inspector
    [SerializeField]
    private Color _deltaColor;
    [SerializeField]
    private float _intensity = 1f;
    [SerializeField]
    private bool _substract;
    [SerializeField]
    private float _radius = 0.5f;
    [SerializeField]
    private float _smooth = 2f;

    private void Update()
    {
        PaintAction paintAction = PaintAction
            .FromSphere(_deltaColor, transform.position, _radius)
            .WithSmooth(_smooth)
            .WithIntensity(Time.deltaTime * _intensity);

        if (_substract)
            paintAction = paintAction.SubstractColor();

        _meshColoring.Paint(paintAction);
    }

    private void OnDrawGizmos()
    {
        Color color = _deltaColor;

        color.a = 1f;
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, _radius);

        color.a = .3f;
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, _radius + _smooth);
    }
}
```
- To see any changes, your graphics must have Material thats supports vertex colors. For example, `Default-Line`.
- Next, we need to add `SimpleMeshColoring` to GameObject with `MeshFilter` or `SkinnedMeshColoring` to GameObject with `SkinnedMeshRenderer`.
- Then, set reference to `MeshColoring` in `MeshColoringExample`, run playmode and try to move `MeshColoringExample` around the mesh.
