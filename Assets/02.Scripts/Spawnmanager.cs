using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("�� ���� ����")]
    public GameObject[] enemyPrefabs;
    [Range(0f, 100f)]
    public float[] spawnChances; // enemyPrefabs�� 1:1 ����

    public Transform[] spawnPoints;
    public float spawnInterval = 3f;

    [Header("�����")]
    public bool showDebugMessages = true;

    private float timer = 0f;

    void Start()
    {
        // �迭 ũ�� ���߱�
        ValidateArraySizes();
        // Ȯ�� ����
        ValidateSpawnChances();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0)
        {
            Debug.LogWarning("�� �������̳� ���� ����Ʈ�� �������� �ʾҽ��ϴ�!");
            return;
        }

        // Ȯ�� ������� �� ����
        GameObject selectedEnemy = SelectEnemyByChance();

        if (selectedEnemy != null)
        {
            // ���� ���� ��ġ ����
            int spawnIndex = Random.Range(0, spawnPoints.Length);
            Vector3 spawnPos = spawnPoints[spawnIndex].position;

            Instantiate(selectedEnemy, spawnPos, Quaternion.identity);

            if (showDebugMessages)
            {
                Debug.Log($"{selectedEnemy.name} ������ at {spawnPoints[spawnIndex].name}");
            }
        }
    }

    private GameObject SelectEnemyByChance()
    {
        // ��ü Ȯ�� �հ� ���
        float totalChance = 0f;
        for (int i = 0; i < spawnChances.Length; i++)
        {
            totalChance += spawnChances[i];
        }

        if (totalChance <= 0f)
        {
            Debug.LogWarning("��� ���� ���� Ȯ���� 0�Դϴ�!");
            return null;
        }

        // 0���� ��ü Ȯ�� �հ���� ���� �� ����
        float randomValue = Random.Range(0f, totalChance);
        float currentChance = 0f;

        // Ȯ���� ���� �� ����
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            currentChance += spawnChances[i];
            if (randomValue <= currentChance)
            {
                return enemyPrefabs[i];
            }
        }

        // Ȥ�� �� ��� ù ��° �� ��ȯ
        return enemyPrefabs[0];
    }

    private void ValidateArraySizes()
    {
        // spawnChances �迭�� enemyPrefabs�� ũ�Ⱑ �ٸ��� �����ֱ�
        if (spawnChances.Length != enemyPrefabs.Length)
        {
            float[] newSpawnChances = new float[enemyPrefabs.Length];

            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                if (i < spawnChances.Length)
                {
                    newSpawnChances[i] = spawnChances[i];
                }
                else
                {
                    newSpawnChances[i] = 10f; // �⺻��
                }
            }

            spawnChances = newSpawnChances;

            if (showDebugMessages)
            {
                Debug.Log("SpawnChances �迭 ũ�⸦ EnemyPrefabs�� ���� �����߽��ϴ�.");
            }
        }
    }

    private void ValidateSpawnChances()
    {
        float totalChance = 0f;
        for (int i = 0; i < spawnChances.Length; i++)
        {
            totalChance += spawnChances[i];
        }

        if (showDebugMessages)
        {
            Debug.Log($"��ü ���� Ȯ�� �հ�: {totalChance}");

            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                if (i < spawnChances.Length)
                {
                    float percentage = (spawnChances[i] / totalChance) * 100f;
                    Debug.Log($"{enemyPrefabs[i].name}: {percentage:F1}% Ȯ�� (����ġ: {spawnChances[i]})");
                }
            }
        }
    }
}
