using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DayNightIconAnimator : MonoBehaviour
{
    [Header("回転の親オブジェクト (中心点)")]
    public RectTransform rotateRoot; // 回転の中心となる親

    [Header("UIアイコン")]
    public CanvasGroup sunGroup;
    public RectTransform sunIcon;

    public CanvasGroup moonGroup;
    public RectTransform moonIcon;

    [Header("アニメーション時間")]
    public float duration = 1f;
    public float radius = 100f; // 半径（上下アイコン距離の半分）

    private bool isNight = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 初期回転角度は0°（昼）
        rotateRoot.localEulerAngles = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetNight(bool night)
    {
        if (night == isNight) return;
        isNight = night;

        if (night)
        {
            // 太陽を上から下に回転退場
            sunGroup.DOFade(0f, duration);

            // 月を表示して回転登場
            moonGroup.alpha = 0f;
            moonGroup.gameObject.SetActive(true);
            moonGroup.DOFade(1f, duration);

            rotateRoot.DOLocalRotate(new Vector3(0, 0, 180f), duration, RotateMode.FastBeyond360).SetEase(Ease.InOutSine);
        }
        else
        {
            // 月を上から下に回転退場
            moonGroup.DOFade(0f, duration);
            GameStats.nightCount++;

            // 太陽を表示して回転登場
            sunGroup.alpha = 0f;
            sunGroup.gameObject.SetActive(true);
            sunGroup.DOFade(1f, duration);

            rotateRoot.DOLocalRotate(new Vector3(0, 0, 360f), duration, RotateMode.FastBeyond360).SetEase(Ease.InOutSine)
                .OnComplete(() => rotateRoot.localEulerAngles = Vector3.zero); // 回転角度を0に戻す
        }
    }
}
