using System.Collections.Generic;
using UnityEngine;

public class GhostFollowAndAttack : MonoBehaviour
{
    [Header("공격 대상 Well의 Health 컴포넌트")]
    public Health targetHealth;

    [Header("유령 Animator")]
    public Animator animator;        // Animator 컴포넌트 참조

    [Header("이동 속도")]
    public float moveSpeed = 3f;

    [Header("공격 사정거리")]
    public float attackRange = 1.5f;

    [Header("공격 간격 (초)")]
    public float attackInterval = 1f;

    [Header("공격력")]
    public int damageAmount = 10;


    [Header("이 유령이 파괴될 때 주어질 점수")]
    public int scoreValue = 1;      // ← 인스펙터에서 각 유령별로 조절 가능

    [Header("접시 음식 감지")]
    public float plateDetectionRange = 5f; // 접시 감지 범위
    public string plateTag = "Plate"; // 접시 태그
    public float plateCheckInterval = 1f; // 접시 확인 주기


    float attackTimer = 0f;
    float plateCheckTimer = 0f;
    private GhostPreferenceSystem preferenceSystem;


    void Awake()
    {
        // Target Health 자동 할당 (태그 기반)
        if (targetHealth == null)
        {
            var wellGO = GameObject.FindGameObjectWithTag("Well");
            if (wellGO != null)
                targetHealth = wellGO.GetComponent<Health>();
            else
                Debug.LogError("Well을 찾지 못했습니다! 태그나 이름을 확인하세요.");
        }

        // Animator 자동 할당 (Inspector 비워둘 수 있음)
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
                Debug.LogWarning("Animator 컴포넌트를 찾을 수 없습니다. 프리팹에 Animator 붙여주세요.");
        }

        // GhostPreferenceSystem 참조
        preferenceSystem = GetComponent<GhostPreferenceSystem>();
        if (preferenceSystem == null)
        {
            Debug.LogWarning("GhostPreferenceSystem 컴포넌트를 찾을 수 없습니다!");
        }
    }

    void Update()
    {

        if (targetHealth == null)
            return;

        Transform targetT = targetHealth.transform;
        float dist = Vector3.Distance(transform.position, targetT.position);

        if (dist > attackRange)
        {
            // 사정거리 밖: 이동만 수행, 타이머 초기화
            Vector3 dir = (targetT.position - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
            transform.LookAt(targetT);
            attackTimer = 0f;  // 재진입 시 즉시 공격하지 않으려면 초기화
        }
        else
        {
            // 접시 음식 감지 타이머
            plateCheckTimer += Time.deltaTime;
            if (plateCheckTimer >= plateCheckInterval)
            {
                plateCheckTimer = 0f;
                CheckForPlateFood();
            }

            // 사정거리 내: 공격 카운트
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                attackTimer = 0f;


                // 1) Attack 트리거 발동 → Attack 애니메이션 재생
                animator.SetTrigger("attackTrigger");

                // 2) Well에 데미지 적용
                targetHealth.TakeDamage(damageAmount);
                Debug.Log($"{gameObject.name} attacked {targetHealth.gameObject.name} for {damageAmount}");

            }
        }
    }

    /// <summary>
    /// 이 유령이 파괴(사망)되는 시점에 점수 값을 얻기 위한 접근자 메서드입니다.
    /// </summary>
    public int GetScoreValue()
    {
        return scoreValue;
    }

    private void CheckForPlateFood()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, plateDetectionRange);

        foreach (Collider collider in nearbyColliders)
        {
            if (collider.CompareTag(plateTag))
            {
                PlateController plateController = collider.GetComponent<PlateController>();

                if (plateController != null && plateController.hasSkewerNearby)
                {
                    GameObject skewer = plateController.GetClosestSkewer();

                    if (skewer != null)
                    {
                        EatSkewerFromPlate(skewer, plateController);
                        return;
                    }
                }
            }
        }
    }

    private void EatSkewerFromPlate(GameObject skewer, PlateController plateController)
    {
        Debug.Log($"{gameObject.name}이 접시에서 꼬치를 먹습니다!");

        // 재료 분석
        if (preferenceSystem != null)
        {
            List<string> ingredients = preferenceSystem.GetIngredientsOnSkewer(skewer);
            preferenceSystem.AnalyzeIngredientsAndReact(ingredients, skewer);
        }
        else
        {
            Destroy(skewer);
        }

        // 접시에서 꼬치 제거
        plateController.RemoveSkewer(skewer);
    }
    void OnDrawGizmosSelected()
    {
        // 접시 감지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, plateDetectionRange);
    }
}
