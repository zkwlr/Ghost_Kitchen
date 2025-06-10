using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System;

public class RecordBoardUI : MonoBehaviour
{
    [Header("기록을 표시할 UI Text")]
    public Text recordText;

    [Header("기록 저장파일명")]
    public string recordFileName = "records.json";

    private void Start()
    {
        if (recordText == null)
        {
            Debug.LogError("RecordBoardUI: recordText가 할당되지 않았습니다.");
            return;
        }

        // 기록 불러오기 (GameSessionManager가 없어도 작동)
        List<GameRecord> records = LoadAllRecords();

        var sb = new StringBuilder();

        sb.AppendLine(); // 빈 줄 추가
        sb.AppendLine(); // 빈 줄 추가

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
                $"{records[i].score} 냥   {records[i].date}"
            );
        }

        recordText.supportRichText = true;
        recordText.text = sb.ToString();
    }

    // 기록용 데이터 클래스
    [Serializable]
    public class GameRecord
    {
        public int score;
        public string date;   // 기록 시각
    }

    [Serializable]
    public class GameRecordList
    {
        public List<GameRecord> records = new List<GameRecord>();
    }

    private string RecordFilePath
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, recordFileName);
        }
    }

    /// <summary>
    /// 기록 파일에서 모든 기록 불러오기
    /// </summary>
    private List<GameRecord> LoadAllRecords()
    {
        try
        {
            if (File.Exists(RecordFilePath))
            {
                string json = File.ReadAllText(RecordFilePath);
                var recordList = JsonUtility.FromJson<GameRecordList>(json);
                if (recordList != null && recordList.records != null)
                {
                    // 점수 내림차순 정렬
                    recordList.records.Sort((a, b) => b.score.CompareTo(a.score));
                    return recordList.records;
                }
            }
            Debug.Log($"[RecordBoardUI] 기록 파일을 찾을 수 없거나 비어있습니다: {RecordFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[RecordBoardUI] 기록 불러오기 실패: {e.Message}");
        }

        return new List<GameRecord>();
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
