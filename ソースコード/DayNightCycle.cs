using UnityEngine;
using UnityEngine.Rendering;

public class DayNightCycle : MonoBehaviour
{
    [Header("太陽の設定")]
    public Light sunLight;
    public Gradient sunLightColor;
    public AnimationCurve lightIntensityCurve;
    public Gradient ambientColor;

    [Header("月光の設定")]
    public Light moonLight; // 月のライト
    public Gradient moonLightColor; // 月の光の色
    public AnimationCurve moonIntensityCurve; // 月の強さ（時間に応じて）

    [Header("時間設定")]
    [Tooltip("一日の長さ（秒）")]
    public float dayDuration = 60f;

    [Tooltip("当たり前时间（0-1）0は朝，0.5は昼，1は夜")]
    [Range(0f, 1f)]
    public float timeOfDay = 0.5f;

    private float timeSpeed;

    public DayNightIconAnimator iconAnimator;

    void Start()
    {
        // ディレクショナルライトが未設定の場合、シーンの太陽を取得
        if (!sunLight)
            sunLight = RenderSettings.sun;

        if (!moonLight)
            moonLight = GameObject.Find("MoonLight")?.GetComponent<Light>();

        // 一日の経過速度を計算（1秒あたりに進む割合）
        timeSpeed = 1f / dayDuration;
    }

    void Update()
    {
        // 現在の時間を更新（0.0〜1.0）
        timeOfDay += timeSpeed * Time.deltaTime;
        if (timeOfDay > 1f) timeOfDay -= 1f;

        // 太陽の回転角度を計算して適用
        float sunAngle = timeOfDay * 360f - 90f;
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        // 太陽光の色と強さを時間に応じて設定
        sunLight.color = sunLightColor.Evaluate(timeOfDay);
        sunLight.intensity = lightIntensityCurve.Evaluate(timeOfDay);

        // 環境光の色を時間に応じて変更
        RenderSettings.ambientLight = ambientColor.Evaluate(timeOfDay);

        // 月の角度は太陽と逆方向
        float moonAngle = sunAngle + 180f;
        moonLight.transform.rotation = Quaternion.Euler(moonAngle, 170f, 0f);

        // 月の色と強さを更新
        moonLight.color = moonLightColor.Evaluate(timeOfDay);
        moonLight.intensity = moonIntensityCurve.Evaluate(timeOfDay);

        bool nowNight = (timeOfDay <= 0.25f || timeOfDay >= 0.75f);
        iconAnimator?.SetNight(nowNight);
    }
}
