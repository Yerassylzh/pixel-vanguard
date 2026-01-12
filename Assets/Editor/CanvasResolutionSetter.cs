using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace PixelVanguard.Editor
{
    public class CanvasResolutionSetter : EditorWindow
    {
        // Android Settings
        private Vector2 androidReferenceResolution = new Vector2(1080, 1920);
        private float androidMatchValue = 0.5f;

        // WebGL Settings
        private Vector2 webglReferenceResolution = new Vector2(1920, 1080);
        private float webglMatchValue = 0.5f;

        private BuildTarget selectedPlatform = BuildTarget.Android;

        [MenuItem("Tools/Canvas Resolution Setter")]
        public static void ShowWindow()
        {
            GetWindow<CanvasResolutionSetter>("Canvas Resolution Setter");
        }

        private void OnGUI()
        {
            GUILayout.Label("Canvas Resolution Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Platform Selection
            selectedPlatform = (BuildTarget)EditorGUILayout.EnumPopup("Target Platform", selectedPlatform);
            EditorGUILayout.Space();

            // Android Settings
            EditorGUILayout.LabelField("Android Settings", EditorStyles.boldLabel);
            androidReferenceResolution = EditorGUILayout.Vector2Field("Reference Resolution", androidReferenceResolution);
            androidMatchValue = EditorGUILayout.Slider("Match Width/Height", androidMatchValue, 0f, 1f);
            EditorGUILayout.HelpBox($"Width: {androidReferenceResolution.x}, Height: {androidReferenceResolution.y}\nMatch: {androidMatchValue} (0=Width, 1=Height)", MessageType.Info);
            EditorGUILayout.Space();

            // WebGL Settings
            EditorGUILayout.LabelField("WebGL Settings", EditorStyles.boldLabel);
            webglReferenceResolution = EditorGUILayout.Vector2Field("Reference Resolution", webglReferenceResolution);
            webglMatchValue = EditorGUILayout.Slider("Match Width/Height", webglMatchValue, 0f, 1f);
            EditorGUILayout.HelpBox($"Width: {webglReferenceResolution.x}, Height: {webglReferenceResolution.y}\nMatch: {webglMatchValue} (0=Width, 1=Height)", MessageType.Info);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("This will update all Canvas Scalers in all scenes in Build Settings.", MessageType.Warning);
            EditorGUILayout.Space();

            if (GUILayout.Button("Apply to Current Scene Only", GUILayout.Height(30)))
            {
                ApplyToCurrentScene();
            }

            if (GUILayout.Button("Apply to All Scenes in Build Settings", GUILayout.Height(30)))
            {
                ApplyToAllScenes();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Quick Apply - Android", GUILayout.Height(25)))
            {
                selectedPlatform = BuildTarget.Android;
                ApplyToAllScenes();
            }

            if (GUILayout.Button("Quick Apply - WebGL", GUILayout.Height(25)))
            {
                selectedPlatform = BuildTarget.WebGL;
                ApplyToAllScenes();
            }
        }

        private void ApplyToCurrentScene()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            int count = ProcessScene(currentScene);
            
            EditorSceneManager.MarkSceneDirty(currentScene);
            EditorSceneManager.SaveScene(currentScene);
            
            Debug.Log($"[CanvasResolutionSetter] Updated {count} Canvas Scaler(s) in scene '{currentScene.name}' for {selectedPlatform}");
            EditorUtility.DisplayDialog("Success", $"Updated {count} Canvas Scaler(s) in current scene for {selectedPlatform}", "OK");
        }

        private void ApplyToAllScenes()
        {
            if (!EditorUtility.DisplayDialog("Confirm", 
                $"This will modify all Canvas Scalers in all scenes for {selectedPlatform} platform. Continue?", 
                "Yes", "Cancel"))
            {
                return;
            }

            // Save current scene
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }

            string currentScenePath = SceneManager.GetActiveScene().path;
            List<string> processedScenes = new List<string>();
            int totalCanvasScalers = 0;

            // Get all scenes in build settings
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

            if (scenes.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "No scenes found in Build Settings!", "OK");
                return;
            }

            try
            {
                for (int i = 0; i < scenes.Length; i++)
                {
                    if (!scenes[i].enabled)
                        continue;

                    string scenePath = scenes[i].path;
                    EditorUtility.DisplayProgressBar("Processing Scenes", 
                        $"Processing {System.IO.Path.GetFileNameWithoutExtension(scenePath)}...", 
                        (float)i / scenes.Length);

                    // Open scene
                    Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    
                    // Process canvas scalers
                    int count = ProcessScene(scene);
                    totalCanvasScalers += count;

                    // Save scene
                    EditorSceneManager.SaveScene(scene);
                    processedScenes.Add(scene.name);

                    Debug.Log($"[CanvasResolutionSetter] {scene.name}: Updated {count} Canvas Scaler(s)");
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();

                // Restore original scene
                if (!string.IsNullOrEmpty(currentScenePath))
                {
                    EditorSceneManager.OpenScene(currentScenePath);
                }
            }

            string message = $"Successfully updated {totalCanvasScalers} Canvas Scaler(s) across {processedScenes.Count} scene(s) for {selectedPlatform}:\n\n";
            message += string.Join(", ", processedScenes);

            Debug.Log($"[CanvasResolutionSetter] Complete! {message}");
            EditorUtility.DisplayDialog("Success", message, "OK");
        }

        private int ProcessScene(Scene scene)
        {
            Vector2 targetResolution = selectedPlatform == BuildTarget.Android ? androidReferenceResolution : webglReferenceResolution;
            float targetMatch = selectedPlatform == BuildTarget.Android ? androidMatchValue : webglMatchValue;

            // Find all canvas scalers in the scene
            CanvasScaler[] canvasScalers = FindObjectsOfType<CanvasScaler>();
            int count = 0;

            foreach (CanvasScaler scaler in canvasScalers)
            {
                if (scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
                {
                    Undo.RecordObject(scaler, "Update Canvas Scaler Resolution");
                    
                    scaler.referenceResolution = targetResolution;
                    scaler.matchWidthOrHeight = targetMatch;
                    
                    EditorUtility.SetDirty(scaler);
                    count++;

                    Debug.Log($"[CanvasResolutionSetter] Updated '{scaler.gameObject.name}' - Resolution: {targetResolution}, Match: {targetMatch}");
                }
            }

            return count;
        }
    }
}
