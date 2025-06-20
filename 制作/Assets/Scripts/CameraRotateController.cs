using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraRotateController : MonoBehaviour
{
    [Header("カメラが回転する中心点")]
    public Transform target;         // 回転の中心（例：マップの中心、城など）

    [Header("回転速度（度/秒）")]
    public float rotationSpeed = 50f;

    [Header("自動回転するか")]
    public bool autoRotateInTitle = true;

    [Header("各Sceneの名前")]
    public string titleScene;
    public string endScene;

    private bool isAutoRotate = false;

    private void Start()
    {
        // ターゲットを常に注視する（オプション）
        transform.LookAt(target.position);

        // 現在のシーンが TitleScene/EndScene の場合は自動回転を有効にする
        string sceneName = SceneManager.GetActiveScene().name;
        if (autoRotateInTitle && (sceneName == titleScene || sceneName == endScene))
        {
            isAutoRotate = true;
        }
    }

    void Update()
    {
        if (target == null) return;

        float input = 0f;

        // 自動で左回転（反時計回り）
        if (isAutoRotate)
        {
            input = -1f;
            rotationSpeed = 10f;
        }
        else
        {
            if (Input.GetKey(KeyCode.Q)) input = -1f;
            if (Input.GetKey(KeyCode.E)) input = 1f;
        }

        if (input != 0f)
        {
            // target を中心に Y軸で回転
            transform.RotateAround(
                target.position,
                Vector3.up,
                input * rotationSpeed * Time.deltaTime
            );

            // ターゲットを常に注視する（オプション）
            transform.LookAt(target.position);
        }
    }
}
