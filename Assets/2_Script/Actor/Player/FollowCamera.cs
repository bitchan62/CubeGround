using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
//using UnityEditor.VersionControl;
using UnityEngine;
//using static UnityEditor.Experimental.GraphView.GraphView;
//using static UnityEngine.UI.Image;


/// <summary>
/// offset만큼 떨어진 위치에서 계속 target을 따라다니는 카메라
/// </summary>
public class FollowCamera : MonoBehaviour
{
    [SerializeField] protected Transform target; // 따라갈 타겟(플레이어)
    [SerializeField] protected Vector3 offset;
    // 오프셋 비율 (줌인/줌아웃)
    protected float offsetRate = 1f;

    [SerializeField] protected float cameraSpeed = 20f;

    [Header("화면 흔들림 관련 정보")]
    public float shakeRange = 1f;
    public float shakeTime = 0.2f;
    [Tooltip("흔들림 방향 (0 초과 = 해당 방향으로 흔들림 있음)")]
    public Vector3 shakeVec;
    // 흔들리고 있는 상태
    protected Vector3 shakingVec;

    // 투명화 처리
    [SerializeField] protected Material faderMaterial; // 투명화에 사용할 머티리얼
    //[SerializeField] protected Material bossArmFaderMaterial;
    protected Vector3 targetDirection;
    protected float targetDistance;
    protected int targetLayer;

    // 이전에 투명 처리했던 오브젝트들을 추적하기 위한 리스트
    protected HashSet<CubeFader> fadedObjects = new HashSet<CubeFader>();


    private void Start()
    {
        if (target == null) { target = GameObject.FindGameObjectWithTag("Player").transform; }
        if (target == null) { Debug.LogError("FollowCamera: 타겟(플레이어)을 찾을 수 없습니다."); }

        // <- AudioListener는 원래 플레이어에 붙어있어야 함
        AudioListener listener = GetComponent<AudioListener>();
        Destroy(listener);

        listener = target.GetComponent<AudioListener>();
        if (listener == null) { target.AddComponent<AudioListener>(); }

        targetLayer = LayerMask.GetMask("Cube");//  LayerMask.GetMask("Cube", "CanNotThrough");
        Timer.Instance.StartEndlessTimer(this, "ObjectFader", 0.2f, ObjectFader);
    }


    void LateUpdate()
    {
        if (target != null)
        {
            // 위치 이동
            this.transform.position = Vector3.MoveTowards(transform.position,
                target.position + offset * offsetRate + shakingVec,
                cameraSpeed * Time.deltaTime);


            Vector3 toTarget = target.transform.position - this.transform.position;
            targetDistance = toTarget.magnitude;
            targetDirection = toTarget.normalized;

            //transform.position = target.position + offset + shakingVec;
            shakingVec = Vector3.zero;
            // 타겟을 항상 바라보게
            transform.LookAt(target.position);
        }
    }

    // ==== FollowCamera ==== //
    public void FocusChange(Transform target)
    { this.target = target; }

    public void PosReset(float rate)
    { transform.position = target.position + offset * rate; }

    public void SpeedChage(float speed)
    { cameraSpeed = speed; }

    public void OffsetRateChage(float rate)
    { offsetRate = rate; }


    // ==== ShakeCamera ==== //

    /// <summary>
    /// 음수 X, 흔드는 정도
    /// </summary>
    /// <param name="shakeRange"></param>
    public void ShakeCamera()
    {
        Timer.Instance.StartRepeatTimer(this, "shake", shakeTime, () => {
            if (0 < shakeVec.x) { shakingVec.x = Random.Range(-shakeRange, shakeRange); }
            if (0 < shakeVec.y) { shakingVec.y = Random.Range(-shakeRange, shakeRange); }
            if (0 < shakeVec.z) { shakingVec.z = Random.Range(-shakeRange, shakeRange); }
        });
    }

    public void ShakeCamera(float newShakeRange, float newShakeTime)
    {
        Timer.Instance.StartRepeatTimer(this, "shake", newShakeTime, () => {
            if (0 < shakeVec.x) { shakingVec.x = Random.Range(-newShakeRange, newShakeRange); }
            if (0 < shakeVec.y) { shakingVec.y = Random.Range(-newShakeRange, newShakeRange); }
            if (0 < shakeVec.z) { shakingVec.z = Random.Range(-newShakeRange, newShakeRange); }
        });
    }


    // ==== ObjectFader ==== //
    /// <summary>
    /// 주기적으로 호출되어 카메라와 타겟 사이의 오브젝트를 투명화하고 복원합니다.
    /// </summary>
    private void ObjectFader()
    {
        // 타겟이나 투명 머티리얼이 없으면 아무것도 하지 않습니다.
        if (faderMaterial == null) return;

        // 1. 현재 프레임에서 카메라와 타겟 사이를 가리는 모든 오브젝트를 감지합니다.
        RaycastHit[] hits = Physics.RaycastAll(target.position, -targetDirection, targetDistance, targetLayer);
#if UNITY_EDITOR
        Debug.DrawRay(target.position, -targetDirection * targetDistance, Color.yellow, 0.2f);
#endif

        // 2. 감지된 오브젝트들을 중복 없이 HashSet에 담습니다.
        HashSet<CubeFader> nowOccluding = new HashSet<CubeFader>();
        foreach (RaycastHit hit in hits)
        {
            CubeFader fader = hit.collider.GetComponent<CubeFader>();
            if (fader != null) { nowOccluding.Add(fader); }
        }

        // 3. (FadeIn 처리) 이전에 투명했지만 이제는 가리지 않는 오브젝트들을 원래대로 되돌립니다.
        // 'fadedObjects'(List)에는 있지만 'nowOccluding'(HashSet)에는 없는 오브젝트들을 찾습니다.
        List<CubeFader> objectsToFadeIn = fadedObjects.Where(fader => !nowOccluding.Contains(fader)).ToList();
        foreach (CubeFader fader in objectsToFadeIn)
        {
            fader.FadeIn();
        }
        
        // 4. (FadeOut 처리) 새로 시야를 가리기 시작한 오브젝트들을 투명하게 만듭니다.
        foreach (CubeFader fader in nowOccluding)
        {
            //if (fader.gameObject.layer == LayerMask.NameToLayer("Cube"))
            //{
                fader.FadeOut(faderMaterial);
            //}
            // else if (fader.gameObject.layer == LayerMask.NameToLayer("CanNotThrough"))
            // {
            //     var colorChage = fader.GetComponent<ColorChangeAction>();
            //     fader.whenFadeIn.Add(colorChage.RestoreOriginalColors, 1);
            //     fader.FadeOut(bossArmFaderMaterial);
            // }
        }
        
        // 5. 다음 주기를 위해 현재 상태를 저장합니다.
        // HashSet을 List로 변환하여 저장합니다.
        fadedObjects = nowOccluding;
    }

}