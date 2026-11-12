using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class GameAudio : MonoBehaviour
{
    private const int SampleRate = 24000;
    private const float UrgentTime = 20f;
    private AudioSource music;
    private AudioSource heartbeat;
    private AudioSource effects;
    private AudioClip pickup;
    private AudioClip win;
    private AudioClip lose;

    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.42f;
    [SerializeField, Range(0f, 1f)] private float effectsVolume = 0.55f;

    private void Awake()
    {
        music = AddSource();
        music.clip = MakeClip("Manor suspense", 8f, t =>
        {
            float beat = t % 0.5f;
            float step = Mathf.Floor(t * 2f);
            float note = step % 4f == 3f ? 49f : 55f;
            float pluck = Mathf.Min(1f, beat * 140f) * Mathf.Exp(-beat * 10f);
            float drone = Wave(55f * t) * 0.13f + Wave(58.25f * t) * 0.07f;
            float swell = (0.5f - 0.5f * Mathf.Cos(t * Mathf.PI * 0.5f));
            return drone + swell * (Wave(82.5f * t) + Wave(87.25f * t)) * 0.045f
                + pluck * (Wave(note * t) + 0.2f * Wave(note * 2f * t)) * 0.28f;
        });
        music.loop = true;
        music.volume = musicVolume;
        music.Play();

        heartbeat = AddSource();
        heartbeat.clip = MakeClip("Heartbeat", 0.8f, t =>
            Thump(t) + Thump(t - 0.18f) * 0.65f);
        heartbeat.loop = true;
        heartbeat.volume = 0f;

        effects = AddSource();
        pickup = MakeClip("Gem pickup", 0.34f, t =>
            Mathf.Exp(-t * 9f) * (Wave(660f * t) * 0.40f + Wave(990f * t) * 0.23f));
        win = MakeClip("Heist complete", 1.25f, t =>
            Mathf.Exp(-t * 1.6f) * (Wave(220f * t) + Wave(277f * t) + Wave(330f * t)) * 0.20f);
        lose = MakeClip("Time up", 1.4f, t =>
            Mathf.Exp(-t * 1.9f) * (Wave(98f * t - 19f * t * t) + Wave(104f * t - 19f * t * t)) * 0.30f);
    }

    public void SetTimeRemaining(float seconds)
    {
        if (seconds > UrgentTime)
            return;

        if (!heartbeat.isPlaying)
            heartbeat.Play();

        float urgency = 1f - Mathf.Clamp01(seconds / UrgentTime);
        heartbeat.volume = Mathf.Lerp(0.20f, 0.62f, urgency) * effectsVolume;
        heartbeat.pitch = Mathf.Lerp(0.9f, 1.4f, urgency);
    }

    public void PlayPickup()
    {
        effects.PlayOneShot(pickup, effectsVolume);
    }

    public void PlayResult(bool won)
    {
        music.Stop();
        heartbeat.Stop();
        effects.Stop();
        effects.PlayOneShot(won ? win : lose, effectsVolume);
    }

    private AudioSource AddSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        return source;
    }

    private static AudioClip MakeClip(string name, float seconds, Func<float, float> sample)
    {
        float[] data = new float[Mathf.RoundToInt(seconds * SampleRate)];
        for (int i = 0; i < data.Length; i++)
        {
            float t = (float)i / SampleRate;
            float edgeFade = Mathf.Clamp01(Mathf.Min(t, seconds - t) * 100f);
            data[i] = Mathf.Clamp(sample(t) * edgeFade, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static float Wave(float cycles)
    {
        return Mathf.Sin(cycles * 2f * Mathf.PI);
    }

    private static float Thump(float t)
    {
        if (t < 0f)
            return 0f;

        return Wave(51f * t) * Mathf.Min(1f, t * 150f) * Mathf.Exp(-t * 18f);
    }

    private void OnDestroy()
    {
        if (music != null) Destroy(music.clip);
        if (heartbeat != null) Destroy(heartbeat.clip);
        Destroy(pickup);
        Destroy(win);
        Destroy(lose);
    }
}
