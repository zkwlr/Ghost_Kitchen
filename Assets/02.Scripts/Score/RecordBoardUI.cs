using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;

public class RecordBoardUI : MonoBehaviour
{
    [Header("기록을 표시할 UI Text")]
    public Text recordText;

    private void Start()
    {
        if (recordText == null)
        {
            Debug.LogError("RecordBoardUI: recordText가 할당되지 않았습니다.");
            return;
        }

        // GameSessionManager가 존재하지 않는 경우 처리
        if (GameSessionManager.Instance == null)
        {
            Debug.LogError("RecordBoardUI: GameSessionManager.Instance가 null입니다. GameSessionManager 객체가 씬에 존재하는지 확인하세요.");
            recordText.text = "기록을 불러올 수 없습니다.";
            return;
        }

        // 저장된 모든 기록 불러오기
        List<GameSessionManager.GameRecord> records
            = GameSessionManager.Instance.GetAllRecords();

        var sb = new StringBuilder();

        // sb.AppendLine("<b><color=#FFC107>ScoreBoard</color></b>");
        sb.AppendLine(); // 제목과 내용 사이 빈 줄 추가
        sb.AppendLine(); // 제목과 내용 사이 빈 줄 추가


        for (int i = 0; i < records.Count; i++)
        {
            int place = i + 1;
            string suffix = GetOrdinalSuffix(place);

            // 색상 선택
            string colorHex = place switch
            {
                1 => "#FFD700",    // Gold
                2 => "#C0C0C0",    // Silver
                3 => "#CD7F32",    // Bronze
                _ => "#FFFFFF"
            };

            sb.AppendLine(
                $"<color={colorHex}>{place}{suffix}</color>   " +
                $"{records[i].score} pts   {records[i].date}"
            );
        }

        recordText.supportRichText = true;
        recordText.text = sb.ToString();
    }

    private string GetOrdinalSuffix(int number)
    {
        int n100 = number % 100;    // ← 100은 정수 리터럴!
        if (n100 >= 11 && n100 <= 13)
            return "th";

        return (number % 10) switch // ← 10도 정수
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th",
        };
    }
}


