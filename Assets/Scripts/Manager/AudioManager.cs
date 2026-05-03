using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("背景音乐播放列表")]
    public AudioClip[] musicPlaylist;         
    public PlayMode playMode = PlayMode.Sequential;

    [Header("音乐设置")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    private AudioSource musicSource;
    private int currentTrackIndex = 0;

    public enum PlayMode
    {
        Sequential,   // 顺序循环
        Random        // 随机播放
    }

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 获取或添加 AudioSource
        musicSource = GetComponent<AudioSource>();

        musicSource.loop = false;      
        musicSource.volume = musicVolume;
        musicSource.playOnAwake = false;
    }

    private void Start()
    {
        if (musicPlaylist != null && musicPlaylist.Length > 0)
        {
            PlayRandomOrFirst();
        }
        else
        {
            Debug.LogWarning("没有添加任何背景音乐，请检查 AudioManager 的 musicPlaylist 数组。");
        }
    }

    private void Update()
    {
        // 检测音乐是否播放完毕
        if (musicSource != null && !musicSource.isPlaying && musicPlaylist != null && musicPlaylist.Length > 0)
        {
            PlayNextTrack();
        }
    }

    /// <summary>
    /// 播放下一首（根据模式选择）
    /// </summary>
    private void PlayNextTrack()
    {
        if (playMode == PlayMode.Sequential)
        {
            currentTrackIndex = (currentTrackIndex + 1) % musicPlaylist.Length;
            PlayTrackByIndex(currentTrackIndex);
        }
        else if (playMode == PlayMode.Random)
        {
            // 随机模式：避免连续重复同一首（除非列表只有一首）
            if (musicPlaylist.Length == 1)
            {
                PlayTrackByIndex(0);
                return;
            }
            int newIndex;
            do
            {
                newIndex = Random.Range(0, musicPlaylist.Length);
            } while (newIndex == currentTrackIndex);
            currentTrackIndex = newIndex;
            PlayTrackByIndex(currentTrackIndex);
        }
    }

    private void PlayRandomOrFirst()
    {
        if (playMode == PlayMode.Random)
        {
            currentTrackIndex = Random.Range(0, musicPlaylist.Length);
        }
        else
        {
            currentTrackIndex = 0;
        }
        PlayTrackByIndex(currentTrackIndex);
    }

    private void PlayTrackByIndex(int index)
    {
        if (index < 0 || index >= musicPlaylist.Length) return;
        musicSource.clip = musicPlaylist[index];
        musicSource.Play();
        Debug.Log($"正在播放: {musicSource.clip.name}");
    }

    /// <summary>
    /// 外部可调用：重新开始播放（例如从第一首开始）
    /// </summary>
    public void RestartPlaylist()
    {
        if (musicPlaylist == null || musicPlaylist.Length == 0) return;
        if (playMode == PlayMode.Random)
        {
            currentTrackIndex = Random.Range(0, musicPlaylist.Length);
        }
        else
        {
            currentTrackIndex = 0;
        }
        PlayTrackByIndex(currentTrackIndex);
    }

    /// <summary>
    /// 静音切换
    /// </summary>
    public void ToggleMute() => musicSource.mute = !musicSource.mute;

    /// <summary>
    /// 设置音量（0~1）
    /// </summary>
    public void SetVolume(float vol)
    {
        musicVolume = Mathf.Clamp01(vol);
        musicSource.volume = musicVolume;
    }
}
