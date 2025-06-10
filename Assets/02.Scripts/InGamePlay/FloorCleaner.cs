using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FloorCleaner : MonoBehaviour
{
    [Header("정리 설정")]
    public float cleanupDelay = 5f; // 바닥에 떨어진 후 삭제까지 시간
    public bool showDebugMessages = true;
    
    [Header("정리 대상 태그 (선택사항)")]
    public string[] targetTags = { "Skewer", "Food", "ThrowableObject" };
    public bool useTagFilter = false; // 태그 필터 사용 여부
    
    private Dictionary<GameObject, Coroutine> cleanupCoroutines = new Dictionary<GameObject, Coroutine>();
    
    void OnCollisionEnter(Collision collision)
    {
        GameObject hitObject = collision.gameObject;
        
        if (showDebugMessages)
        {
            Debug.Log($"바닥에 충돌: {hitObject.name}");
        }
        
        // 정리 대상인지 확인
        if (IsTargetObject(hitObject))
        {
            StartCleanupTimer(hitObject);
        }
    }
    
    bool IsTargetObject(GameObject obj)
    {
        // Rigidbody가 있는 오브젝트만 대상으로 함
        if (obj.GetComponent<Rigidbody>() == null)
        {
            return false;
        }
        
        // 태그 필터를 사용하는 경우
        if (useTagFilter)
        {
            foreach (string tag in targetTags)
            {
                if (obj.CompareTag(tag))
                {
                    return true;
                }
            }
            return false;
        }
        
        // 태그 필터를 사용하지 않으면 모든 Rigidbody 오브젝트가 대상
        return true;
    }
    
    void StartCleanupTimer(GameObject targetObject)
    {
        // 이미 타이머가 있다면 취소하고 새로 시작
        if (cleanupCoroutines.ContainsKey(targetObject))
        {
            StopCoroutine(cleanupCoroutines[targetObject]);
        }
        
        Coroutine cleanupCoroutine = StartCoroutine(CleanupAfterDelay(targetObject));
        cleanupCoroutines[targetObject] = cleanupCoroutine;
        
        if (showDebugMessages)
        {
            Debug.Log($"{targetObject.name}이(가) 바닥에 떨어졌습니다. {cleanupDelay}초 후 정리됩니다.");
        }
    }
    
    IEnumerator CleanupAfterDelay(GameObject targetObject)
    {
        yield return new WaitForSeconds(cleanupDelay);
        
        if (targetObject != null)
        {
            if (showDebugMessages)
            {
                Debug.Log($"바닥 정리: {targetObject.name} 삭제됨");
            }
            
            cleanupCoroutines.Remove(targetObject);
            Destroy(targetObject);
        }
    }
    
    // 수동으로 특정 오브젝트 정리
    public void CleanupObject(GameObject obj)
    {
        if (cleanupCoroutines.ContainsKey(obj))
        {
            StopCoroutine(cleanupCoroutines[obj]);
            cleanupCoroutines.Remove(obj);
        }
        
        Destroy(obj);
    }
    
    // 바닥의 모든 오브젝트 즉시 정리
    public void CleanupAllObjects()
    {
        foreach (var pair in cleanupCoroutines)
        {
            if (pair.Key != null)
            {
                Destroy(pair.Key);
            }
            StopCoroutine(pair.Value);
        }
        
        cleanupCoroutines.Clear();
        Debug.Log("바닥의 모든 오브젝트 정리 완료");
    }
}
