using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("敵キャラのプレハブ")]
    public GameObject enemyPrefab1;
    public GameObject enemyPrefab2;

    [Header("ヘックスマップのルート")]
    public Transform hexRoot;

    [Header("ターゲット（例：城）")]
    public Transform target;

    [Header("出現設定")]
    public int spawnCount = 5;
    public float spawnInterval = 10f; // 秒ごとの出現間隔
    public int maxEnemyCount = 100; // 敵の最大数

    private float spawnTimer = 0f;

    [Header("DayNightCycle参照")]
    public DayNightCycle dayNightCycle;

    // ヘックスタイルのデータ（座標とタイプ）
    private Dictionary<(int q, int r), HexTileType> hexMap = new();
    private List<GameObject> activeEnemies = new();

    // 六角形の隣接方向（pointy-top基準）
    private static readonly (int dq, int dr)[] directions = new (int, int)[]
    {
        (1, 0), (1, -1), (0, -1),
        (-1, 0), (-1, 1), (0, 1)
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BuildHexMap();
        SpawnEnemiesOnGrassEdgeNearWater();
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval && activeEnemies.Count < maxEnemyCount)
        {
            spawnTimer = 0f;
            SpawnEnemiesOnGrassEdgeNearWater();
        }
        spawnInterval = IsNight() ? 5f : 10f;
        spawnCount = IsNight() ? 10 : 5;

        // 死亡・消滅した敵をリストから除外
        activeEnemies.RemoveAll(enemy => enemy == null);
    }

    // マップ上の全タイル情報を収集
    void BuildHexMap()
    {
        hexMap.Clear();
        foreach (Transform child in hexRoot)
        {
            HexTile tile = child.GetComponent<HexTile>();
            if (tile != null)
            {
                hexMap[(tile.q, tile.r)] = tile.type;
            }
        }
    }

    // 水タイルに隣接している草地タイルを探す
    List<(int q, int r)> GetGrassTilesNearWater()
    {
        List<(int, int)> spawnableTiles = new();

        foreach (var kvp in hexMap)
        {
            if (kvp.Value != HexTileType.Grass)
                continue;

            var (q, r) = kvp.Key;
            foreach (var (dq, dr) in directions)
            {
                var neighbor = (q + dq, r + dr);
                if (hexMap.ContainsKey(neighbor) && hexMap[neighbor] == HexTileType.Water)
                {
                    spawnableTiles.Add((q, r));
                    break; // 一つでも水に隣接していればOK
                }
            }
        }

        return spawnableTiles;
    }

    // 草地の外周（＝水に隣接）に敵を生成
    void SpawnEnemiesOnGrassEdgeNearWater()
    {
        GameObject enemy;
        List<(int, int)> grassEdgeTiles = GetGrassTilesNearWater();

        for (int i = 0; i < spawnCount; i++)
        {
            if (grassEdgeTiles.Count == 0) break;

            int index = Random.Range(0, grassEdgeTiles.Count);
            var (q, r) = grassEdgeTiles[index];
            Vector3 pos = HexToWorld(q, r);

            // NavMesh に吸着させる（地面にしっかり配置）
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(pos, out navHit, 2f, NavMesh.AllAreas))
            {
                pos = navHit.position;
            }
            else
            {
                Debug.LogWarning($"NavMesh 吸着失敗：({q}, {r})");
                grassEdgeTiles.RemoveAt(index);
                continue; // NavMeshがない場合はスキップ
            }

            if (IsNight())
            {
                enemy = Instantiate(enemyPrefab2, pos, Quaternion.identity);
            }
            else
            {
                enemy = Instantiate(enemyPrefab1, pos, Quaternion.identity);
                
            }
            activeEnemies.Add(enemy);

            var unitAttack = enemy.GetComponent<UnitAttack>();
            if (unitAttack != null)
            {
                unitAttack.fallbackTarget = target;
            }

            // NavMeshAgent を使用してターゲットへ自動移動
            if (target && enemy.TryGetComponent(out NavMeshAgent agent))
            {
                Vector3 randomOffset = Random.insideUnitCircle.normalized * 2f; // 半径2范围偏移
                Vector3 finalTarget = target.position + new Vector3(randomOffset.x, 0, randomOffset.y);
                agent.SetDestination(finalTarget);
            }


            grassEdgeTiles.RemoveAt(index); // 同じ場所に生成しないよう削除
        }
    }

    // 六角グリッド座標をワールド座標に変換（pointy-top）
    Vector3 HexToWorld(int q, int r)
    {
        float radius = 1f; // 半径、Prefabサイズに合わせて調整
        float x = radius * Mathf.Sqrt(3f) * (q + r / 2f);
        float z = radius * 1.5f * r;
        return new Vector3(x, 0, z);
    }

    bool IsNight()
    {
        float t = dayNightCycle.timeOfDay;
        return t <= 0.25f || t >= 0.75f;
    }
}
