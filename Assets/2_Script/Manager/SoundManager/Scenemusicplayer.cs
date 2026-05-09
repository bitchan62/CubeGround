using UnityEngine;

/// <summary>
/// 각 씬마다 배치하는 배경음악 재생기
/// SoundManager의 볼륨 설정을 자동으로 적용
/// 씬이 파괴되면 자동으로 음악도 정지됨
/// </summary>
public class SceneMusicPlayer : MonoBehaviour
{
    [Header("씬 배경음악 설정")]
    [Tooltip("이 씬에서 재생할 배경음악")]
    public AudioClip backgroundMusic;

    [Tooltip("배경음악 루프 여부")]
    public bool backgroundMusicLoop = true; // 개별 루프 설정

    [Tooltip("랭킹 UI 표시 시 재생할 음악")]
    public AudioClip rankingMusic;

    [Tooltip("랭킹 음악 루프 여부")]
    public bool rankingMusicLoop = false; // 개별 루프 설정

    [Tooltip("개별 음악 볼륨 (0~1) - SoundManager의 전역 볼륨과 곱해짐")]
    [Range(0f, 1f)]
    public float musicVolume = 1f;

    [Tooltip("게임 시작 시 자동 재생")]
    public bool playOnStart = true;

    private AudioSource audioSource;

    #region ===== 초기화 =====

    private void Awake()
    {
        // AudioSource 자동 생성
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        // SoundManager의 볼륨 변경 이벤트 구독
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.OnMusicVolumeChanged += UpdateVolume;
        }

        // 자동 재생
        if (playOnStart && backgroundMusic != null)
        {
            PlayMusic();
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.OnMusicVolumeChanged -= UpdateVolume;
        }
    }

    #endregion

    #region ===== 음악 재생 제어 =====

    /// <summary>
    /// 배경음악 재생
    /// </summary>
    public void PlayMusic()
    {
        if (audioSource == null || backgroundMusic == null) return;

        audioSource.clip = backgroundMusic;
        audioSource.loop = backgroundMusicLoop; // 배경음악 루프 설정
        ApplyVolume();
        audioSource.Play();

        Debug.Log($"[SceneMusicPlayer] {gameObject.scene.name} 씬 BGM 재생 시작: {backgroundMusic.name} (Loop: {backgroundMusicLoop})");
    }

    /// <summary>
    /// 다른 배경음악으로 교체 재생
    /// </summary>
    public void ChangeMusic(AudioClip newClip, bool loop = true)
    {
        if (audioSource == null || newClip == null) return;

        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.loop = loop;
        ApplyVolume();
        audioSource.Play();

        Debug.Log($"[SceneMusicPlayer] BGM 변경: {newClip.name} (Loop: {loop})");
    }

    /// <summary>
    /// 랭킹 음악으로 교체 재생 (씬 내 전용 기능)
    /// </summary>
    public void PlayRankingMusic()
    {
        if (rankingMusic == null)
        {
            Debug.LogWarning("[SceneMusicPlayer] 랭킹 음악이 설정되어 있지 않습니다.");
            return;
        }

        audioSource.Stop();
        audioSource.clip = rankingMusic;
        audioSource.loop = rankingMusicLoop; // 랭킹 음악 루프 설정
        ApplyVolume();
        audioSource.Play();

        Debug.Log($"[SceneMusicPlayer] 랭킹 BGM 재생: {rankingMusic.name} (Loop: {rankingMusicLoop})");
    }

    /// <summary>
    /// 배경음악 정지
    /// </summary>
    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log($"[SceneMusicPlayer] {gameObject.scene.name} 씬 BGM 정지");
        }
    }

    /// <summary>
    /// 배경음악 일시정지
    /// </summary>
    public void PauseMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    /// <summary>
    /// 배경음악 재개
    /// </summary>
    public void ResumeMusic()
    {
        if (audioSource != null)
        {
            audioSource.UnPause();
        }
    }

    #endregion

    #region ===== 볼륨 관리 =====

    private void ApplyVolume()
    {
        float globalVolume = SoundManager.Instance != null
            ? SoundManager.Instance.GetMusicVolume()
            : 1f;
        audioSource.volume = musicVolume * globalVolume;
    }

    /// <summary>
    /// SoundManager의 전역 볼륨이 변경될 때 자동으로 호출됨
    /// </summary>
    private void UpdateVolume(float globalVolume)
    {
        if (audioSource != null)
        {
            audioSource.volume = musicVolume * globalVolume;
        }
    }

    /// <summary>
    /// 이 씬만의 개별 볼륨 설정
    /// </summary>
    public void SetLocalVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplyVolume();
    }

    #endregion

    #region ===== 유틸리티 =====

    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }

    public float GetCurrentTime()
    {
        return audioSource != null ? audioSource.time : 0f;
    }

    public void SetCurrentTime(float time)
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.time = Mathf.Clamp(time, 0f, audioSource.clip.length);
        }
    }

    #endregion

    #region ===== 디버그 =====

    [ContextMenu("테스트 - 음악 재생")]
    private void TestPlayMusic()
    {
        PlayMusic();
    }

    [ContextMenu("테스트 - 랭킹 음악 재생")]
    private void TestPlayRankingMusic()
    {
        PlayRankingMusic();
    }

    [ContextMenu("테스트 - 음악 정지")]
    private void TestStopMusic()
    {
        StopMusic();
    }

    #endregion
}