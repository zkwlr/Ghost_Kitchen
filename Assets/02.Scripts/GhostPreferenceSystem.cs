using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostPreferenceSystem : MonoBehaviour
{
    [Header("���� ��ȣ�� ����")]
    public List<string> favoriteIngredients = new List<string>(); // �����ϴ� ���
    public List<string> hatedIngredients = new List<string>();    // �Ⱦ��ϴ� ���

    [Header("�ൿ ��ȭ")]
    public GameObject[] satisfiedEffects;  // �������� �� ����Ʈ
    public GameObject[] angryEffects;      // ȭ���� �� ����Ʈ

    [Header("���� ��ȭ")]
    public float angrySpeedMultiplier = 2f;    // ȭ���� �� �ӵ� ���
    public float angryDamageMultiplier = 1.5f; // ȭ���� �� ���ݷ� ���

    [Header("�����")]
    public bool showDebugMessages = true;

    private GhostFollowAndAttack ghostAI;
    private float originalSpeed;
    private int originalDamage;

    void Start()
    {
        ghostAI = GetComponent<GhostFollowAndAttack>();
        if (ghostAI != null)
        {
            originalSpeed = ghostAI.moveSpeed;
            originalDamage = ghostAI.damageAmount;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Skewer"))
            return;

        GameObject skewer = collision.gameObject;
        List<string> ingredients = GetIngredientsOnSkewer(skewer);

        if (showDebugMessages)
        {
            Debug.Log($"��ġ ���: {string.Join(", ", ingredients)}");
        }

        // ��� �м� �� �ൿ ����
        AnalyzeIngredientsAndReact(ingredients, skewer);
    }

    private List<string> GetIngredientsOnSkewer(GameObject skewer)
    {
        List<string> ingredients = new List<string>();

        // ��ġ�� ��� �ڽ� ������Ʈ�� Ȯ��
        foreach (Transform child in skewer.transform)
        {
            IngredientItem ingredientItem = child.GetComponent<IngredientItem>();
            if (ingredientItem != null)
            {
                ingredients.Add(ingredientItem.ingredientType.ToString());
                if (showDebugMessages)
                {
                    Debug.LogWarning($"{child.name}�� IngredientType�� �������� �ʾҽ��ϴ�.");
                }
            }
            else
            {
                // ������Ʈ�� ������ �±׳� �̸����� �Ǵ�
                ingredients.Add(child.name.Replace("(Clone)", "").Trim());
            }
        }

        return ingredients;
    }

    private void AnalyzeIngredientsAndReact(List<string> ingredients, GameObject skewer)
    {
        int favoriteCount = 0;
        int hatedCount = 0;

        // ��� �м�
        foreach (string ingredient in ingredients)
        {
            if (favoriteIngredients.Contains(ingredient))
                favoriteCount++;
            if (hatedIngredients.Contains(ingredient))
                hatedCount++;
        }

        if (showDebugMessages)
        {
            Debug.Log($"�����ϴ� ���: {favoriteCount}��, �Ⱦ��ϴ� ���: {hatedCount}��");
        }

        // �ൿ ����
        if (hatedCount > 0)
        {
            // �Ⱦ��ϴ� ��ᰡ ������ ȭ��
            BecomeAngry();
            CreateEffects(angryEffects);
            Destroy(skewer);

            if (showDebugMessages)
                Debug.Log("������ ȭ�����ϴ�!");
        }
        else if (favoriteCount >= 1) // �����ϴ� ��ᰡ 1�� �̻��̸� ����
        {
            // �����ϸ� �����
            BecomeSatisfied();
            CreateEffects(satisfiedEffects);

            if (showDebugMessages)
                Debug.Log("������ �����Ͽ� ������ϴ�!");

            Destroy(skewer);
            Destroy(gameObject);
        }
        else
        {
            // ����� ���� - ��ġ�� �ı�
            if (showDebugMessages)
                Debug.Log("������ �������մϴ�.");

            Destroy(skewer);
        }
    }

    private void BecomeAngry()
    {
        if (ghostAI != null)
        {
            ghostAI.moveSpeed = originalSpeed * angrySpeedMultiplier;
            ghostAI.damageAmount = (int)(originalDamage * angryDamageMultiplier);

            // ȭ�� ���� ǥ�� (��: ���� ����)
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.red;
            }
        }
    }

    private void BecomeSatisfied()
    {
        // ������ ������ �ð��� ȿ��
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.green;
        }
    }

    private void CreateEffects(GameObject[] effects)
    {
        if (effects != null && effects.Length > 0)
        {
            foreach (GameObject effect in effects)
            {
                if (effect != null)
                {
                    Instantiate(effect, transform.position, transform.rotation);
                }
            }
        }
    }
}
