using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine;

public class AudioManager : Singletone<AudioManager>
{
    private const string localBGMVolume = "BGM_Volume";
    private const string localEffectVolume = "Effect_Volume";

    private List<AudioSource> _liAudioSource = new List<AudioSource>();
    private Dictionary<string, AudioClip> _dicAudioClip = new Dictionary<string, AudioClip>();

    public float BgmVolume { get; private set; } = 1f;

    public float EffectVolume { get; private set; } = 1f;

    public float Ratio { get; private set; } = 0.5f;

    bool _initialize = false;

    public void Init()
    {
        if (_initialize)
            return;

        _initialize = true;

        _liAudioSource.Add(new AudioSource());

        for (int i = 1; i < Enum.GetNames(typeof(CommonEnum.EAudio)).Length; i++)
        {
            GameObject go = new GameObject { name = $"{(CommonEnum.EAudio)i}" };
            go.transform.parent = transform;

            var audioSource = go.AddComponent<AudioSource>();
            _liAudioSource.Add(audioSource);
        }

        _liAudioSource[(int)CommonEnum.EAudio.BGM].loop = true;

        DontDestroyOnLoad(transform);
    }

    async Task<AudioClip> LoadSound(CommonEnum.EAudio eAudio)
    {
        TaskCompletionSource<AudioClip> taskCompletionSource = new TaskCompletionSource<AudioClip>();

        ResourceManager.Instance.LoadAudio(eAudio, (sound) =>
        {
            taskCompletionSource.SetResult(sound);
        });

        return await taskCompletionSource.Task;
    }

    public void SetBGMVolume(float volume)
    {
        _liAudioSource[(int)CommonEnum.EAudio.BGM].volume = BgmVolume * Ratio;
    }

    public void SetEffectVolume(float volume)
    {
        for (int i = 0; i < _liAudioSource.Count; ++i)
        {
            if (i == (int)CommonEnum.EAudio.BGM)
                continue;

            _liAudioSource[i].volume = EffectVolume * Ratio;
        }
    }

    public async void Play(CommonEnum.EAudio audioType, float pitch = 1.0f)
    {
        AudioClip audioClip = await GetOrAddAudioClip(audioType);
        AudioSource audioSource = _liAudioSource[(int)audioType];

        audioSource.pitch = pitch;
        audioSource.clip = audioClip;

        if (audioType == CommonEnum.EAudio.BGM)
        {
            audioSource.volume = BgmVolume * Ratio;

            if (audioSource.isPlaying)
                return;

            audioSource.Play();
        }
        else
        {
            audioSource.volume = EffectVolume * Ratio;
            audioSource.Play();
        }
    }

    async Task<AudioClip> GetOrAddAudioClip(CommonEnum.EAudio audioType)
    {
        AudioClip audioClip = null;

        if (_dicAudioClip.TryGetValue(audioType.ToString(), out audioClip) == false)
        {
            audioClip = await LoadSound(audioType);
            _dicAudioClip.Add(audioType.ToString(), audioClip);
        }

        if (audioClip == null)
            Debug.Log($"AudioClip Missing! {audioType.ToString()}");

        return audioClip;
    }
}
