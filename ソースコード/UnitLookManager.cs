using DG.Tweening;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UnitLookManager : MonoBehaviour
{
    private Transform cameraTransform;
    private GameObject[] units;

    private Animator anim;
    public string sceneName;
    public string tag;

    [Header("回転のスムーズ速度")]
    public float rotateSpeed = 5f;

    void Start()
    {
        // メインカメラの Transform を取得
        cameraTransform = Camera.main.transform;

        // "unit" タグを持つ全てのゲームオブジェクトを取得
        units = GameObject.FindGameObjectsWithTag(tag);

        if (sceneName == "TitleScene")
        {
            OnTitleScene();
        }
        else if (sceneName == "EndScene")
        {
            OnEndScene();
        }
    }

    void Update()
    {
        if (cameraTransform == null || units == null) return;

        foreach (GameObject unit in units)
        {
            if (unit == null) continue;

            // 向かうべき方向ベクトル（Y軸固定）
            Vector3 lookDir = cameraTransform.position - unit.transform.position;
            lookDir.y = 0f;

            if (lookDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                unit.transform.rotation = Quaternion.Slerp(
                    unit.transform.rotation,
                    targetRotation,
                    Time.deltaTime * rotateSpeed
                );
            }
        }
    }

    void OnTitleScene()
    {
        foreach (var unit in units)
        {
            anim = unit.GetComponent<Animator>();

            if (anim != null)
            {
                anim.SetTrigger("Cheer");
            }
        }
    }

    void OnEndScene()
    {
        foreach (var unit in units)
        {
            anim = unit.GetComponent<Animator>();

            if (anim != null)
            {
                anim.SetTrigger("Taunt");
            }
        }
    }
}
