using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

public class SkewerController : MonoBehaviour
{
    [Header("Skewer Settings")]
    public Transform[] slotPositions = new Transform[3];

    [Header("Detection Settings")]
    public Vector3 detectionBoxSize = new Vector3(0.3f, 0.3f, 2.0f); // 꼬치보다 조금 크게

    [Header("Visual Feedback")]
    public GameObject[] slotIndicators = new GameObject[3];
    public Material availableSlotMaterial;
    public Material occupiedSlotMaterial;

    [Header("Physics Settings")]
    public bool autoUpdateBounds = true;

    private bool[] occupiedSlots = new bool[3];
    private List<GameObject> attachedIngredients = new List<GameObject>();
    private XRGrabInteractable skewerGrab;
    private Rigidbody skewerRigidbody;
    private BoxCollider physicsCollider;

    void Start()
    {
        skewerGrab = GetComponent<XRGrabInteractable>();
        skewerRigidbody = GetComponent<Rigidbody>();

        UpdateSlotVisuals();
        SetupColliders();
        EnsureSkewerPhysics();
    }

    private void SetupColliders()
    {
        // 기존 Box Collider는 물리용으로 유지
        physicsCollider = GetComponent<BoxCollider>();
        if (physicsCollider != null)
        {
            physicsCollider.isTrigger = false;
            // 꼬치 실제 크기 유지 (예: 0.1, 0.1, 1.5)
        }

        // 물리 콜라이더가 없으면 생성
        if (physicsCollider == null)
        {
            physicsCollider = gameObject.AddComponent<BoxCollider>();
            physicsCollider.isTrigger = false;
            physicsCollider.size = new Vector3(0.1f, 0.1f, 1.5f);
        }
    }

    private void EnsureSkewerPhysics()
    {
        if (skewerRigidbody == null)
        {
            skewerRigidbody = gameObject.AddComponent<Rigidbody>();
        }

        skewerRigidbody.mass = 0.1f;
        skewerRigidbody.drag = 1f;
        skewerRigidbody.angularDrag = 5f;

        if (skewerGrab == null)
        {
            skewerGrab = gameObject.AddComponent<XRGrabInteractable>();
        }

        skewerGrab.movementType = XRBaseInteractable.MovementType.VelocityTracking;
    }

    void Update()
    {
        // 꼬치가 잡혀있을 때만 재료 감지
        if (skewerGrab != null && skewerGrab.isSelected)
        {
            CheckForIngredientsInRange();
        }
    }

    private void CheckForIngredientsInRange()
    {
        // Physics.OverlapBox로 Detection Range 내 재료 찾기
        Collider[] nearbyColliders = Physics.OverlapBox(
            transform.position,                    // 중심점
            detectionBoxSize * 0.5f,              // 반 크기
            transform.rotation                     // 회전
        );

        foreach (Collider col in nearbyColliders)
        {
            IngredientItem ingredient = col.GetComponent<IngredientItem>();
            if (ingredient != null && !ingredient.isOnSkewer)
            {
                XRGrabInteractable ingredientGrab = ingredient.GetComponent<XRGrabInteractable>();

                if (ingredientGrab != null && ingredientGrab.isSelected)
                {
                    if (AreDifferentHands(skewerGrab, ingredientGrab))
                    {
                        TryAttachIngredient(ingredient);
                    }
                }
            }
        }
    }

    private bool AreDifferentHands(XRGrabInteractable skewer, XRGrabInteractable ingredient)
    {
        var skewerInteractor = skewer.firstInteractorSelecting;
        var ingredientInteractor = ingredient.firstInteractorSelecting;

        if (skewerInteractor != null && ingredientInteractor != null)
        {
            return skewerInteractor != ingredientInteractor;
        }
        return false;
    }

    public bool TryAttachIngredient(IngredientItem originalIngredient)
    {
        int requiredSlots = originalIngredient.sizeSlots;
        int availableStartSlot = FindAvailableSlots(requiredSlots);

        if (availableStartSlot != -1)
        {
            for (int i = 0; i < requiredSlots; i++)
            {
                occupiedSlots[availableStartSlot + i] = true;
            }

            GameObject newIngredient = CreateIngredientCopy(originalIngredient, availableStartSlot);
            attachedIngredients.Add(newIngredient);

            Destroy(originalIngredient.gameObject);

            UpdateSlotVisuals();
            TriggerHapticFeedback();

            if (autoUpdateBounds)
            {
                UpdatePhysicsColliderToBounds();
            }

            Debug.Log($"Ingredient attached to slot {availableStartSlot}");
            return true;
        }

        return false;
    }

    private GameObject CreateIngredientCopy(IngredientItem originalIngredient, int slotIndex)
    {
        GameObject newIngredient = Instantiate(originalIngredient.gameObject);

        // 검색 결과 [1]의 방법: 부모를 null로 설정하고 스케일 설정 후 다시 부모 설정
        Transform originalParent = transform;

        // 1. 부모를 null로 설정
        newIngredient.transform.SetParent(null);

        // 2. 원하는 크기로 설정 (절대 크기)
        IngredientItem ingredientScript = newIngredient.GetComponent<IngredientItem>();
        if (ingredientScript != null)
        {
            newIngredient.transform.localScale = ingredientScript.skewerScale;
        }

        // 3. 위치 설정
        newIngredient.transform.position = GetSlotPosition(slotIndex);
        newIngredient.transform.rotation = GetSlotRotation();

        // 4. 다시 부모 설정
        newIngredient.transform.SetParent(originalParent);

        // 나머지 컴포넌트 제거...
        XRGrabInteractable grabComponent = newIngredient.GetComponent<XRGrabInteractable>();
        if (grabComponent != null) Destroy(grabComponent);

        Rigidbody rb = newIngredient.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);

