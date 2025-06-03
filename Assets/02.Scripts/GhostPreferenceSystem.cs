using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostPreferenceSystem : MonoBehaviour
{
    [Header("유령 선호도 설정")]
    public List<string> favoriteIngredients = new List<string>(); // 좋아하는 재료
    public List<string> hatedIngredients = new List<string>();    // 싫어하는 재료

    [Header("행동 변화")]
    public GameObject[] satisfiedEffects;  // 만족했을 때 이펙트
    public GameObject[] angryEffects;      // 화났을 때 이펙트

    [Header("스탯 변화")]
    public float angrySpeedMultiplier = 2f;    // 화났을 때 속도 배수
    public float angryDamageMultiplier = 1.5f; // 화났을 때 공격력 배수

    [Header("디버그")]
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
            Debug.Log($"꼬치 재료: {string.Join(", ", ingredients)}");
        }

        // 재료 분석 및 행동 결정
        AnalyzeIngredientsAndReact(ingredients, skewer);
    }

    private List<string> GetIngredientsOnSkewer(GameObject skewer)
    {
        List<string> ingredients = new List<string>();

        // 꼬치의 모든 자식 오브젝트를 확인
        foreach (Transform child in skewer.transform)
        {
            IngredientItem ingredientItem = child.GetComponent<IngredientItem>();
            if (ingredientItem != null)
            {
                ingredients.Add(ingredientItem.ingredientType.ToString());
                if (showDebugMessages)
                {
                    Debug.LogWarning($"{child.name}의 IngredientType이 설정되지 않았습니다.");
                }
            }
            else
            {
                // 컴포넌트가 없으면 태그나 이름으로 판단
                ingredients.Add(child.name.Replace("(Clone)", "").Trim());
            }
        }

        return ingredients;
    }

    private void AnalyzeIngredientsAndReact(List<string> ingredients, GameObject skewer)
    {
        int favoriteCount = 0;
        int hatedCount = 0;

        // 재료 분석
        foreach (string ingredient in ingredients)
        {
            if (favoriteIngredients.Contains(ingredient))
                favoriteCount++;
            if (hatedIngredients.Contains(ingredient))
                hatedCount++;
        }

        if (showDebugMessages)
        {
            Debug.Log($"좋아하는 재료: {favoriteCount}개, 싫어하는 재료: {hatedCount}개");
        }

        // 행동 결정
        if (hatedCount > 0)
        {
            // 싫어하는 재료가 있으면 화남
            BecomeAngry();
            CreateEffects(angryEffects);
            Destroy(skewer);

            if (showDebugMessages)
                Debug.Log("유령이 화났습니다!");
        }
        else if (favoriteCount >= 1) // 좋아하는 재료가 1개 이상이면 만족
        {
            // 만족하면 사라짐
            BecomeSatisfied();
            CreateEffects(satisfiedEffects);

            if (showDebugMessages)
                Debug.Log("유령이 만족하여 사라집니다!");

            Destroy(skewer);
            Destroy(gameObject);
        }
        else
        {
            // 평범한 반응 - 꼬치만 파괴
            if (showDebugMessages)
                Debug.Log("유령이 무관심합니다.");

            Destroy(skewer);
        }
    }

    private void BecomeAngry()
    {
        if (ghostAI != null)
        {
            ghostAI.moveSpeed = originalSpeed * angrySpeedMultiplier;
            ghostAI.damageAmount = (int)(originalDamage * angryDamageMultiplier);

            // 화난 상태 표시 (예: 색상 변경)
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.red;
            }
        }
    }

    private void BecomeSatisfied()
    {
        // 만족한 상태의 시각적 효과
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
