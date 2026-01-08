#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using PixelVanguard.UI;
using System.Collections.Generic;

namespace PixelVanguard.Editor
{
    /// <summary>
    /// Editor tool to automatically find text components and attach LocalizedText with correct keys.
    /// Menu: Tools → Localization → Auto-Assign Keys
    /// </summary>
    public class TextKeyReplacerWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<ReplaceInfo> foundComponents = new List<ReplaceInfo>();
        private int replaceCount = 0;

        private class ReplaceInfo
        {
            public TextMeshProUGUI Component;
            public string CurrentText;
            public string MatchedKey;
            public string MatchedEnglish;
            public bool WillReplace;
            public bool IsExactMatch;
        }

        // Dictionary of Text -> Key (loaded dynamically from Translations.asset)
        private static Dictionary<string, string> textToKey = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);

        [MenuItem("Tools/Localization/Auto-Assign Keys")]
        public static void ShowWindow()
        {
            // Load translations from asset before opening window
            LoadTranslationsFromAsset();
            
            var window = GetWindow<TextKeyReplacerWindow>("Auto-Assign Keys");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }
        
        /// <summary>
        /// Load all translations from Translations.asset and build text-to-key dictionary.
        /// This ensures the tool stays in sync with TranslationPopulator automatically.
        /// </summary>
        private static void LoadTranslationsFromAsset()
        {
            textToKey.Clear();
            
            // Load the Translations asset
            string assetPath = "Assets/Resources/Translations.asset";
            var translationData = AssetDatabase.LoadAssetAtPath<PixelVanguard.Data.TranslationData>(assetPath);

            if (translationData == null)
            {
                Debug.LogError($"[TextKeyReplacer] Translations.asset not found at {assetPath}");
                EditorUtility.DisplayDialog("Error", 
                    "Translations.asset not found!\n\nPlease run 'Tools → Localization → Populate Translations' first.", "OK");
                return;
            }
            
            // Use reflection to access the private strings list
            var stringsField = typeof(PixelVanguard.Data.TranslationData).GetField("strings", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (stringsField == null)
            {
                Debug.LogError("[TextKeyReplacer] Could not access strings field in TranslationData");
                return;
            }
            
            var strings = stringsField.GetValue(translationData) as System.Collections.Generic.List<PixelVanguard.Data.TranslationData.LocalizedString>;
            
            if (strings == null)
            {
                Debug.LogError("[TextKeyReplacer] Strings list is null");
                return;
            }
            
            // Build dictionary from English text -> Key
            int count = 0;
            foreach (var entry in strings)
            {
                if (!string.IsNullOrEmpty(entry.english) && !string.IsNullOrEmpty(entry.key))
                {
                    // Use English text as the lookup key (case-insensitive)
                    textToKey[entry.english] = entry.key;
                    count++;
                }
            }
            
            Debug.Log($"[TextKeyReplacer] ✅ Loaded {count} translation entries from Translations.asset");
        }

        private void OnGUI()
        {
            GUILayout.Label("Auto-Assign Localization Keys", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This tool finds TextMeshProUGUI components, matches their text to known English translations, " +
                "and automatically adds the LocalizedText component with the correct Key.",
                MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("1. Find Text Matches", GUILayout.Height(35)))
            {
                FindMatches();
            }

            GUILayout.Space(5);

            EditorGUI.BeginDisabledGroup(foundComponents.Count == 0);
            {
                if (GUILayout.Button($"2. Apply Localization Keys ({replaceCount} matches)", GUILayout.Height(35)))
                {
                    ApplyKeys();
                }
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(10);

            // List of found components
            if (foundComponents.Count > 0)
            {
                GUILayout.Label($"Found {foundComponents.Count} components ({replaceCount} matches):", EditorStyles.boldLabel);

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                {
                    foreach (var info in foundComponents)
                    {
                        if (info.Component == null) continue;

                        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                        {
                            // Status Icon
                            if (info.MatchedKey != null)
                                GUILayout.Label("✅", GUILayout.Width(25));
                            else
                                GUILayout.Label("⚠️", GUILayout.Width(25));

                            // Text Content
                            EditorGUILayout.BeginVertical();
                            {
                                EditorGUILayout.LabelField(info.Component.gameObject.name, EditorStyles.boldLabel);
                                EditorGUILayout.LabelField($"Text: \"{info.CurrentText}\"");
                                if (info.MatchedKey != null)
                                {
                                    GUI.color = Color.green;
                                    EditorGUILayout.LabelField($"Key: {info.MatchedKey}");
                                    GUI.color = Color.white;
                                }
                                else
                                {
                                    GUI.color = Color.gray;
                                    EditorGUILayout.LabelField("No match found");
                                    GUI.color = Color.white;
                                }
                            }
                            EditorGUILayout.EndVertical();

                            // Ping button
                            if (GUILayout.Button("Select", GUILayout.Width(60), GUILayout.Height(40)))
                            {
                                Selection.activeGameObject = info.Component.gameObject;
                                EditorGUIUtility.PingObject(info.Component.gameObject);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private void FindMatches()
        {
            foundComponents.Clear();
            replaceCount = 0;

            var allComponents = FindObjectsOfType<TextMeshProUGUI>(true);
            
            foreach (var tmp in allComponents)
            {
                var info = new ReplaceInfo
                {
                    Component = tmp,
                    CurrentText = tmp.text
                };

                // Remove existing LocalizedText if present? No, check if it's already set?
                // For now, let's just find matches for the text content
                
                string cleanText = tmp.text.Trim();
                
                // Try to clean up text that might have dynamic parts like "Remove Ads - 29 YAN" -> "Remove Ads - {0}"?
                // This is hard to automate perfectly. We will stick to exact matches from our dictionary first.
                
                if (textToKey.TryGetValue(cleanText, out string key))
                {
                    info.MatchedKey = key;
                    info.MatchedEnglish = cleanText;
                    replaceCount++;
                }
                
                foundComponents.Add(info);
            }
            
            // Sort so matches are at top
            foundComponents.Sort((a, b) => 
            {
                if (a.MatchedKey != null && b.MatchedKey == null) return -1;
                if (a.MatchedKey == null && b.MatchedKey != null) return 1;
                return 0;
            });

        }

        private void ApplyKeys()
        {
            if (foundComponents.Count == 0) return;

            int applied = 0;
            Undo.RecordObjects(foundComponents.ConvertAll(x => x.Component.gameObject).ToArray(), "Auto-Assign Localization Keys");

            foreach (var info in foundComponents)
            {
                if (info.Component == null || info.MatchedKey == null) continue;

                var go = info.Component.gameObject;
                var localizedText = go.GetComponent<LocalizedText>();

                if (localizedText == null)
                {
                    localizedText = Undo.AddComponent<LocalizedText>(go);
                }

                // Use SerializedObject to modify properties safely for Undo/Dirty
                SerializedObject so = new SerializedObject(localizedText);
                so.Update();
                SerializedProperty keyProp = so.FindProperty("translationKey");
                keyProp.stringValue = info.MatchedKey;
                so.ApplyModifiedProperties();

                applied++;
            }

            EditorUtility.DisplayDialog("Complete", $"Successfully assigned localization keys to {applied} text components!", "OK");
        }
    }
}
#endif
