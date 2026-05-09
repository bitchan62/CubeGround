using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 구조체 기반 개선된 SoundManager
/// 각 사운드마다 개별 볼륨 조절 가능
/// 과음 재생 + 볼륨 관리 담당
/// 배경음악은 SceneMusicPlayer가 담당
/// 
/// [추가] 동일 사운드 동시 재생 개수 제한 기능
/// </summary>
public class SoundManager : SingletonT<SoundManager>
{
    #region ===== 사운드 클립 구조체 =====

    [System.Serializable]
    public class SoundClip
    {
        [Tooltip("사운드 파일")]
        public AudioClip clip;

        [Tooltip("개별 볼륨 (0~1)")]
        [Range(0f, 1f)]
        public float volume = 1f;

        [Tooltip("동시 재생 최대 개수 (0 = 무제한)")]
        public int maxSimultaneous = 0;

        // 생성자
        public SoundClip()
        {
            clip = null;
            volume = 1f;
            maxSimultaneous = 0;
        }

        public SoundClip(AudioClip audioClip, float vol = 1f, int maxCount = 0)
        {
            clip = audioClip;
            volume = vol;
            maxSimultaneous = maxCount;
        }
    }

    #endregion

    #region ===== 플레이어 기본 액션 사운드 =====

    [Header("=== 플레이어 기본 액션 사운드 ===")]
    [Tooltip("플레이어 이동 소리")]
    public SoundClip playerMove;

    [Tooltip("플레이어 점프 소리")]
    public SoundClip playerJump;

    [Tooltip("플레이어 착지 소리")]
    public SoundClip playerLand;

    [Tooltip("플레이어 피격 소리")]
    public SoundClip playerHit;

    [Tooltip("플레이어 사망 소리")]
    public SoundClip playerDeath;

    #endregion

    #region ===== 플레이어 공격 사운드 =====

    [Header("=== 플레이어 공격 사운드 ===")]
    [Tooltip("기본 공격 소리")]
    public SoundClip playerBasicAttack;

    [Tooltip("낙하 공격 시전 소리")]
    public SoundClip playerDropAttack;

    [Tooltip("낙하 공격 착지 임팩트 소리")]
    public SoundClip playerDropImpact;

    [Tooltip("닷지 공격 소리")]
    public SoundClip playerDodgeAttack;

    #endregion

    #region ===== 몬스터 공통 사운드 =====

    [Header("=== 몬스터 공통 사운드 ===")]
    [Tooltip("몬스터 스폰 소리")]
    public SoundClip monsterSpawn;

    [Tooltip("몬스터 피격 소리")]
    public SoundClip monsterHit;

    #endregion

    #region ===== 몬스터별 공격 사운드 =====

    [Header("=== 몬스터별 공격 사운드 ===")]
    [Tooltip("미니언 일반 공격")]
    public SoundClip minionAttack;

    [Tooltip("궁수 화살 발사")]
    public SoundClip archerFire;

    [Tooltip("마법사 주문 시전")]
    public SoundClip mageSpell;

    [Tooltip("방패병 돌진 공격")]
    public SoundClip shielderCharge;

    #endregion

    #region ===== 큐브 시스템 사운드 =====

    [Header("=== 큐브 시스템 사운드 ===")]

    [Tooltip("큐브 이동 시작 소리")]
    public SoundClip cubeMoveStart;

    [Tooltip("큐브 이동 도착 소리")]
    public SoundClip cubeMoveEnd;

    [Tooltip("큐브 흔들림 소리")]
    public SoundClip cubeCollapseShake;

    #endregion

    #region ===== 멧돼지 시스템 사운드 =====

    [Header("=== 멧돼지 시스템 사운드 ===")]
    [Tooltip("멧돼지 경고 소리")]
    public SoundClip boarWarning;

    [Tooltip("멧돼지 돌진 소리")]
    public SoundClip boarCharge;

    #endregion

    #region ===== 아이템 사운드 =====

    [Header("=== 아이템 사운드 ===")]
    [Tooltip("힐 아이템 픽업 소리")]
    public SoundClip itemHealPickup;

    [Tooltip("코인 픽업 소리")]
    public SoundClip itemCoinPickup;

    #endregion

    #region ===== UI 및 시스템 사운드 =====

    [Header("=== UI 및 시스템 사운드 ===")]
    [Tooltip("UI 클릭 소리")]
    public SoundClip uiClick;

