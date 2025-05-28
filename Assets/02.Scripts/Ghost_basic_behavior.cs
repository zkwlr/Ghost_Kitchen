using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFollowAndAttack : MonoBehaviour
{
    [Header("플레이어 Transform")]
    public Transform player;

    [Header("이동 속도")]
    public float moveSpeed = 3f;

    [Header("공격 범위")]
    public float attackRange = 1.5f; // 공격 시작 거리

    [Header("공격 간격 (초)")]
    public float attackInterval = 1f; // 연속 공격 사이 대기 시간

    private float attackTimer = 0f;
    private bool isAttacking = false;

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            // 플레이어를 향해 이동
            isAttacking = false;
            transform.LookAt(player);
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
        else
        {
            // 사정거리 내, 공격 모드로 전환
            isAttacking = true;
            AttackPlayer();
        }
    }

    void AttackPlayer()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            // 실제 공격 처리 (예: 데미지 적용)
            Debug.Log("플레이어 공격!");
            // player.GetComponent<PlayerHealth>().TakeDamage(공격력);
        }
    }
}