using UnityEngine;
using UnityEngine.Serialization;


// 이펙트 데이터
[System.Serializable]
public class EffectData : IData
{
    [Tooltip("발생할 시각적 이펙트 프리팹")]
    public GameObject effectPrefab = null;
    [Tooltip("발생할 시각적 이펙트 프리팹의 위치")]
    public Transform effectPos = null;

    [Tooltip("발생할 사운드 클립")]
    public AudioClip audioClip = null;
    [Tooltip("사운드 클립 볼륨")]
    public float audioVolume = 1f;

    // 사운드 재생용 AudioSource
    private AudioSource audioSource = null;

    /// <summary>
    /// pos로 미리 지정한 위치에서 이펙트 발생
    /// </summary>
    public GameObject Instantiate(GameObject owner) // <- owner 필요없을듯
    {
        if (owner == null) { Debug.Log("owner 없음"); return null; }

        PlayEffectSound(owner);

        if (effectPrefab == null) { return null; }
        GameObject effect;
        if (effectPos == null) { effect = GameObject.Instantiate(effectPrefab, owner.transform.position, owner.transform.rotation); }
        else                   { effect = GameObject.Instantiate(effectPrefab, effectPos); }

        // <- ToDo
        // * EffectData::Instantiate에서 Effect 재활성 시 Rotation 문제 해결
            
        //GameObject effect = PoolManager.GetObject(effectPrefab, effectPos);

        //if(effectPos != null)
        //{
        //    effect.transform.SetParent(effectPos);
        //    effect.transform.position = effectPos.transform.position;
        //    
        //}
        //effect.transform.position = effectPos.position;

        //if (owner != null)
        //{ effect.transform.SetParent(owner.transform); }
        return effect;
    }

    /// <summary>
    /// 매개변수로 지정한 위치에서 이펙트 발생
    /// </summary>
    public GameObject Instantiate(GameObject owner, Vector3 effectPos, Quaternion effectRot)
    {
        PlayEffectSound(owner);

        if (effectPrefab == null) { return null; }
        return GameObject.Instantiate(effectPrefab, effectPos, effectRot);
        //return PoolManager.GetObject(effectPrefab, effectPos, effectRot);
    }

    // 효과음 사운드 재생
    private void PlayEffectSound(GameObject owner)
    {
        if (audioClip == null) { return; }
        else if (audioSource == null)
        {
            audioSource = owner.AddComponent<AudioSource>();
            audioSource.loop = false;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D 사운드
            audioSource.minDistance = 10f;
            audioSource.maxDistance = 40f;
        }

        // SoundManager의 볼륨을 반영한 최종 볼륨
        float finalVolume = audioVolume * SoundManager.Instance.currentEffectVolume;

        // 사운드 재생
        audioSource.PlayOneShot(audioClip, finalVolume);
    }

}
