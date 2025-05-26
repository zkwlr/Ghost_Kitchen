using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFollowAndAttack : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 3f;
    public float attackRange = 1.5f; // ���� ���� �Ÿ�
    public float attackInterval = 1f; // ���� �ֱ�(��)
    private float attackTimer = 0f;

    private bool isAttacking = false;

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            // �̵�
            isAttacking = false;
            transform.LookAt(player);
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
        else
        {
            // ����
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
            // ���� ���� (����: ������ �ֱ�)
            Debug.Log("�÷��̾� ����!");
            // player.GetComponent<PlayerHealth>().TakeDamage(���ݷ�); �� ���� ���� ����
        }
    }
}
