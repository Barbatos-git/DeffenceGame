using UnityEngine;
using UnityEngine.AI;

public class UnitPlayer : MonoBehaviour
{
    [Header("プレイヤーの兵士Prefab")]
    public GameObject[] unitPrefabs; // 複数の兵士Prefabを格納
    private int selectedUnitIndex = -1; // 選択中の兵士（-1 = 未選択）

    [Header("プレースメントプレビュー（透明影）")]
    public GameObject[] previewPrefabs;
    private GameObject previewInstance;
    private Renderer[] previewRenderers;
    private Color validColor = new Color(1f, 1f, 1f, 0.3f);   // 白（配置可能）
    private Color invalidColor = new Color(1f, 0f, 0f, 0.3f); // 赤（配置不可）

    [Header("NavMeshに吸着させるか？")]
    public bool snapToNavMesh = true;

    [Header("Raycastに使用するレイヤー")]
    public LayerMask groundLayer;

    private UnitButtonController currentButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (selectedUnitIndex < 0) return; // 未選択なら処理しない

        // カーソル位置にプレビュー表示
        UpdatePreviewPosition();

        // 左クリックでユニットを配置
        if (Input.GetMouseButtonDown(0) && previewInstance != null && previewInstance.activeSelf)
        {
            Vector3 placePos = previewInstance.transform.position;

            // 有効なNavMesh上のみ配置
            if (NavMesh.SamplePosition(placePos, out NavMeshHit navHit, 1f, NavMesh.AllAreas))
            {
                GameObject unit = Instantiate(unitPrefabs[selectedUnitIndex], navHit.position, Quaternion.identity);
                LookAtCamera(unit);

                if (currentButton != null)
                {
                    currentButton.StartCooldown();
                }

                selectedUnitIndex = -1;
                Destroy(previewInstance);
                previewInstance = null;
                currentButton = null;
            }
        }

        // 右クリックでキャンセル
        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
        }
    }

    // UIボタンから呼ばれ、兵士の種類を選択
    public void SelectUnit(int index, UnitButtonController button)
    {
        if (index < 0 || index >= unitPrefabs.Length || button == null) return;
        if (button.IsCooling()) return;

        selectedUnitIndex = index;
        currentButton = button;

        // 既存のプレビューがあれば削除
        if (previewInstance != null)
            Destroy(previewInstance);

        // プレビュー生成
        if (previewPrefabs[index] != null)
        {
            previewInstance = Instantiate(previewPrefabs[index]);
            previewRenderers = previewInstance.GetComponentsInChildren<Renderer>();
            SetPreviewVisible(false);
        }
    }

    // 配置キャンセル
    void CancelPlacement()
    {
        selectedUnitIndex = -1;
        if (previewInstance != null)
        {
            Destroy(previewInstance);
            previewInstance = null;
        }
    }

    // カーソル位置にプレースメントプレビューを移動
    void UpdatePreviewPosition()
    {
        if (previewInstance == null) return;

        bool valid = false;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 placePos = hit.point;

            // NavMesh 吸着処理
            if (snapToNavMesh && NavMesh.SamplePosition(placePos, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            {
                placePos = navHit.position;
                valid = true;
            }

            previewInstance.transform.position = placePos;
            LookAtCamera(previewInstance);
            UpdatePreviewColor(valid);
            SetPreviewVisible(true);
        }
        else
        {
            SetPreviewVisible(false);
        }
    }

    // プレビューの可視状態を設定
    void SetPreviewVisible(bool visible)
    {
        if (previewInstance != null)
            previewInstance.SetActive(visible);
    }

    // 対象をカメラの方向に向ける
    void LookAtCamera(GameObject obj)
    {
        if (Camera.main == null) return;

        Vector3 camDir = Camera.main.transform.position - obj.transform.position;
        camDir.y = 0f; // Y軸を無視して水平回転
        if (camDir.sqrMagnitude > 0.01f)
        {
            obj.transform.rotation = Quaternion.LookRotation(camDir);
        }
    }

    // 配置可能かによって色を変更
    void UpdatePreviewColor(bool valid)
    {
        if (previewRenderers == null) return;

        Color targetColor = valid ? validColor: invalidColor;
        foreach (var r in previewRenderers)
        {
            if (r.material.HasProperty("_Color"))
                r.material.color = targetColor;
        }
    }
}
