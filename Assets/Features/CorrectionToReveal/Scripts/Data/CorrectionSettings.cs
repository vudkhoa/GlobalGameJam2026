using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// ScriptableObject settings for Correction mini-game
/// Configurable parameters for game behavior
/// </summary>
[CreateAssetMenu(fileName = "CorrectionSettings", menuName = "CorrectionToReveal/Settings")]
public class CorrectionSettings : ScriptableObject
{
    [System.Serializable]
    public struct ShaderPropertySettings
    {
        public string PropertyName;
        public string DisplayName;
        [Range(-10f, 10f)]
        public float InitialValue;
        [Range(-10f, 10f)]
        public float TargetValue;
        public float MinValue;
        public float MaxValue;
    }

    [Header("Shader Settings")]
    [Tooltip("Material with ClarityScale shader")]
    public Material CorrectionMaterial;

    [Tooltip("Texture to apply correction to")]
    public Texture2D TargetTexture;

    [Header("Dynamic Shader Config")]
    [Tooltip("Auto-populated list of shader properties")]
    public List<ShaderPropertySettings> ShaderProperties = new List<ShaderPropertySettings>();

    [Header("Game Rules")]
    [Tooltip("Tolerance for winning condition")]
    [Range(0.01f, 1.0f)]
    public float Tolerance = 0.1f;

    [Header("UI Settings")]
    [Tooltip("Ruler prefab for parameter adjustment")]
    public GameObject RulerPrefab;

    [Tooltip("Spacing between rulers")]
    public float RulerSpacing = 150f;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (CorrectionMaterial != null && CorrectionMaterial.shader != null)
        {
            RefreshShaderProperties(CorrectionMaterial.shader);
        }
    }

    private void RefreshShaderProperties(Shader shader)
    {
        // Dictionary to track existing settings to preserve values
        var existingSettings = new Dictionary<string, ShaderPropertySettings>();
        foreach (var prop in ShaderProperties)
        {
            if (!string.IsNullOrEmpty(prop.PropertyName))
                existingSettings[prop.PropertyName] = prop;
        }

        var newProperties = new List<ShaderPropertySettings>();
        int propertyCount = ShaderUtil.GetPropertyCount(shader);

        for (int i = 0; i < propertyCount; i++)
        {
            ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType(shader, i);

            // Only care about Float and Range properties for rulers
            if (type == ShaderUtil.ShaderPropertyType.Float || type == ShaderUtil.ShaderPropertyType.Range)
            {
                string propName = ShaderUtil.GetPropertyName(shader, i);
                string displayName = ShaderUtil.GetPropertyDescription(shader, i);

                // Get Range limits if available
                float defMin = 0f;
                float defMax = 10f; // Default fallback
                float defVal = 0f;

                if (type == ShaderUtil.ShaderPropertyType.Range)
                {
                    defMin = ShaderUtil.GetRangeLimits(shader, i, 1); // 1 = min (defmin)
                    defMax = ShaderUtil.GetRangeLimits(shader, i, 2); // 2 = max (defmax)
                    defVal = ShaderUtil.GetRangeLimits(shader, i, 0); // 0 = default value
                }

                // If exists, use existing values, otherwise init defaults
                if (existingSettings.TryGetValue(propName, out var existing))
                {
                    // Update display name in case it changed in shader, but keep values
                    existing.DisplayName = displayName;
                    // Update limits if shader defines new limits? 
                    // Usually we trust the config, but if shader is source of truth for limits:
                    // existing.MinValue = defMin;
                    // existing.MaxValue = defMax;
                    newProperties.Add(existing);
                }
                else
                {
                    newProperties.Add(new ShaderPropertySettings
                    {
                        PropertyName = propName,
                        DisplayName = displayName,
                        InitialValue = defVal,
                        TargetValue = defVal,
                        MinValue = defMin,
                        MaxValue = defMax
                    });
                }
            }
        }

        ShaderProperties = newProperties;
    }
#endif
}
