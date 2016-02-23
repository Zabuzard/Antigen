namespace Antigen.Sound
{
    /// <summary>
    /// Sounds belong either to category music or effect.
    /// Play music looped until their instance is stopped.
    /// Play sound effects only once per call.
    /// </summary>
    enum SoundCategory
    {
        Music,
        Effect
    }
}
