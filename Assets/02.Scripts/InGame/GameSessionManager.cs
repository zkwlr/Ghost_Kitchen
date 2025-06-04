using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameSessionManager : MonoBehaviour
{
    [Header("게임 제한 시간 (초)")]
    public float gameDuration = 120f;

    [Header("Well(목표)의 Health 컴포넌트")]
    public Health wellHealth;

    [Header("게임 결과 씬 이름")]
    public string gameResultSceneName = "GameResultScene";

    private float remainingTime;
    private bool isGameOver = false;

    private void Awake()
    {
        remainingTime = gameDuration;
        if (wellHealth == null)
        {
            GameObject wellGO = GameObject.FindGameObjectWithTag("Well");
            if (wellGO != null)
                wellHealth = wellGO.GetComponent<Health>();
            else
                Debug.LogError("[GameSessionManager] 'Well' 태그 오브젝트를 찾을 수 없습니다.");
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
        SceneManager.LoadScene(gameResultSceneName);
    }

    private void OnDestroy()
    {
        if (wellHealth != null)
            wellHealth.OnDie -= OnWellDied;
    }

    /// <summary>
    /// 외부에서 남은 시간을 읽어올 수 있도록 public으로 노출
    /// </summary>
    public float GetRemainingTime()
    {
        return remainingTime;
    }
}