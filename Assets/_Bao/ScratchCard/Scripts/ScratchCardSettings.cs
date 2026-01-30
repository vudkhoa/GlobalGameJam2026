using UnityEngine;

namespace ScratchCard
{
    /// <summary>
    /// ScriptableObject containing all configuration for scratch card mechanics
    /// </summary>
    [CreateAssetMenu(fileName = "ScratchCardSettings", menuName = "ScratchCard/Settings")]
    public class ScratchCardSettings : ScriptableObject
    {
        [Header("Brush Settings")]
        [Tooltip("Size of the scratch brush in pixels")]
        [Range(10f, 200f)]
        public float brushSize = 50f;

        [Tooltip("Hardness of the brush edge (0 = soft, 1 = hard)")]
        [Range(0f, 1f)]
        public float brushHardness = 0.5f;

        [Tooltip("Opacity of each brush stroke")]
        [Range(0f, 1f)]
        public float brushOpacity = 1f;

        [Header("Win Condition")]
        [Tooltip("Percentage of card that must be scratched to trigger win (0-100)")]
        [Range(0f, 100f)]
        public float winThresholdPercent = 70f;

        [Header("Performance")]
        [Tooltip("Resolution of the mask texture (higher = better quality but slower)")]
        public int maskResolution = 512;

        [Tooltip("Minimum distance between scratch points to register (optimization)")]
        [Range(1f, 20f)]
        public float minScratchDistance = 5f;

        [Header("Visual")]
        [Tooltip("Color tint for the scratch layer")]
        public Color scratchLayerColor = Color.white;

        [Tooltip("Texture for the scratch layer (metallic/silver effect)")]
        public Texture2D scratchLayerTexture;
    }
}
