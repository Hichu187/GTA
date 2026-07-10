// CityGeneratorProBuilder.cs
// Editor tool: sinh nhanh 1 khu thành phố demo bằng ProBuilder (building = block vuông đơn giản).
// YÊU CẦU: package "com.unity.probuilder" đã cài trong project.
// ĐẶT FILE NÀY TRONG 1 FOLDER TÊN "Editor" (VD: Assets/Editor/CityGeneratorProBuilder.cs)
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;

public class CityGeneratorWindow : EditorWindow
{
    // ---- Cấu hình grid ----
    private int gridRows = 6;
    private int gridCols = 6;
    private float cellSize = 10f;
    private float roadWidth = 4f;
    private float buildingPadding = 1f;

    // ---- Cấu hình building ----
    private float heightMin = 5f;
    private float heightMax = 30f;
    private float emptyCellChance = 0.15f; // xác suất ô trống (công viên / bãi đất)

    // ---- Misc ----
    private bool useSeed = true;
    private int seed = 12345;
    private string rootName = "GeneratedCity_Demo";

    [MenuItem("Tools/ProBuilder City Generator")]
    public static void ShowWindow()
    {
        GetWindow<CityGeneratorWindow>("City Generator");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Grid Thành Phố", EditorStyles.boldLabel);
        gridRows = EditorGUILayout.IntField("Số hàng (rows)", gridRows);
        gridCols = EditorGUILayout.IntField("Số cột (cols)", gridCols);
        cellSize = EditorGUILayout.FloatField("Kích thước ô (cell size)", cellSize);
        roadWidth = EditorGUILayout.FloatField("Độ rộng đường (road width)", roadWidth);
        buildingPadding = EditorGUILayout.FloatField("Padding building trong ô", buildingPadding);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Building", EditorStyles.boldLabel);
        heightMin = EditorGUILayout.FloatField("Chiều cao Min", heightMin);
        heightMax = EditorGUILayout.FloatField("Chiều cao Max", heightMax);
        emptyCellChance = EditorGUILayout.Slider("Tỉ lệ ô trống", emptyCellChance, 0f, 1f);

        EditorGUILayout.Space();
        useSeed = EditorGUILayout.Toggle("Dùng Seed cố định", useSeed);
        using (new EditorGUI.DisabledScope(!useSeed))
            seed = EditorGUILayout.IntField("Seed", seed);
        rootName = EditorGUILayout.TextField("Tên Root Object", rootName);

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate City", GUILayout.Height(30)))
            GenerateCity();

        if (GUILayout.Button("Clear City"))
            ClearCity();
    }

    private void ClearCity()
    {
        var existing = GameObject.Find(rootName);
        if (existing != null) DestroyImmediate(existing);
    }

    private void GenerateCity()
    {
        ClearCity();

        var root = new GameObject(rootName);
        Undo.RegisterCreatedObjectUndo(root, "Generate City");

        System.Random rng = useSeed ? new System.Random(seed) : new System.Random();

        float totalWidth = gridCols * (cellSize + roadWidth) + roadWidth;
        float totalDepth = gridRows * (cellSize + roadWidth) + roadWidth;

        CreateGround(root.transform, totalWidth, totalDepth);

        var buildingsParent = new GameObject("Buildings").transform;
        buildingsParent.SetParent(root.transform);

        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridCols; col++)
            {
                if (rng.NextDouble() < emptyCellChance)
                    continue; // để trống làm công viên / bãi đất trống

                float x = roadWidth + col * (cellSize + roadWidth);
                float z = roadWidth + row * (cellSize + roadWidth);

                float footprint = cellSize - buildingPadding * 2f;
                if (footprint <= 0.5f) footprint = cellSize * 0.6f;

                float height = Mathf.Lerp(heightMin, heightMax, (float)rng.NextDouble());

                Vector3 localPos = new Vector3(
                    x + cellSize * 0.5f - totalWidth * 0.5f,
                    0f,
                    z + cellSize * 0.5f - totalDepth * 0.5f);

                CreateBuilding(buildingsParent, localPos, new Vector3(footprint, height, footprint), row, col);
            }
        }

        Selection.activeGameObject = root;
        Debug.Log($"[CityGenerator] Đã sinh xong thành phố: {gridRows}x{gridCols} ô, kích thước {totalWidth}x{totalDepth}.");
    }

    private void CreateGround(Transform parent, float width, float depth)
    {
        ProBuilderMesh groundPb = ShapeGenerator.GenerateCube(PivotLocation.Center, new Vector3(width, 0.2f, depth));
        groundPb.gameObject.name = "Ground";
        groundPb.transform.SetParent(parent);
        groundPb.transform.localPosition = new Vector3(0f, -0.1f, 0f);
        FinalizeMesh(groundPb);

        var renderer = groundPb.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = GetOrCreateMaterial("City_Ground", new Color(0.35f, 0.35f, 0.35f));
    }

    private void CreateBuilding(Transform parent, Vector3 localPos, Vector3 size, int row, int col)
    {
        ProBuilderMesh buildingPb = ShapeGenerator.GenerateCube(PivotLocation.Center, size);
        buildingPb.gameObject.name = $"Building_{row}_{col}";
        buildingPb.transform.SetParent(parent);
        buildingPb.transform.localPosition = localPos + new Vector3(0f, size.y * 0.5f, 0f);
        FinalizeMesh(buildingPb);

        var renderer = buildingPb.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = GetOrCreateMaterial("City_Building", new Color(0.6f, 0.62f, 0.65f));
    }

    private void FinalizeMesh(ProBuilderMesh mesh)
    {
        mesh.ToMesh();
        mesh.Refresh();

        // Thêm MeshCollider để test va chạm / raycast ngay được
        if (mesh.GetComponent<MeshCollider>() == null)
        {
            var mc = mesh.gameObject.AddComponent<MeshCollider>();
            mc.sharedMesh = mesh.GetComponent<MeshFilter>().sharedMesh;
        }
    }

    // Cache material ra Assets để không tạo material mới liên tục mỗi lần Generate
    private Material GetOrCreateMaterial(string name, Color color)
    {
        string folder = "Assets/Editor/GeneratedCityMaterials";
        string path = $"{folder}/{name}.mat";

        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        if (!AssetDatabase.IsValidFolder("Assets/Editor"))
            AssetDatabase.CreateFolder("Assets", "Editor");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/Editor", "GeneratedCityMaterials");

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        var mat = new Material(shader) { color = color };
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }
}
#endif