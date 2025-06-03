using UnityEngine;
using UnityEngine.SceneManagement;  // 씬 전환용
using TMPro;                        // TextMeshProUGUI 사용 시

public class GhostMenuController : MonoBehaviour
{
    [Header("이 Ghost가 메뉴 중 어떤 기능인지 구분 (Start, Record 등)")]
    public MenuType menuType = MenuType.Start;

    [Header("기록 확인 시, 기록을 출력할 TextMeshProUGUI")]
    public TextMeshProUGUI recordDisplayText;

    // Enum으로 메뉴 기능 타입을 구분
    public enum MenuType
    {
        Start,   // 게임 시작
        Record,  // 기록 확인
        Quit     // (필요하다면) 종료
    }

    // 충돌을 감지하는 메서드 (Trigger가 아닌, 일반 Collider + Rigidbody 방식)
    private void OnCollisionEnter(Collision collision)
    {
        // 1) 충돌한 오브젝트가 “ThrowCollisionDestroyer” 스크립트(혹은 꼬치)인지 확인
        //    예: 충돌 오브젝트에 “ThrowCollisionDestroyer” 컴포넌트가 있는지 확인
        if (collision.gameObject.GetComponent<ThrowCollisionDestroyer>() == null)
            return;

        // 2) 이 Ghost를 처리할 때마다, 꼬치가 부착한 아이템 수 검사(원한다면)
        //    여기서는 메뉴용 Ghost이므로 반드시 부착 아이템이 있어야 하는 건 아닙니다.
        //    그냥 맞추면 바로 기능 실행해도 무방.
        //    필요하다면 아래 코드 예시처럼 할 수 있음:
        /*
        int initialChildCount = …; // 씬에 미리 구해 놓거나, Awake()에서 저장
        int attachedCount = transform.childCount - initialChildCount;
        if (attachedCount < requiredItemCount)
            return;
        */

        // 3) 충돌이 확정되면 두 개 모두 파괴(유령과 꼬치)
        Destroy(collision.gameObject);  // ThrowCollisionDestroyer(꼬치) 파괴
        Destroy(gameObject);            // 이 메뉴용 Ghost 파괴 (옵션: 파괴 대신 재배치할 수도 있음)

        // 4) menuType에 따라 기능 실행
        switch (menuType)
        {
            case MenuType.Start:
                // GameScene으로 전환 (빌드 세팅에 “GameScene” 이름이 등록되어 있어야 함)
                SceneManager.LoadScene("GameScene");
                break;

            case MenuType.Record:
                // 기록("PlayerPrefs" 등) 불러와서 recordDisplayText에 띄워 줌
                if (recordDisplayText != null)
                {
                    int highScore = PlayerPrefs.GetInt("HighScore", 0);
                    int lastScore = PlayerPrefs.GetInt("LastScore", 0);
                    recordDisplayText.text = $"최고 점수: {highScore}\n최근 점수: {lastScore}";
                }
                // RecordGhost는 파괴하지 않고, 다시 나타나도록 하려면 Destroy(gameObject) 부분을 주석처리
                break;

            case MenuType.Quit:
                // 빌드된 게임에서는 종료, 에디터에서는 플레이 모드 종료
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
        }
    }
}
