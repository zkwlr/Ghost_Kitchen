using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class IngredientBox : MonoBehaviour
{
    [Header("재료 설정")]
    public GameObject[] ingredientPrefabs;

    [Header("손 설정")]
    public XRBaseInteractor leftHand;  // Inspector에서 Left Direct Interactor 연결
    public XRBaseInteractor rightHand; // Inspector에서 Right Direct Interactor 연결

    private BoxCollider boxCollider;
    private bool canSpawn = true;
    private float spawnCooldown = 0.5f;
    private float lastSpawnTime;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            Debug.LogError("BoxCollider가 필요합니다!");
        }
    }

    void Update()
    {
        CheckForHandsInBox();
    }

    void CheckForHandsInBox()
    {
        if (!canSpawn && Time.time - lastSpawnTime < spawnCooldown)
            return;

        Vector3 worldCenter = transform.TransformPoint(boxCollider.center);
        Vector3 worldHalfExtents = Vector3.Scale(boxCollider.size, transform.lossyScale) * 0.5f;

        // 왼손 체크
        if (leftHand != null && IsHandInBox(leftHand.transform.position, worldCenter, worldHalfExtents))
        {
            if (!leftHand.hasSelection && CheckXRInput(leftHand))
            {
                CreateIngredient(leftHand);
                return;
            }
        }

        // 오른손 체크
        if (rightHand != null && IsHandInBox(rightHand.transform.position, worldCenter, worldHalfExtents))
        {
            if (!rightHand.hasSelection && CheckXRInput(rightHand))
            {
                CreateIngredient(rightHand);
                return;
            }
        }
    }

    bool IsHandInBox(Vector3 handPosition, Vector3 boxCenter, Vector3 boxHalfExtents)
    {
        // 손 위치가 박스 영역 내에 있는지 체크
        Vector3 localPoint = transform.InverseTransformPoint(handPosition);
        Vector3 localCenter = boxCollider.center;
        Vector3 localHalfExtents = boxCollider.size * 0.5f;

        return (Mathf.Abs(localPoint.x - localCenter.x) <= localHalfExtents.x &&
                Mathf.Abs(localPoint.y - localCenter.y) <= localHalfExtents.y &&
                Mathf.Abs(localPoint.z - localCenter.z) <= localHalfExtents.z);
    }
    bool CheckXRInput(XRBaseInteractor hand)
    {
        // XR Direct Interactor의 select 상태 체크
        if (hand is XRDirectInteractor directInteractor)
        {
            // 현재 프레임에서 select가 활성화되었는지 체크
            return directInteractor.isSelectActive;
        }

        return false;
    }

    void CreateIngredient(XRBaseInteractor hand)
    {
        if (ingredientPrefabs.Length == 0) return;

        int index = Random.Range(0, ingredientPrefabs.Length);
        Vector3 spawnPosition = hand.transform.position;
        GameObject newIngredient = Instantiate(ingredientPrefabs[index], spawnPosition, Quaternion.identity);

        XRGrabInteractable grabInteractable = newIngredient.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            if (hand.interactionManager != null)
            {
                // 새로운 인터페이스 방식으로 캐스팅
                hand.interactionManager.SelectEnter((IXRSelectInteractor)hand, (IXRSelectInteractable)grabInteractable);
            }
        }

        lastSpawnTime = Time.time;
        canSpawn = false;
        Invoke(nameof(ResetSpawnCooldown), spawnCooldown);

        Debug.Log($"{name}에서 {newIngredient.name} 생성됨!");
    }

    void ResetSpawnCooldown()
    {
        canSpawn = true;
    }
}
