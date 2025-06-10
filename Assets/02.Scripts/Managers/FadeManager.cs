using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static FadeManager Instance { get; private set; }

    [Header("페이드용 Image (검정색)")]
    [Tooltip("Screen Space - Overlay Canvas 아래에 배치한 전체 화면 Image를 할당하세요.")]
    public Image fadeImage;

    [Header("페이드 속도 (1 초에 얼마나 페이드될지)")]
    public float fadeDuration = 1.0f;

    private void Awake()
    {
        // 싱글톤 세팅
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 시작 시 화면을 완전히 검정(알파=1)으로 설정하고, 곧바로 페이드 인
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;
            StartCoroutine(FadeIn());
        }
    }

    /// <summary>
    /// 씬 전환 시 외부에서 호출하는 함수.
    /// 지정한 씬으로 페이드 아웃 → 씬 로드 → 페이드 인을 순차적으로 실행합니다.
    /// </summary>
    public void FadeToScene(string sceneName)
    {
        // 이미 페이드 중이라면 중첩 호출 방지
        if (fadeImage == null) return;
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    /// <summary>
    /// 페이드 아웃 → 씬 로딩 → 페이드 인 코루틴
    /// </summary>
    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        // 1) 페이드 아웃 (검정 화면으로 덮기)
        yield return StartCoroutine(Fade(0f, 1f));

        // 2) 씬 로드
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        // 로드 완료까지 대기
        while (!op.isDone)
            yield return null;

        // 3) 로드된 씬에서도 fadeImage가 유지되려면,
        //    새 씬에서 Canvas & FadeManager가 활성화되어 있어야 합니다.
        //    (DontDestroyOnLoad 처리 덕분에 FadeManager 오브젝트는 살아남습니다.)

        // 4) 씬 로딩 직후 페이드 인
        yield return StartCoroutine(Fade(1f, 0f));
    }

    /// <summary>
    /// 알파 채널을 startAlpha → endAlpha로 fadeDuration 시간 동안 보간합니다.
    /// </summary>
    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        Color c = fadeImage.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            c.a = Mathf.Lerp(startAlpha, endAlpha, t);
            fadeImage.color = c;
            yield return null;
        }
        // 최종 값 보정
        c.a = endAlpha;
        fadeImage.color = c;
    }

    /// <summary>
    /// 씬 시작 시, 초기에 검정 화면에서 서서히 투명해지는 페이드 인을 수행합니다.
    /// </summary>
    private IEnumerator FadeIn()
    {
        yield return StartCoroutine(Fade(1f, 0f));
        // 페이드 인이 끝나면 Image 오브젝트를 비활성화해 두면 성능이 약간 더 이득
        // fadeImage.gameObject.SetActive(false);
    }
}
