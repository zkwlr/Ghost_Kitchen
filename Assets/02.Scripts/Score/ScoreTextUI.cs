using UnityEngine;
using TMPro;

public class ScoreTextUI : MonoBehaviour
{
    [Header("점수 표시용 TextMeshProUGUI")]
    public TextMeshProUGUI scoreText; // Hierarchy의 ScoreText와 연결

    private void Start()
    {
        if (ScoreManager.Instance == null)
        {
            Debug.LogError("ScoreManager를 찾을 수 없습니다!");
            return;
        }

        // 1) 초기 점수 표시
        scoreText.text = $"Score: {ScoreManager.Instance.GetScore()}";

        // 2) ScoreManager.OnScoreChanged 이벤트에 구독
        ScoreManager.Instance.OnScoreChanged.AddListener(OnScoreChanged);
    }

    private void OnDestroy()
    {
        // 씬이 사라지거나 오브젝트가 삭제될 때 이벤트 구독 해제
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged.RemoveListener(OnScoreChanged);
    }

    // ScoreManager에서 점수가 바뀔 때마다 호출됨
    private void OnScoreChanged(int newScore)
    {
        scoreText.text = $"Score: {newScore}";
    }
}