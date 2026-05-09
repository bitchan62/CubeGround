using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 슬라이더에 붙여서 자동으로 볼륨 조절하는 컴포넌트
/// SceneMoveButton처럼 독립적으로 작동
/// </summary>
public class VolumeSlider : MonoBehaviour
{
    public enum VolumeType
    {
        Music,
        Effect
    }

    [Header("볼륨 타입")]
    [Tooltip("음악 볼륨인지 효과음 볼륨인지 선택")]
    public VolumeType volumeType = VolumeType.Music;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        if (slider == null)
        {
            Debug.LogError($"[VolumeSlider] {gameObject.name}에 Slider 컴포넌트가 없습니다!");
            return;
        }

        // 기존 이벤트 제거 후 새로 연결
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(OnValueChanged);

        Debug.Log($"[VolumeSlider] {gameObject.name} 초기화 완료 - Type: {volumeType}");
    }

    private void Start()
    {
        // SoundManager가 준비될 때까지 대기 후 초기값 설정
        if (SoundManager.Instance != null)
        {
            UpdateSliderValue();
        }
        else
        {
            // SoundManager가 없으면 0.1초 후 재시도
            if (Timer.Instance != null)
            {
                Timer.Instance.StartTimer(this, 0.1f, UpdateSliderValue);
            }
        }
    }

    private void UpdateSliderValue()
    {
        if (SoundManager.Instance == null || slider == null) return;

        // 이벤트 발생 방지를 위해 임시로 제거
        slider.onValueChanged.RemoveAllListeners();

        // 현재 볼륨 값으로 슬라이더 설정
        if (volumeType == VolumeType.Music)
        {
            slider.value = SoundManager.Instance.GetMusicVolume();
            Debug.Log($"[VolumeSlider] 음악 볼륨 초기화: {slider.value}");
        }
        else
        {
            slider.value = SoundManager.Instance.GetEffectVolume();
            Debug.Log($"[VolumeSlider] 효과음 볼륨 초기화: {slider.value}");
        }

        // 이벤트 다시 연결
        slider.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(float value)
    {
        if (SoundManager.Instance == null) return;

        if (volumeType == VolumeType.Music)
        {
            SoundManager.Instance.SetMusicVolume(value);
            Debug.Log($"[VolumeSlider] 음악 볼륨 변경: {value}");
        }
        else
        {
            SoundManager.Instance.SetEffectVolume(value);
            Debug.Log($"[VolumeSlider] 효과음 볼륨 변경: {value}");
        }
    }

    private void OnEnable()
    {
        // 패널이 활성화될 때마다 값 갱신
        if (SoundManager.Instance != null && slider != null)
        {
            UpdateSliderValue();
        }
    }
}