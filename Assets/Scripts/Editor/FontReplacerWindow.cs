#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

namespace PixelVanguard.Editor
{
    /// <summary>
    /// Editor tool to batch-replace TextMeshPro fonts in the current scene.
    /// Useful for switching to Russian-compatible fonts.
    /// Menu: Tools → Localization → Font Replacer
    /// </summary>
    public class FontReplacerWindow : EditorWindow
    {
        private TMP_FontAsset oldFont;
        private TMP_FontAsset newFont;
        private Vector2 scrollPosition;
        private List<TextMeshProUGUI> foundComponents = new List<TextMeshProUGUI>();
        private int replaceCount = 0;

        [MenuItem("Tools/Localization/Font Replacer")]
        public static void ShowWindow()
        {
            var window = GetWindow<FontReplacerWindow>("Font Replacer");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("TextMeshPro Font Replacer", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This tool finds all TextMeshProUGUI components in the current scene and replaces fonts that match 'Old Font' with 'New Font'.",
                MessageType.Info);

            GUILayout.Space(10);

            // Font selection
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                oldFont = (TMP_FontAsset)EditorGUILayout.ObjectField(
                    "Old Font (to replace)", 
                    oldFont, 
                    typeof(TMP_FontAsset), 
                    false);

                newFont = (TMP_FontAsset)EditorGUILayout.ObjectField(
                    "New Font (replacement)", 
                    newFont, 
                    typeof(TMP_FontAsset), 
                    false);
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            // Buttons
            EditorGUI.BeginDisabledGroup(oldFont == null || newFont == null);
            {
                if (GUILayout.Button("1. Find All TextMeshPro Components", GUILayout.Height(30)))
                {
                    FindAllTextComponents();
                }

                GUILayout.Space(5);

                EditorGUI.BeginDisabledGroup(foundComponents.Count == 0);
                {
                    if (GUILayout.Button($"2. Replace Fonts ({foundComponents.Count} components)", GUILayout.Height(30)))
                    {
                        ReplaceFonts();
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(10);

            // Results
            if (replaceCount > 0)
            {
                EditorGUILayout.HelpBox(
                    $"✅ Successfully replaced {replaceCount} fonts!",
                    MessageType.Info);
            }

            // List of found components
            if (foundComponents.Count > 0)
            {
                GUILayout.Space(10);
                GUILayout.Label($"Found {foundComponents.Count} TextMeshProUGUI components:", EditorStyles.boldLabel);

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
                {
                    foreach (var tmp in foundComponents)
                    {
                        if (tmp == null) continue;

                        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                        {
                            // Component info
                            EditorGUILayout.LabelField(
                                $"{tmp.gameObject.name}",
                                GUILayout.Width(150));

                            // Current font
                            string fontName = tmp.font != null ? tmp.font.name : "None";
                            EditorGUILayout.LabelField(
                                $"Font: {fontName}",
                                GUILayout.Width(120));

                            // Will replace?
                            bool willReplace = tmp.font == oldFont;
                            GUI.color = willReplace ? Color.yellow : Color.green;
                            EditorGUILayout.LabelField(
                                willReplace ? "Will Replace" : "No Change",
                                GUILayout.Width(80));
                            GUI.color = Color.white;

                            // Ping button
                            if (GUILayout.Button("Select", GUILayout.Width(60)))
                            {
                                Selection.activeGameObject = tmp.gameObject;
                                EditorGUIUtility.PingObject(tmp.gameObject);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();
            }

            GUILayout.Space(10);

            // Instructions
            if (oldFont == null || newFont == null)
            {
                EditorGUILayout.HelpBox(
                    "⚠️ Please assign both Old Font and New Font to begin.",
                    MessageType.Warning);
            }
        }

        private void FindAllTextComponents()
        {
            foundComponents.Clear();
            replaceCount = 0;

            // Find all TextMeshProUGUI components in the scene
            var allComponents = FindObjectsOfType<TextMeshProUGUI>(true);
            foundComponents.AddRange(allComponents);

        }

        private void ReplaceFonts()
        {
            if (oldFont == null || newFont == null)
            {
                Debug.LogError("[FontReplacer] Both fonts must be assigned!");
                return;
            }

            replaceCount = 0;

            // Record undo for all changes
            Undo.RecordObjects(foundComponents.ToArray(), "Replace Fonts");

            foreach (var tmp in foundComponents)
            {
                if (tmp == null) continue;

                // Check if this component uses the old font
                if (tmp.font == oldFont)
                {
                    tmp.font = newFont;
                    replaceCount++;
                    EditorUtility.SetDirty(tmp);
                }
            }

            // Save changes
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            // Show confirmation dialog
            EditorUtility.DisplayDialog(
                "Font Replacement Complete",
                $"Successfully replaced {replaceCount} fonts from '{oldFont.name}' to '{newFont.name}'!",
                "OK");
        }
    }
}
#endif
