using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class ProjectFolderGenerator : EditorWindow
{
    private static readonly List<string> folderPaths = new()
    {
        // Core Project Structure
        "_Project/InputSystem",
        "_Project/Resources",
        "_Project/Settings",
        "_Project/Shaders/Graphs",
        "_Project/Shaders/Includes",
        
        // Art Assets
        "Art/Characters/Ares/Models",
        "Art/Characters/Ares/Materials",
        "Art/Characters/Ares/Animations/Clips",
        "Art/Characters/Ares/Animations/Controllers",
        "Art/Characters/Ares/Prefabs",
        "Art/Characters/Enemies/CRT/Models",
        "Art/Characters/Enemies/CRT/Materials",
        "Art/Characters/Enemies/CRT/Animations",
        "Art/Characters/Enemies/CRT/Prefabs",
        "Art/Characters/Enemies/PsySec/Models",
        "Art/Characters/Enemies/PsySec/Materials",
        "Art/Characters/Enemies/PsySec/Animations",
        "Art/Characters/Enemies/PsySec/Prefabs",
        "Art/Characters/Enemies/MiniBosses/OmniVault",
        "Art/Characters/Enemies/MiniBosses/SynthlifeCurator",
        "Art/Characters/Enemies/MiniBosses/LiquidityEnforcer",
        "Art/Characters/Enemies/MiniBosses/CognitiveDividend",
        "Art/Characters/Enemies/MiniBosses/DividendStalker",

        "Art/Environment/Materials",
        "Art/Environment/Models",
        "Art/Environment/Props",
        "Art/Environment/Lighting",
        "Art/Environment/Skyboxes",

        "Art/FX/Particles",
        "Art/FX/Materials",
        "Art/FX/Textures",
        "Art/FX/Prefabs",

        "Art/UI/Sprites",
        "Art/UI/Fonts",
        "Art/UI/Materials",
        "Art/UI/Prefabs",

        "Art/Audio/Music",
        "Art/Audio/SFX/Weapons",
        "Art/Audio/SFX/Movement",
        "Art/Audio/SFX/UI",
        "Art/Audio/SFX/Voice",
        "Art/Audio/Mixers",
        
        // Scripts
        "Scripts/Runtime/Actors/Player",
        "Scripts/Runtime/Actors/AI",
        "Scripts/Runtime/Core",
        "Scripts/Runtime/GameplaySystems/Movement",
        "Scripts/Runtime/GameplaySystems/Weapons",
        "Scripts/Runtime/GameplaySystems/Skills",
        "Scripts/Runtime/GameplaySystems/UI",
        "Scripts/Runtime/World",
        "Scripts/Editor/Editors",
        "Scripts/Editor/PropertyDrawers",
        "Scripts/Editor/Tools",
        
        // Scenes
        "Scenes/Core",
        "Scenes/Levels/LVL_Aegis_HQ",
        "Scenes/Levels/LVL_Prometheus_Excavation",
        "Scenes/Levels/LVL_Nexus_Plastics_Refinery",
        "Scenes/Levels/LVL_Veridian_AgriDome",
        "Scenes/Levels/LVL_OmniCorp_NeuroHub",
        "Scenes/Levels/LVL_Apex_Tower",
        
        // Prefabs
        "Prefabs/Actors",
        "Prefabs/Weapons",
        "Prefabs/Environment",
        "Prefabs/FX",
        "Prefabs/UI",
        
        // Data
        "Data/ScriptableObjects",
        "Data/Text",
        "Data/Animation",
        
        // Third Party (empty, but structure ready)
        "ThirdParty/Feel",
        "ThirdParty/OdinInspector",
        "ThirdParty/POLYGON"
    };

    [MenuItem("Tools/Project Setup/Create Folder Structure")]
    public static void GenerateFolderStructure()
    {
        try
        {
            // Create all directories
            foreach (string folderPath in folderPaths)
            {
                string fullPath = Path.Combine(Application.dataPath, folderPath);
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                    Debug.Log($"Created directory: {folderPath}");
                }
            }

            // Refresh the asset database to make sure Unity recognizes the new folders
            AssetDatabase.Refresh();

            Debug.Log("Folder structure created successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating folder structure: {e.Message}");
        }
    }

    [MenuItem("Tools/Project Setup/Folder Structure Window")]
    public static void ShowWindow()
    {
        GetWindow<ProjectFolderGenerator>("Folder Structure Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Project Folder Structure Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("This tool will create the recommended folder structure for your project.");
        GUILayout.Space(5);

        GUILayout.Label("The structure includes:");
        GUILayout.Label("- Organized art assets by category");
        GUILayout.Label("- Modular code architecture");
        GUILayout.Label("- Scene organization");
        GUILayout.Label("- Data management folders");
        GUILayout.Label("- Third-party isolation");

        GUILayout.Space(20);

        if (GUILayout.Button("Generate Folder Structure"))
        {
            GenerateFolderStructure();
        }

        GUILayout.Space(10);
        GUILayout.Label("Note: This will only create folders that don't already exist.", EditorStyles.miniLabel);
    }
}