using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class Portal : MonoBehaviour
{
    [Header("포탈 설정")]
    [Tooltip("전환할 목표 씬의 이름")]
    public string targetSceneName;

    [Tooltip("포탈을 활성화할 오브젝트의 태그")]
    public string playerTag = "Player";

    private bool hasTriggered = false;

    private void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"[Portal] {gameObject.name}에 Collider가 없다!");
            return;
        }
        if (!col.isTrigger)
        {
            Debug.LogError($"[Portal] {gameObject.name}의 Collider가 Trigger로 설정되지 않았다!");
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag) || hasTriggered)
            return;

        hasTriggered = true;
        Debug.Log($"[Portal] 플레이어가 포탈 진입 - {targetSceneName}으로 이동");

        if (Fade.Instance != null)
        {
            Fade.Instance.TransitionToScene(targetSceneName);
        }
    }
}