using System.Collections.Generic;
using UnityEngine;

public class SlimeSpawner : MonoBehaviour
{
    [System.Serializable]
    public class LevelSettings
    {
        public int slimeCount = 3;
        public int slimeHP = 30;
    }

    [Header("슬라임 프리팹")]
    public GameObject[] slimePrefabs;

    [Header("레벨별 설정")]
    public LevelSettings[] levels =
    {
        new LevelSettings { slimeCount = 3, slimeHP = 30 },
        new LevelSettings { slimeCount = 5, slimeHP = 40 },
        new LevelSettings { slimeCount = 8, slimeHP = 50 }
    };

    [Header("생성 영역")]
    public Vector2 areaSize = new Vector2(12f, 12f);
    public LayerMask groundLayer;

    [Header("안전 거리")]
    public Transform player;
    public float minPlayerDistance = 3f;
    public float minSlimeDistance = 1.5f;

    [Header("생성 높이")]
    public float spawnHeightOffset = 0f;

    [Header("생성 재시도 횟수")]
    public int maxAttemptsPerSlime = 100;

    [Header("보스 설정")]
    [Min(1)] public int bossHP = 300;
    [Min(1f)] public float bossScale = 3f;
    [Min(1)] public int bossAttackDamage = 25;

    private List<GameObject> spawnedSlimes =
        new List<GameObject>();

    // 레벨별 슬라임 생성
    public int SpawnLevel(int level)
    {
        return SpawnWave(level, false);
    }

    public SlimeHealth SpawnBoss()
    {
        if (SpawnWave(1, true) != 1)
            return null;
        return spawnedSlimes[0].GetComponent<SlimeHealth>();
    }

    private int SpawnWave(int level, bool boss)
    {
        ClearSlimes();

        if (levels == null ||
            level < 1 ||
            level > levels.Length)
        {
            Debug.LogError("레벨 설정 오류: " + level);
            return 0;
        }

        if (slimePrefabs == null ||
            slimePrefabs.Length == 0)
        {
            Debug.LogError("슬라임 프리팹이 없습니다.");
            return 0;
        }

        if (groundLayer.value == 0)
        {
            Debug.LogError("Ground Layer를 설정하세요.");
            return 0;
        }

        LevelSettings settings = boss
            ? new LevelSettings { slimeCount = 1, slimeHP = Mathf.Max(1, bossHP) }
            : levels[level - 1];

        int spawnCount = 0;

        List<Vector3> usedPositions =
            new List<Vector3>();

        for (int i = 0; i < settings.slimeCount; i++)
        {
            bool found = false;
            Vector3 spawnPosition = Vector3.zero;

            for (int attempt = 0;
                 attempt < maxAttemptsPerSlime;
                 attempt++)
            {
                float x = Random.Range(
                    -areaSize.x / 2f,
                    areaSize.x / 2f
                );

                float z = Random.Range(
                    -areaSize.y / 2f,
                    areaSize.y / 2f
                );

                Vector3 rayOrigin =
                    transform.position +
                    new Vector3(x, 50f, z);

                // 아래 방향으로 바닥 검사
                if (!Physics.Raycast(
                    rayOrigin,
                    Vector3.down,
                    out RaycastHit hit,
                    200f,
                    groundLayer,
                    QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                Vector3 candidate =
                    hit.point +
                    Vector3.up * spawnHeightOffset;

                // 플레이어와 거리 확인
                if (player != null)
                {
                    Vector3 distance =
                        candidate - player.position;

                    distance.y = 0f;

                    if (distance.magnitude <
                        minPlayerDistance)
                    {
                        continue;
                    }
                }

                // 다른 슬라임과 거리 확인
                bool tooClose = false;

                foreach (Vector3 pos in usedPositions)
                {
                    Vector3 distance = candidate - pos;
                    distance.y = 0f;

                    if (distance.magnitude <
                        minSlimeDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose)
                    continue;

                spawnPosition = candidate;
                found = true;
                break;
            }

            if (!found)
            {
                Debug.LogWarning(
                    "슬라임 생성 위치를 찾지 못했습니다."
                );
                continue;
            }

            // 슬라임 1, 2, 3 중 랜덤 선택
            GameObject prefab = slimePrefabs[
                Random.Range(0, slimePrefabs.Length)
            ];

            if (prefab == null)
            {
                Debug.LogWarning("빈 프리팹 슬롯이 있습니다.");
                continue;
            }

            GameObject slime = Instantiate(
                prefab,
                spawnPosition,
                Quaternion.identity
            );

            // 레벨별 체력 설정
            SlimeHealth health =
                slime.GetComponent<SlimeHealth>();

            if (health != null)
            {
                health.SetMaxHealth(settings.slimeHP);
                health.questManager = FindFirstObjectByType<QuestManager>();
            }
            else
            {
                Debug.LogWarning(
                    slime.name +
                    "에 SlimeHealth가 없습니다."
                );
                Destroy(slime);
                continue;
            }

            if (boss)
            {
                slime.name = "Slime Boss";
                slime.transform.localScale *= Mathf.Max(1f, bossScale);
                SlimeAI ai = slime.GetComponent<SlimeAI>();
                if (ai != null)
                {
                    ai.player = player;
                    ai.attackDamage = Mathf.Max(1, bossAttackDamage);
                    ai.detectionRange = Mathf.Max(ai.detectionRange, areaSize.magnitude);
                    ai.stopDistance = Mathf.Max(ai.stopDistance, 2f);
                }
            }

            spawnedSlimes.Add(slime);
            usedPositions.Add(spawnPosition);

            spawnCount++;
        }

        Debug.Log(
            "LEVEL " + level +
            " : 슬라임 " + spawnCount + "마리 생성"
        );

        return spawnCount;
    }

    // 이전 레벨 슬라임 정리
    public void ClearSlimes()
    {
        foreach (GameObject slime in spawnedSlimes)
        {
            if (slime != null)
            {
                Destroy(slime);
            }
        }

        spawnedSlimes.Clear();
    }

    // Scene 창에서 스폰 영역 표시
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(
                areaSize.x,
                0.1f,
                areaSize.y
            )
        );
    }
}
