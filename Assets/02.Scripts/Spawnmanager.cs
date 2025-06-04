using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("적 스폰 설정")]
    public GameObject[] enemyPrefabs;
    [Range(0f, 100f)]
    public float[] spawnChances; // enemyPrefabs와 1:1 대응

    public Transform[] spawnPoints;
    public float spawnInterval = 3f;

    [Header("디버그")]
    public bool showDebugMessages = true;

    private float timer = 0f;

    void Start()
    {
        // 배열 크기 맞추기
        ValidateArraySizes();
        // 확률 검증
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
            Debug.LogWarning("적 프리팹이나 스폰 포인트가 설정되지 않았습니다!");
            return;
        }

        // 확률 기반으로 적 선택
        GameObject selectedEnemy = SelectEnemyByChance();

        if (selectedEnemy != null)
        {
            // 랜덤 스폰 위치 선택
            int spawnIndex = Random.Range(0, spawnPoints.Length);
            Vector3 spawnPos = spawnPoints[spawnIndex].position;

            Instantiate(selectedEnemy, spawnPos, Quaternion.identity);

            if (showDebugMessages)
            {
                Debug.Log($"{selectedEnemy.name} 스폰됨 at {spawnPoints[spawnIndex].name}");
            }
        }
    }

    private GameObject SelectEnemyByChance()
    {
        // 전체 확률 합계 계산
        float totalChance = 0f;
        for (int i = 0; i < spawnChances.Length; i++)
        {
            totalChance += spawnChances[i];
        }

        if (totalChance <= 0f)
        {
            Debug.LogWarning("모든 적의 스폰 확률이 0입니다!");
            return null;
        }

        // 0부터 전체 확률 합계까지 랜덤 값 생성
        float randomValue = Random.Range(0f, totalChance);
        float currentChance = 0f;

        // 확률에 따라 적 선택
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            currentChance += spawnChances[i];
            if (randomValue <= currentChance)
            {
                return enemyPrefabs[i];
            }
        }

        // 혹시 모를 경우 첫 번째 적 반환
        return enemyPrefabs[0];
    }

    private void ValidateArraySizes()
    {
        // spawnChances 배열이 enemyPrefabs와 크기가 다르면 맞춰주기
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
                    newSpawnChances[i] = 10f; // 기본값
                }
            }

            spawnChances = newSpawnChances;

            if (showDebugMessages)
            {
                Debug.Log("SpawnChances 배열 크기를 EnemyPrefabs에 맞춰 조정했습니다.");
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
            Debug.Log($"전체 스폰 확률 합계: {totalChance}");

            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                if (i < spawnChances.Length)
                {
                    float percentage = (spawnChances[i] / totalChance) * 100f;
                    Debug.Log($"{enemyPrefabs[i].name}: {percentage:F1}% 확률 (가중치: {spawnChances[i]})");
                }
            }
        }
    }
}
