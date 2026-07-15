using UnityEngine;

/// <summary>
/// Gera pequenos sons de fala cartoon sem depender de ficheiros de audio.
/// O corvo tem um timbre grave e rouco; o pombo um timbre mais leve.
/// </summary>
public sealed class SubtitleVoicePlayer : MonoBehaviour
{
    private AudioSource audioSource;
    private AudioSource ambienceSource;
    private AudioClip crowClip;
    private AudioClip pigeonClip;
    private AudioClip ambienceClip;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.70f;

        ambienceSource = gameObject.AddComponent<AudioSource>();
        ambienceSource.playOnAwake = false;
        ambienceSource.loop = true;
        ambienceSource.spatialBlend = 0f;
        ambienceSource.volume = 0.14f;

        crowClip = CreateVoiceClip("Voz Corvo", 95f, 0.46f, 0.09f);
        pigeonClip = CreateVoiceClip("Voz Pombo", 285f, 0.10f, 0.065f);
    }

    public void PlayCharacter(string speakerName, int characterIndex)
    {
        if (audioSource == null || string.IsNullOrWhiteSpace(speakerName))
            return;

        string normalizedName = speakerName.ToLowerInvariant();
        AudioClip clip;
        float volume;

        if (normalizedName.Contains("corvo"))
        {
            clip = crowClip;
            volume = 0.78f;
            audioSource.pitch = 0.78f + (characterIndex % 4) * 0.018f;
        }
        else if (normalizedName.Contains("pombo"))
        {
            clip = pigeonClip;
            volume = 0.66f;
            audioSource.pitch = 1.02f + (characterIndex % 5) * 0.022f;
        }
        else
        {
            return;
        }

        audioSource.PlayOneShot(clip, volume);
    }

    public void StartAmbience()
    {
        if (ambienceSource == null)
            return;

        if (ambienceClip == null)
            ambienceClip = CreateAmbientClip();

        ambienceSource.clip = ambienceClip;
        if (!ambienceSource.isPlaying)
            ambienceSource.Play();
    }

    public void StopAmbience()
    {
        if (ambienceSource != null)
            ambienceSource.Stop();
    }

    public void StopVoice()
    {
        if (audioSource != null)
            audioSource.Stop();
    }

    private static AudioClip CreateVoiceClip(
        string clipName,
        float frequency,
        float roughness,
        float duration)
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float time = i / (float)sampleRate;
            float progress = i / (float)(sampleCount - 1);
            float envelope = Mathf.Sin(progress * Mathf.PI);
            float fundamental = Mathf.Sin(time * frequency * Mathf.PI * 2f);
            float harmonic = Mathf.Sin(time * frequency * 2.03f * Mathf.PI * 2f) * 0.34f;
            float wobble = Mathf.Sin(time * 31f) * roughness;
            float pseudoNoise = Mathf.Sin(i * 12.9898f) * roughness * 0.22f;
            samples[i] = (fundamental + harmonic + wobble + pseudoNoise) * envelope * 0.36f;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip CreateAmbientClip()
    {
        const int sampleRate = 44100;
        const float duration = 8f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float time = i / (float)sampleRate;
            float movement = 0.82f + Mathf.Sin(time * Mathf.PI * 0.25f) * 0.18f;
            float chord =
                Mathf.Sin(time * 110f * Mathf.PI * 2f) * 0.42f +
                Mathf.Sin(time * 165f * Mathf.PI * 2f) * 0.25f +
                Mathf.Sin(time * 220f * Mathf.PI * 2f) * 0.16f;
            float softPulse = 0.72f + Mathf.Sin(time * Mathf.PI) * 0.10f;
            samples[i] = chord * movement * softPulse * 0.13f;
        }

        AudioClip clip = AudioClip.Create("Ambiente Cutscene", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
