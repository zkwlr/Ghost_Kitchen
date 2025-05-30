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
    public Vector3 originalScale; // 원본 크기 저장
    public Vector3 skewerScale = new Vector3(0.1f, 0.1f, 0.1f); // 꼬치에 꽂힐 때 크기

    [Header("Skewer Settings")]
    public bool isOnSkewer = false;
    public SkewerController currentSkewer;
    public int startSlotIndex = -1;

    private Transform parentSkewer;
    private Vector3 lastParentScale;

    void Start()
    {
        // 원본 크기 저장
        if (originalScale == Vector3.zero)
        {
            originalScale = transform.localScale;
        }
    }

    void Update()
    {
        
    }

    // 꼬치에 붙었을 때 호출
    public void AttachToSkewer(SkewerController skewer, int slotIndex)
    {
        isOnSkewer = true;
        currentSkewer = skewer;
        startSlotIndex = slotIndex;
        parentSkewer = skewer.transform;

        // 원본 스케일 보존 (절대값으로 저장)
        if (originalScale == Vector3.zero)
        {
            originalScale = transform.localScale;
        }

        // 부모 설정
        transform.SetParent(skewer.transform);

        // 원하는 크기를 직접 설정 (역스케일 계산 없이)
        transform.localScale = skewerScale;

        Debug.Log($"{gameObject.name} attached with fixed scale: {skewerScale}");
    }


    // 꼬치에서 분리될 때 호출
    public void DetachFromSkewer()
    {
        isOnSkewer = false;
        currentSkewer = null;
        startSlotIndex = -1;
        parentSkewer = null;

        // 원본 크기로 복원
        transform.localScale = originalScale;

        Debug.Log($"{gameObject.name} detached, restored to original scale: {originalScale}");
    }

    // 꼬치에서의 크기 업데이트
    private void UpdateScaleOnSkewer()
    {
        if (parentSkewer == null) return;

        // 간단하게 원하는 크기로 직접 설정
        transform.localScale = skewerScale;

        // 역스케일 계산 제거 - 이게 문제였음!
    }
}
