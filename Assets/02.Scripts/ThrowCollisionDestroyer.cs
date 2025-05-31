using UnityEngine;

public class ThrowCollisionDestroyer : MonoBehaviour
{
    [Header("충돌 대상 태그 (LG_01)")]
    public string targetTag = "LG";

    [Header("폭발 이펙트 Prefabs (여러 개)")]
    public ParticleSystem[] explosionEffectPrefabs;

    [Header("필요한 부착 아이템 개수")]
    public int requiredItemCount = 3;

    // AttachPoint(들)만 있을 때의 자식 개수를 저장
    int initialChildCount;

    void Awake()
    {
        // 처음에 AttachPoint 자식만 몇 개 있는지 기록해 둡니다.
        initialChildCount = transform.childCount;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag(targetTag))
            return;

        // 붙어 있는 아이템 수 확인 (현재 자식 개수 - 최초 자식 개수)
        int attachedCount = transform.childCount - initialChildCount;

        if (attachedCount < requiredItemCount)
        {
            Debug.Log($"[Destroyer] 현재 부착된 아이템 수 {attachedCount}개 (필요: {requiredItemCount}개)");
            return;
        }

        // ─── 폭발 이펙트를 배열에 담긴 모든 프리팹으로 생성 ───
        for (int i = 0; i < explosionEffectPrefabs.Length; i++)
        {
            var prefab = explosionEffectPrefabs[i];
            if (prefab != null)
            {
                // 매번 같은 위치(transform.position)에 인스턴스화합니다.
                // 위치를 약간씩 어긋나게 만들고 싶다면, offsetVector를 계산해서 더해 주면 됩니다.
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        // ─── 대상과 자신을 파괴 ───
        Destroy(collision.gameObject); // LG_01
        Destroy(gameObject);           // skewer
    }
}