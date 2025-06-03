using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostColorController : MonoBehaviour
{
    [Header("���� ����")]
    public Color normalColor = Color.white;
    public Color angryColor = Color.red;

    [Header("�ִϸ��̼� ����")]
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
            // Base Map ���� ��������
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
            Debug.LogError("�θ� ������Ʈ���� GhostPreferenceSystem�� ã�� �� �����ϴ�!");
        }
    }

    void Update()
    {
        if (parentGhost == null || ghostRenderer == null) return;

        // �θ��� ���¿� ���� ��ǥ ���� ����
        if (parentGhost.isAngry)
        {
            targetColor = angryColor;
        }
        else
        {
            targetColor = normalColor;
        }

        // ���� Base Map ���� ��������
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

        // �ε巴�� ���� ����
        Color newColor = Color.Lerp(currentColor, targetColor, colorChangeSpeed * Time.deltaTime);

        // Base Map ���� ����
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
