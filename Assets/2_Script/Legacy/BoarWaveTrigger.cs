using UnityEngine;

/// <summary>
/// 보스가 에리어에 진입했을 때 BoarWaveController를 트리거하는 컴포넌트
/// 트리거 영역 GameObject에 부착하여 사용
/// </summary>
[RequireComponent(typeof(Collider))]
public class BoarWaveTrigger : MonoBehaviour
{
    [Header("감지 설정")]
    [Tooltip("감지할 보스 태그")]
    public string bossTag = "Monster";

    // 연결된 웨이브 컨트롤러
    private BoarWaveController waveController;

    private void Start()
    {
        // 콜라이더가 트리거인지 확인
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
        }
    }

    /// <summary>
    /// 웨이브 컨트롤러 설정 (BoarWaveController에서 호출)
    /// </summary>
    public void SetWaveController(BoarWaveController controller)
    {
        waveController = controller;
    }

    /// <summary>
    /// 보스가 트리거 영역에 진입했을 때 호출
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 보스 태그 확인
        if (!other.CompareTag(bossTag))
        {
            return;
        }
        if (waveController == null)
        {
            Debug.LogError("웨이브 컨트롤러가 설정되지 않았다");
            return;
        }

        // 웨이브 컨트롤러에 보스 진입 알림
        waveController.OnBossEntered();
    }
}