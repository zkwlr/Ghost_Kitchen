using UnityEngine;
using UnityEngine.UI;

public class TimerTextUI : MonoBehaviour
{
    [Header("GameSessionManager 참조 (남은 시간 가져올 대상)")]
    public GameSessionManager sessionManager;

    [Header("남은 시간을 표시할 Text 컴포넌트")]
    public Text timerText;

    private void Start()
    {
        if (sessionManager == null)
        {
            Debug.LogError("TimerTextUI: sessionManager가 할당되지 않았습니다!");
            enabled = false;
            return;
        }
        if (timerText == null)
        {
            Debug.LogError("TimerTextUI: timerText가 할당되지 않았습니다!");
            enabled = false;
            return;
        }

        // 초기 표시 설정
        timerText.text = FormatTime(sessionManager.GetRemainingTime());
    }

    private void Update()
    {
        // 매 프레임마다 남은 시간을 가져와 텍스트 갱신
        float timeLeft = sessionManager.GetRemainingTime();
        timerText.text = FormatTime(timeLeft);
    }

    // 초 단위 시간을 "MM:SS" 문자열로 만들기
    private string FormatTime(float time)
    {
        if (time < 0f) time = 0f;
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}