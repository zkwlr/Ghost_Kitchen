using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GuideText : MonoBehaviour
{
    [Header("안내 메시지들")]
    public string insufficientIngredientsMessage = "재료가 부족합니다! 재료 3개를 모두 넣어주세요!";
    public string hatedIngredientMessage = "악마 유령은 그 재료를 싫어해요!";
    public string NeededIngredientMessage = "마법사 유령이 좋아하는 재료를 주세요!";

    [Header("표시 설정")]
    public float displayDuration = 3f;     // 표시 시간
    public bool useAnimation = true;
    public float fadeSpeed = 2f;

    // 싱글톤 패턴
    public static GuideText Instance { get; private set; }

    private Text textComponent;
    private Coroutine currentMessageCoroutine;

    void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        textComponent = GetComponent<Text>();
        
        // 초기에는 텍스트 숨기기
        if (textComponent != null)
        {
            textComponent.text = "";
            textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, 0f);
        }
    }

    /// <summary>
    /// 재료 부족 안내문 표시
    /// </summary>
    public void ShowInsufficientIngredientsMessage()
    {
        ShowMessage(insufficientIngredientsMessage);
    }

    /// <summary>
    /// 싫어하는 재료 안내문 표시
    /// </summary>
    public void ShowHatedIngredientMessage()
    {
        ShowMessage(hatedIngredientMessage);
    }

    /// <summary>
    /// 필요한 재료 안내문 표시
    /// </summary>
    public void ShowNeededIngredientMessage()
    {
        ShowMessage(NeededIngredientMessage);
    }

    /// <summary>
    /// 커스텀 메시지 표시
    /// </summary>
    public void ShowCustomMessage(string message)
    {
        ShowMessage(message);
    }

    /// <summary>
    /// 메시지 표시 메인 메서드
    /// </summary>
    private void ShowMessage(string message)
    {
        // 이전 메시지가 표시 중이면 중단
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
        }

        // 새 메시지 표시
        currentMessageCoroutine = StartCoroutine(DisplayMessageCoroutine(message));
    }

    /// <summary>
    /// 메시지 표시 코루틴
    /// </summary>
    private IEnumerator DisplayMessageCoroutine(string message)
    {
        if (textComponent == null) yield break;

        // 메시지 설정
        textComponent.text = message;

        if (useAnimation)
        {
            // 페이드 인
            yield return StartCoroutine(FadeIn());
            
            // 표시 시간 대기
            yield return new WaitForSeconds(displayDuration);
            
            // 페이드 아웃
            yield return StartCoroutine(FadeOut());
        }
        else
        {
            // 애니메이션 없이 표시
            textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, 1f);
            yield return new WaitForSeconds(displayDuration);
            textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, 0f);
        }

        // 텍스트 지우기
        textComponent.text = "";
        currentMessageCoroutine = null;
    }

    /// <summary>
    /// 페이드 인 애니메이션
    /// </summary>
    private IEnumerator FadeIn()
    {
        Color originalColor = textComponent.color;
        float elapsedTime = 0f;
        float duration = 1f / fadeSpeed;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            textComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        
        textComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
    }

    /// <summary>
    /// 페이드 아웃 애니메이션
    /// </summary>
    private IEnumerator FadeOut()
    {
        Color originalColor = textComponent.color;
        float elapsedTime = 0f;
        float duration = 1f / fadeSpeed;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            textComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
        
        textComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    }
}
