using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class VRFadeManager : MonoBehaviour
{
    public static VRFadeManager Instance;
    
    [Header("페이드 설정")]
    public GameObject fadeCubePrefab;
    public float fadeDuration = 1f;
    
    private GameObject currentFadeCube;
    private Material fadeMaterial;
    void Start()
    {
        // 게임 시작 시 페이드 인
        StartFadeIn();
        Debug.Log("VRFadeManager 시작");
    }
    
    public void StartFadeIn()
    {
        if (fadeCubePrefab != null)
        {
            CreateFadeCube();
            StartCoroutine(FadeIn());
        }
    }
    
    public void FadeToScene(string sceneName)
    {
        if (fadeCubePrefab != null)
        {
            CreateFadeCube();
            StartCoroutine(FadeOutAndLoadScene(sceneName));
        }
    }

    void CreateFadeCube()
    {
        if (currentFadeCube != null)
        {
            Destroy(currentFadeCube);
        }

        // 카메라에 큐브 프리팹 인스턴스화
        currentFadeCube = Instantiate(fadeCubePrefab, transform);
        currentFadeCube.transform.localPosition = new Vector3(0, 0, 0);
        currentFadeCube.transform.localScale = Vector3.one * 1f;

        fadeMaterial = currentFadeCube.GetComponent<Renderer>().material;
        Debug.Log("페이드 큐브 생성 완료");
    }
    
    IEnumerator FadeIn()
{
    float elapsedTime = 0;
    
    while (elapsedTime < fadeDuration)
    {
        elapsedTime += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
        
        // _Alpha 대신 _Color의 알파값 직접 변경
        Color color = fadeMaterial.color;
        color.a = alpha;
        fadeMaterial.color = color;
        
        yield return null;
    }
    
    if (currentFadeCube != null)
    {
        Destroy(currentFadeCube);
    }
}


    IEnumerator FadeOutAndLoadScene(string sceneName)
{
    float elapsedTime = 0;

    while (elapsedTime < fadeDuration)
    {
        elapsedTime += Time.deltaTime;
        float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
        
        // _Alpha 대신 _Color의 알파값 직접 변경
        Color color = fadeMaterial.color;
        color.a = alpha;
        fadeMaterial.color = color;
        
        yield return null;
    }

    SceneManager.LoadScene(sceneName);
    Debug.Log("씬 로드 성공");
}

}

