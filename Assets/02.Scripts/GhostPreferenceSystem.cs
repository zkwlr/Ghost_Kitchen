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
    public GameObject[] normalEffects;

   [Header("분노 상태")]
    public bool isAngry = false;

    [Header("분노 변화")]
    public float angrySpeedMultiplier = 2f;    // 화났을 때 속도 배수
    public float angryDamageMultiplier = 1.5f; // 화났을 때 공격력 배수
    public float angryAttackSpeedMultiplier = 2f; // 공격 속도 배수

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
        // 싫어하는 음식이 없으면 satisfied 상태
        else if (hatedCount == 0)
        {
            BecomeSatisfied();
            CreateEffects(satisfiedEffects);

            if (showDebugMessages)
                Debug.Log("유령이 만족하여 사라집니다!");
            var gfa = gameObject.GetComponent<GhostFollowAndAttack>();
            int gainedScore = 1;

            if (gfa != null)
            {
                gainedScore = gfa.GetScoreValue();
            }

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
        else if (favoriteCount >= 1) // 좋아하는 재료가 1개 이상이면 만족
        {
            // 만족하면 사라짐
            BecomeSatisfied();
            CreateEffects(satisfiedEffects);

            if (showDebugMessages)
                Debug.Log("유령이 만족하여 사라집니다!");
            var gfa = gameObject.GetComponent<GhostFollowAndAttack>();
            int gainedScore = 1;

            if (gfa != null)
            {
                gainedScore = gfa.GetScoreValue();
            }

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
            // 중립적 반응 - 꼬치만 사라짐
            if (showDebugMessages)
                Debug.Log("꼬치만 사라집니다.");

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
