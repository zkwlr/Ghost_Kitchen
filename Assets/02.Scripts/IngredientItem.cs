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

    // ��ġ�� ���� ���� �� �̻� ������ ������ �ʿ� ����
    // ��� ó���� SkewerController���� ���
}
