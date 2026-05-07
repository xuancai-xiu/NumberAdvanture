using UnityEngine;
using System.Collections;

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
    private Coroutine playCoroutine;

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
            PlayRandomOrFirst();
        else
            Debug.LogWarning("没有添加任何背景音乐！");
    }

    // ========== 公开的 UI 控制接口 ==========

    /// <summary> 播放上一首 </summary>
    public void PlayPreviousTrack()
    {
        StopCurrentCoroutine();

        if (playMode == PlayMode.Sequential)
        {
            currentTrackIndex--;
            if (currentTrackIndex < 0)
                currentTrackIndex = musicPlaylist.Length - 1;
            PlayTrackByIndex(currentTrackIndex);
        }
        else 
        {
            int newIndex;
            do
            {
                newIndex = Random.Range(0, musicPlaylist.Length);
            } while (newIndex == currentTrackIndex && musicPlaylist.Length > 1);
            currentTrackIndex = newIndex;
            PlayTrackByIndex(currentTrackIndex);
        }
    }

    /// <summary> 播放下一首 </summary>
    public void PlayNextTrack()
    {
        StopCurrentCoroutine();

        if (playMode == PlayMode.Sequential)
        {
            currentTrackIndex = (currentTrackIndex + 1) % musicPlaylist.Length;
            PlayTrackByIndex(currentTrackIndex);
        }
        else 
        {
            int newIndex;
            do
            {
                newIndex = Random.Range(0, musicPlaylist.Length);
            } while (newIndex == currentTrackIndex && musicPlaylist.Length > 1);
            currentTrackIndex = newIndex;
            PlayTrackByIndex(currentTrackIndex);
        }
    }

    /// <summary> 切换播放 / 暂停 </summary>
    public void TogglePlayPause()
    {
        if (musicSource.isPlaying)
            musicSource.Pause();
        else
            musicSource.UnPause();
    }

    /// <summary> 切换播放模式（顺序 / 随机） </summary>
    public void TogglePlayMode()
    {
        playMode = (playMode == PlayMode.Sequential) ? PlayMode.Random : PlayMode.Sequential;
        Debug.Log($"播放模式切换为: {playMode}");
    }

    /// <summary> 设置音量（0-1） </summary>
    public void SetVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    /// <summary> 静音开/关 </summary>
    public void ToggleMute()
    {
        musicSource.mute = !musicSource.mute;
    }

    /// <summary> 获取当前是否正在播放 </summary>
    public bool IsPlaying => musicSource.isPlaying;

    /// <summary> 获取当前播放的曲目名称 </summary>
    public string CurrentTrackName => musicSource.clip != null ? musicSource.clip.name : "无";

    // ========== 内部私有方法 ==========

    private void PlayRandomOrFirst()
    {
        if (playMode == PlayMode.Random)
            currentTrackIndex = Random.Range(0, musicPlaylist.Length);
        else
            currentTrackIndex = 0;

        PlayTrackByIndex(currentTrackIndex);
    }

    private void PlayTrackByIndex(int index)
    {
        if (index < 0 || index >= musicPlaylist.Length) return;
        musicSource.clip = musicPlaylist[index];
        musicSource.Play();
        Debug.Log($"🎵 正在播放: {musicSource.clip.name}");

        StopCurrentCoroutine();
        playCoroutine = StartCoroutine(WaitForMusicEnd());
    }

    private IEnumerator WaitForMusicEnd()
    {
        float totalLength = musicSource.clip.length;
        float playedTime = 0f;

        while (playedTime < totalLength)
        {
            if (musicSource.isPlaying)
            {
                playedTime += Time.deltaTime;
            }
            yield return null;   
        }

        PlayNextTrack();
    }

    private void StopCurrentCoroutine()
    {
        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }
    }

    /// <summary> 重新开始播放（从头或随机） </summary>
    public void RestartPlaylist()
    {
        if (musicPlaylist == null || musicPlaylist.Length == 0) return;
        PlayRandomOrFirst();
    }
}