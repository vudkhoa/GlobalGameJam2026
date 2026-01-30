/// <summary>
/// SRP: Store game-wide constants (hardcoded)
/// Responsibility: Centralize constant values
/// </summary>
public static class GameConstants
{
    // ═══════════════════════════════════════════════════════════
    // INPUT (Mobile-first)
    // ═══════════════════════════════════════════════════════════

    public const bool ALLOW_TOUCH = true;
    public const bool ALLOW_MOUSE = true; // Debug only
    public const bool ALLOW_KEYBOARD = false;

    // ═══════════════════════════════════════════════════════════
    // AUDIO
    // ═══════════════════════════════════════════════════════════

    public const float MUSIC_VOLUME = 0.8f;
    public const float SFX_VOLUME = 1f;

    // ═══════════════════════════════════════════════════════════
    // VISUAL
    // ═══════════════════════════════════════════════════════════

    public const float BACKGROUND_BLUR_AMOUNT = 0.7f;
    public const float BLUR_TRANSITION_DURATION = 0.5f;
}                                                                           