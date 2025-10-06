using OmicronMeshColoring.Attributes;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace OmicronMeshColoring
{
    public abstract class MeshColoring : MonoBehaviour
    {
        [Header("Execution cases")]
        [SerializeField, ReadOnlyInPlayMode]
        private InitFlags _initAndClearCases = InitFlags.AwakeOnDestroy | InitFlags.OnEnableOnDisable;
        [SerializeField]
        private ExecuteFlags _startRefreshCase = ExecuteFlags.Update;
        [SerializeField]
        private ExecuteFlags _finishRefreshCase = ExecuteFlags.LateUpdate;
        [Header("Start color")]
        [SerializeField, ReadOnlyInPlayMode]
        private bool _overrideDefaultColor = true;
        [SerializeField, ReadOnlyInPlayMode]
        private Color _overriddenColor = Color.white;

        private readonly Queue<JobPaintAction> _cashedModifications = new Queue<JobPaintAction>();

        private bool _inited;
        private Mesh _generatedMesh;
        private Mesh _defaultMesh;
        private NativeArray<float4> _colors;
        private NativeArray<float4> _defaultColors;
        private NativeList<JobPaintAction> _modifications;
        private JobHandle? _handle;

        public bool Inited => _inited;

        protected abstract Mesh MeshHolderPointer { get; set; }

        #region Public methods

        public void Paint(PaintAction modification)
        {
            if (_inited)
                _cashedModifications.Enqueue(JobPaintAction.FromColorModification(modification, transform));
        }

        [ContextMenu(nameof(ResetToDefaultColors))]
        public void ResetToDefaultColors()
        {
            if (_inited == false)
                return;

            ForceCompleteJobProcess();

            _colors.CopyFrom(_defaultColors);
            _generatedMesh.SetColors(_colors);
        }

        public void ManualInit() => TryInit();

        public void ManualClear() => TryClear();

        /// <summary>
        /// Clear and Init at the same place. Call it, for example, if you changed SkinnedMeshRenderer.sharedMesh from other place.
        /// </summary>
        public void ManualReInit()
        {
            TryClear();
            TryInit();
        }

        public void ManualTryStartRefresh() => TryStartJob();

        public void ManualTryFinishRefresh() => TryFinishJob();

        /// <summary>
        /// Finish current calculations and apply current modifications at the same moment. Can be expensive, use with care.
        /// </summary>
        public void ManualFullRefreshMomentum()
        {
            if (_inited == false)
                return;

            if (_handle.HasValue)
                FinishJob();

            TryStartJob();

            if (_handle.HasValue)
                FinishJob();
        }

        #endregion

        #region Abstract methods

        protected abstract void OnIniting(Mesh generatedMesh);

        protected abstract void OnClearing();

        protected abstract void OnBeforeJobStart();

        protected abstract NativeArray<float3> GetLocalBackedPositions();

        #endregion

        #region Built-in methods

        private void Awake()
        {
            if ((_initAndClearCases & InitFlags.AwakeOnDestroy) != 0)
                TryInit();
        }

        private void OnDestroy()
        {
            if ((_initAndClearCases & InitFlags.AwakeOnDestroy) != 0)
                TryClear();
        }

        private void OnEnable()
        {
            if ((_initAndClearCases & InitFlags.OnEnableOnDisable) != 0)
                TryInit();
        }

        private void OnDisable()
        {
            if ((_initAndClearCases & InitFlags.OnEnableOnDisable) != 0)
                TryClear();
        }

        private void Update()
        {
            if ((_finishRefreshCase & ExecuteFlags.Update) != 0)
                TryFinishJob();

            if ((_startRefreshCase & ExecuteFlags.Update) != 0)
                TryStartJob();
        }

        private void LateUpdate()
        {
            if ((_finishRefreshCase & ExecuteFlags.LateUpdate) != 0)
                TryFinishJob();

            if ((_startRefreshCase & ExecuteFlags.LateUpdate) != 0)
                TryStartJob();
        }

        #endregion

        #region Private methods

        private static void CopyBlendShapes(Mesh source, Mesh target)
        {
            for (int shapeIndex = 0; shapeIndex < source.blendShapeCount; shapeIndex++)
            {
                string shapeName = source.GetBlendShapeName(shapeIndex);
                int frameCount = source.GetBlendShapeFrameCount(shapeIndex);

                for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
                {
                    float frameWeight = source.GetBlendShapeFrameWeight(shapeIndex, frameIndex);
                    Vector3[] deltaVertices = new Vector3[source.vertexCount];
                    Vector3[] deltaNormals = new Vector3[source.vertexCount];
                    Vector3[] deltaTangents = new Vector3[source.vertexCount];

                    source.GetBlendShapeFrameVertices(shapeIndex, frameIndex, deltaVertices, deltaNormals, deltaTangents);

                    target.AddBlendShapeFrame(shapeName, frameWeight, deltaVertices, deltaNormals, deltaTangents);
                }
            }
        }

        private void TryInit()
        {
            if (_inited)
                return;

            Init();
        }

        private void Init()
        {
            _defaultMesh = MeshHolderPointer;
            _generatedMesh = new Mesh()
            {
                vertices = _defaultMesh.vertices,
                triangles = _defaultMesh.triangles,
                normals = _defaultMesh.normals,
                tangents = _defaultMesh.tangents,
                bounds = _defaultMesh.bounds,
                uv = _defaultMesh.uv,
                uv2 = _defaultMesh.uv2,
                uv3 = _defaultMesh.uv3,
                uv4 = _defaultMesh.uv4,
                uv5 = _defaultMesh.uv5,
                uv6 = _defaultMesh.uv6,
                uv7 = _defaultMesh.uv7,
                uv8 = _defaultMesh.uv8,
                colors = _defaultMesh.colors,
                bindposes = _defaultMesh.bindposes,
                boneWeights = _defaultMesh.boneWeights,
                indexFormat = _defaultMesh.indexFormat,
            };

            CopyBlendShapes(_defaultMesh, _generatedMesh);

            MeshHolderPointer = _generatedMesh;

            _colors = new NativeArray<float4>(_generatedMesh.vertexCount, Allocator.Persistent);
            _defaultColors = new NativeArray<float4>(_generatedMesh.vertexCount, Allocator.Persistent);
            _modifications = new NativeList<JobPaintAction>(Allocator.Persistent);
            OnIniting(_generatedMesh);
            FillDefaultColors();

            _inited = true;
        }

        private void FillDefaultColors()
        {
            if (_overrideDefaultColor)
            {
                float4 defaultColor = new float4(_overriddenColor.r, _overriddenColor.g, _overriddenColor.b, _overriddenColor.a);
                for (int i = 0; i < _defaultColors.Length; i++)
                    _defaultColors[i] = defaultColor;
            }
            else
            {
                using (Mesh.MeshDataArray dataArray = Mesh.AcquireReadOnlyMeshData(_generatedMesh))
                {
                    Mesh.MeshData data = dataArray[0];
                    if (data.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Color))
                        data.GetColors(_defaultColors.Reinterpret<Color>());
                }
            }

            _colors.CopyFrom(_defaultColors);
            _generatedMesh.SetColors(_colors);
        }

        private void TryStartJob()
        {
            if (_inited == false)
                return;

            if (_handle.HasValue)
                return;

            if (_cashedModifications.Count == 0)
                return;

            StartJob();
        }

        private void StartJob()
        {
            _modifications.Clear();
            while (_cashedModifications.Count > 0)
                _modifications.Add(_cashedModifications.Dequeue());

            OnBeforeJobStart();

            _handle = new PaintActionsApplyJob()
            {
                Colors = _colors,
                LocalPositions = GetLocalBackedPositions(),
                Modifications = _modifications,
            }.Schedule(_colors.Length, Settings.ThreadsCount);
        }

        private void TryFinishJob()
        {
            if (_inited == false)
                return;

            if (_handle.HasValue == false)
                return;

            if (_handle.Value.IsCompleted == false)
                return;

            FinishJob();
        }

        private void FinishJob()
        {
            ForceCompleteJobProcess();
            _generatedMesh.SetColors(_colors);
        }

        private void ForceCompleteJobProcess()
        {
            if (_handle.HasValue == false)
                return;

            _handle.Value.Complete();
            _handle = null;
        }

        private void TryClear()
        {
            if (_inited == false)
                return;

            Clear();
        }

        private void Clear()
        {
            ForceCompleteJobProcess();

            if (_cashedModifications.Count > 0)
                _cashedModifications.Clear();

            if (_defaultMesh != null && MeshHolderPointer == _generatedMesh)
                MeshHolderPointer = _defaultMesh;

            if (_generatedMesh != null)
            {
                Destroy(_generatedMesh);
                _generatedMesh = null;
            }

            if (_colors.IsCreated)
                _colors.Dispose();

            if (_defaultColors.IsCreated)
                _defaultColors.Dispose();

            if (_modifications.IsCreated)
                _modifications.Dispose();

            OnClearing();

            _inited = false;
        }

        #endregion
    }
}