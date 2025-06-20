using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class HexMapEditorWindow : EditorWindow
{
    public GameObject grassPrefab;
    public GameObject waterPrefab;
    public GameObject towerPrefab;
    public float hexRadius = 1f;
    public HexTileType selectedType = HexTileType.Grass;

    private GameObject previewInstance;
    private Transform root;
    private Dictionary<(int, int), GameObject> tileMap = new();

    [MenuItem("Tools/Hex Map Editor (Advanced)")]
    public static void Open()
    {
        GetWindow<HexMapEditorWindow>("Hex Map Editor");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        RebuildTileMap();
        ClearPreview();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        ClearPreview();
    }

    private void OnGUI()
    {
        GUILayout.Label("Tile Prefabs", EditorStyles.boldLabel);
        grassPrefab = (GameObject)EditorGUILayout.ObjectField("Grass Prefab", grassPrefab, typeof(GameObject), false);
        waterPrefab = (GameObject)EditorGUILayout.ObjectField("Water Prefab", waterPrefab, typeof(GameObject), false);
        towerPrefab = (GameObject)EditorGUILayout.ObjectField("Tower Prefab", towerPrefab, typeof(GameObject), false);
        selectedType = (HexTileType)EditorGUILayout.EnumPopup("Selected Type", selectedType);
        hexRadius = EditorGUILayout.FloatField("Hex Radius", hexRadius);
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        if (e == null || sceneView.camera == null) return;

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 worldPos = hit.point;
            (int q, int r) = WorldToHex(worldPos);
            Vector3 gridPos = HexToWorld(q, r);

            UpdatePreview(gridPos);

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                PlaceTile(q, r, gridPos);
                e.Use();
            }
            else if (e.type == EventType.MouseDown && e.button == 1)
            {
                RemoveTile(q, r);
                e.Use();
            }
        }
        else
        {
            ClearPreview();
        }
    }


    void PlaceTile(int q, int r, Vector3 pos)
    {
        if (!root) root = GameObject.Find("HexMapRoot")?.transform;
        if (!root)
        {
            GameObject go = new GameObject("HexMapRoot");
            root = go.transform;
        }

        if (tileMap.ContainsKey((q, r)))
        {
            DestroyImmediate(tileMap[(q, r)]);
        }

        GameObject prefab = selectedType switch
        {
            HexTileType.Grass => grassPrefab,
            HexTileType.Water => waterPrefab,
            HexTileType.Tower => towerPrefab,
            _ => null
        };

        if (prefab == null) return;

        GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        tile.transform.position = pos;
        tile.transform.SetParent(root);
        var hex = tile.GetComponent<HexTile>();
        if (!hex) hex = tile.AddComponent<HexTile>();
        hex.Init(q, r, selectedType);

        tileMap[(q, r)] = tile;
    }

    void RemoveTile(int q, int r)
    {
        if (tileMap.TryGetValue((q, r), out GameObject tile))
        {
            DestroyImmediate(tile);
            tileMap.Remove((q, r));
        }
    }

    void UpdatePreview(Vector3 pos)
    {
        if (!previewInstance)
        {
            previewInstance = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            previewInstance.name = "PreviewTile";
            previewInstance.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Transparent/Diffuse"));
            previewInstance.GetComponent<Collider>().enabled = false;
        }

        previewInstance.transform.position = pos + Vector3.up * 0.01f;
        previewInstance.transform.localScale = Vector3.one * hexRadius * 1.8f;
    }

    void ClearPreview()
    {
        if (previewInstance) DestroyImmediate(previewInstance);
    }

    (int, int) WorldToHex(Vector3 pos)
    {
        float q = (Mathf.Sqrt(3f) / 3f * pos.x - 1f / 3f * pos.z) / hexRadius;
        float r = (2f / 3f * pos.z) / hexRadius;
        return CubeRound(q, r);
    }

    (int, int) CubeRound(float qf, float rf)
    {
        float x = qf;
        float z = rf;
        float y = -x - z;

        int rx = Mathf.RoundToInt(x);
        int ry = Mathf.RoundToInt(y);
        int rz = Mathf.RoundToInt(z);

        float dx = Mathf.Abs(rx - x);
        float dy = Mathf.Abs(ry - y);
        float dz = Mathf.Abs(rz - z);

        if (dx > dy && dx > dz) rx = -ry - rz;
        else if (dy > dz) ry = -rx - rz;
        else rz = -rx - ry;

        return (rx, rz);
    }

    Vector3 HexToWorld(int q, int r)
    {
        float x = hexRadius * Mathf.Sqrt(3f) * (q + r / 2f);
        float z = hexRadius * 1.5f * r;
        return new Vector3(x, 0, z);
    }

    private void RebuildTileMap()
    {
        tileMap.Clear();

        root = GameObject.Find("HexMapRoot")?.transform;
        if (root == null) return;

        foreach (Transform child in root)
        {
            HexTile tile = child.GetComponent<HexTile>();
            if (tile != null)
            {
                var key = (tile.q, tile.r);
                if (!tileMap.ContainsKey(key))
                    tileMap[key] = child.gameObject;
            }
        }
    }
}
