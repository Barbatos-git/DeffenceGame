using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class TowerEffect : MonoBehaviour
{
    [Header("爆発エフェクトのプレハブ")]
    public GameObject explosionPrefab;

    [Header("火エフェクトのプレハブ")]
    public GameObject firePrefab;

    [Header("煙のプレハブ")]
    public GameObject smokePrefab;

    [Header("エフェクト生成位置")]
    public Transform explosionPoint;
    public List<Transform> firePoints = new List<Transform>();

    [Header("爆発後に火が発生するまでの時間（秒）")]
    public float fireDelay = 1.5f;

    [Header("火が地下から浮かび上がる距離")]
    public float fireRiseOffset = 1.5f;

    [Header("火が浮かび上がる時間（秒）")]
    public float riseDuration = 1.0f;

    [Header("火のフェードイン時間（秒）")]
    public float fadeDuration = 1.0f;

    private bool hasExploded = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 城の体力がゼロになった時に呼び出す
    public void TriggerDestruction()
    {
        if (hasExploded) return;
        hasExploded = true;

        // 爆発生成
        if (explosionPrefab != null && explosionPoint != null)
        {
            Instantiate(explosionPrefab, explosionPoint.position, explosionPrefab.transform.rotation, transform);
            GameOverManager.Instance.blackPanel.raycastTarget = true;
        }

        Instantiate(smokePrefab, explosionPoint.position, smokePrefab.transform.rotation, transform);
        // 火エフェクトを遅れて生成
        Invoke(nameof(SpawnFire), fireDelay);

        // さらに遅延してGameOverを表示（火が出現してから2秒後）
        Invoke(nameof(TriggerGameOver), fireDelay + 2f);
    }

    void SpawnFire()
    {
        foreach (Transform point in firePoints)
        {
            if (firePrefab != null && point != null)
            {
                // 地下から出現させる位置
                Vector3 startPos = point.position + Vector3.down * fireRiseOffset;

                // 火エフェクトを生成
                GameObject fire = Instantiate(firePrefab, startPos, firePrefab.transform.rotation, transform);

                // 対象の高さまで移動（ふわっと登場）
                fire.transform.DOMoveY(point.position.y, riseDuration).SetEase(Ease.OutSine);

                // マテリアルの透明度をフェードイン
                FadeInMaterialAlpha(fire);
            }
        }
    }

    void FadeInMaterialAlpha(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<ParticleSystemRenderer>();
        foreach (var r in renderers)
        {
            Material mat = r.material;
            if (mat.HasProperty("_Color"))
            {
                Color c = mat.color;
                c.a = 0;
                mat.color = c;
                mat.DOFade(1f, fadeDuration);
            }
        }
    }

    void TriggerGameOver()
    {
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.TriggerGameOver(0f); // 遅延なしですぐ表示
        }
    }
}
