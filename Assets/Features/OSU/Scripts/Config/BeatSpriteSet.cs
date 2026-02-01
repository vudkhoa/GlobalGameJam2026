using UnityEngine;

/// <summary>
/// ScriptableObject: Chứa set sprites cho beat
/// Allows artists to create different visual styles
/// </summary>
[CreateAssetMenu(fileName = "BeatSpriteSet", menuName = "OSU/Beat Sprite Set")]
public class BeatSpriteSet : ScriptableObject
{
    [Header("Beat Sprites")]
    [Tooltip("Sprite cho outer ring (vòng ngoài)")]
    public Sprite outerRingSprite;

    [Tooltip("Sprite cho inner ring (vòng trong, sẽ shrink)")]
    public Sprite innerRingSprite;

    [Header("Hit Effect Sprite")]
    [Tooltip("Sprite hiện khi tap trúng beat (optional)")]
    public Sprite hitEffectSprite;





}