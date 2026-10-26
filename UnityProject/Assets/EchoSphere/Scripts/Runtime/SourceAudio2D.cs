using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Runtime
{
    /// <summary>
    /// Procedural port of new-desing/src/audio.ts. Generates short clips in memory,
    /// so the source game's synthesized UI/combat sounds need no external assets.
    /// Audio presentation never reads or advances the gameplay SeededRng.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SourceAudio2D : MonoBehaviour
    {
        private struct Tone
        {
            public float StartHz, EndHz, Duration, Gain;
            public bool Linear;
            public Tone(float startHz, float endHz, float duration, float gain, bool linear = false)
            { StartHz = startHz; EndHz = endHz; Duration = duration; Gain = gain; Linear = linear; }
        }

        private static SourceAudio2D _instance;
        private static readonly Dictionary<string, AudioClip> Clips = new Dictionary<string, AudioClip>();
        private static readonly Dictionary<string, float> LastPlayed = new Dictionary<string, float>();
        private AudioSource _source;
        private bool _enabled = true;
        private const int SampleRate = 22050;
        private const float MasterGain = 0.3f;

        public static bool IsEnabled => _instance == null || _instance._enabled;

        public static void SetEnabled(bool enabled)
        {
            EnsureInstance();
            _instance._enabled = enabled;
            if (_instance._source != null) _instance._source.volume = enabled ? MasterGain : 0f;
        }

        public static void Play(string sound)
        {
            if (string.IsNullOrEmpty(sound)) return;
            EnsureInstance();
            if (_instance == null || !_instance._enabled || _instance._source == null) return;
            var now = Time.unscaledTime;
            var interval = sound == "hit" ? 0.045f : sound == "shoot" ? 0.06f :
                sound == "pickup" ? 0.035f : sound == "kill" ? 0.045f : 0f;
            if (interval > 0f && LastPlayed.TryGetValue(sound, out var previous) && now - previous < interval) return;
            if (interval > 0f) LastPlayed[sound] = now;
            if (!Clips.TryGetValue(sound, out var clip))
            {
                clip = BuildClip(sound);
                if (clip == null) return;
                Clips[sound] = clip;
            }
            _instance._source.PlayOneShot(clip, 1f);
        }

        private static void EnsureInstance()
        {
            if (_instance != null) return;
            _instance = EchoSphere.NativeObjectIdentity.FirstInExistingOrder<SourceAudio2D>();
            if (_instance == null)
            {
                var go = new GameObject("ECHO SPHERE Source Audio");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<SourceAudio2D>();
            }
            _instance.Initialize();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            Initialize();
        }

        private void Initialize()
        {
            if (_source != null) return;
            _source = GetComponent<AudioSource>();
            if (_source == null) _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = false;
            _source.spatialBlend = 0f;
            _source.volume = _enabled ? MasterGain : 0f;
        }

        private static AudioClip BuildClip(string name)
        {
            Tone[] tones;
            string wave;
            switch (name)
            {
                case "hit": wave = "square"; tones = new[] { new Tone(260,105,.06f,.055f), new Tone(520,250,.055f,.018f) }; break;
                case "shoot": wave = "sine"; tones = new[] { new Tone(400,200,.05f,.03f) }; break;
                case "crit": wave = "saw"; tones = new[] { new Tone(720,145,.12f,.105f), new Tone(980,560,.16f,.045f) }; break;
                case "kill": wave = "triangle"; tones = new[] { new Tone(165,65,.1f,.055f), new Tone(360,520,.16f,.022f,true) }; break;
                case "levelup": wave = "sine"; tones = new[] { new Tone(440,880,.3f,.1f,true), new Tone(660,1320,.25f,.08f,true) }; break;
                case "boss": wave = "saw"; tones = new[] { new Tone(80,40,.6f,.15f,true) }; break;
                case "bosshit": wave = "square"; tones = new[] { new Tone(100,50,.1f,.1f) }; break;
                case "place": wave = "sine"; tones = new[] { new Tone(280,520,.15f,.065f,true), new Tone(840,620,.135f,.018f,true) }; break;
                case "damage": wave = "saw"; tones = new[] { new Tone(200,50,.2f,.12f) }; break;
                case "pickup": wave = "sine"; tones = new[] { new Tone(600,900,.08f,.04f,true) }; break;
                case "health": wave = "sine"; tones = new[] { new Tone(500,800,.2f,.08f,true) }; break;
                case "evolve": wave = "sine"; tones = new[] { new Tone(330,990,.5f,.1f,true) }; break;
                case "dash": wave = "triangle"; tones = new[] { new Tone(800,200,.12f,.08f) }; break;
                case "chest": wave = "sine"; tones = new[] { new Tone(440,880,.3f,.1f,true) }; break;
                case "wave": wave = "triangle"; tones = new[] { new Tone(220,330,.2f,.06f,true) }; break;
                case "gameover": wave = "saw"; tones = new[] { new Tone(300,50,1f,.12f,true) }; break;
                case "elite": wave = "square"; tones = new[] { new Tone(150,300,.2f,.1f,true) }; break;
                case "explosion": wave = "noise"; tones = new[] { new Tone(120,30,.25f,.1f) }; break;
                default: return null;
            }

            var duration = 0f;
            foreach (var tone in tones) duration = Mathf.Max(duration, tone.Duration);
            if (name == "levelup") duration = .4f;
            else if (name == "place") duration = .2f;
            else if (name == "chest") duration = .35f;
            else if (name == "evolve") duration = .6f;
            else if (name == "dash") duration = .15f;
            else if (name == "boss") duration = .7f;
            else if (name == "bosshit") duration = .12f;
            else if (name == "damage") duration = .25f;
            else if (name == "gameover") duration = 1.1f;
            else if (name == "elite" || name == "wave" || name == "health") duration = .25f;
            else if (name == "explosion") duration = .35f;
            else if (name == "crit") duration = .17f;
            else if (name == "kill") duration = .18f;
            else if (name == "hit") duration = .07f;
            else if (name == "shoot") duration = .06f;
            else if (name == "pickup") duration = .1f;

            var count = Mathf.Max(1, Mathf.CeilToInt(duration * SampleRate));
            var samples = new float[count];
            for (var i = 0; i < count; i++)
            {
                var time = (float)i / SampleRate;
                var sample = 0f;
                foreach (var tone in tones)
                {
                    if (time > tone.Duration) continue;
                    var progress = Mathf.Clamp01(time / tone.Duration);
                    var hz = tone.Linear ? Mathf.Lerp(tone.StartHz, tone.EndHz, progress) :
                        tone.StartHz * Mathf.Pow(Mathf.Max(.0001f, tone.EndHz / tone.StartHz), progress);
                    var phase = 2f * Mathf.PI * hz * time;
                    float value;
                    if (wave == "noise") value = UnityEngine.Random.value * 2f - 1f;
                    else if (wave == "square") value = Mathf.Sin(phase) >= 0f ? 1f : -1f;
                    else if (wave == "saw") value = 2f * (phase / (2f * Mathf.PI) - Mathf.Floor(.5f + phase / (2f * Mathf.PI)));
                    else if (wave == "triangle") value = 2f / Mathf.PI * Mathf.Asin(Mathf.Sin(phase));
                    else value = Mathf.Sin(phase);
                    var envelope = Mathf.Exp(-7.5f * progress);
                    sample += value * tone.Gain * envelope;
                }
                samples[i] = Mathf.Clamp(sample, -1f, 1f);
            }
            var clip = AudioClip.Create("ECHO SPHERE " + name, count, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }
}