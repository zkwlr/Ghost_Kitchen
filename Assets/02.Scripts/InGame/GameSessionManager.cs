using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSessionManager : MonoBehaviour
{

    [Header("게임 제한 시간 (초)")]
    public float gameDuration = 120f;

    [Header("Well(목표)의 Health 컴포넌트")]
    public Health wellHealth;

    [Header("게임 결과 씬 이름")]
    public string gameResultSceneName = "GameResultScene";

    [Header("기록 저장파일명")]
    public string recordFileName = "records.json";

    [Header("최대 저장 기록 개수")]
    public int maxRecords = 20;

    float remainingTime;
    bool isGameOver = false;

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

    private void Awake()
    {
        remainingTime = gameDuration;

        if (wellHealth == null)
        {
            var wellGO = GameObject.FindGameObjectWithTag("Well");
            if (wellGO != null) wellHealth = wellGO.GetComponent<Health>();
            else Debug.LogError("[GameSessionManager] 'Well' 태그 오브젝트를 찾을 수 없습니다.");
        }
    }

    private void Start()
    {
        if (wellHealth != null)
            wellHealth.OnDie += OnWellDied;
    }

    private void Update()
    {
        if (isGameOver) return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            GameOver("TimeUp");
        }
    }

    private void OnWellDied()
    {
        if (!isGameOver)
            GameOver("WellDestroyed");
    }

    private void GameOver(string reason)
    {
        isGameOver = true;
        Debug.Log($"[GameSessionManager] Game Over! Reason: {reason}");

        // ★ 스테이지 클리어 시 남은 Well 체력만큼 추가 점수
        if (reason == "TimeUp")  // 스테이지 클리어
        {
            if (wellHealth != null)
            {
                int bonus = wellHealth.CurrentHealth;
                ScoreManager.Instance.AddScore(bonus);
                Debug.Log($"[GameSessionManager] Stage Clear! Added bonus score: {bonus}");
            }
        }

        // static 클래스에 결과를 저장
        GameResultData.LastScore = ScoreManager.Instance.GetScore();
        GameResultData.Reason    = (reason == "TimeUp")
            ? GameEndReason.StageClear
            : GameEndReason.GameOver;

        // 1) 점수/시간/날짜를 기록에 남긴다
        SaveRecord();

        // 2) 결과 씬으로 전환
        SceneManager.LoadScene(gameResultSceneName);
    }

    private void OnDestroy()
    {
        if (wellHealth != null)
            wellHealth.OnDie -= OnWellDied;
    }

    /// <summary>
    /// 현재 남은 시간을 외부에서 읽어올 수 있도록 공개
    /// </summary>
    public float GetRemainingTime() => remainingTime;

    // ────────────────────────────────────────────────────
    // 기록 저장/불러오기
    // ────────────────────────────────────────────────────

    private string RecordFilePath
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, recordFileName);
        }
    }

    /// <summary>
    /// records.json 을 로드하거나, 없으면 새 인스턴스를 리턴
    /// </summary>
    private GameRecordList LoadRecordList()
    {
        try
        {
            if (File.Exists(RecordFilePath))
            {
                string json = File.ReadAllText(RecordFilePath);
                var list = JsonUtility.FromJson<GameRecordList>(json);
                if (list != null) return list;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[GameSessionManager] 기록 로드 실패: " + e.Message);
        }
        return new GameRecordList();
    }

    /// <summary>
    /// currentScore, DateTime.Now 를 JSON 파일에 저장
    /// </summary>
    private void SaveRecord()
    {
        // 1) 기존 기록 불러오기
        var list = LoadRecordList();

        // 2) 새로운 기록 추가
        var rec = new GameRecord
        {
            score = ScoreManager.Instance.GetScore(),
            // timeLeft = remainingTime,
            date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        list.records.Add(rec);

        // 3) 예: 점수 내림차순 정렬 후 상위 maxRecords개만 남기기
        list.records.Sort((a, b) => b.score.CompareTo(a.score));
        if (list.records.Count > maxRecords)
            list.records.RemoveRange(maxRecords, list.records.Count - maxRecords);

        // 4) JSON 직렬화 + 파일 쓰기
        try
        {
            string json = JsonUtility.ToJson(list, true);
            File.WriteAllText(RecordFilePath, json);
            Debug.Log($"[GameSessionManager] 기록 저장 완료 → {RecordFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError("[GameSessionManager] 기록 저장 실패: " + e.Message);
        }
    }

    /// <summary>
    /// 저장된 모든 기록을 반환
    /// </summary>
    public List<GameRecord> GetAllRecords()
    {
        return LoadRecordList().records;
    }
}
