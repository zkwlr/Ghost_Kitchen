using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlateController : MonoBehaviour
{
    [Header("Settings")]
    public Transform attachPoint; // 꼬치가 붙을 위치
    public float snapDistance = 0.3f; // 스냅 거리

    [Header("Skewer Detection")]
    public float detectionRange = 1.5f; // 꼬치 감지 범위 (스냅보다 넓게)
    public float checkInterval = 0.3f; // 감지 주기
    public bool hasSkewerNearby = false; // 근처에 꼬치가 있는지

    [Header("Debug")]
    public bool showDebugMessages = true;

    private SkewerController attachedSkewer;
    private List<SkewerController> nearbySkewers = new List<SkewerController>();
    private float checkTimer = 0f;

    void Start()
    {
        SetupCapsuleCollider();
        CreateAttachPoint();
    }

    private void SetupCapsuleCollider()
    {
        CapsuleCollider capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
        capsuleCollider.isTrigger = true;
        capsuleCollider.radius = 0.8f;
        capsuleCollider.height = 0.3f;
        capsuleCollider.center = new Vector3(0, 0.15f, 0);
    }

    private void CreateAttachPoint()
    {
        if (attachPoint == null)
        {
            GameObject point = new GameObject("AttachPoint");
            point.transform.SetParent(transform);
            point.transform.localPosition = Vector3.up * 0.1f;
            attachPoint = point.transform;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (attachedSkewer != null) return;

        SkewerController skewer = other.GetComponent<SkewerController>();
        if (skewer == null) return;

        XRGrabInteractable grab = skewer.GetComponent<XRGrabInteractable>();
        if (grab == null || grab.isSelected) return; // 잡고 있으면 무시

        if (Vector3.Distance(skewer.transform.position, attachPoint.position) <= snapDistance)
        {
            SnapSkewer(skewer);
        }
    }

    private void SnapSkewer(SkewerController skewer)
    {
        attachedSkewer = skewer;

        // 위치만 고정 (물리는 그대로)
        skewer.transform.position = attachPoint.position;
        skewer.transform.rotation = attachPoint.rotation;

        Debug.Log("Skewer snapped to plate");
    }

    // Update에서 위치 고정 + 잡으면 해제 체크
    void Update()
    {
        // 근처 꼬치 감지 (주기적으로)
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            DetectNearbySkewers();
        }

        // 기존 붙인 꼬치 관리
        if (attachedSkewer != null)
        {
            XRGrabInteractable grab = attachedSkewer.GetComponent<XRGrabInteractable>();

            // 꼬치를 잡으면 즉시 해제
            if (grab != null && grab.isSelected)
            {
                DetachSkewer();
                return;
            }

            // 잡지 않은 상태에서는 위치 강제 고정
            attachedSkewer.transform.position = attachPoint.position;
            attachedSkewer.transform.rotation = attachPoint.rotation;
        }
    }
    private void DetectNearbySkewers()
    {
        // 이전 리스트 클리어
        nearbySkewers.Clear();

        // Physics.OverlapSphere로 근처 꼬치 찾기
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, detectionRange);

        foreach (Collider collider in nearbyColliders)
        {
            SkewerController skewer = collider.GetComponent<SkewerController>();
            if (skewer != null)
            {
                nearbySkewers.Add(skewer);
            }
        }

        // 상태 업데이트
        bool previousState = hasSkewerNearby;
        hasSkewerNearby = nearbySkewers.Count > 0 || attachedSkewer != null; // 붙은 꼬치도 포함

        // 상태 변화 디버그
        if (showDebugMessages && previousState != hasSkewerNearby)
        {
            if (hasSkewerNearby)
            {
                Debug.Log($"접시 {gameObject.name}에 꼬치 감지됨 (근처: {nearbySkewers.Count}개, 붙음: {(attachedSkewer != null ? 1 : 0)}개)");
            }
            else
            {
                Debug.Log($"접시 {gameObject.name} 근처에 꼬치가 없음");
            }
        }
    }
    private void DetachSkewer()
    {
        if (attachedSkewer != null)
        {
            Debug.Log("Skewer grabbed and detached from plate");
            attachedSkewer = null;
            // 물리 설정은 건드리지 않음 - 원래 상태 그대로
        }
    }

    // 유령이 먹을 수 있는 꼬치 반환 (가장 가까운 것)
    public GameObject GetClosestSkewer()
    {
        // 붙어있는 꼬치가 있으면 우선 반환
        if (attachedSkewer != null)
        {
            return attachedSkewer.gameObject;
        }

        // 근처 꼬치 중 가장 가까운 것 반환
        if (nearbySkewers.Count == 0)
            return null;

        SkewerController closest = null;
        float closestDistance = float.MaxValue;

        foreach (SkewerController skewer in nearbySkewers)
        {
            if (skewer == null) continue; // 파괴된 오브젝트 체크

            float distance = Vector3.Distance(transform.position, skewer.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = skewer;
            }
        }

        return closest != null ? closest.gameObject : null;
    }
    public void RemoveSkewer(GameObject skewerObj)
    {
        SkewerController skewer = skewerObj.GetComponent<SkewerController>();
        if (skewer == null) return;

        // 붙어있는 꼬치인 경우
        if (attachedSkewer == skewer)
        {
            attachedSkewer = null;
            if (showDebugMessages)
            {
                Debug.Log("붙어있던 꼬치가 제거됨");
            }
        }

        // 근처 꼬치 리스트에서 제거
        if (nearbySkewers.Contains(skewer))
        {
            nearbySkewers.Remove(skewer);
            if (showDebugMessages)
            {
                Debug.Log($"근처 꼬치 제거됨. 남은 꼬치: {nearbySkewers.Count}개");
            }
        }

        // 상태 업데이트
        hasSkewerNearby = nearbySkewers.Count > 0 || attachedSkewer != null;
    }

    void OnDrawGizmosSelected()
    {
        if (attachPoint != null)
        {
            //스냅 범위(빨강/초록)
            Gizmos.color = attachedSkewer != null ? Color.red : Color.green;
            Gizmos.DrawWireSphere(attachPoint.position, snapDistance);

            //감지 범위(노랑/회색)
            Gizmos.color = hasSkewerNearby ? Color.yellow : Color.gray;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}
