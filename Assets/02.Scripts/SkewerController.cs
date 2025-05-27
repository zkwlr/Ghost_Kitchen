using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

public class SkewerController : MonoBehaviour
{
    [Header("Skewer Settings")]
    public Transform[] slotPositions = new Transform[3];
    public float detectionRadius = 0.3f;

    [Header("Ingredient Settings")]
    public Vector3 ingredientScaleOnSkewer = new Vector3(0.1f, 0.1f, 0.1f);

    [Header("Visual Feedback")]
    public GameObject[] slotIndicators = new GameObject[3];
    public Material availableSlotMaterial;
    public Material occupiedSlotMaterial;

    [Header("Physics Settings")]
    public bool autoUpdateBounds = true; // �ڵ����� ��� ������Ʈ

    private bool[] occupiedSlots = new bool[3];
    private List<GameObject> attachedIngredients = new List<GameObject>();
    private XRGrabInteractable skewerGrab;
    private Rigidbody skewerRigidbody;
    private BoxCollider physicsCollider;
    private BoxCollider triggerCollider;
    private Vector3 lastParentScale;

    void Start()
    {
        skewerGrab = GetComponent<XRGrabInteractable>();
        skewerRigidbody = GetComponent<Rigidbody>();

        UpdateSlotVisuals();
        SetupColliders();
        EnsureSkewerPhysics();

        lastParentScale = transform.lossyScale;
    }

    private void SetupColliders()
    {
        Collider[] existingColliders = GetComponents<Collider>();

        // ������ �ݶ��̴� (ù ��°)
        if (existingColliders.Length > 0)
        {
            physicsCollider = existingColliders[0] as BoxCollider;
            if (physicsCollider != null)
            {
                physicsCollider.isTrigger = false;
            }
        }

        // Ʈ���ſ� �ݶ��̴� �߰� (�� ��°)
        triggerCollider = gameObject.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector3(0.5f, 0.5f, 2.0f);

        // ���� �ݶ��̴��� ������ ����
        if (physicsCollider == null)
        {
            physicsCollider = gameObject.AddComponent<BoxCollider>();
            physicsCollider.isTrigger = false;
            physicsCollider.size = new Vector3(0.1f, 0.1f, 1.5f);
        }
    }

    // ��ġ�� ��� ������ ���� ��� ���
    public Bounds GetCombinedBounds()
    {
        Bounds combinedBounds = new Bounds();
        bool hasBounds = false;

        // ��ġ ��ü�� Renderer ����
        Renderer skewerRenderer = GetComponent<Renderer>();
        if (skewerRenderer != null)
        {
            combinedBounds = skewerRenderer.bounds;
            hasBounds = true;
        }

        // ��� ������ Renderer ����
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

        // ��谡 ������ �⺻ ��� ����
        if (!hasBounds)
        {
            combinedBounds = new Bounds(transform.position, Vector3.one);
        }

        return combinedBounds;
    }

    // ���� �ݶ��̴��� ���� ��迡 �°� ������Ʈ
    public void UpdatePhysicsColliderToBounds()
    {
        if (physicsCollider == null) return;

        Bounds combinedBounds = GetCombinedBounds();

        // ���� ��ǥ�� ���� ��ǥ�� ��ȯ
        Vector3 localCenter = transform.InverseTransformPoint(combinedBounds.center);
        Vector3 localSize = transform.InverseTransformVector(combinedBounds.size);

        // ���� ��� (�������� ������ �� ����)
        localSize = new Vector3(
            Mathf.Abs(localSize.x),
            Mathf.Abs(localSize.y),
            Mathf.Abs(localSize.z)
        );

        physicsCollider.center = localCenter;
        physicsCollider.size = localSize;

        Debug.Log($"Physics collider updated - Center: {localCenter}, Size: {localSize}");
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

    void OnTriggerStay(Collider other)
    {
        IngredientItem ingredient = other.GetComponent<IngredientItem>();

        if (ingredient != null && !ingredient.isOnSkewer)
        {
            XRGrabInteractable ingredientGrab = ingredient.GetComponent<XRGrabInteractable>();

            if (skewerGrab != null && skewerGrab.isSelected &&
                ingredientGrab != null && ingredientGrab.isSelected)
            {
                if (AreDifferentHands(skewerGrab, ingredientGrab))
                {
                    TryAttachIngredient(ingredient);
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

            // ��� �߰� �� ��� ������Ʈ
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

        newIngredient.transform.SetParent(transform);
        newIngredient.transform.position = GetSlotPosition(slotIndex);
        newIngredient.transform.rotation = GetSlotRotation();

        // �������� ����
        Vector3 parentLossyScale = transform.lossyScale;
        Vector3 desiredWorldScale = ingredientScaleOnSkewer;

        Vector3 inverseParentScale = new Vector3(
            parentLossyScale.x != 0 ? desiredWorldScale.x / parentLossyScale.x : desiredWorldScale.x,
            parentLossyScale.y != 0 ? desiredWorldScale.y / parentLossyScale.y : desiredWorldScale.y,
            parentLossyScale.z != 0 ? desiredWorldScale.z / parentLossyScale.z : desiredWorldScale.z
        );

        newIngredient.transform.localScale = inverseParentScale;

        // ��ȣ�ۿ� ������Ʈ ���� (Renderer�� ����!)
        XRGrabInteractable grabComponent = newIngredient.GetComponent<XRGrabInteractable>();
        if (grabComponent != null) Destroy(grabComponent);

        Rigidbody rb = newIngredient.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);

        Collider col = newIngredient.GetComponent<Collider>();
        if (col != null) Destroy(col);

        // IngredientItem ��ũ��Ʈ ����
        IngredientItem ingredientScript = newIngredient.GetComponent<IngredientItem>();
        if (ingredientScript != null)
        {
            ingredientScript.isOnSkewer = true;
            ingredientScript.currentSkewer = this;
            ingredientScript.startSlotIndex = slotIndex;
        }

        return newIngredient;
    }

    public void RemoveIngredient(GameObject ingredient, int startSlot, int sizeSlots)
    {
        if (attachedIngredients.Contains(ingredient))
        {
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

            // ��� ���� �� ��� ������Ʈ
            if (autoUpdateBounds)
            {
                UpdatePhysicsColliderToBounds();
            }
        }
    }

    // �������� ��� ������Ʈ (��ư�̳� Ư�� ��Ȳ���� ȣ��)
    [ContextMenu("Update Bounds")]
    public void ManualUpdateBounds()
    {
        UpdatePhysicsColliderToBounds();
    }

    public void UpdateIngredientScales()
    {
        foreach (GameObject ingredient in attachedIngredients)
        {
            if (ingredient != null)
            {
                Vector3 parentLossyScale = transform.lossyScale;
                Vector3 desiredWorldScale = ingredientScaleOnSkewer;

                Vector3 inverseParentScale = new Vector3(
                    parentLossyScale.x != 0 ? desiredWorldScale.x / parentLossyScale.x : desiredWorldScale.x,
                    parentLossyScale.y != 0 ? desiredWorldScale.y / parentLossyScale.y : desiredWorldScale.y,
                    parentLossyScale.z != 0 ? desiredWorldScale.z / parentLossyScale.z : desiredWorldScale.z
                );

                ingredient.transform.localScale = inverseParentScale;
            }
        }
    }

    void Update()
    {
        if (lastParentScale != transform.lossyScale)
        {
            lastParentScale = transform.lossyScale;
            UpdateIngredientScales();

            if (autoUpdateBounds)
            {
                UpdatePhysicsColliderToBounds();
            }
        }
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
}
