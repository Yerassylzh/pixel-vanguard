#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using PixelVanguard.Data;

namespace PixelVanguard.Editor
{
    /// <summary>
    /// Editor utility to populate Translations.asset with all game translations.
    /// Menu: Tools → Localization → Populate Translations
    /// </summary>
    public static class TranslationPopulator
    {
        [MenuItem("Tools/Localization/Populate Translations")]
        public static void PopulateTranslations()
        {
            // Load the Translations asset
            string assetPath = "Assets/Resources/Translations.asset";
            TranslationData data = AssetDatabase.LoadAssetAtPath<TranslationData>(assetPath);

            if (data == null)
            {
                Debug.LogError($"[TranslationPopulator] Translations.asset not found at {assetPath}");
                Debug.LogError("[TranslationPopulator] Please create it via: Create → Localization → Translation Data");
                return;
            }

            // Use reflection to access private field
            var stringsField = typeof(TranslationData).GetField("strings", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var strings = stringsField.GetValue(data) as System.Collections.Generic.List<TranslationData.LocalizedString>;
            
            if (strings == null)
            {
                Debug.LogError("[TranslationPopulator] Could not access strings field");
                return;
            }

            // Clear existing entries
            strings.Clear();
            
            // Main Menu
            AddTranslation(strings, "ui.mainmenu.play", "Play", "Играть");
            AddTranslation(strings, "ui.mainmenu.shop", "Shop", "Магазин");
            AddTranslation(strings, "ui.mainmenu.settings", "Settings", "Настройки");
            AddTranslation(strings, "ui.mainmenu.quit", "Quit", "Выход");

            // Settings
            AddTranslation(strings, "ui.settings.title", "Settings", "Настройки");
            AddTranslation(strings, "ui.settings.music", "Music", "Музыка");
            AddTranslation(strings, "ui.settings.sounds", "Sounds", "Звуки");
            AddTranslation(strings, "ui.settings.show_damage", "Show Damage", "Показать урон");
            AddTranslation(strings, "ui.settings.show_fps", "Show FPS", "Показать ФПС");
            AddTranslation(strings, "ui.settings.language", "Language", "Язык");
            AddTranslation(strings, "ui.settings.apply", "Apply", "Применить");
            AddTranslation(strings, "ui.settings.back", "Back", "Назад");
            AddTranslation(strings, "ui.settings.contact_us", "Contact Us", "Связаться с нами");
            AddTranslation(strings, "ui.settings.privacy_policy", "Privacy Policy", "Политика конфиденциальности");
            AddTranslation(strings, "ui.settings.version", "Pixel Vanguard v.0.1", "Pixel Vanguard в.0.1");
            AddTranslation(strings, "ui.settings.remove_ads", "Remove Ads - 4990 coins", "Убрать рекламу - 4990 монет");
            AddTranslation(strings, "ui.settings.ads_removed", "Ads Removed", "Реклама удалена");

            AddTranslation(strings, "ui.currency.coins", "Coins", "Монет(ы)");

            // Startup Screen
            AddTranslation(strings, "ui.startup.press_to_start", "Press to Start", "Нажмите для начала");

            // Shop
            AddTranslation(strings, "ui.shop.title", "Shop", "Магазин");
            AddTranslation(strings, "ui.shop.select_upgrade", "Select upgrade to see details", "Выберите улучшение для подробностей");

            // Might
            AddTranslation(strings, "ui.shop.might.name", "MIGHT", "МОЩЬ");
            AddTranslation(strings, "ui.shop.might.short", "Damage +10%", "Урон +10%");
            AddTranslation(strings, "ui.shop.might.desc", "Increases base damage by 10% per level. Stacks multiplicatively with weapon upgrades.", "Увеличивает базовый урон на 10% за уровень. Складывается мультипликативно с улучшениями оружия.");

            // Vitality
            AddTranslation(strings, "ui.shop.vitality.name", "VITALITY", "ЖИВУЧЕСТЬ");
            AddTranslation(strings, "ui.shop.vitality.short", "Max HP +10", "Макс. HP +10");
            AddTranslation(strings, "ui.shop.vitality.desc", "Increases maximum health by 10 HP per level. Take more hits before dying.", "Увеличивает максимальное здоровье на 10 HP за уровень. Выдерживайте больше ударов.");

            // Greaves
            AddTranslation(strings, "ui.shop.greaves.name", "GREAVES", "ПОНОЖИ");
            AddTranslation(strings, "ui.shop.greaves.short", "Speed +5%", "Скорость +5%");
            AddTranslation(strings, "ui.shop.greaves.desc", "Increases movement speed by 5% per level. Dodge enemies and reposition faster.", "Увеличивает скорость передвижения на 5% за уровень. Уворачивайтесь от врагов быстрее.");

            // Magnet
            AddTranslation(strings, "ui.shop.magnet.name", "MAGNET", "МАГНИТ");
            AddTranslation(strings, "ui.shop.magnet.short", "Range +10%", "Радиус +10%");
            AddTranslation(strings, "ui.shop.magnet.desc", "Increases XP/item collection radius by 10% per level. Collect from farther away.", "Увеличивает радиус сбора XP/предметов на 10% за уровень. Собирайте издалека.");

            // Ad Packs
            AddTranslation(strings, "ui.shop.ad_pack.watch_5", "WATCH 5 ADS", "5 РЕКЛАМ");
            AddTranslation(strings, "ui.shop.ad_pack.coins_5", "1990 Coins", "1990 монет");
            AddTranslation(strings, "ui.shop.ad_pack.progress_5", "({0}/5)", "({0}/5)");
            AddTranslation(strings, "ui.shop.ad_pack.desc_5", "Watch 5 ads to earn 1,990 coins. Progress persists across sessions.", "Посмотрите 5 реклам, чтобы заработать 1,990 монет. Прогресс сохраняется.");

            AddTranslation(strings, "ui.shop.ad_pack.watch_10", "WATCH 10 ADS", "10 РЕКЛАМ");
            AddTranslation(strings, "ui.shop.ad_pack.coins_10", "4990 Coins", "4990 монет");
            AddTranslation(strings, "ui.shop.ad_pack.progress_10", "({0}/10)", "({0}/10)");
            AddTranslation(strings, "ui.shop.ad_pack.desc_10", "Watch 10 ads to earn 4,990 coins. Best value for your time!", "Посмотрите 10 реклам, чтобы заработать 4,990 монет. Лучшее соотношение!");

            // Gold Pack
            AddTranslation(strings, "ui.shop.gold_pack.title", "SPECIAL OFFER", "СПЕЦИАЛЬНОЕ");
            AddTranslation(strings, "ui.shop.gold_pack.amount", "29900 Coins", "29900 монет");
            AddTranslation(strings, "ui.shop.gold_pack.price", "79 YAN", "79 ЯН");
            AddTranslation(strings, "ui.shop.gold_pack.desc", "Premium gold pack with the best value. Purchase directly to support development!", "Премиум пак золота с лучшим соотношением. Купите, чтобы поддержать разработку!");

            // Common Shop
            AddTranslation(strings, "ui.common.coins", "Coins", "Монет");
            AddTranslation(strings, "ui.common.watch_ad", "Watch Ad", "Посмотреть рекламу");
            AddTranslation(strings, "ui.common.locked", "LOCKED", "ЗАБЛОКИРОВАНО");
            AddTranslation(strings, "ui.common.purchase_to_unlock", "Purchase to unlock", "Купите, чтобы разблокировать");

            // Character Selection
            AddTranslation(strings, "ui.character.title", "Character Selection", "Выбор персонажа");
            AddTranslation(strings, "ui.character.select", "Select", "Выбрать");
            AddTranslation(strings, "ui.character.selected", "Selected", "Выбрано");
            AddTranslation(strings, "ui.character.purchase", "Purchase for {0}", "Купить за {0}");
            AddTranslation(strings, "ui.character.continue", "Continue", "Продолжить");
            AddTranslation(strings, "ui.character.confirm", "Confirm", "Подтвердить");

            // Character Names
            AddTranslation(strings, "ui.character.knight.name", "Knight", "Рыцарь");
            AddTranslation(strings, "ui.character.pyromancer.name", "Pyromancer", "Пироман");
            AddTranslation(strings, "ui.character.ranger.name", "Ranger", "Следопыт");
            AddTranslation(strings, "ui.character.santa.name", "Santa", "Санта");
            AddTranslation(strings, "ui.character.zombie.name", "Zombie", "Зомби");

            // Character Stats
            AddTranslation(strings, "ui.character.stats.weapon", "Weapon", "Оружие");
            AddTranslation(strings, "ui.character.stats.health", "Health", "Здоровье");
            AddTranslation(strings, "ui.character.stats.speed", "Speed", "Скорость");
            AddTranslation(strings, "ui.character.stats.damage", "Damage", "Урон");
            AddTranslation(strings, "ui.character.stats.base_health", "Base Health", "Базовое здоровье");
            AddTranslation(strings, "ui.character.stats.base_speed", "Base Speed", "Базовая скорость");

            // Weapon Names
            AddTranslation(strings, "ui.weapon.greatsword", "Greatsword", "Большой меч");
            AddTranslation(strings, "ui.weapon.holywater", "HolyWater", "Святая вода");
            AddTranslation(strings, "ui.weapon.magicorbitals", "MagicOrbitals", "Магические орбиты");
            AddTranslation(strings, "ui.weapon.autocrossbow", "AutoCrossbow", "Автоматический арбалет");

            // HUD
            AddTranslation(strings, "ui.hud.fps", "FPS", "ФПС");
            AddTranslation(strings, "ui.hud.level_short", "LV", "УР");
            AddTranslation(strings, "ui.hud.level_full", "Level", "Уровень");
            AddTranslation(strings, "ui.hud.gold", "Gold", "Золото");
            AddTranslation(strings, "ui.hud.time", "Time", "Время");
            
            // Level Up
            AddTranslation(strings, "ui.levelup.new_level", "New Level!", "Новый уровень!");

            // Game Over
            AddTranslation(strings, "ui.gameover.title", "Game Over", "Конец игры");
            AddTranslation(strings, "ui.gameover.revive", "Revive", "Возродиться");
            AddTranslation(strings, "ui.gameover.quit", "Quit", "Выход");

            // Results
            AddTranslation(strings, "ui.results.title", "Results", "Результаты");
            AddTranslation(strings, "ui.results.victory", "VICTORY", "ПОБЕДА");
            AddTranslation(strings, "ui.results.defeated", "DEFEATED", "ПОРАЖЕНИЕ");
            AddTranslation(strings, "ui.results.session_summary", "SESSION SUMMARY", "ИТОГИ СЕССИИ");
            AddTranslation(strings, "ui.results.time_survived", "Time Survived", "Время выживания");
            AddTranslation(strings, "ui.results.enemies_killed", "Enemies Killed", "Убито врагов");
            AddTranslation(strings, "ui.results.level_reached", "Level Reached", "Достигнут уровень");
            AddTranslation(strings, "ui.results.gold_earned", "Gold Earned", "Заработано золота");
            AddTranslation(strings, "ui.results.watch_ad_bonus", "GET {0} GOLD", "{0} Монет");
            AddTranslation(strings, "ui.results.main_menu", "Main Menu", "Главное меню");
            AddTranslation(strings, "ui.results.continue", "Continue", "Продолжить");
            AddTranslation(strings, "ui.results.resume", "Resume", "Продолжить");

            // Notifications
            AddTranslation(strings, "ui.notification.gold_doubled", "Gold Doubled!", "Золото удвоено!");
            AddTranslation(strings, "ui.notification.purchase_success", "Purchase successful!", "Покупка успешна!");
            AddTranslation(strings, "ui.notification.ad_watched", "Ad watched successfully!", "Реклама просмотрена!");
            AddTranslation(strings, "ui.notification.insufficient_gold", "Not enough gold!", "Недостаточно золота!");

            // Time Format
            AddTranslation(strings, "ui.time.format", "{0:00}:{1:00}", "{0:00}:{1:00}");
            AddTranslation(strings, "ui.time.minutes", "min", "мин");
            AddTranslation(strings, "ui.time.seconds", "sec", "сек");

            // ============================================
            // UPGRADES
            // ============================================
            
            // Greatsword Upgrades
            AddTranslation(strings, "upgrade.berserker_fury.name", "Berserker Fury", "Ярость берсерка");
            AddTranslation(strings, "upgrade.berserker_fury.desc", "Greatsword attacks 15% faster", "Меч атакует на 15% быстрее");
            
            AddTranslation(strings, "upgrade.executioner_edge.name", "Executioner's Edge", "Клинок палача");
            AddTranslation(strings, "upgrade.executioner_edge.desc", "Greatsword: +20% damage, +100% knockback", "Меч: +20% урон, +100% отброса");
            
            AddTranslation(strings, "upgrade.mirror_slash.name", "Mirror Slash", "Зеркальный удар");
            AddTranslation(strings, "upgrade.mirror_slash.desc", "Spawn 2nd greatsword on opposite side", "Создать 2-й меч с противоположной стороны");
            
            // Crossbow Upgrades
            AddTranslation(strings, "upgrade.dual_crossbows.name", "Dual Crossbows", "Двойные арбалеты");
            AddTranslation(strings, "upgrade.dual_crossbows.desc", "Fire 2 arrows simultaneously", "Выстрел 2 стрелами одновременно");
            
            AddTranslation(strings, "upgrade.triple_barrage.name", "Triple Barrage", "Тройной залп");
            AddTranslation(strings, "upgrade.triple_barrage.desc", "Fire 3 arrows simultaneously", "Выстрел 3 стрелами одновременно");
            
            AddTranslation(strings, "upgrade.piercing_bolts.name", "Piercing Bolts", "Пробивные болты");
            AddTranslation(strings, "upgrade.piercing_bolts.desc", "Arrows pierce through enemies", "Стрелы пробивают врагов");
            
            // Holy Water Upgrades
            AddTranslation(strings, "upgrade.burning_touch.name", "Burning Touch", "Жгучее прикосновение");
            AddTranslation(strings, "upgrade.burning_touch.desc", "Damage scales +6% of enemy max HP", "Урон масштабируется на +6% от макс. HP врага");
            
            AddTranslation(strings, "upgrade.eternal_flame.name", "Eternal Flame", "Вечное пламя");
            AddTranslation(strings, "upgrade.eternal_flame.desc", "Puddle lasts 1.5x longer duration", "Лужа держится в 1.5 раза дольше");
            
            AddTranslation(strings, "upgrade.sanctified_expansion.name", "Sanctified Expansion", "Освященное расширение");
            AddTranslation(strings, "upgrade.sanctified_expansion.desc", "Puddle radius +40%", "Радиус лужи +40%");
            
            // Magic Orbitals Upgrades
            AddTranslation(strings, "upgrade.expanded_orbit.name", "Expanded Orbit", "Расширенная орбита");
            AddTranslation(strings, "upgrade.expanded_orbit.desc", "Orbit radius +40%", "Радиус орбиты +40%");
            
            // Passive Upgrades
            AddTranslation(strings, "upgrade.lifesteal.name", "Lifesteal", "Вампиризм");
            AddTranslation(strings, "upgrade.lifesteal.desc", "Heal 3% of damage dealt", "Исцеление 3% от нанесённого урона");
            
            AddTranslation(strings, "upgrade.lucky_coins.name", "Lucky Coins", "Удачные монеты");
            AddTranslation(strings, "upgrade.lucky_coins.desc", "Gold drops give +40% more gold", "Выпадает на +40% больше золота");
            
            AddTranslation(strings, "upgrade.magnet_field.name", "Magnet Field", "Магнитное поле");
            AddTranslation(strings, "upgrade.magnet_field.desc", "Pickup radius +50%", "Радиус сбора +50%");
            
            // General Stat Upgrades
            AddTranslation(strings, "upgrade.increase_damage.name", "Increase Damage", "Увеличить урон");
            AddTranslation(strings, "upgrade.increase_damage.desc", "Increase weapon damage by 5", "Увеличить урон оружия на 5");
            
            AddTranslation(strings, "upgrade.rapid_attack.name", "Rapid Attack", "Быстрая атака");
            AddTranslation(strings, "upgrade.rapid_attack.desc", "All Weapons Attack 15% faster", "Всё оружие атакует на 15% быстрее");
            
            AddTranslation(strings, "upgrade.swift_feet.name", "Swift Feet", "Быстрые ноги");
            AddTranslation(strings, "upgrade.swift_feet.desc", "Increase move speed by 10%", "Увеличить скорость на 10%");
            
            AddTranslation(strings, "upgrade.health_boost.name", "Health Boost", "Усиление здоровья");
            AddTranslation(strings, "upgrade.health_boost.desc", "Increase max HP by 10", "Увеличить макс. HP на 10");
            
            // New Weapon Unlocks
            AddTranslation(strings, "upgrade.unlock_autocrossbow.name", "Auto Crossbow", "Автоарбалет");
            AddTranslation(strings, "upgrade.unlock_autocrossbow.desc", "Fires arrows at the nearest enemy", "Стреляет стрелами в ближайшего врага");
            
            AddTranslation(strings, "upgrade.unlock_greatsword.name", "Greatsword", "Двуручный меч");
            AddTranslation(strings, "upgrade.unlock_greatsword.desc", "Powerful melee weapon that swings in a 360° arc every few seconds", "Мощное оружие ближнего боя с ударом на 360°");
            
            AddTranslation(strings, "upgrade.unlock_holywater.name", "Holy Water", "Святая вода");
            AddTranslation(strings, "upgrade.unlock_holywater.desc", "Throws flasks that create damaging puddles on impact", "Бросает колбы, создающие лужи урона");
            
            AddTranslation(strings, "upgrade.unlock_orbitals.name", "Magic Orbitals", "Магические орбиты");
            AddTranslation(strings, "upgrade.unlock_orbitals.desc", "Magical shields orbit continuously, damaging enemies", "Магические щиты вращаются, нанося урон врагам");

            // Mark asset as dirty and save
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Translation Populator", 
                $"Successfully added {strings.Count} translations to Translations.asset!", "OK");
        }

        private static void AddTranslation(System.Collections.Generic.List<TranslationData.LocalizedString> list, 
            string key, string english, string russian)
        {
            list.Add(new TranslationData.LocalizedString
            {
                key = key,
                english = english,
                russian = russian
            });
        }
    }
}
#endif
