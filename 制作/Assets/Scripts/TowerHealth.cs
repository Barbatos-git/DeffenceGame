using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class TowerHealth : MonoBehaviour, IDamageable
{
    [Header("城の最大HP")]
    public int maxHP = 100;

    [Header("現在のHP")]
    public int currentHP;

    [Header("UI 血条の塗り部分（Image）")]
    public Image healthFill;

    [Header("アニメーション時間（秒）")]
    public float animateDuration = 0.5f;

    [Header("HPの数字表示")]
    public TMP_Text hpText;

    [SerializeField] TowerEffect towerEffect;
    private bool isDead = false;

    void Start()
    {
        // 現在HPを最大値に初期化
        currentHP = maxHP;

        // 血条の初期表示を満タンに設定
        if (healthFill != null)
        {
            healthFill.fillAmount = 1f;
        }

        if (hpText != null)
            hpText.text = $"{maxHP} / {currentHP}";
    }

    public bool IsDead() => isDead;

    // 敵からダメージを受ける関数
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        // HPを減少させ、0〜maxHPの範囲に制限
        currentHP -= amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        // 血条が設定されていれば、アニメーションで更新
        if (healthFill != null)
        {
            float targetFill = (float)currentHP / maxHP;

            // 既存のTweenを停止（重複防止）
            healthFill.DOKill();

            // fillAmount をアニメーションで滑らかに変化させる
            healthFill.DOFillAmount(targetFill, animateDuration).SetEase(Ease.OutQuad);
        }

        if (hpText != null)
            hpText.text = $"{maxHP} / {currentHP}";

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }

        Debug.Log($"城が攻撃を受けた！ 残りHP: {currentHP}");
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("城が破壊されました！");
        if (towerEffect != null)
        {
            towerEffect.TriggerDestruction();
        }
        // TODO: ゲームオーバー演出やシーン遷移
    }

    public float GetCurrentHealth()
    {
        return currentHP;
    }

    public float GetMaxHealth()
    {
        return maxHP;
    }
}
