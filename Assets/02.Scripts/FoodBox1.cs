using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class IngredientBox : MonoBehaviour
{
    [Header("��� ����")]
    public GameObject[] ingredientPrefabs;

    [Header("�� ����")]
    public XRBaseInteractor leftHand;  // Inspector���� Left Direct Interactor ����
    public XRBaseInteractor rightHand; // Inspector���� Right Direct Interactor ����

    private BoxCollider boxCollider;
    private bool canSpawn = true;
    private float spawnCooldown = 0.5f;
    private float lastSpawnTime;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            Debug.LogError("BoxCollider�� �ʿ��մϴ�!");
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

        // �޼� üũ
        if (leftHand != null && IsHandInBox(leftHand.transform.position, worldCenter, worldHalfExtents))
        {
            if (!leftHand.hasSelection && CheckXRInput(leftHand))
            {
                CreateIngredient(leftHand);
                return;
            }
        }

        // ������ üũ
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
        // �� ��ġ�� �ڽ� ���� ���� �ִ��� üũ
        Vector3 localPoint = transform.InverseTransformPoint(handPosition);
        Vector3 localCenter = boxCollider.center;
        Vector3 localHalfExtents = boxCollider.size * 0.5f;

        return (Mathf.Abs(localPoint.x - localCenter.x) <= localHalfExtents.x &&
                Mathf.Abs(localPoint.y - localCenter.y) <= localHalfExtents.y &&
                Mathf.Abs(localPoint.z - localCenter.z) <= localHalfExtents.z);
    }
    bool CheckXRInput(XRBaseInteractor hand)
    {
        // XR Direct Interactor�� select ���� üũ
        if (hand is XRDirectInteractor directInteractor)
        {
            // ���� �����ӿ��� select�� Ȱ��ȭ�Ǿ����� üũ
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
                // ���ο� �������̽� ������� ĳ����
                hand.interactionManager.SelectEnter((IXRSelectInteractor)hand, (IXRSelectInteractable)grabInteractable);
            }
        }

        lastSpawnTime = Time.time;
        canSpawn = false;
        Invoke(nameof(ResetSpawnCooldown), spawnCooldown);

        Debug.Log($"{name}���� {newIngredient.name} ������!");
    }

    void ResetSpawnCooldown()
    {
        canSpawn = true;
    }
}
