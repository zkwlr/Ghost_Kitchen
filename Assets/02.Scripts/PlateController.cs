using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlateController : MonoBehaviour
{
    [Header("Settings")]
    public Transform attachPoint; // ��ġ�� ���� ��ġ
    public float snapDistance = 0.3f; // ���� �Ÿ�

    [Header("Skewer Detection")]
    public float detectionRange = 1.5f; // ��ġ ���� ���� (�������� �а�)
    public float checkInterval = 0.3f; // ���� �ֱ�
    public bool hasSkewerNearby = false; // ��ó�� ��ġ�� �ִ���

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
        if (grab == null || grab.isSelected) return; // ��� ������ ����

        if (Vector3.Distance(skewer.transform.position, attachPoint.position) <= snapDistance)
        {
            SnapSkewer(skewer);
        }
    }

    private void SnapSkewer(SkewerController skewer)
    {
        attachedSkewer = skewer;

        // ��ġ�� ���� (������ �״��)
        skewer.transform.position = attachPoint.position;
        skewer.transform.rotation = attachPoint.rotation;

        Debug.Log("Skewer snapped to plate");
    }

    // Update���� ��ġ ���� + ������ ���� üũ
    void Update()
    {
        // ��ó ��ġ ���� (�ֱ�������)
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            DetectNearbySkewers();
        }

        // ���� ���� ��ġ ����
        if (attachedSkewer != null)
        {
            XRGrabInteractable grab = attachedSkewer.GetComponent<XRGrabInteractable>();

            // ��ġ�� ������ ��� ����
            if (grab != null && grab.isSelected)
            {
                DetachSkewer();
                return;
            }

            // ���� ���� ���¿����� ��ġ ���� ����
            attachedSkewer.transform.position = attachPoint.position;
            attachedSkewer.transform.rotation = attachPoint.rotation;
        }
    }
    private void DetectNearbySkewers()
    {
        // ���� ����Ʈ Ŭ����
        nearbySkewers.Clear();

        // Physics.OverlapSphere�� ��ó ��ġ ã��
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, detectionRange);

        foreach (Collider collider in nearbyColliders)
        {
            SkewerController skewer = collider.GetComponent<SkewerController>();
            if (skewer != null)
            {
                nearbySkewers.Add(skewer);
            }
        }

        // ���� ������Ʈ
        bool previousState = hasSkewerNearby;
        hasSkewerNearby = nearbySkewers.Count > 0 || attachedSkewer != null; // ���� ��ġ�� ����

        // ���� ��ȭ �����
        if (showDebugMessages && previousState != hasSkewerNearby)
        {
            if (hasSkewerNearby)
            {
                Debug.Log($"���� {gameObject.name}�� ��ġ ������ (��ó: {nearbySkewers.Count}��, ����: {(attachedSkewer != null ? 1 : 0)}��)");
            }
            else
            {
                Debug.Log($"���� {gameObject.name} ��ó�� ��ġ�� ����");
            }
        }
    }
    private void DetachSkewer()
    {
        if (attachedSkewer != null)
        {
            Debug.Log("Skewer grabbed and detached from plate");
            attachedSkewer = null;
            // ���� ������ �ǵ帮�� ���� - ���� ���� �״��
        }
    }

    // ������ ���� �� �ִ� ��ġ ��ȯ (���� ����� ��)
    public GameObject GetClosestSkewer()
    {
        // �پ��ִ� ��ġ�� ������ �켱 ��ȯ
        if (attachedSkewer != null)
        {
            return attachedSkewer.gameObject;
        }

        // ��ó ��ġ �� ���� ����� �� ��ȯ
        if (nearbySkewers.Count == 0)
            return null;

        SkewerController closest = null;
        float closestDistance = float.MaxValue;

        foreach (SkewerController skewer in nearbySkewers)
        {
            if (skewer == null) continue; // �ı��� ������Ʈ üũ

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

        // �پ��ִ� ��ġ�� ���
        if (attachedSkewer == skewer)
        {
            attachedSkewer = null;
            if (showDebugMessages)
            {
                Debug.Log("�پ��ִ� ��ġ�� ���ŵ�");
            }
        }

        // ��ó ��ġ ����Ʈ���� ����
        if (nearbySkewers.Contains(skewer))
        {
            nearbySkewers.Remove(skewer);
            if (showDebugMessages)
            {
                Debug.Log($"��ó ��ġ ���ŵ�. ���� ��ġ: {nearbySkewers.Count}��");
            }
        }

        // ���� ������Ʈ
        hasSkewerNearby = nearbySkewers.Count > 0 || attachedSkewer != null;
    }

    void OnDrawGizmosSelected()
    {
        if (attachPoint != null)
        {
            //���� ����(����/�ʷ�)
            Gizmos.color = attachedSkewer != null ? Color.red : Color.green;
            Gizmos.DrawWireSphere(attachPoint.position, snapDistance);

            //���� ����(���/ȸ��)
            Gizmos.color = hasSkewerNearby ? Color.yellow : Color.gray;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}
