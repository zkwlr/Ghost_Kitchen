using UnityEngine;
using UnityEngine.SceneManagement;  // 씬 전환용

public class GhostMenuController : MonoBehaviour
{
    [Header("이 Ghost가 메뉴 중 어떤 기능인지 구분 (Start, Record, Retry, Quit)")]
    public MenuType menuType = MenuType.Start;

    public enum MenuType
    {
        Start,       // 게임 시작
        RecordScene, // 기록 확인용 씬으로 이동
        Retry,       // 재시작
        Quit         // 종료
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 오브젝트가 꼬치인지를 확인
        if (collision.gameObject.GetComponent<ThrowCollisionDestroyer>() == null)
            return;

        // 꼬치와 이 Ghost 모두 파괴 - skewer 단에서 처리함
        // Destroy(collision.gameObject);
        // Destroy(gameObject);

        // 페이드 매니저를 통해 씬 전환
        switch (menuType)
        {
            case MenuType.Start:
                if (FadeManager.Instance != null)
                    FadeManager.Instance.FadeToScene("BG_test");
                else
                    SceneManager.LoadScene("BG_test");
                break;

            // 기록 신 임시로 메인 신으로 사용
            case MenuType.RecordScene:
                if (FadeManager.Instance != null)
                    FadeManager.Instance.FadeToScene("MainmenuScene");
                else
                    SceneManager.LoadScene("MainmenuScene");
                break;

            case MenuType.Retry:
                string currentScene = SceneManager.GetActiveScene().name;
                if (FadeManager.Instance != null)
                    FadeManager.Instance.FadeToScene(currentScene);
                else
                    SceneManager.LoadScene(currentScene);
                break;

            case MenuType.Quit:
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
        }
    }
}