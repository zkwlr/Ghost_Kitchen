using UnityEngine;
using UnityEngine.SceneManagement;  // 씬 전환용

public class GhostMenuController : MonoBehaviour
{
    [Header("이 Ghost가 메뉴 중 어떤 기능인지 구분 (Start, Record, Retry, Quit)")]
    public MenuType menuType = MenuType.Start;

    // Enum으로 메뉴 기능 타입을 구분
    public enum MenuType
    {
        Start,       // 게임 시작
        RecordScene, // 기록 확인용 씬으로 이동
        Retry,       // 재시작
        Quit         // 종료
    }

    // 충돌을 감지하는 메서드 (일반 Collider + Rigidbody 방식)
    private void OnCollisionEnter(Collision collision)
    {
        // 1) 충돌한 오브젝트가 “ThrowCollisionDestroyer” 컴포넌트를 가지는지 확인
        if (collision.gameObject.GetComponent<ThrowCollisionDestroyer>() == null)
            return;

        // 2) 충돌이 확정되면 꼬치 오브젝트(ThrowCollisionDestroyer)와 이 메뉴용 Ghost를 파괴
        Destroy(collision.gameObject);  // 꼬치 파괴
        Destroy(gameObject);            // 이 메뉴용 Ghost 파괴 (원한다면 재배치 방식으로 수정 가능)

        // 3) menuType에 따라 기능 실행
        switch (menuType)
        {
            case MenuType.Start:
                // "GameScene" 씬으로 전환 (Build Settings에 반드시 등록되어 있어야 함)
                SceneManager.LoadScene("background_develophdh");
                break;

            case MenuType.RecordScene:
                // "RecordScene" 씬으로 전환 (기록 확인 씬)
                SceneManager.LoadScene("RecordScene");
                break;

            case MenuType.Retry:
                // 현재 활성 씬을 다시 불러옴 (게임 재시작 용)
                Scene current = SceneManager.GetActiveScene();
                SceneManager.LoadScene(current.name);
                break;

            case MenuType.Quit:
                // 에디터에서는 플레이 모드 종료, 빌드된 게임에서는 Application.Quit()
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
        }
    }
}