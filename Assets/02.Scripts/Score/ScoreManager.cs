using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    // ───────────────────────────────────────────────────────
    // 1) 싱글톤 인스턴스
    public static ScoreManager Instance { get; private set; }

    [Header("초기 점수 설정")]
    public int startScore = 0;

    [Header("점수를 초기화할 씬 이름")]
    [Tooltip("이 씬이 로드될 때 currentScore를 startScore로 초기화합니다.")]
    public string resetSceneName = "GameScene";

    // 현재 점수를 저장
    private int currentScore;

    // 점수가 변경될 때(올라갈 때) 호출할 이벤트
    public UnityEvent<int> OnScoreChanged = new UnityEvent<int>();

    private void Awake()
    {
        // ── 싱글톤 세팅 ──
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 초기 점수를 세팅
        currentScore = startScore;

        // 초기 점수가 필요하다면 이벤트 한 번 발생
        OnScoreChanged.Invoke(currentScore);

        // 씬 로드 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // 씬 로드 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 씬이 새로 로드될 때 호출되는 콜백.
    /// resetSceneName과 일치하는 씬이 로드되면 점수를 초기화합니다.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == resetSceneName)
        {
            ResetScore();
        }
    }

    /// <summary>
    /// currentScore를 startScore로 초기화하고, 이벤트를 발생시킵니다.
    /// </summary>
    private void ResetScore()
    {
        currentScore = startScore;
        OnScoreChanged.Invoke(currentScore);
        Debug.Log("[ScoreManager] Score reset to startScore (" + startScore + ") on scene load.");
    }

    /// <summary>
    /// delta만큼 점수를 더합니다(음수인 경우 뺍니다).
    /// </summary>
    public void AddScore(int delta)
    {
        currentScore += delta;
        if (currentScore < 0) currentScore = 0;

        // 점수가 바뀔 때마다 등록된 리스너(예: UI)가 있으면 호출
        OnScoreChanged.Invoke(currentScore);
    }

    /// <summary>
    /// 현재 점수를 리턴합니다.
    /// </summary>
    public int GetScore()
    {
        return currentScore;
    }
}
