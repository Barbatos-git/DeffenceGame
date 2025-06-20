using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

// シーン遷移と黒幕フェードを統括して管理するコントローラー（現在のシーンによって自動制御）
public class SceneTransitionController : MonoBehaviour
{
    public static SceneTransitionController Instance;

    [Header("黒幕（UI Image）")]
    public Image blackOverlay;

    [Header("フェード時間（秒）")]
    public float fadeDuration = 1.2f;

    [Header("現在のシーンに応じたキー遷移を有効にする")]
    public bool allowKeySceneTransition = true;

    [Header("各Sceneの名前")]
    public string titleScene;
    public string gameScene;
    public string endScene;

    private string currentSceneName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;

        // シーン内の新しい黒幕を再取得
        blackOverlay = GameObject.Find("BlackOverlay")?.GetComponent<Image>();

        if (blackOverlay != null)
        {
            // シーン開始時に黒から透明にフェードイン
            blackOverlay.color = new Color(0, 0, 0, 1);
            blackOverlay.DOFade(0f, fadeDuration).SetEase(Ease.OutQuad);
        }
    }

    void Update()
    {
        if (!allowKeySceneTransition) return;

        // 空白キーで遷移（シーンごとに処理を分岐）
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentSceneName == titleScene)
            {
                GameStats.Reset();
                FadeToScene(gameScene);
            }
            else if (currentSceneName == endScene)
            {
                FadeToScene(titleScene);
            }
            // GameScene はキー遷移不可
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (currentSceneName == gameScene)
            {
                FadeToScene(endScene);
            }
        }
    }

    // 指定シーンへフェード付きで遷移
    public void FadeToScene(string sceneName)
    {
        if (blackOverlay == null) return;

        blackOverlay.DOFade(1f, fadeDuration).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            SceneManager.LoadScene(sceneName);
        });
    }

    // 遅延してシーン遷移（ゲームオーバーなど）
    public void FadeToSceneAfterDelay(string sceneName, float delaySeconds)
    {
        Invoke(nameof(DelayedSceneLoad), delaySeconds);
        delayedSceneName = sceneName;
    }

    private string delayedSceneName;

    private void DelayedSceneLoad()
    {
        FadeToScene(delayedSceneName);
    }
}
