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

# Quick start
## Requirements
Mesh must be `isReadable == true`!

## Example
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
- To see any changes, your graphics must have a material that supports vertex colors. For example, `Default-Line`.
- Next, you need to add `SimpleMeshColoring` to a GameObject with a `MeshFilter`, or `SkinnedMeshColoring` to a GameObject with a `SkinnedMeshRenderer`.
- Then, set a reference to `MeshColoring` in `MeshColoringExample`, run play mode, and try moving `MeshColoringExample` around the mesh.

# Usage
## OmicronMeshColoring.MeshColoring class
Main abstract class that provides public ways to change vertex colors.

**Inspector settings:**
| Field | Description | Possible value |
| ----------- | ----------- | ----------- |
| `initAndClearCases` | When the component will be initialized and cleared | `ManualOnly`/`OnEnableOnDisable`/`AwakeOnDestroy` |
| `startRefreshCase` | When color refresh will try to start | `ManualOnly`/`Update`/`LateUpdate` |
| `finishRefreshCase` | When color refresh will try to finish | `ManualOnly`/`Update`/`LateUpdate` |
| `overrideDefaultColor` | Whether the default mesh colors will be overridden | `true`/`false` |
| `overriddenColor` | If the previous field is true, the model will be filled with this color | `UnityEngine.Color` |

**Change color methods:**
- `MeshColoring.Paint(PaintAction modification)` - Modifies colors in a specified area (see `PaintAction` for more details). Works only if MeshColoring is already initialized. Changes will be visible after `FinishRefresh`.
- `MeshColoring.ResetToDefaultColors()` - Resets all vertex colors to their default values. Works only if MeshColoring is already initialized. Changes will be visible instantly.

**Manual refresh methods:**

You will need these if your project has a special execution order.
- `MeshColoring.ManualTryStartRefresh()` - Starts applying all cached color changes if there is no processing currently.
- `MeshColoring.ManualTryFinishRefresh()` - Finishes processing color changes if they are ready to finish.
- `MeshColoring.ManualFullRefreshMomentum()` - Instantly processes and applies all changes. Can be expensive; use with care.

**Manual Init/Clear methods:**
- `MeshColoring.ManualInit()` - Sets up the component for further work.
- `MeshColoring.ManualClear()` - Finishes processes and clears/disposes all data. **If you use `initAndClearCases`-`ManualOnly`, you must call this manually to avoid memory leaks**.
- `MeshColoring.ManualReInit()` - Combines the previous methods.

## OmicronMeshColoring.PaintAction struct
Unit act of painting. Stores position info and color delta change.

You can use some methods to create and set up `PaintAction`, like in the example, or set values manually.
| Field | Description |
| ----------- | ----------- |
| `float4 Delta` | `Color.rgba` delta of change |
| `float3 WorldPosition` | World origin of intention |
| `float MinRadius` | Every vertex within this radius will be fully affected by the delta |
| `float MaxRadius` | From `MinRadius` to `MaxRadius`, the effect of the delta linearly decreases to nothing |

