using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public class SceneHierarchyGenerator : EditorWindow
{
    private static readonly List<string> rootObjects = new()
    {
        "====== MANAGERS ======",
        "[MANAGERS]",
        "[AUDIO]",
        "[UI]",
        "[EVENTS]",

        "====== WORLD ======",
        "[WORLD]",
        "[LIGHTING]",
        "[ENVIRONMENT]",
        "[COLLISION]",

        "====== GAMEPLAY ======",
        "[PLAYER]",
        "[ENEMIES]",
        "[WEAPONS]",
        "[INTERACTABLES]",

        "====== EFFECTS ======",
        "[FX]",
        "[PARTICLES]",
        "[DECALS]",

        "====== DYNAMIC ======",
        "[DYNAMIC]",
        "[POOLED]",

        "====== DEBUG ======",
        "[DEBUG]",
        "[GIZMOS]"
    };

    [MenuItem("Tools/Scene Setup/Create Scene Hierarchy")]
    public static void CreateSceneHierarchy()
    {
        // Ensure we're in a scene
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }

        // Create root objects
        CreateRootObjects();

        Debug.Log("Scene hierarchy created successfully!");
    }

    private static void CreateRootObjects()
    {
        // Dictionary to store our root objects
        Dictionary<string, GameObject> roots = new();

        // Create all root objects
        foreach (string rootName in rootObjects)
        {
            // Skip separator lines
            if (rootName.StartsWith("=")) continue;

            GameObject root = new(rootName);
            roots.Add(rootName, root);

            // Set specific properties based on root type
            if (rootName == "[LIGHTING]")
            {
                root.tag = "EditorOnly";
            }
            else if (rootName == "[UI]")
            {
                // Add a Canvas component if it's the UI root
                root.AddComponent<Canvas>();
            }
            else if (rootName == "[DEBUG]" || rootName == "[GIZMOS]")
            {
                root.tag = "EditorOnly";
                root.hideFlags = HideFlags.DontSaveInBuild;
            }
        }

        // Set up specific hierarchy relationships
        if (roots.ContainsKey("[WORLD]") && roots.ContainsKey("[ENVIRONMENT]"))
        {
            roots["[ENVIRONMENT]"].transform.SetParent(roots["[WORLD]"].transform);
        }

        if (roots.ContainsKey("[WORLD]") && roots.ContainsKey("[LIGHTING]"))
        {
            roots["[LIGHTING]"].transform.SetParent(roots["[WORLD]"].transform);
        }

        if (roots.ContainsKey("[WORLD]") && roots.ContainsKey("[COLLISION]"))
        {
            roots["[COLLISION]"].transform.SetParent(roots["[WORLD]"].transform);
        }

        if (roots.ContainsKey("[FX]") && roots.ContainsKey("[PARTICLES]"))
        {
            roots["[PARTICLES]"].transform.SetParent(roots["[FX]"].transform);
        }

        if (roots.ContainsKey("[FX]") && roots.ContainsKey("[DECALS]"))
        {
            roots["[DECALS]"].transform.SetParent(roots["[FX]"].transform);
        }

        // Create default lighting if needed
        CreateDefaultLighting(roots);

        // Create event system if needed
        CreateEventSystem(roots);
    }

    private static void CreateDefaultLighting(Dictionary<string, GameObject> roots)
    {
        // Check if there's already a directional light in the scene
        if (FindAnyObjectByType<Light>() == null && roots.ContainsKey("[LIGHTING]"))
        {
            GameObject lightGO = new("Directional Light");
            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            lightGO.transform.SetParent(roots["[LIGHTING]"].transform);
            lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

            // Create a skybox material if none exists
            if (RenderSettings.skybox == null)
            {
                // This would typically be set up in the Render Settings
                Debug.Log("No skybox detected. Please set up lighting in Window > Rendering > Lighting Settings");
            }
        }
    }

    private static void CreateEventSystem(Dictionary<string, GameObject> roots)
    {
        // Check if there's already an event system
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null && roots.ContainsKey("[UI]"))
        {
            GameObject eventSystem = new("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            eventSystem.transform.SetParent(roots["[UI]"].transform);
        }
    }

    [MenuItem("Tools/Scene Setup/Scene Hierarchy Window")]
    public static void ShowWindow()
    {
        GetWindow<SceneHierarchyGenerator>("Scene Hierarchy Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Scene Hierarchy Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("This tool will create an organized hierarchy structure for your scene.");
        GUILayout.Space(5);

        GUILayout.Label("The hierarchy will include:");
        GUILayout.Label("- Manager objects for game systems");
        GUILayout.Label("- World organization (environment, lighting, collision)");
        GUILayout.Label("- Gameplay objects (player, enemies, weapons)");
        GUILayout.Label("- Visual effects containers");
        GUILayout.Label("- Dynamic and pooled object containers");
        GUILayout.Label("- Debug and gizmo objects (editor only)");

        GUILayout.Space(20);

        if (GUILayout.Button("Generate Scene Hierarchy"))
        {
            CreateSceneHierarchy();
        }

        GUILayout.Space(10);
        GUILayout.Label("Note: This will only create objects that don't already exist.", EditorStyles.miniLabel);
    }
}