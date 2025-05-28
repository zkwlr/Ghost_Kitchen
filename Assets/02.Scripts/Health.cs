using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [Header("최대 체력")]
    public int maxHealth = 100;

    [Header("초기 체력 (0~최대 체력, -1이면 maxHealth로 시작)")]
    public int initialHealth = -1;

    // 현재 체력 (외부에서 읽기만 가능)
    public int CurrentHealth { get; private set; }

    // 체력 변경 시 UI 갱신 등에서 구독할 수 있는 이벤트
    public event Action<int, int> OnHealthChanged;

    // 사망 시 호출되는 이벤트
    public event Action OnDie;

    void Awake()
    {
        // 초기 체력 설정
        CurrentHealth = (initialHealth >= 0 && initialHealth <= maxHealth)
            ? initialHealth
            : maxHealth;

        // (필요하다면) 초기 UI 갱신
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    /// <summary>
    /// amount만큼 체력을 깎습니다. 이미 사망 상태면 무시.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (CurrentHealth <= 0)
            return;  // 이미 죽었다면 무시

        // 데미지 적용
        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        // 사망 처리
        if (CurrentHealth == 0)
            Die();
    }

    void Die()
    {
        Debug.Log($"{gameObject.name}이(가) 사망했습니다.");
        OnDie?.Invoke();
        // (추가) Destroy(gameObject);
    }
}