using UnityEngine;
using TMPro;
using DG.Tweening;

public class EndSceneUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI killCountText;
    [SerializeField] private TextMeshProUGUI nightCountText;

    void Start()
    {
        // 設定テキスト内容
        killCountText.text = $"Defeated {GameStats.killCount} enemies!";
        nightCountText.text = $"Survived {GameStats.nightCount} nights!";

        // アニメーション表示
        AnimateTextBounce(killCountText);
        AnimateTextBounce(nightCountText);
    }

    // テキストを1回だけフェードインしてスケールでバウンド表示する
    void AnimateTextBounce(TextMeshProUGUI tmp)
    {
        tmp.alpha = 0f;
        tmp.transform.localScale = Vector3.zero;

        tmp.DOFade(0.8f, 1f); // 1秒でフェードイン
        tmp.transform.DOScale(Vector3.one, 0.6f)
            .SetEase(Ease.OutBack); // 弾むように拡大
    }
}
