using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingClock : MonoBehaviour
{
    [Header("회전 효과")]
    public float rotationSpeed = 90f;     // 회전 속도 (도/초)
    
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, - rotationSpeed * Time.deltaTime,0);
    }
}