#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.IO;

public class SceneImporterWindow : EditorWindow
{
    private GameObject selectedFBX;
    private Material terrainMaterial;
    private PhysicsMaterial terrainPhysicsMaterial; // NEW
    private string terrainTag;
    private int terrainLayer;
    private static string prefabFolderPath = "Assets/Prefabs";

    private const string PREF_FBX = "SceneImporter_LastFBX";
    private const string PREF_MATERIAL = "SceneImporter_LastMaterial";
    private const string PREF_PHYSICS = "SceneImporter_LastPhysics"; // NEW
    private const string PREF_TAG = "SceneImporter_LastTag";
    private const string PREF_LAYER = "SceneImporter_LastLayer";

    [MenuItem("Tools/FBX Importer Window")]
    public static void ShowWindow()
    {
        GetWindow<SceneImporterWindow>("FBX Importer");
    }

    private void OnEnable()
    {
        string fbxPath = EditorPrefs.GetString(PREF_FBX, "");
        if (!string.IsNullOrEmpty(fbxPath))
            selectedFBX = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);

        string materialPath = EditorPrefs.GetString(PREF_MATERIAL, "");
        if (!string.IsNullOrEmpty(materialPath))
            terrainMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

        string physicsPath = EditorPrefs.GetString(PREF_PHYSICS, "");
        if (!string.IsNullOrEmpty(physicsPath))
            terrainPhysicsMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(physicsPath);

        terrainTag = EditorPrefs.GetString(PREF_TAG, "Untagged");
        terrainLayer = EditorPrefs.GetInt(PREF_LAYER, 0);
    }

    private void OnGUI()
    {
        GUILayout.Label("FBX Import & Replace", EditorStyles.boldLabel);

        selectedFBX = (GameObject)EditorGUILayout.ObjectField("FBX Root Object", selectedFBX, typeof(GameObject), true);
        terrainMaterial = (Material)EditorGUILayout.ObjectField("Terrain Material", terrainMaterial, typeof(Material), false);

        // NEW: Physics Material field
        terrainPhysicsMaterial = (PhysicsMaterial)EditorGUILayout.ObjectField(
            "Terrain Physics Material",
            terrainPhysicsMaterial,
            typeof(PhysicsMaterial),
            false
        );

        terrainTag = EditorGUILayout.TagField("Terrain Tag", terrainTag);
        terrainLayer = EditorGUILayout.LayerField("Terrain Layer", terrainLayer);

        if (GUI.changed)
            SavePreferences();

        if (GUILayout.Button("Replace Objects"))
        {
            if (selectedFBX == null)
            {
                Debug.LogError("No FBX selected.");
                return;
            }

            ImportFBXAndReplace(selectedFBX, terrainMaterial, terrainPhysicsMaterial, terrainTag, terrainLayer);
        }
    }

    private void SavePreferences()
    {
        EditorPrefs.SetString(PREF_FBX, selectedFBX ? AssetDatabase.GetAssetPath(selectedFBX) : "");
        EditorPrefs.SetString(PREF_MATERIAL, terrainMaterial ? AssetDatabase.GetAssetPath(terrainMaterial) : "");
        EditorPrefs.SetString(PREF_PHYSICS, terrainPhysicsMaterial ? AssetDatabase.GetAssetPath(terrainPhysicsMaterial) : "");
        EditorPrefs.SetString(PREF_TAG, terrainTag);
        EditorPrefs.SetInt(PREF_LAYER, terrainLayer);
    }

    private static void ImportFBXAndReplace(
        GameObject selectedFBX,
        Material terrainMaterial,
        PhysicsMaterial terrainPhysicsMaterial,
        string terrainTag,
        int terrainLayer)
    {
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { prefabFolderPath });
        GameObject[] allPrefabs = new GameObject[prefabGUIDs.Length];

        Debug.Log($"--- Prefabs found in {prefabFolderPath} ---");

        for (int i = 0; i < prefabGUIDs.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGUIDs[i]);
            allPrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (allPrefabs[i] != null)
                Debug.Log($"Prefab: {allPrefabs[i].name} | Path: {path}");
        }

        Debug.Log("----------------------------------------");

        MeshFilter[] meshFilters = selectedFBX.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.sharedMesh == null)
                continue;

            string meshName = meshFilter.sharedMesh.name;

            if (meshName.ToLower().Contains("_terrain"))
            {
                MeshRenderer terrainRenderer = meshFilter.GetComponent<MeshRenderer>();
                if (terrainRenderer && terrainMaterial)
                    terrainRenderer.sharedMaterial = terrainMaterial;

                meshFilter.gameObject.isStatic = true;
                meshFilter.gameObject.tag = terrainTag;
                meshFilter.gameObject.layer = terrainLayer;

                MeshCollider collider = meshFilter.gameObject.GetComponent<MeshCollider>();
                if (!collider)
                    collider = meshFilter.gameObject.AddComponent<MeshCollider>();

                // NEW: Assign physics material
                if (terrainPhysicsMaterial)
                    collider.sharedMaterial = terrainPhysicsMaterial;

                continue;
            }

            GameObject bestMatchPrefab = FindClosestPrefab(allPrefabs, meshName);
            if (bestMatchPrefab != null)
            {
                Transform t = meshFilter.transform;
                Transform parent = t.parent;

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(bestMatchPrefab);
                instance.transform.SetPositionAndRotation(t.position, t.rotation);
                instance.transform.localScale = t.localScale;
                instance.transform.SetParent(parent, true);

                DestroyImmediate(t.gameObject);
            }
        }

        Debug.Log("FBX Import & Replace Completed!");
    }

    static GameObject FindClosestPrefab(GameObject[] allPrefabs, string meshName)
    {
        string baseName = GetBaseName(meshName);

        foreach (GameObject prefab in allPrefabs)
        {
            MeshFilter mf = prefab.GetComponentInChildren<MeshFilter>();
            if (mf && mf.sharedMesh && mf.sharedMesh.name == baseName)
                return prefab;
        }

        return null;
    }

    static string GetBaseName(string name)
    {
        int dot = name.LastIndexOf('.');
        if (dot > 0)
        {
            string suffix = name.Substring(dot + 1);
            if (suffix.Length == 3 && int.TryParse(suffix, out _))
                return name.Substring(0, dot);
        }
        return name;
    }
}

#endif
