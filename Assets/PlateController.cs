using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlateController : MonoBehaviour
{
    [Header("Settings")]
    public Transform attachPoint; // 꼬치가 붙을 위치
    public float snapDistance = 0.3f; // 스냅 거리

    private SkewerController attachedSkewer;

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

    private void DetachSkewer()
    {
        if (attachedSkewer != null)
        {
            Debug.Log("Skewer grabbed and detached from plate");
            attachedSkewer = null;
            // 물리 설정은 건드리지 않음 - 원래 상태 그대로
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.15f, 0.8f);

        if (attachPoint != null)
        {
            Gizmos.color = attachedSkewer != null ? Color.red : Color.green;
            Gizmos.DrawWireSphere(attachPoint.position, snapDistance);
        }
    }
}
