using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostColorController : MonoBehaviour
{
    [Header("색상 설정")]
    public Color normalColor = Color.white;
    public Color angryColor = Color.red;

    [Header("애니메이션 설정")]
    public float colorChangeSpeed = 2f;

    private GhostPreferenceSystem parentGhost;
    private Renderer ghostRenderer;
    private Color targetColor;
    private Color originalColor;

    void Start()
    {
        parentGhost = GetComponentInParent<GhostPreferenceSystem>();
        ghostRenderer = GetComponent<Renderer>();

        if (ghostRenderer != null && ghostRenderer.material != null)
        {
            // Base Map 색상 가져오기
            if (ghostRenderer.material.HasProperty("_BaseColor"))
            {
                originalColor = ghostRenderer.material.GetColor("_BaseColor");
            }
            else if (ghostRenderer.material.HasProperty("_Color"))
            {
                originalColor = ghostRenderer.material.GetColor("_Color");
            }
            else
            {
                originalColor = ghostRenderer.material.color;
            }

            targetColor = originalColor;
        }

        if (parentGhost == null)
        {
            Debug.LogError("부모 오브젝트에서 GhostPreferenceSystem을 찾을 수 없습니다!");
        }
    }

    void Update()
    {
        if (parentGhost == null || ghostRenderer == null) return;

        // 부모의 상태에 따라 목표 색상 결정
        if (parentGhost.isAngry)
        {
            targetColor = angryColor;
        }
        else
        {
            targetColor = normalColor;
        }

        // 현재 Base Map 색상 가져오기
        Color currentColor;
        if (ghostRenderer.material.HasProperty("_BaseColor"))
        {
            currentColor = ghostRenderer.material.GetColor("_BaseColor");
        }
        else if (ghostRenderer.material.HasProperty("_Color"))
        {
            currentColor = ghostRenderer.material.GetColor("_Color");
        }
        else
        {
            currentColor = ghostRenderer.material.color;
        }

        // 부드럽게 색상 변경
        Color newColor = Color.Lerp(currentColor, targetColor, colorChangeSpeed * Time.deltaTime);

        // Base Map 색상 설정
        if (ghostRenderer.material.HasProperty("_BaseColor"))
        {
            ghostRenderer.material.SetColor("_BaseColor", newColor);
        }
        else if (ghostRenderer.material.HasProperty("_Color"))
        {
            ghostRenderer.material.SetColor("_Color", newColor);
        }
        else
        {
            ghostRenderer.material.color = newColor;
        }
    }
}
