using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("대상 Health 컴포넌트")]
    public Health target;

    [Header("UI 요소")]
    public Slider healthSlider;
    public Text healthText; // 체력 수치 표시용 텍스트

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("HealthUI: target이 할당되지 않았습니다.");
            return;
        }

        // 초기 값 설정
        healthSlider.maxValue = target.maxHealth;
        healthSlider.value = target.CurrentHealth;
        healthText.text = $"{target.CurrentHealth}/{target.maxHealth}";

        // 이벤트 등록
        target.OnHealthChanged += UpdateUI;
    }

    void UpdateUI(int current, int max)
    {
        healthSlider.value = current;
        if (healthText != null)
            healthText.text = $"{current}/{max}";
    }

    void OnDestroy()
    {
        // 이벤트 해제
        if (target != null)
            target.OnHealthChanged -= UpdateUI;
    }
}