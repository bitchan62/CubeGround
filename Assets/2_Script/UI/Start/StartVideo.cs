using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class StartVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;   // 에디터에서 연결
    public RawImage rawImage;         // 동영상 출력용 UI
    public Canvas videoCanvas;        // 껐다켰다(동영상 캔버스)
    public Canvas startTextCanvas;

    public float time = 15f; // 동영상 시작까지 대기시간


    void Start()
    {
        if (videoPlayer == null) { gameObject.SetActive(false); return; }
        if (rawImage == null) { gameObject.SetActive(false); return; }
        if (videoCanvas == null) { gameObject.SetActive(false); return; }
        if (startTextCanvas == null) { gameObject.SetActive(false); return; }
        if (videoPlayer.clip == null) { gameObject.SetActive(false) ; return; }

        videoPlayer.prepareCompleted += VideoPrepared;
        videoPlayer.Prepare();

        videoCanvas?.gameObject.SetActive(false);
        startTextCanvas?.gameObject.SetActive(false);
        Timer.Instance?.StartTimer(this, "동영상", time,
            () =>
            {
                Timer.Instance?.StartEndlessTimer(this, "깜빡", 1f, () =>
                {
                    tempBool = !tempBool;
                    startTextCanvas?.gameObject.SetActive(tempBool);
                    videoCanvas.gameObject.SetActive(true);
                });
            });
    }

    bool tempBool = false;

    void Update()
    {
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            startTextCanvas?.gameObject.SetActive(false);
            videoCanvas?.gameObject.SetActive(false);

            Timer.Instance?.StopTimer(this, "동영상");
            Timer.Instance?.StopEndlessTimer(this, "깜빡");

            Timer.Instance?.StartTimer(this, "동영상", time, () => {
                videoCanvas?.gameObject.SetActive(true);
                Timer.Instance?.StartEndlessTimer(this, "깜빡", 1f, () => {
                    tempBool = !tempBool;
                    startTextCanvas?.gameObject.SetActive(tempBool);
                });
            });
        }
    }

    void VideoPrepared(VideoPlayer vp)
    {
        rawImage.texture = vp.texture;
        videoPlayer.Play();
    }

}
