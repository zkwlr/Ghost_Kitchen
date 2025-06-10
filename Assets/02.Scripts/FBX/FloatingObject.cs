using UnityEngine;

/// <summary>
/// 이 컴포넌트를 붙인 GameObject가 Up/Down으로 부유(oscillate)하도록 합니다.
/// Inspector에서 amplitude(진폭), frequency(진동수), phaseOffset(위상 오프셋)을 설정할 수 있습니다.
/// </summary>
public class FloatingObject : MonoBehaviour
{
    [Header("부유 효과 설정")]
    [Tooltip("기본 위치로부터 상하로 이동할 최대 거리 (단위: 유니티 월드 좌표)")]
    public float amplitude = 0.2f;

    [Tooltip("부유 속도 (진동 주기 = 2π/frequency)")]
    public float frequency = 1f;

    [Tooltip("현재 오브젝트가 다른 오브젝트와 다른 위상을 가지게 할 오프셋(라디안 단위)")]
    public float phaseOffset = 0f;

    // Start() 시점에 초기 위치를 저장해 둡니다.
    private Vector3 initialPosition;

    void Awake()
    {
        // 최초 위치(스케일 및 회전 적용 전 월드 위치)를 저장
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        // 시간에 따라 sin 파동을 계산
        float newY = Mathf.Sin((Time.time * frequency) + phaseOffset) * amplitude;
        // 초기 위치에 Offset을 더해서 최종 위치를 세팅
        transform.localPosition = initialPosition + new Vector3(0f, newY, 0f);
    }

    // 편의를 위해, inspector에서 phaseOffset을 쉽게 0~2π 사이에서 랜덤으로 주도록 하는 버튼을 추가하고 싶으면
    // 아래 코드를 주석 해제하세요.
#if UNITY_EDITOR
    [ContextMenu("Randomize Phase")]
    private void RandomizePhase()
    {
        phaseOffset = Random.Range(0f, 2f * Mathf.PI);
    }
#endif
}