    [Tooltip("UI 대화 출력 소리")]
    public SoundClip uiDialog;

    [Tooltip("사운드 조절 소리")]
    public SoundClip volumeSlider;

    #endregion


    #region ===== 내부 변수 =====

    // PlayerPrefs 키 상수
    private const string MusicVolumeKey = "MusicVolume";
    private const string EffectVolumeKey = "EffectVolume";

    // 현재 볼륨 값들
    private float currentMusicVolume = 1.0f;
    public float currentEffectVolume { private set; get; } = 1.0f;

    // 자동 생성되는 AudioSource들
    private AudioSource effectSource;       // 모든 효과음용
    private AudioSource loopSource;         // 루프 사운드용 (큐브 이동, 흔들림 등)

    // 중복 초기화 방지용 변수
    private bool hasInitialized = false;

    // 동시 재생 추적용 딕셔너리 (AudioClip -> 현재 재생 중인 개수)
    private Dictionary<AudioClip, int> playingCounts = new Dictionary<AudioClip, int>();

    #endregion

    #region ===== 볼륨 변경 이벤트 =====

    /// <summary>
    /// 음악 볼륨이 변경될 때 발생하는 이벤트
    /// SceneMusicPlayer가 구독하여 실시간 볼륨 변경 적용
    /// </summary>
    public System.Action<float> OnMusicVolumeChanged;

    /// <summary>
    /// 효과음 볼륨이 변경될 때 발생하는 이벤트
    /// </summary>
    public System.Action<float> OnEffectVolumeChanged;

    #endregion

    #region ===== 초기화 =====

    protected override void Awake()
    {
        // 싱글톤 설정 (기존 코드 유지)
        base.Awake();

        // AudioSource 자동 생성
        CreateAudioSources();

        // 모든 효과음 정지
        StopAllEffectSounds();
    }

    private void Start()
    {
        // 이미 초기화되었다면 스킵
        if (hasInitialized)
            return;

        // 저장된 볼륨 값 불러오기
        float savedMusicVolume = LoadVolume(MusicVolumeKey, 1.0f);
        float savedEffectVolume = LoadVolume(EffectVolumeKey, 1.0f);

        SetMusicVolume(savedMusicVolume);
        SetEffectVolume(savedEffectVolume);

        hasInitialized = true;

        Debug.Log("[SoundManager] 초기화 완료 - 효과음 및 볼륨 관리 준비됨");
    }

    /// <summary>
    /// AudioSource 자동 생성
    /// </summary>
    private void CreateAudioSources()
    {
        // 효과음용 AudioSource  
        effectSource = gameObject.AddComponent<AudioSource>();
        effectSource.loop = false;
        effectSource.playOnAwake = false;

        // 루프 사운드용 AudioSource
        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.loop = true;
        loopSource.playOnAwake = false;

        Debug.Log("[SoundManager] AudioSource 자동 생성 완료 (총 2개)");
    }

    /// <summary>
    /// 모든 효과음 정지
    /// </summary>
    private void StopAllEffectSounds()
    {
        if (effectSource != null && effectSource.isPlaying)
            effectSource.Stop();

        if (loopSource != null && loopSource.isPlaying)
            loopSource.Stop();
    }

    #endregion

    #region ===== 볼륨 관리 =====

    /// <summary>
    /// 음악 볼륨 설정 (SceneMusicPlayer용)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        currentMusicVolume = volume;
        SaveVolume(MusicVolumeKey, volume);

