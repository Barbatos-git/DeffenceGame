using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GameOverManager : MonoBehaviour
{
    [Header("黒幕パネル（Image）")]
    public Image blackPanel;

    [Header("Game Over のテキスト（TextMeshPro）")]
    public TextMeshProUGUI gameOverText;

    [Header("フェードイン時間（秒）")]
    public float fadeDuration = 1.5f;

    [Header("GameOver後にEndSceneへ遷移するまでの時間")]
    public float gameOverToEndDelay = 4.0f;

    // シングルトンとして使用する
    public static GameOverManager Instance;

    private void Awake()
    {
        // シングルトンの初期化
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // ゲームオーバー演出をトリガーする（一定時間後に開始）
    public void TriggerGameOver(float delay)
    {
        // 遅延してからGameOver演出を呼び出す
        Invoke(nameof(PlayGameOverSequence), delay);
    }

    // フェードイン演出とGame Over表示
    private void PlayGameOverSequence()
    {
        if (blackPanel != null)
        {
            // 現在のアルファを0に設定（透明）
            Color panelColor = blackPanel.color;
            panelColor.a = 0;
            blackPanel.color = panelColor;

            // フェードイン（黒幕）
            blackPanel.DOFade(0.6f, fadeDuration).SetEase(Ease.InOutQuad);
        }

        if (gameOverText != null)
        {
            // 現在のアルファを0に設定（透明）
            Color textColor = gameOverText.color;
            textColor.a = 0;
            gameOverText.color = textColor;

            // フェードイン（文字）
            gameOverText.DOFade(1f, fadeDuration).SetEase(Ease.InOutQuad).SetDelay(fadeDuration * 0.5f);
        }

        // 一定時間後に EndScene へ遷移
        Invoke(nameof(GoToEndScene), gameOverToEndDelay);
    }

    void GoToEndScene()
    {
        if (SceneTransitionController.Instance != null)
        {
            SceneTransitionController.Instance.FadeToScene("EndScene");
        }
    }
}
