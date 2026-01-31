using UnityEngine;

/// <summary>
/// ScriptableObject settings for Correction mini-game
/// Configurable parameters for game behavior
/// </summary>
[CreateAssetMenu(fileName = "CorrectionSettings", menuName = "CorrectionToReveal/Settings")]
public class CorrectionSettings : ScriptableObject
{
    [Header("Shader Settings")]
    [Tooltip("Material with ClarityScale shader")]
    public Material CorrectionMaterial;

    [Tooltip("Texture to apply correction to")]
    public Texture2D TargetTexture;

    [Header("Initial Values")]
    [Tooltip("Initial blur amount (distorted)")]
    [Range(0f, 10f)]
    public float InitialBlurAmount = 5.0f;

    [Tooltip("Initial horizontal scale (distorted)")]
    [Range(1f, 3f)]
    public float InitialHorizontalScale = 1.5f;

    [Header("Target Values")]
    [Tooltip("Target blur amount (correct)")]
    [Range(0f, 10f)]
    public float TargetBlurAmount = 0.0f;

    [Tooltip("Target horizontal scale (correct)")]
    [Range(1f, 3f)]
    public float TargetHorizontalScale = 1.0f;

    [Header("Game Rules")]
    [Tooltip("Tolerance for winning condition")]
    [Range(0.01f, 1.0f)]
    public float Tolerance = 0.1f;

    [Header("UI Settings")]
    [Tooltip("Ruler prefab for parameter adjustment")]
    public GameObject RulerPrefab;

    [Tooltip("Spacing between rulers")]
    public float RulerSpacing = 150f;

    [Header("Shader Property Names")]
    public string BlurPropertyName = "_BlurAmount";
    public string ScalePropertyName = "_HorizontalScale";
}