        // 볼륨 변경 이벤트 발생 (SceneMusicPlayer들이 받음)
        OnMusicVolumeChanged?.Invoke(volume);
    }

    /// <summary>
    /// 효과음 볼륨 설정
    /// </summary>
    public void SetEffectVolume(float volume)
    {
        currentEffectVolume = volume;

        if (effectSource != null)
        {
            effectSource.volume = volume;
        }

        if (loopSource != null)
        {
            loopSource.volume = volume;
        }

        SaveVolume(EffectVolumeKey, volume);

        // 볼륨 변경 이벤트 발생
        OnEffectVolumeChanged?.Invoke(volume);
    }

    /// <summary>
    /// 볼륨 저장
    /// </summary>
    public void SaveVolume(string key, float volume)
    {
        PlayerPrefs.SetFloat(key, volume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 볼륨 불러오기
    /// </summary>
    public float LoadVolume(string key, float defaultValue)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    /// <summary>
    /// 현재 음악 볼륨 반환 (SceneMusicPlayer용)
    /// </summary>
    public float GetMusicVolume()
    {
        return currentMusicVolume;
    }

    /// <summary>
    /// 현재 효과음 볼륨 반환
    /// </summary>
    public float GetEffectVolume()
    {
        return currentEffectVolume;
    }

    #endregion

    #region ===== 플레이어 기본 액션 사운드 메서드들 =====

    public void PlayPlayerMove()
    {
        PlaySound(playerMove);
    }

    public void PlayPlayerJump()
    {
        PlaySound(playerJump);
    }

    public void PlayPlayerLand()
    {
        PlaySound(playerLand);
    }

    public void PlayPlayerHit()
    {
        PlaySound(playerHit);
    }

    public void PlayPlayerDeath()
    {
        PlaySound(playerDeath);
    }

    #endregion

    #region ===== 플레이어 공격 사운드 메서드들 =====

    public void PlayPlayerBasicAttack()
    {
        PlaySound(playerBasicAttack);
    }

    public void PlayPlayerDodgeAttack()
    {
        PlaySound(playerDodgeAttack);
    }

    public void PlayPlayerDropAttack()
    {
        PlaySound(playerDropAttack);
    }

    public void PlayPlayerDropImpact()
    {
        PlaySound(playerDropImpact);
    }

    /// <summary>
    /// AttackName에 따른 플레이어 공격 사운드 재생
    /// </summary>
    public void PlayPlayerAttackByType(AttackName attackType)
    {
        switch (attackType)
        {
            case AttackName.Player_BasicAttack:
                PlayPlayerBasicAttack();
                break;
            case AttackName.Player_JumpComboAttack:
                PlayPlayerDropAttack();
                break;
            case AttackName.Player_WhenDodge:
                PlayPlayerDodgeAttack();
                break;
        }
    }

    #endregion

    #region ===== 몬스터 사운드 메서드들 =====

    // 공통 몬스터 사운드
    public void PlayMonsterSpawn()
    {
        PlaySound(monsterSpawn);
    }

    public void PlayMonsterHit()
    {
        PlaySound(monsterHit);
    }

    // 몬스터별 공격 사운드
    public void PlayArcherFire()
    {
        PlaySound(archerFire);
    }

    public void PlayMageSpell()
    {
        PlaySound(mageSpell);
    }

    public void PlayMinionAttack()
    {
        PlaySound(minionAttack);
    }

    public void PlayShielderCharge()
    {
        PlaySound(shielderCharge);
    }

    /// <summary>
    /// AttackName에 따른 몬스터 공격 사운드 재생
    /// </summary>
    public void PlayMonsterAttackByType(AttackName attackType)
    {
        switch (attackType)
        {
            case AttackName.Monster_ArcherFireAttack:
                PlayArcherFire();
                break;
            case AttackName.Monster_MageSpellAttack:
                PlayMageSpell();
                break;
            case AttackName.Monster_MinionNormalAttack:
                PlayMinionAttack();
                break;
            case AttackName.Monster_ShieldChargeAttack:
                PlayShielderCharge();
                break;
        }
    }

    #endregion

    #region ===== 큐브 시스템 사운드 메서드들 =====

    public void PlayCubeMoveStart()
    {
        PlaySound(cubeMoveStart);
    }

    public void PlayCubeMoveEnd()
    {
        //Debug.Log("PlayCubeMoveEnd");
        PlaySound(cubeMoveEnd);
    }

    public void PlayCubeCollapseShake()
    {
        PlaySound(cubeCollapseShake);
    }

    #endregion

    #region ===== 멧돼지 시스템 사운드 메서드들 =====

    public void PlayBoarWarning()
    {
        PlaySound(boarWarning);
    }

    public void PlayBoarCharge()
    {
        PlaySound(boarCharge);
    }

    #endregion

    #region ===== 아이템 사운드 메서드들 =====

    public void PlayItemHealPickup()
    {
        PlaySound(itemHealPickup);
    }

    public void PlayItemCoinPickup()
    {
        PlaySound(itemCoinPickup);
    }

    #endregion

    #region ===== UI 사운드 메서드들 =====

    public void PlayUIClick()
    {
        PlaySound(uiClick);
    }

    public void PlayUIDialog()
    {
        PlaySound(uiDialog);
    }

    public void PlayVolumeSlider()
    {
        PlaySound(volumeSlider);
    }

    #endregion

    #region ===== 헬퍼 메서드들 =====

    /// <summary>
    /// 일반 사운드 재생 (SoundClip 사용) - 최대 개수 제한 포함
    /// </summary>
    private void PlaySound(SoundClip soundClip)
    {
        if (effectSource != null && soundClip != null && soundClip.clip != null)
        {
            // 최대 동시 재생 개수 체크
            if (soundClip.maxSimultaneous > 0)
            {
                int currentCount = GetPlayingCount(soundClip.clip);

                if (currentCount >= soundClip.maxSimultaneous)
                {
                    // 이미 최대 개수만큼 재생 중이면 무시
                    return;
                }
            }

            float finalVolume = soundClip.volume * currentEffectVolume;
            float clipLength = soundClip.clip.length;

            // 재생 카운트 증가
            IncrementPlayingCount(soundClip.clip);

            // 사운드 재생
            effectSource.PlayOneShot(soundClip.clip, finalVolume);

            // 사운드 길이만큼 후에 카운트 감소
            StartCoroutine(DecrementPlayingCountAfterDelay(soundClip.clip, clipLength));
        }
    }

    /// <summary>
    /// 현재 재생 중인 사운드 개수 반환
    /// </summary>
    private int GetPlayingCount(AudioClip clip)
    {
        if (playingCounts.ContainsKey(clip))
        {
            return playingCounts[clip];
        }
        return 0;
    }

    /// <summary>
    /// 재생 카운트 증가
    /// </summary>
    private void IncrementPlayingCount(AudioClip clip)
    {
        if (playingCounts.ContainsKey(clip))
        {
            playingCounts[clip]++;
        }
        else
        {
            playingCounts[clip] = 1;
        }
    }

    /// <summary>
    /// 일정 시간 후 재생 카운트 감소
    /// </summary>
    private IEnumerator DecrementPlayingCountAfterDelay(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (playingCounts.ContainsKey(clip))
        {
            playingCounts[clip]--;
            if (playingCounts[clip] <= 0)
            {
                playingCounts.Remove(clip);
            }
        }
    }

    /// <summary>
    /// 루프 사운드 재생 (SoundClip 사용)
    /// </summary>
    private void PlayLoopSound(SoundClip soundClip)
    {
        if (loopSource != null && soundClip != null && soundClip.clip != null)
        {
            if (loopSource.isPlaying)
                loopSource.Stop();

            loopSource.clip = soundClip.clip;
            loopSource.volume = soundClip.volume * currentEffectVolume;
            loopSource.Play();
        }
    }

    /// <summary>
    /// 루프 사운드 정지
    /// </summary>
    private void StopLoopSound()
    {
        if (loopSource != null && loopSource.isPlaying)
        {
            loopSource.Stop();
        }
    }

    #endregion

    #region ===== 공개 유틸리티 메서드들 =====

    /// <summary>
    /// 특정 AudioSource가 재생 중인지 확인
    /// </summary>
    public bool IsEffectPlaying()
    {
        return effectSource != null && effectSource.isPlaying;
    }

    public bool IsLoopPlaying()
    {
        return loopSource != null && loopSource.isPlaying;
    }

    /// <summary>
    /// 모든 사운드 일시 정지
    /// </summary>
    public void PauseAllSounds()
    {
        AudioListener.pause = true;
    }

    /// <summary>
    /// 모든 사운드 재개
    /// </summary>
    public void ResumeAllSounds()
    {
        AudioListener.pause = false;
    }

    /// <summary>
    /// 재시작을 위한 완전 리셋 (PauseMenu에서 호출)
    /// </summary>
    public void ForceReset()
    {
        // 모든 AudioSource 완전 정지
        if (effectSource != null)
        {
            effectSource.Stop();
            effectSource.clip = null;
        }

        if (loopSource != null)
        {
            loopSource.Stop();
            loopSource.clip = null;
        }

        // 재생 카운트 초기화
        playingCounts.Clear();

        // 초기화 플래그 리셋
        hasInitialized = false;

        // AudioListener 정상화
        AudioListener.pause = false;

        Debug.Log("[SoundManager] 완전 리셋 완료");
    }
    #endregion
}