using UnityEngine;

public class PlayerAttackSFX : MonoBehaviour
{
    [Header("Voice (±‚«’)")]
    [SerializeField] private AudioClip[] shoutClips;
    [Range(0f, 1f)][SerializeField] private float shoutVolume = 0.8f;

    [Header("Weapon Swing")]
    [SerializeField] private AudioClip[] swingClips;
    [Range(0f, 1f)][SerializeField] private float swingVolume = 0.8f;

    public void PlayShout()
    {
        AudioManager.Instance?.PlayRandom2DSfx(shoutClips, shoutVolume, 0.95f, 1.05f);
    }

    public void PlaySwing()
    {
        AudioManager.Instance?.PlayRandom2DSfx(swingClips, swingVolume);
    }
}