        Collider col = newIngredient.GetComponent<Collider>();
        if (col != null) Destroy(col);

        return newIngredient;
    }



    public void RemoveIngredient(GameObject ingredient, int startSlot, int sizeSlots)
    {
        if (attachedIngredients.Contains(ingredient))
        {
            // 재료 스크립트에 분리 알림
            IngredientItem ingredientScript = ingredient.GetComponent<IngredientItem>();
            if (ingredientScript != null)
            {
                ingredientScript.DetachFromSkewer();
            }

            for (int i = 0; i < sizeSlots; i++)
            {
                if (startSlot + i < occupiedSlots.Length)
                {
                    occupiedSlots[startSlot + i] = false;
                }
            }

            attachedIngredients.Remove(ingredient);
            Destroy(ingredient);
            UpdateSlotVisuals();

            if (autoUpdateBounds)
            {
                UpdatePhysicsColliderToBounds();
            }
        }
    }

    public void RemoveLastIngredient()
    {
        if (attachedIngredients.Count > 0)
        {
            GameObject lastIngredient = attachedIngredients[attachedIngredients.Count - 1];
            IngredientItem ingredientScript = lastIngredient.GetComponent<IngredientItem>();

            if (ingredientScript != null)
            {
                RemoveIngredient(lastIngredient, ingredientScript.startSlotIndex, ingredientScript.sizeSlots);
            }
        }
    }

    // 꼬치와 모든 재료들의 통합 경계 계산
    public Bounds GetCombinedBounds()
    {
        Bounds combinedBounds = new Bounds();
        bool hasBounds = false;

        // 꼬치 자체의 Renderer 포함
        Renderer skewerRenderer = GetComponent<Renderer>();
        if (skewerRenderer != null)
        {
            combinedBounds = skewerRenderer.bounds;
            hasBounds = true;
        }

        // 모든 재료들의 Renderer 포함
        foreach (GameObject ingredient in attachedIngredients)
        {
            if (ingredient != null)
            {
                Renderer ingredientRenderer = ingredient.GetComponent<Renderer>();
                if (ingredientRenderer != null)
                {
                    if (!hasBounds)
                    {
                        combinedBounds = ingredientRenderer.bounds;
                        hasBounds = true;
                    }
                    else
                    {
                        combinedBounds.Encapsulate(ingredientRenderer.bounds);
                    }
                }
            }
        }

        // 경계가 없으면 기본 경계 설정
        if (!hasBounds)
        {
            combinedBounds = new Bounds(transform.position, Vector3.one);
        }

        return combinedBounds;
    }

    // 물리 콜라이더를 통합 경계에 맞게 업데이트
    public void UpdatePhysicsColliderToBounds()
    {
        if (physicsCollider == null) return;

        Bounds combinedBounds = GetCombinedBounds();

        // 월드 좌표를 로컬 좌표로 변환
        Vector3 localCenter = transform.InverseTransformPoint(combinedBounds.center);
        Vector3 localSize = transform.InverseTransformVector(combinedBounds.size);

        // 절댓값 사용 (스케일이 음수일 수 있음)
        localSize = new Vector3(
            Mathf.Abs(localSize.x),
            Mathf.Abs(localSize.y),
            Mathf.Abs(localSize.z)
        );

        physicsCollider.center = localCenter;
        physicsCollider.size = localSize;

        Debug.Log($"Physics collider updated - Center: {localCenter}, Size: {localSize}");
    }

    // 수동으로 경계 업데이트
    [ContextMenu("Update Bounds")]
    public void ManualUpdateBounds()
    {
        UpdatePhysicsColliderToBounds();
    }

    private void TriggerHapticFeedback()
    {
        if (skewerGrab != null && skewerGrab.isSelected)
        {
            var interactor = skewerGrab.firstInteractorSelecting as XRBaseControllerInteractor;
            if (interactor != null)
            {
                interactor.SendHapticImpulse(0.5f, 0.2f);
            }
        }
    }

    private int FindAvailableSlots(int requiredSlots)
    {
        for (int i = 0; i <= occupiedSlots.Length - requiredSlots; i++)
        {
            bool canFit = true;
            for (int j = 0; j < requiredSlots; j++)
            {
                if (occupiedSlots[i + j])
                {
                    canFit = false;
                    break;
                }
            }
            if (canFit) return i;
        }
        return -1;
    }

    public Vector3 GetSlotPosition(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slotPositions.Length && slotPositions[slotIndex] != null)
        {
            return slotPositions[slotIndex].position;
        }
        return transform.position;
    }

    public Quaternion GetSlotRotation()
    {
        return transform.rotation;
    }

    private void UpdateSlotVisuals()
    {
        for (int i = 0; i < slotIndicators.Length; i++)
        {
            if (slotIndicators[i] != null)
            {
                Renderer renderer = slotIndicators[i].GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = occupiedSlots[i] ? occupiedSlotMaterial : availableSlotMaterial;
                }
            }
        }
    }

    public List<GameObject> GetAttachedIngredients()
    {
        return new List<GameObject>(attachedIngredients);
    }

    void OnDrawGizmosSelected()
    {
        // Detection Range 시각화 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, detectionBoxSize);

        // 물리 콜라이더 시각화 (빨간색)
        if (physicsCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(physicsCollider.center, physicsCollider.size);
        }

        // 통합 경계 표시 (녹색)
        if (Application.isPlaying)
        {
            Bounds combinedBounds = GetCombinedBounds();
            Gizmos.color = Color.green;
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawWireCube(combinedBounds.center, combinedBounds.size);
        }
    }
}
