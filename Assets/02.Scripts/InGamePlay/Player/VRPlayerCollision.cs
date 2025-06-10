using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VRPlayerCollision : MonoBehaviour
{
    [Header("플레이어 충돌 설정")]
    public Transform xrOrigin;
    public Transform headTransform; // Main Camera
    public Transform floorReference; // 바닥 기준점
    public float playerRadius = 0.3f;
    
    private CapsuleCollider playerCollider;
    private Rigidbody playerRigidbody;
    private GameObject collisionObject;
    
    void Start()
    {
        SetupPlayerCollision();
    }
    
    void SetupPlayerCollision()
    {
        // XR Origin의 자식으로 충돌 감지용 오브젝트 생성
        collisionObject = new GameObject("PlayerCollision");
        collisionObject.transform.SetParent(xrOrigin);
        collisionObject.transform.localPosition = Vector3.zero;
        
        // Rigidbody 설정 (물리 충돌 활성화)
        playerRigidbody = collisionObject.AddComponent<Rigidbody>();
        playerRigidbody.isKinematic = false; // 물리 충돌 허용
        playerRigidbody.useGravity = false;
        playerRigidbody.freezeRotation = true; // 회전 고정
        playerRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        // Capsule Collider 설정
        playerCollider = collisionObject.AddComponent<CapsuleCollider>();
        playerCollider.radius = playerRadius;
        
        // 충돌 핸들러 추가
        VRCollisionHandler handler = collisionObject.AddComponent<VRCollisionHandler>();
        handler.headTransform = headTransform;
        handler.floorReference = floorReference;
        handler.xrOrigin = xrOrigin;
    }
}

public class VRCollisionHandler : MonoBehaviour
{
    [HideInInspector] public Transform headTransform;
    [HideInInspector] public Transform floorReference;
    [HideInInspector] public Transform xrOrigin;
    
    private CapsuleCollider capsuleCollider;
    private Rigidbody rb;
    private Vector3 lastValidPosition;
    
    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        lastValidPosition = xrOrigin.position;
    }
    
    void Update()
    {
        UpdateColliderPosition();
        UpdateLastValidPosition();
    }
    
    void UpdateColliderPosition()
    {
        if (headTransform == null || floorReference == null) return;
        
        // 머리와 바닥 사이의 높이 계산
        float height = headTransform.position.y - floorReference.position.y;
        height = Mathf.Max(height, 0.5f); // 최소 높이 보장
        
        // 콜라이더 높이와 위치 업데이트
        capsuleCollider.height = height;
        capsuleCollider.center = new Vector3(0, height / 2, 0);
        
        // 콜라이더를 머리 아래 위치로 이동
        transform.position = new Vector3(
            headTransform.position.x,
            floorReference.position.y,
            headTransform.position.z
        );
    }
    
    void UpdateLastValidPosition()
    {
        // 충돌하지 않을 때만 유효한 위치로 업데이트
        if (!IsCollidingWithWall())
        {
            lastValidPosition = xrOrigin.position;
        }
    }
    
    bool IsCollidingWithWall()
    {
        // 벽과의 충돌만 검사 (바닥 제외)
        Collider[] colliders = Physics.OverlapCapsule(
            transform.position + Vector3.up * 0.1f,
            transform.position + Vector3.up * (capsuleCollider.height - 0.1f),
            capsuleCollider.radius
        );
        
        foreach (Collider col in colliders)
        {
            if (col != capsuleCollider && col.CompareTag("Wall"))
            {
                return true;
            }
        }
        return false;
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // 벽과 충돌했을 때만 처리 (바닥은 무시)
        if (collision.gameObject.CompareTag("Wall"))
        {
            // 속도 멈추기
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            // XR Origin을 마지막 유효한 위치로 되돌리기
            xrOrigin.position = lastValidPosition;
            
            Debug.Log("벽에 부딪혔습니다. 이동이 제한됩니다.");
        }
    }
    
    void OnCollisionStay(Collision collision)
    {
        // 벽에 계속 닿아있을 때 움직임 방지
        if (collision.gameObject.CompareTag("Wall"))
        {
            rb.velocity = Vector3.zero;
        }
    }
}
