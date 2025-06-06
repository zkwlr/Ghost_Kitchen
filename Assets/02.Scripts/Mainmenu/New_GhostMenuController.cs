using UnityEngine;
using UnityEngine.SceneManagement;  // 씬 전환용

public class New_GhostMenuController : MonoBehaviour
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

        // VRFadeManager를 찾아서 씬 전환
        VRFadeManager fadeManager = GetVRFadeManager();
        
        switch (menuType)
        {
            case MenuType.Start:
                if (fadeManager != null)
                    fadeManager.FadeToScene("BG_test");
                else
                    SceneManager.LoadScene("BG_test");
                break;

            case MenuType.RecordScene:
                if (fadeManager != null)
                    fadeManager.FadeToScene("MainmenuScene");
                else
                    SceneManager.LoadScene("MainmenuScene");
                break;

            case MenuType.Retry:
                string currentScene = SceneManager.GetActiveScene().name;
                if (fadeManager != null)
                    fadeManager.FadeToScene(currentScene);
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
    
    // VRFadeManager를 찾는 메서드
    private VRFadeManager GetVRFadeManager()
    {
        // 먼저 메인 카메라에서 찾기
        if (Camera.main != null)
        {
            VRFadeManager fadeManager = Camera.main.GetComponent<VRFadeManager>();
            if (fadeManager != null)
                return fadeManager;
        }
        
        // 메인 카메라에 없다면 씬에서 찾기
        return FindObjectOfType<VRFadeManager>();
    }
}
