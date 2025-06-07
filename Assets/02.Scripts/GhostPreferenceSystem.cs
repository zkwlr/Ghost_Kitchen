using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GhostPreferenceSystem : MonoBehaviour
{
    [Header("유령 선호도 설정")]
    public List<string> favoriteIngredients = new List<string>(); // 좋아하는 재료
    public List<string> hatedIngredients = new List<string>();    // 싫어하는 재료

    [Header("행동 변화")]
    public GameObject[] satisfiedEffects;  // 만족했을 때 이펙트
    public GameObject[] angryEffects;      // 화났을 때 이펙트
    public GameObject[] normalEffects;

    [Header("분노 상태")]
    public bool isAngry = false;

    [Header("분노 변화")]
    public float angrySpeedMultiplier = 2f;    // 화났을 때 속도 배수
    public float angryDamageMultiplier = 1.5f; // 화났을 때 공격력 배수
    public float angryAttackSpeedMultiplier = 2f; // 공격 속도 배수

    [Header("다양성 점수 시스템")]
    public int baseScore = 100;                    // 기본 점수
    public int diversityBonusPerType = 30;         // 재료 종류당 보너스

    [Header("디버깅")]
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
            Debug.Log($"꼬치 재료: {string.Join(", ", ingredients)}");
        }

        // 재료 분석 후 행동 결정
        AnalyzeIngredientsAndReact(ingredients, skewer);
    }

    public List<string> GetIngredientsOnSkewer(GameObject skewer)
    {
        List<string> ingredients = new List<string>();

        // 꼬치의 모든 자식 오브젝트를 확인
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
        
        // 재료 다양성 계산
        HashSet<string> uniqueIngredients = new HashSet<string>(ingredients);
        int diversityCount = uniqueIngredients.Count;

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
            Debug.Log($"총 재료: {totalIngredients}개, 재료 종류: {diversityCount}개");
            Debug.Log($"좋아하는 재료: {favoriteCount}개, 싫어하는 재료: {hatedCount}개");
        }

        // 행동 결정
        if (totalIngredients < 3)
        {
            // 조건: 재료가 3개 미만이면 화남
            BecomeAngry();
            CreateEffects(angryEffects);
            Destroy(skewer);

            if (showDebugMessages)
                Debug.Log($"유령이 화났습니다! (재료가 {totalIngredients}개로 부족함)");
        }
        else if (hatedCount > 0)
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
            // 만족하면 사라짐 + 다양성 보너스 점수 계산
            BecomeSatisfied();
            CreateEffects(satisfiedEffects);

            if (showDebugMessages)
                Debug.Log("유령이 만족하여 사라집니다!");

            var gfa = gameObject.GetComponent<GhostFollowAndAttack>();
            int finalScore = CalculateDiversityScore(ingredients, gfa);

            Destroy(skewer);

            if (gameObject.CompareTag("LG"))
            {
                Destroy(gameObject); // LG_01
                if (gfa != null)
                {
                    // ScoreManager 싱글톤을 통해 점수 추가
                    if (ScoreManager.Instance != null)
                    {
                        ScoreManager.Instance.AddScore(finalScore);
                        Debug.Log($"[ScoreManager] 다양성 보너스 적용! 획득 점수: {finalScore}, 현재 점수: {ScoreManager.Instance.GetScore()}");
                    }
                    else
                    {
                        Debug.LogWarning("[GhostPreferenceSystem] ScoreManager.Instance가 null입니다!");
                    }
                }
            }
        }
        else
        {
            // 중립적 반응 - 꼬치만 사라짐
            if (showDebugMessages)
                Debug.Log("꼬치만 사라집니다.");

            Destroy(skewer);
            CreateEffects(normalEffects);
        }
    }

    /// <summary>
    /// 간단한 재료 다양성에 따른 점수 계산
    /// </summary>
    private int CalculateDiversityScore(List<string> ingredients, GhostFollowAndAttack gfa)
    {
        int baseGhostScore = (gfa != null) ? gfa.GetScoreValue() : baseScore;
        
        // 재료 종류 다양성 계산
        HashSet<string> uniqueIngredients = new HashSet<string>(ingredients);
        int diversityCount = uniqueIngredients.Count;
        
        // 간단한 다양성 보너스 계산
        int diversityBonus = (diversityCount - 1) * diversityBonusPerType; // 첫 번째 재료는 기본
        
        int finalScore = baseGhostScore + diversityBonus;
        
        if (showDebugMessages)
        {
            Debug.Log($"=== 간단 점수 계산 ===");
            Debug.Log($"기본 점수: {baseGhostScore}");
            Debug.Log($"다양성 보너스: {diversityBonus} (재료 종류: {diversityCount}개)");
            Debug.Log($"최종 점수: {finalScore}");
        }
        
        return finalScore;
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
                Debug.Log($"분노 강화됨! 이동속도: {ghostAI.moveSpeed:F1}, 공격력: {ghostAI.damageAmount:F1}, 공격간격: {ghostAI.attackInterval:F2}초");
            }
        }
    }

    private void BecomeSatisfied()
    {
        
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
