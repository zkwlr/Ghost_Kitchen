using UnityEngine;
#if UNITY_XR_INTERACTION_TOOLKIT
using UnityEngine.XR.Interaction.Toolkit;
#endif

public class ResultSceneInitializer : MonoBehaviour
{
    [Header("카메라 스폰 포인트")]
    public Transform spawnPoint;

    void Start()
    {
        if (spawnPoint == null)
        {
            Debug.LogError("ResultSceneInitializer: spawnPoint가 설정되지 않았습니다.");
            return;
        }

        // 1) XR Interaction Toolkit 사용 시
#if UNITY_XR_INTERACTION_TOOLKIT
        var xrOrigin = FindObjectOfType<XROrigin>();
        if (xrOrigin != null)
        {
            xrOrigin.transform.position = spawnPoint.position;
            xrOrigin.transform.rotation = spawnPoint.rotation;
            return;
        }
#endif

        // 2) 아니면 Camera.main의 부모(카메라 리그) 옮기기
        var cam = Camera.main;
        if (cam != null)
        {
            var rig = cam.transform.parent ?? cam.transform;
            rig.position = spawnPoint.position;
            rig.rotation = spawnPoint.rotation;
        }
        else
        {
            Debug.LogError("ResultSceneInitializer: Camera.main을 찾을 수 없습니다.");
        }
    }
}