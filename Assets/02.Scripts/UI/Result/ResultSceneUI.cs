using UnityEngine;
using UnityEngine.UI;

public class ResultSceneUI : MonoBehaviour
{
    [Header("Stage Clear / Game Over 텍스트")]
    public Text headerText;

    [Header("최종 점수 텍스트")]
    public Text scoreText;

    private void Start()
    {
        // 1) 종료 이유에 따라 헤더 변경
        if (GameResultData.Reason == GameEndReason.StageClear)
        {
            headerText.text = "Stage Clear!";
            headerText.color = Color.blue; // 스테이지 클리어 시 파란색
        }
        else
        {
            headerText.text = "Game Over";
            headerText.color = Color.red; // 게임 오버 시 빨간색
        }

        // 2) 항상 점수는 보여줌
        scoreText.text = $"Score: {GameResultData.LastScore}";
    }
}