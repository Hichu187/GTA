using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class URPToBuiltInWizard : EditorWindow
{
    [MenuItem("Tools/URP → Built-In Material Converter")]
    public static void Open() => GetWindow<URPToBuiltInWizard>("URP → Built-In Converter");

    private struct MatEntry
    {
        public Material Mat;
        public string   ShaderLabel;
        public bool     IsError;   // true = Hidden/InternalErrorShader (URP package removed)
    }

    private readonly List<MatEntry> _found = new();
    private Vector2 _scroll;
    private bool _scanned;

    private static readonly Color ErrorColor = new Color(1f, 0.4f, 0.4f);
    private static readonly Color URPColor   = new Color(1f, 0.85f, 0.4f);

    private void OnGUI()
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.HelpBox(
            "Scans for:\n• URP materials (Universal Render Pipeline/*)\n• Error/pink materials (shader missing after removing URP package)",
            MessageType.Info);
        EditorGUILayout.Space(4);

        if (GUILayout.Button("Scan All Materials", GUILayout.Height(32)))
            Scan();

        if (!_scanned) return;

        EditorGUILayout.Space(4);

        int errCount = 0, urpCount = 0;
        foreach (var e in _found) { if (e.IsError) errCount++; else urpCount++; }

        EditorGUILayout.LabelField(
            $"Found {_found.Count} problem material(s)  —  {errCount} ERROR  |  {urpCount} URP",
            EditorStyles.boldLabel);
        EditorGUILayout.Space(2);

        _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));
        foreach (var entry in _found)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var prevBg = GUI.backgroundColor;
                GUI.backgroundColor = entry.IsError ? ErrorColor : URPColor;
                EditorGUILayout.ObjectField(entry.Mat, typeof(Material), false, GUILayout.Width(200));
                GUI.backgroundColor = prevBg;
                EditorGUILayout.LabelField(entry.ShaderLabel, EditorStyles.miniLabel);
            }
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(6);

        using (new EditorGUI.DisabledGroupScope(_found.Count == 0))
        {
            if (GUILayout.Button($"Convert All ({_found.Count}) → Standard", GUILayout.Height(36)))
                ConvertAll();
        }
    }

    private void Scan()
    {
        _found.Clear();
        var guids = AssetDatabase.FindAssets("t:Material");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var mat  = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;

            if (mat.shader == null)
            {
                _found.Add(new MatEntry { Mat = mat, ShaderLabel = "(null shader)", IsError = true });
            }
            else if (mat.shader.name == "Hidden/InternalErrorShader")
            {
                _found.Add(new MatEntry { Mat = mat, ShaderLabel = "ERROR (missing shader)", IsError = true });
            }
            else if (mat.shader.name.StartsWith("Universal Render Pipeline"))
            {
                _found.Add(new MatEntry { Mat = mat, ShaderLabel = mat.shader.name, IsError = false });
            }
        }
        _scanned = true;
        Repaint();
    }

    private void ConvertAll()
    {
        int converted = 0;
        try
        {
            for (int i = 0; i < _found.Count; i++)
            {
                EditorUtility.DisplayProgressBar(
                    "Converting Materials",
                    _found[i].Mat.name,
                    (float)i / _found.Count);

                if (_found[i].IsError)
                {
                    if (ConvertErrorMaterial(_found[i].Mat)) converted++;
                }
                else
                {
                    if (ConvertURPMaterial(_found[i].Mat)) converted++;
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog(
            "Done",
            $"Converted {converted} of {_found.Count} materials to Built-In.",
            "OK");

        Scan();
    }

    // ── Error material (Hidden/InternalErrorShader) ───────────────────────────
    // Shader is gone so HasProperty() returns false for everything.
    // Use SerializedObject to read saved textures/colors directly from the asset.
    private static bool ConvertErrorMaterial(Material mat)
    {
        var so = new SerializedObject(mat);
        so.Update();

        Texture mainTex   = null;
        Color   mainColor = Color.white;
        float   metallic  = 0f;
        float   smoothness = 0.5f;

        ReadSavedTexture(so, "_BaseMap",  ref mainTex);
        ReadSavedTexture(so, "_MainTex",  ref mainTex);   // fallback if already Built-In props saved

        ReadSavedColor(so, "_BaseColor", ref mainColor);
        ReadSavedColor(so, "_Color",     ref mainColor);

        ReadSavedFloat(so, "_Metallic",    ref metallic);
        ReadSavedFloat(so, "_Smoothness",  ref smoothness);
        ReadSavedFloat(so, "_Glossiness",  ref smoothness);

        var standard = Shader.Find("Standard");
        if (standard == null) return false;

        Undo.RecordObject(mat, "Convert Error → Standard");
        mat.shader = standard;

        mat.SetColor("_Color", mainColor);
        if (mainTex != null) mat.SetTexture("_MainTex", mainTex);
        mat.SetFloat("_Metallic",   metallic);
        mat.SetFloat("_Glossiness", smoothness);
        SetBuiltInMode(mat, 0); // default Opaque

        EditorUtility.SetDirty(mat);
        return true;
    }

    // ── URP material (shader still present) ──────────────────────────────────
    private static bool ConvertURPMaterial(Material mat)
    {
        string urpName = mat.shader.name;

        Color   baseColor    = Get(mat, "_BaseColor",         Color.white);
        Texture baseTex      = mat.HasProperty("_BaseMap")      ? mat.GetTexture("_BaseMap")     : null;
        float   metallic     = Get(mat, "_Metallic",           0f);
        float   smoothness   = Get(mat, "_Smoothness",         0.5f);
        Texture normalMap    = mat.HasProperty("_BumpMap")      ? mat.GetTexture("_BumpMap")     : null;
        float   normalScale  = Get(mat, "_BumpScale",          1f);
        Texture emissionMap  = mat.HasProperty("_EmissionMap")  ? mat.GetTexture("_EmissionMap") : null;
        Color   emissionCol  = Get(mat, "_EmissionColor",      Color.black);
        int     surface      = (int)Get(mat, "_Surface",       0f);
        float   alphaClip    = Get(mat, "_AlphaClip",          0f);
        float   cutoff       = Get(mat, "_Cutoff",             0.5f);
        Texture occlusionMap = mat.HasProperty("_OcclusionMap") ? mat.GetTexture("_OcclusionMap"): null;
        float   occlusionStr = Get(mat, "_OcclusionStrength",  1f);

        Shader target;
        if      (urpName.Contains("Particles/Lit"))   target = Shader.Find("Particles/Standard Surface");
        else if (urpName.Contains("Particles/Unlit")) target = Shader.Find("Particles/Standard Unlit");
        else if (urpName.Contains("Terrain/Lit"))     target = Shader.Find("Nature/Terrain/Standard");
        else if (urpName.Contains("Unlit"))
            target = (baseTex != null) ? Shader.Find("Unlit/Texture") : Shader.Find("Unlit/Color");
        else
            target = Shader.Find("Standard");

        if (target == null)
        {
            Debug.LogWarning($"[URPConverter] No Built-In shader for '{urpName}' on '{mat.name}' — skipped.");
            return false;
        }

        Undo.RecordObject(mat, "Convert URP → Built-In");
        mat.shader = target;

        mat.SetColor("_Color", baseColor);
        if (baseTex != null) mat.SetTexture("_MainTex", baseTex);

        if (target.name == "Standard")
        {
            mat.SetFloat("_Metallic",   metallic);
            mat.SetFloat("_Glossiness", smoothness);

            if (normalMap != null)
            {
                mat.SetTexture("_BumpMap", normalMap);
                mat.SetFloat("_BumpScale", normalScale);
                mat.EnableKeyword("_NORMALMAP");
            }

            if (occlusionMap != null)
            {
                mat.SetTexture("_OcclusionMap", occlusionMap);
                mat.SetFloat("_OcclusionStrength", occlusionStr);
            }

            bool hasEmission = emissionMap != null || emissionCol != Color.black;
            if (hasEmission)
            {
                mat.SetColor("_EmissionColor", emissionCol);
                if (emissionMap != null) mat.SetTexture("_EmissionMap", emissionMap);
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            }

            if (alphaClip > 0.5f)        { SetBuiltInMode(mat, 1); mat.SetFloat("_Cutoff", cutoff); }
            else if (surface == 1)         SetBuiltInMode(mat, 3);
            else                           SetBuiltInMode(mat, 0);
        }

        EditorUtility.SetDirty(mat);
        return true;
    }

    // ── SerializedObject helpers for error materials ──────────────────────────
    private static void ReadSavedTexture(SerializedObject so, string propName, ref Texture result)
    {
        var arr = so.FindProperty("m_SavedProperties.m_TexEnvs");
        if (arr == null) return;
        for (int i = 0; i < arr.arraySize; i++)
        {
            var elem = arr.GetArrayElementAtIndex(i);
            if (elem.FindPropertyRelative("first").stringValue != propName) continue;
            var tex = elem.FindPropertyRelative("second.m_Texture")?.objectReferenceValue as Texture;
            if (tex != null) result = tex;
            return;
        }
    }

    private static void ReadSavedColor(SerializedObject so, string propName, ref Color result)
    {
        var arr = so.FindProperty("m_SavedProperties.m_Colors");
        if (arr == null) return;
        for (int i = 0; i < arr.arraySize; i++)
        {
            var elem = arr.GetArrayElementAtIndex(i);
            if (elem.FindPropertyRelative("first").stringValue != propName) continue;
            var c = elem.FindPropertyRelative("second");
            if (c != null) result = c.colorValue;
            return;
        }
    }

    private static void ReadSavedFloat(SerializedObject so, string propName, ref float result)
    {
        var arr = so.FindProperty("m_SavedProperties.m_Floats");
        if (arr == null) return;
        for (int i = 0; i < arr.arraySize; i++)
        {
            var elem = arr.GetArrayElementAtIndex(i);
            if (elem.FindPropertyRelative("first").stringValue != propName) continue;
            var f = elem.FindPropertyRelative("second");
            if (f != null) result = f.floatValue;
            return;
        }
    }

    // ── Built-In rendering mode ───────────────────────────────────────────────
    private static void SetBuiltInMode(Material mat, int mode)
    {
        mat.SetFloat("_Mode", mode);
        switch (mode)
        {
            case 0:
                mat.SetOverrideTag("RenderType", "Opaque");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = -1;
                break;
            case 1:
                mat.SetOverrideTag("RenderType", "TransparentCutout");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);
                mat.EnableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 2450;
                break;
            case 3:
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                break;
        }
    }

    private static Color Get(Material m, string prop, Color fallback)
        => m.HasProperty(prop) ? m.GetColor(prop) : fallback;

    private static float Get(Material m, string prop, float fallback)
        => m.HasProperty(prop) ? m.GetFloat(prop) : fallback;
}
