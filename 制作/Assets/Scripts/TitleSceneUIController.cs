using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;
using Unity.Burst.CompilerServices;

public class TitleSceneUIController : MonoBehaviour
{
    [Header("ゲームタイトル（TextまたはImage）")]
    public RectTransform titleText;

    [Header("スペースキー表示テキスト")]
    public TextMeshProUGUI pressText;

    [Header("タイトル落下位置")]
    public float dropTargetY = 100f;

    [Header("落下時間")]
    public float dropDuration = 1.2f;

    [Header("バウンド強度")]
    public float bouncePower = 1.5f;

    void Start()
    {
        if (titleText != null)
        {
            // 初期位置を画面外上に設定（現在位置を記録してから上に上げる）
            Vector3 startPos = titleText.anchoredPosition;
            startPos.y = Screen.height + 300f;
            titleText.anchoredPosition = startPos;
        }

        AnimateTitleDrop();
    }

    void AnimateTitleDrop()
    {
        if (titleText != null)
        {
            // DOTween で弾むように落下
            titleText.DOAnchorPosY(dropTargetY, dropDuration)
                     .SetEase(Ease.OutBounce)
                     .OnComplete(StartBlinkText);
        }
    }

    void StartBlinkText()
    {
        if (pressText != null)
        {
            pressText.DOFade(1f, 1f).OnComplete(() =>
            {
                // アルファ値をループでフェードイン/アウト
                pressText.DOFade(0f, 0.8f)
                              .SetLoops(-1, LoopType.Yoyo)
                              .SetEase(Ease.InOutSine);
            });
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Text のループを止める
            if (pressText != null)
            {
                pressText.DOKill();
            }
        }
    }
}
