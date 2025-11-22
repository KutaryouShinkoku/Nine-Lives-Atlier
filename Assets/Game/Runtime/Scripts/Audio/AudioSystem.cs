using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Data;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Audio;
using WanFramework.Base;
using WanFramework.Resource;

namespace Game.Audio
{
    /// <summary>
    /// 音效事件
    /// </summary>
    public struct AudioEvent
    {
        public string Path;
        public AudioEventType Type;
        public float Volume;
    }
    /// <summary>
    /// 音轨信息
    /// </summary>
    [Serializable]
    public class AudioTrack
    {
        [SerializeField]
        private float transmitTime = 1.0f;
        [SerializeField]
        private bool isLoop;
        [SerializeField]
        private string name;
        public string Name => name;
        [SerializeField]
        private AudioMixerGroup mixerGroup;

        private long _currentEventId = 0;
        private long _currentPendingEventCount = 0;
        
        private GameObject _root;
        private List<AudioSource> _curAudioSources = new();
        private Queue<AudioSource> _audioSourcePool = new();
        private CancellationTokenSource _cts;
        private CancellationTokenSource _fadeInCts;
        
        public void Init(AudioSystem system)
        {
            _root = new GameObject(name)
            {
                transform =
                {
                    parent = system.transform
                }
            };
            _cts = new CancellationTokenSource();
        }
        private AudioSource GetFreeAudioSource()
        {
            for (var i = _curAudioSources.Count - 1; i >= 0; i--)
                if (_curAudioSources[i].timeSamples == _curAudioSources[i].clip?.samples)
                {
                    _curAudioSources[i].Stop();
                    _curAudioSources[i].clip = null;
                    _curAudioSources[i].enabled = false;
                    _audioSourcePool.Enqueue(_curAudioSources[i]);
                    _curAudioSources.RemoveAt(i);
                }
            var source = _audioSourcePool.Count > 0 ? 
                _audioSourcePool.Dequeue() : 
                _root.AddComponent<AudioSource>();
            source.loop = isLoop;
            source.outputAudioMixerGroup = mixerGroup;
            _curAudioSources.Add(source);
            source.enabled = true;
            return source;
        }
        private void ReleaseAudioSource(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.enabled = false;
            _curAudioSources.Remove(source);
            _audioSourcePool.Enqueue(source);
        }
        public void Destroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _fadeInCts?.Cancel();
            _fadeInCts?.Dispose();
            _fadeInCts = null;
        }
        private bool CheckEventEqual(long evtId) => evtId == _currentEventId;
        public async UniTask OnEventAsync(AudioEvent evt, CancellationToken token)
        {
            var eventId = _currentPendingEventCount++;
            var audioClip = await GetAudioClipAsync(evt.Path, token);
            await UniTask.WaitUntil(eventId, CheckEventEqual, cancellationToken: token);
            ++_currentEventId;
            if (audioClip != null)
                switch (evt.Type)
                {
                    case AudioEventType.Solo:
                        PlaySolo(audioClip, evt.Volume);
                        return;
                    case AudioEventType.Transmit:
                        PlayTransmit(audioClip, evt.Volume);
                        return;
                    case AudioEventType.SyncTransmit:
                        PlayTransmit(audioClip, evt.Volume, true);
                        return;
                    case AudioEventType.Mix:
                        PlayMix(audioClip, evt.Volume);
                        return;
                }
            Debug.LogError(
                "Failed to send audio event with args " +
                $"Path={evt.Path}, " +
                $"EventType={evt.Type}, " +
                $"VolumeScale={evt.Volume}");
        }
        public void OnEvent(AudioEvent evt) => OnEventAsync(evt, _cts.Token).Forget();
        [CanBeNull] private static UniTask<AudioClip> GetAudioClipAsync(string path, CancellationToken token)
            => ResourceSystem.Instance.LoadAsyncUniTask<AudioClip>($"Content/Audio/{path}", token);
        private void PlaySolo(AudioClip clip, float volume)
        {
            for (var i = 0; i < _curAudioSources.Count; i++)
                ReleaseAudioSource(_curAudioSources[i]);
            var s = GetFreeAudioSource();
            s.clip = clip;
            s.volume = volume;
            s.Play();
        }
        private void PlayTransmit(AudioClip clip, float volume, bool syncTime = false)
        {
            _fadeInCts?.Cancel();
            _fadeInCts?.Dispose();
            _fadeInCts = new CancellationTokenSource();
            PlayTransmitAsync(clip, volume, syncTime, _cts.Token, _fadeInCts.Token).Forget();
        }
        private async UniTask PlayTransmitAsync(AudioClip clip, float volume, bool syncTime, CancellationToken ctOut, CancellationToken ctIn)
        {
            int startSample;
            if (syncTime) startSample = _curAudioSources.Count != 0 ? _curAudioSources[0].timeSamples : 0;
            else startSample = 0;
            var fadeOutTask = FadeOutAsync(ctOut);
            var fadeInTask = FadeInAsync(clip, volume, startSample, ctIn);
            await fadeOutTask;
            await fadeInTask;
        }
        private void PlayMix(AudioClip clip, float volume)
        {
            AudioSource source;
            if (_curAudioSources.Count != 0)
                source = _curAudioSources[0];
            else
            {
                source = GetFreeAudioSource();
                source.volume = 1;
            }
            source.PlayOneShot(clip, volume);
        }
        private async UniTask FadeOutAsync(CancellationToken ct)
        {
            var fadingOutCount = _curAudioSources.Count;
            if (fadingOutCount == 0) return;
            var fadingOutSources = ArrayPool<AudioSource>.Shared.Rent(fadingOutCount);
            for (var i = 0; i < fadingOutCount; i++)
                fadingOutSources[i] = _curAudioSources[i];
            _curAudioSources.Clear();
            var oldVolumes = ArrayPool<float>.Shared.Rent(fadingOutCount);
            for (var i = 0; i < fadingOutCount; i++)
                oldVolumes[i] = fadingOutSources[i].volume;
            try
            {
                for (var t = transmitTime; t > 0; t -= Time.deltaTime)
                {
                    for (var i = 0; i < fadingOutCount; i++)
                        fadingOutSources[i].volume = oldVolumes[i] * t / transmitTime;
                    await UniTask.Yield(ct);
                }
            }
            finally
            {
                for (var i = 0; i < fadingOutCount; i++)
                {
                    if (!fadingOutSources[i]) continue;
                    fadingOutSources[i].volume = 0;
                    ReleaseAudioSource(fadingOutSources[i]);
                }
                ArrayPool<float>.Shared.Return(oldVolumes);
                ArrayPool<AudioSource>.Shared.Return(fadingOutSources);
            }
        }
        private async UniTask FadeInAsync(AudioClip clip, float volume, int startSample, CancellationToken ct)
        {
            var source = GetFreeAudioSource();
            source.volume = 0;
            source.clip = clip;
            source.timeSamples = startSample;
            source.Play();
            for (var t = 0f; t < transmitTime; t += Time.deltaTime)
            {
                source.volume = volume * t / transmitTime;
                await UniTask.Yield(ct);
            }
            source.volume = volume;
        }
    }
    
    [SystemPriority(SystemPriorities.DataSystem - 1)]
    public class AudioSystem : SystemBase<AudioSystem>
    {
        [SerializeField]
        private AudioTrack[] tracks;
        [SerializeField]
        private AudioMixer mixer;
        private Dictionary<string, AudioTrack> _trackDict = new();
        public override UniTask Init()
        {
            foreach (var track in tracks)
            {
                _trackDict[track.Name] = track;
                track.Init(this);
            }
            return base.Init();
        }
        private void OnDestroy()
        {
            foreach (var track in tracks)
                track.Destroy();
        }
        public void SendEvent(AudioIds id)
        {
            var evtData = id.Data();
            if (!_trackDict.TryGetValue(evtData.Track, out var track))
            {
                Debug.LogError($"No track found for {evtData.Track}");
                return;
            }
            track.OnEvent(new AudioEvent
            {
                Path = evtData.Path,
                Type = evtData.EventType,
                Volume = evtData.VolumeScale,
            });
        }
        public void SetValue(string valName, float val) => mixer.SetFloat(valName, val);
        public static float ToDb(float val)
        {
            if (val <= 0.0f) val = 0.000001f;
            return Mathf.Log10(val) * 20.0f;
        }
    }
}