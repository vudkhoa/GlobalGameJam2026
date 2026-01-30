using UnityEngine;

/// <summary>
/// Configuration for scratch reveal system
/// ScriptableObject for easy tweaking in Inspector
/// </summary>
[CreateAssetMenu(fileName = "ScratchSettings", menuName = "ScratchToReveal/Settings")]
public class ScratchSettings : ScriptableObject
{
    [Header("Brush Settings")]
    [Tooltip("Size of the scratch brush in pixels")]
    [Range(10f, 200f)]
    public float brushSize = 50f;

    [Tooltip("Opacity of each brush stroke")]
    [Range(0f, 1f)]
    public float brushOpacity = 1f;

    [Header("Performance")]
    [Tooltip("Resolution of the mask texture (higher = better quality but slower)")]
    public int maskResolution = 512;

    [Tooltip("Minimum distance between scratch points to register (optimization)")]
    [Range(1f, 20f)]
    public float minMoveDistance = 5f;

    [Header("Visual")]
    [Tooltip("Color tint for the scratch layer")]
    public Color scratchLayerColor = Color.white;

    [Tooltip("Texture for the scratch layer (metallic/silver effect)")]
    public Texture2D scratchLayerTexture;
}
