
// ImagePreHubDevkit.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace ImagePreHub
{
    [AddComponentMenu("ImagePreHub/ImagePreHubDevkit")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class ImagePreHubDevkit : UdonSharpBehaviour
    {
        [SerializeField] private List<DiagnosticBase> diagnostics = new List<DiagnosticBase>();
        [SerializeField] private OptimizationProfile optimizationProfile;
        [SerializeField] private bool runOnStart = true;

        private void Start()
        {
            if (runOnStart)
            {
                RunDiagnostics();
            }
        }

        public void RunDiagnostics()
        {
            try
            {
                foreach (var diagnostic in diagnostics)
                {
                    diagnostic.Run(optimizationProfile);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void HandleException(Exception ex)
        {
            Debug.LogError($"An error occurred: {ex.Message}\n{ex.StackTrace}");
            // Add error handling logic here
            // Example: Send error reports, perform recovery actions, etc.
        }

        public void AddDiagnostic(DiagnosticBase diagnostic)
        {
            if (!diagnostics.Contains(diagnostic))
            {
                diagnostics.Add(diagnostic);
            }
        }

        public void RemoveDiagnostic(DiagnosticBase diagnostic)
        {
            if (diagnostics.Contains(diagnostic))
            {
                diagnostics.Remove(diagnostic);
            }
        }

        public void SetOptimizationProfile(OptimizationProfile profile)
        {
            optimizationProfile = profile;
        }
    }

    // OptimizationProfile.cs
    [CreateAssetMenu(fileName = "OptimizationProfile", menuName = "ImagePreHub/OptimizationProfile")]
    public class OptimizationProfile : ScriptableObject
    {
        public int maxPolygonCount = 50000;
        public int maxTextureSize = 2048;
        public int maxMaterialCount = 10;
        // Add more optimization settings as needed
    }

    // DiagnosticBase.cs
    public abstract class DiagnosticBase : MonoBehaviour
    {
        public abstract void Run(OptimizationProfile profile);
    }

    // PolygonCountDiagnostic.cs
    public class PolygonCountDiagnostic : DiagnosticBase
    {
        public override void Run(OptimizationProfile profile)
        {
            int totalPolygonCount = CalculateTotalPolygonCount();

            if (totalPolygonCount > profile.maxPolygonCount)
            {
                Debug.LogWarning($"Polygon count exceeds the limit: {totalPolygonCount}");
            }
            else
            {
                Debug.Log($"Polygon count: {totalPolygonCount}");
            }
        }

        private int CalculateTotalPolygonCount()
        {
            int totalCount = 0;
            var renderers = FindObjectsOfType<Renderer>();

            foreach (var renderer in renderers)
            {
                if (renderer is MeshRenderer meshRenderer)
                {
                    var meshFilters = meshRenderer.GetComponentsInChildren<MeshFilter>();
                    foreach (var meshFilter in meshFilters)
                    {
                        var mesh = meshFilter.sharedMesh;
                        if (mesh != null)
                        {
                            totalCount += mesh.triangles.Length / 3;
                        }
                    }
                }
                else if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                {
                    var mesh = skinnedMeshRenderer.sharedMesh;
                    if (mesh != null)
                    {
                        totalCount += mesh.triangles.Length / 3;
                    }
                }
            }

            return totalCount;
        }
    }

    // TextureSizeDiagnostic.cs
    public class TextureSizeDiagnostic : DiagnosticBase
    {
        public override void Run(OptimizationProfile profile)
        {
            var textures = FindObjectsOfType<Texture>();

            foreach (var texture in textures)
            {
                if (texture.width > profile.maxTextureSize || texture.height > profile.maxTextureSize)
                {
                    Debug.LogWarning($"Texture size exceeds the limit: {texture.name} ({texture.width}x{texture.height})");
                }
            }

            Debug.Log("Texture size diagnostic completed.");
        }
    }

    // MaterialCountDiagnostic.cs
    public class MaterialCountDiagnostic : DiagnosticBase
    {
        public override void Run(OptimizationProfile profile)
        {
            var materials = FindObjectsOfType<Material>();

            if (materials.Length > profile.maxMaterialCount)
            {
                Debug.LogWarning($"Material count exceeds the limit: {materials.Length}");
            }
            else
            {
                Debug.Log($"Material count: {materials.Length}");
            }
        }
    }
}
