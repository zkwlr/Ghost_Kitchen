using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public enum IngredientType
{
    Meat, Vegetable, Seafood, Sauce, Spice
}

public class IngredientItem : MonoBehaviour
{
    [Header("Ingredient Settings")]
    public int sizeSlots = 1;
    public IngredientType ingredientType;

    [Header("Scale Settings")]
    public Vector3 originalScale; // ���� ũ�� ����
    public Vector3 skewerScale = new Vector3(0.1f, 0.1f, 0.1f); // ��ġ�� ���� �� ũ��

    [Header("Skewer Settings")]
    public bool isOnSkewer = false;
    public SkewerController currentSkewer;
    public int startSlotIndex = -1;

    private Transform parentSkewer;
    private Vector3 lastParentScale;

    void Start()
    {
        // ���� ũ�� ����
        if (originalScale == Vector3.zero)
        {
            originalScale = transform.localScale;
        }
    }

    void Update()
    {
        
    }

    // ��ġ�� �پ��� �� ȣ��
    public void AttachToSkewer(SkewerController skewer, int slotIndex)
    {
        isOnSkewer = true;
        currentSkewer = skewer;
        startSlotIndex = slotIndex;
        parentSkewer = skewer.transform;

        // ���� ������ ���� (���밪���� ����)
        if (originalScale == Vector3.zero)
        {
            originalScale = transform.localScale;
        }

        // �θ� ����
        transform.SetParent(skewer.transform);

        // ���ϴ� ũ�⸦ ���� ���� (�������� ��� ����)
        transform.localScale = skewerScale;

        Debug.Log($"{gameObject.name} attached with fixed scale: {skewerScale}");
    }


    // ��ġ���� �и��� �� ȣ��
    public void DetachFromSkewer()
    {
        isOnSkewer = false;
        currentSkewer = null;
        startSlotIndex = -1;
        parentSkewer = null;

        // ���� ũ��� ����
        transform.localScale = originalScale;

        Debug.Log($"{gameObject.name} detached, restored to original scale: {originalScale}");
    }

    // ��ġ������ ũ�� ������Ʈ
    private void UpdateScaleOnSkewer()
    {
        if (parentSkewer == null) return;

        // �����ϰ� ���ϴ� ũ��� ���� ����
        transform.localScale = skewerScale;

        // �������� ��� ���� - �̰� ��������!
    }
}
