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

    [Header("Skewer Settings")]
    public bool isOnSkewer = false;
    public SkewerController currentSkewer;
    public int startSlotIndex = -1;

    // 꼬치에 붙은 재료는 더 이상 복잡한 로직이 필요 없음
    // 모든 처리는 SkewerController에서 담당
}
