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
    public GameObject[] normalEffects;

   [Header("���� ����")]
    public bool isAngry = false;

    [Header("���� ��ȭ")]
    public float angrySpeedMultiplier = 2f;    // ȭ���� �� �ӵ� ���
    public float angryDamageMultiplier = 1.5f; // ȭ���� �� ���ݷ� ���
    public float angryAttackSpeedMultiplier = 2f; // ���� �ӵ� ���

    [Header("�����")]
    public bool showDebugMessages = true;

    private GhostFollowAndAttack ghostAI;
    private float originalSpeed;
    private int originalDamage;
    private float originalAttackInterval;

    void Start()
    {
        ghostAI = GetComponent<GhostFollowAndAttack>();
        if (ghostAI != null)
        {
            originalSpeed = ghostAI.moveSpeed;
            originalDamage = ghostAI.damageAmount;
            originalAttackInterval = ghostAI.attackInterval;
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
    public List<string> GetIngredientsOnSkewer(GameObject skewer)
    {
        List<string> ingredients = new List<string>();

        // ��ġ�� ��� �ڽ� ������Ʈ�� Ȯ��
        foreach (Transform child in skewer.transform)
        {
            IngredientItem ingredientItem = child.GetComponent<IngredientItem>();
            if (ingredientItem != null)
            { 
                ingredients.Add(ingredientItem.ingredientType.ToString()); 
            }
        }

        return ingredients;
    }

    public void AnalyzeIngredientsAndReact(List<string> ingredients, GameObject skewer)
    {
        int favoriteCount = 0;
        int hatedCount = 0;
        int totalIngredients = ingredients.Count;

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
        else if (totalIngredients < 3)
        {
            // 2����: ��ᰡ 3�� �̸��̸� ȭ��
            BecomeAngry();
            CreateEffects(angryEffects);
            Destroy(skewer);

            if (showDebugMessages)
                Debug.Log($"������ ȭ�����ϴ�! (��ᰡ {totalIngredients}���� ������)");
        }
        else if (favoriteCount >= 1) // �����ϴ� ��ᰡ 1�� �̻��̸� ����
        {
            // �����ϸ� �����
            BecomeSatisfied();
            CreateEffects(satisfiedEffects);

            if (showDebugMessages)
                Debug.Log("������ �����Ͽ� ������ϴ�!");
            var gfa = gameObject.GetComponent<GhostFollowAndAttack>();
            int gainedScore = 1;
            Destroy(skewer);
            if (gameObject.CompareTag("LG"))
            {
                Destroy(gameObject); // LG_01
                if (gfa != null)
                {
                    // 유령이 파괴될 때 점수 추가
                    ScoreManager.Instance.AddScore(gainedScore);
                    Debug.Log($"[Destroyer] 충돌 대상 {gameObject.name} 파괴됨");
                    Debug.Log($"[ScoreManager] 점수 증가! 현재 점수: {ScoreManager.Instance.GetScore()}");
                }
                else
                {
                    Debug.Log("[Destroyer] 메인 메뉴 충돌");
                }

            }
        }
        else
        {
            // ����� ���� - �׳� �����
            if (showDebugMessages)
                Debug.Log("������ �������մϴ�.");

            Destroy(skewer);
            CreateEffects(normalEffects);
        }
    }

    private void BecomeAngry()
    {
        if (ghostAI != null)
        {
            isAngry = true;
            ghostAI.moveSpeed = originalSpeed * angrySpeedMultiplier;
            ghostAI.damageAmount = (int)(originalDamage * angryDamageMultiplier);
            ghostAI.attackInterval = originalAttackInterval / angryAttackSpeedMultiplier;
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.speed = angryAttackSpeedMultiplier;
            }

            if (showDebugMessages)
            {
                Debug.Log($"���� ����ȭ! �̵��ӵ�: {ghostAI.moveSpeed:F1}, ���ݷ�: {ghostAI.damageAmount:F1}, ���ݰ���: {ghostAI.attackInterval:F2}��");
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
