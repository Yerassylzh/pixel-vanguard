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

        // Dictionary of Text -> Key
        // Derived from TranslationPopulator.cs
        private static readonly Dictionary<string, string> textToKey = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            // Main Menu
            {"PIXEL VANGUARD", "ui.mainmenu.title"},
            {"Play", "ui.mainmenu.play"},
            {"Shop", "ui.shop.title"}, // Favor generic Shop title or main menu? Assuming they are same string "Shop"
            {"Settings", "ui.settings.title"}, // Same string "Settings"
            {"Quit", "ui.mainmenu.quit"},

            // Settings
            {"Music", "ui.settings.music"},
            {"Sounds", "ui.settings.sounds"},
            {"Show Damage", "ui.settings.show_damage"},
            {"Show FPS", "ui.settings.show_fps"},
            {"Language", "ui.settings.language"},
            {"Apply", "ui.settings.apply"},
            {"Back", "ui.settings.back"},
            {"Contact Us", "ui.settings.contact_us"},
            {"Privacy Policy", "ui.settings.privacy_policy"},
            {"Pixel Vanguard v.0.1", "ui.settings.version"},
            {"Ads Removed ✓", "ui.settings.ads_removed"},

            // Shop
            {"Select upgrade to see details", "ui.shop.select_upgrade"},
            
            // Might
            {"MIGHT", "ui.shop.might.name"},
            {"Damage +10%", "ui.shop.might.short"},
            
            // Vitality
            {"VITALITY", "ui.shop.vitality.name"},
            {"Max HP +10", "ui.shop.vitality.short"},

            // Greaves
            {"GREAVES", "ui.shop.greaves.name"},
            {"Speed +5%", "ui.shop.greaves.short"},

            // Magnet
            {"MAGNET", "ui.shop.magnet.name"},
            {"Range +10%", "ui.shop.magnet.short"},

            // Ad Packs
            {"WATCH 5 ADS", "ui.shop.ad_pack.watch_5"},
            {"1990 Coins", "ui.shop.ad_pack.coins_5"},
            {"WATCH 10 ADS", "ui.shop.ad_pack.watch_10"},
            {"4990 Coins", "ui.shop.ad_pack.coins_10"},

            // Gold Pack
            {"SPECIAL OFFER", "ui.shop.gold_pack.title"},
            {"29900 Coins", "ui.shop.gold_pack.amount"},
            {"79 YAN", "ui.shop.gold_pack.price"},

            // Common
            {"Coins", "ui.common.coins"},
            {"Watch Ad", "ui.common.watch_ad"},
            {"LOCKED", "ui.common.locked"},
            {"Purchase to unlock", "ui.common.purchase_to_unlock"},

            // Character
            {"Character Selection", "ui.character.title"},
            {"Select", "ui.character.select"},
            {"Selected", "ui.character.selected"},
            {"Continue", "ui.character.continue"},
            {"Confirm", "ui.character.confirm"},

            // Names
            {"Knight", "ui.character.knight.name"},
            {"Pyromancer", "ui.character.pyromancer.name"},
            {"Ranger", "ui.character.ranger.name"},
            {"Santa", "ui.character.santa.name"},
            {"Zombie", "ui.character.zombie.name"},

            // Stats
            {"Weapon", "ui.character.stats.weapon"},
            {"Health", "ui.character.stats.health"},
            {"Speed", "ui.character.stats.speed"},
            {"Damage", "ui.character.stats.damage"},
            {"Base Health", "ui.character.stats.base_health"},
            {"Base Speed", "ui.character.stats.base_speed"},

            // Weapon Names
            {"Greatsword", "ui.weapon.greatsword"},
            {"HolyWater", "ui.weapon.holywater"},
            {"MagicOrbitals", "ui.weapon.magicorbitals"},
            {"AutoCrossbow", "ui.weapon.autocrossbow"},

            // HUD
            {"FPS", "ui.hud.fps"},
            {"LV", "ui.hud.level_short"},
            {"Level", "ui.hud.level_full"},
            {"Gold", "ui.hud.gold"},
            {"Time", "ui.hud.time"},

            // Game Over
            {"Game Over", "ui.gameover.title"},
            {"Revive (watch ad)", "ui.gameover.revive"},

            // Results
            {"Results", "ui.results.title"},
            {"VICTORY", "ui.results.victory"},
            {"DEFEATED", "ui.results.defeated"},
            {"SESSION SUMMARY", "ui.results.session_summary"},
            {"Time Survived", "ui.results.time_survived"},
            {"Enemies Killed", "ui.results.enemies_killed"},
            {"Level Reached", "ui.results.level_reached"},
            {"Gold Earned", "ui.results.gold_earned"},
            {"Main Menu", "ui.results.main_menu"},
            {"Resume", "ui.results.resume"}
        };

        [MenuItem("Tools/Localization/Auto-Assign Keys")]
        public static void ShowWindow()
        {
            var window = GetWindow<TextKeyReplacerWindow>("Auto-Assign Keys");
            window.minSize = new Vector2(500, 400);
            window.Show();
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
