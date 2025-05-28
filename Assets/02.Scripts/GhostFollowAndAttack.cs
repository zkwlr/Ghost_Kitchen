using UnityEngine;

public class GhostFollowAndAttack : MonoBehaviour
{
    [Header("공격 대상 Well의 Health 컴포넌트")]
    public Health targetHealth;

    [Header("이동 속도")]
    public float moveSpeed = 3f;

    [Header("공격 사정거리")]
    public float attackRange = 1.5f;

    [Header("공격 간격 (초)")]
    public float attackInterval = 1f;

    [Header("공격력")]
    public int damageAmount = 10;

    float attackTimer = 0f;
    void Awake()
    {
        // Inspector에서 안 넣어 줬다면 런타임에 찾아서 연결
        if (targetHealth == null)
        {
            // 태그 방식
            var wellGO = GameObject.FindGameObjectWithTag("Well");
            // 또는 이름 방식이라면
            // var wellGO = GameObject.Find("Well");
            if (wellGO != null)
                targetHealth = wellGO.GetComponent<Health>();
            else
                Debug.LogError("Well을 찾지 못했습니다! 태그나 이름을 확인하세요.");
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
            // 사정거리 밖 → 이동
            Vector3 dir = (targetT.position - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
            transform.LookAt(targetT);
            // 이동 중에는 공격 타이머가 누적되지 않게 초기화하고 싶으면 아래 한 줄 추가:
            // attackTimer = attackInterval;
        }
        else
        {
            // 사정거리 내 → 공격
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackInterval)
            {
                attackTimer = 0f;
                targetHealth.TakeDamage(damageAmount);
                Debug.Log($"{gameObject.name} attacked {targetHealth.gameObject.name} for {damageAmount}");
                // well 체력 출력
                Debug.Log($"{targetHealth.gameObject.name} Health: {targetHealth.CurrentHealth}");
            }
        }
    }
}