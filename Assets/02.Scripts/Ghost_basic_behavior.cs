using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFollowAndAttack : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 3f;
    public float attackRange = 1.5f; // 공격 시작 거리
    public float attackInterval = 1f; // 공격 주기(초)
    private float attackTimer = 0f;

    private bool isAttacking = false;

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            // 이동
            isAttacking = false;
            transform.LookAt(player);
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
        else
        {
            // 공격
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
            // 공격 로직 (예시: 데미지 주기)
            Debug.Log("플레이어 공격!");
            // player.GetComponent<PlayerHealth>().TakeDamage(공격력); 와 같이 구현 가능
        }
    }
}
