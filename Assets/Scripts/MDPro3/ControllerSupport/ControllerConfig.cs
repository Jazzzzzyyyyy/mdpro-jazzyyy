using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace MDPro3.ControllerSupport
{
    /// <summary>
    /// Handles JSON-based controller configuration for button mapping and profiles.
    /// Supports Xbox, PlayStation, and custom controller layouts.
    /// </summary>
    public static class ControllerConfig
    {
        private const string CONFIG_FOLDER = "Data/Controller";
        private const string DEFAULT_CONFIG_FILE = "controller_config.json";

        private static ControllerProfile currentProfile;
        private static Dictionary<string, ControllerProfile> profiles = new Dictionary<string, ControllerProfile>();

        public static ControllerProfile CurrentProfile => currentProfile;

        /// <summary>
        /// Initialize controller configuration system
        /// </summary>
        public static void Initialize()
        {
            EnsureConfigFolder();
            LoadDefaultProfiles();
            LoadUserConfig();
        }

        private static void EnsureConfigFolder()
        {
            var path = GetConfigFolderPath();
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Gets the appropriate config folder path based on the platform.
        /// Uses the same Data folder structure as MDPro3.
        /// </summary>
        private static string GetConfigFolderPath()
        {
            // MDPro3 uses a relative Data folder from the executable location
            // This is consistent with how Config.cs handles the config.conf file
            return CONFIG_FOLDER;
        }

        private static void LoadDefaultProfiles()
        {
            // Xbox Profile (default)
            var xboxProfile = new ControllerProfile
            {
                ProfileName = "Xbox",
                ControllerType = ControllerType.Xbox,
                ButtonMappings = new Dictionary<string, GameAction>
                {
                    { "buttonSouth", GameAction.Confirm },      // A
                    { "buttonEast", GameAction.Cancel },        // B
                    { "buttonWest", GameAction.Secondary },     // X
                    { "buttonNorth", GameAction.Tertiary },     // Y
                    { "leftShoulder", GameAction.PreviousTab },
                    { "rightShoulder", GameAction.NextTab },
                    { "leftTrigger", GameAction.ZoomOut },
                    { "rightTrigger", GameAction.ZoomIn },
                    { "select", GameAction.Menu },
                    { "start", GameAction.Pause },
                    { "leftStickPress", GameAction.ToggleView },
                    { "rightStickPress", GameAction.CenterCamera },
                    { "dpadUp", GameAction.NavigateUp },
                    { "dpadDown", GameAction.NavigateDown },
                    { "dpadLeft", GameAction.NavigateLeft },
                    { "dpadRight", GameAction.NavigateRight }
                },
                NavigationSettings = new NavigationSettings
                {
                    DeadZone = 0.3f,
                    RepeatDelay = 0.4f,
                    RepeatRate = 0.15f,
                    AnalogSensitivity = 1.0f
                }
            };
            profiles["Xbox"] = xboxProfile;

            // PlayStation Profile
            var psProfile = new ControllerProfile
            {
                ProfileName = "PlayStation",
                ControllerType = ControllerType.PlayStation,
                ButtonMappings = new Dictionary<string, GameAction>
                {
                    { "buttonSouth", GameAction.Confirm },      // Cross
                    { "buttonEast", GameAction.Cancel },        // Circle
                    { "buttonWest", GameAction.Secondary },     // Square
                    { "buttonNorth", GameAction.Tertiary },     // Triangle
                    { "leftShoulder", GameAction.PreviousTab }, // L1
                    { "rightShoulder", GameAction.NextTab },    // R1
                    { "leftTrigger", GameAction.ZoomOut },      // L2
                    { "rightTrigger", GameAction.ZoomIn },      // R2
                    { "select", GameAction.Menu },              // Share/Create
                    { "start", GameAction.Pause },              // Options
                    { "leftStickPress", GameAction.ToggleView }, // L3
                    { "rightStickPress", GameAction.CenterCamera }, // R3
                    { "dpadUp", GameAction.NavigateUp },
                    { "dpadDown", GameAction.NavigateDown },
                    { "dpadLeft", GameAction.NavigateLeft },
                    { "dpadRight", GameAction.NavigateRight }
                },
                NavigationSettings = new NavigationSettings
                {
                    DeadZone = 0.3f,
                    RepeatDelay = 0.4f,
                    RepeatRate = 0.15f,
                    AnalogSensitivity = 1.0f
                }
            };
            profiles["PlayStation"] = psProfile;

            // Nintendo Profile (swapped confirm/cancel for Japanese layout)
            var nintendoProfile = new ControllerProfile
            {
                ProfileName = "Nintendo",
                ControllerType = ControllerType.Nintendo,
                ButtonMappings = new Dictionary<string, GameAction>
                {
                    { "buttonSouth", GameAction.Cancel },       // B (swapped)
                    { "buttonEast", GameAction.Confirm },       // A (swapped)
                    { "buttonWest", GameAction.Tertiary },      // Y
                    { "buttonNorth", GameAction.Secondary },    // X
                    { "leftShoulder", GameAction.PreviousTab }, // L
                    { "rightShoulder", GameAction.NextTab },    // R
                    { "leftTrigger", GameAction.ZoomOut },      // ZL
                    { "rightTrigger", GameAction.ZoomIn },      // ZR
                    { "select", GameAction.Menu },              // -
                    { "start", GameAction.Pause },              // +
                    { "leftStickPress", GameAction.ToggleView },
                    { "rightStickPress", GameAction.CenterCamera },
                    { "dpadUp", GameAction.NavigateUp },
                    { "dpadDown", GameAction.NavigateDown },
                    { "dpadLeft", GameAction.NavigateLeft },
                    { "dpadRight", GameAction.NavigateRight }
                },
                NavigationSettings = new NavigationSettings
                {
                    DeadZone = 0.3f,
                    RepeatDelay = 0.4f,
                    RepeatRate = 0.15f,
                    AnalogSensitivity = 1.0f
                }
            };
            profiles["Nintendo"] = nintendoProfile;

            // Set Xbox as default
            currentProfile = xboxProfile;
        }

        private static void LoadUserConfig()
        {
            var configPath = GetConfigPath();
            if (!File.Exists(configPath))
            {
                SaveConfig();
                return;
            }

            try
            {
                var json = File.ReadAllText(configPath);
                var userConfig = JsonConvert.DeserializeObject<UserControllerConfig>(json);
                
                if (userConfig != null && profiles.ContainsKey(userConfig.ActiveProfile))
                {
                    currentProfile = profiles[userConfig.ActiveProfile];
                    
                    // Apply custom mappings if any
                    if (userConfig.CustomMappings != null)
                    {
                        foreach (var mapping in userConfig.CustomMappings)
                        {
                            if (currentProfile.ButtonMappings.ContainsKey(mapping.Key))
                                currentProfile.ButtonMappings[mapping.Key] = mapping.Value;
                        }
                    }

                    // Apply navigation settings
                    if (userConfig.NavigationSettings != null)
                        currentProfile.NavigationSettings = userConfig.NavigationSettings;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ControllerConfig] Failed to load config: {e.Message}");
            }
        }

        /// <summary>
        /// Save current configuration to file
        /// </summary>
        public static void SaveConfig()
        {
            try
            {
                var configPath = GetConfigPath();
                var userConfig = new UserControllerConfig
                {
                    ActiveProfile = currentProfile.ProfileName,
                    CustomMappings = currentProfile.ButtonMappings,
                    NavigationSettings = currentProfile.NavigationSettings
                };

                var json = JsonConvert.SerializeObject(userConfig, Formatting.Indented);
                File.WriteAllText(configPath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ControllerConfig] Failed to save config: {e.Message}");
            }
        }

        /// <summary>
        /// Switch to a different controller profile
        /// </summary>
        public static void SetProfile(string profileName)
        {
            if (profiles.TryGetValue(profileName, out var profile))
            {
                currentProfile = profile;
                SaveConfig();
            }
        }

        /// <summary>
        /// Set profile based on detected controller type
        /// </summary>
        public static void SetProfileByType(ControllerType type)
        {
            var profileName = type switch
            {
                ControllerType.PlayStation => "PlayStation",
                ControllerType.Nintendo => "Nintendo",
                _ => "Xbox"
            };
            SetProfile(profileName);
        }

        /// <summary>
        /// Remap a button to a different action
        /// </summary>
        public static void RemapButton(string button, GameAction action)
        {
            if (currentProfile.ButtonMappings.ContainsKey(button))
            {
                currentProfile.ButtonMappings[button] = action;
                SaveConfig();
            }
        }

        /// <summary>
        /// Get the action mapped to a specific button
        /// </summary>
        public static GameAction GetAction(string button)
        {
            if (currentProfile.ButtonMappings.TryGetValue(button, out var action))
                return action;
            return GameAction.None;
        }

        /// <summary>
        /// Reset profile to default mappings
        /// </summary>
        public static void ResetToDefaults()
        {
            LoadDefaultProfiles();
            SaveConfig();
        }

        private static string GetConfigPath()
        {
            return Path.Combine(GetConfigFolderPath(), DEFAULT_CONFIG_FILE);
        }

        /// <summary>
        /// Get all available profile names
        /// </summary>
        public static IEnumerable<string> GetProfileNames()
        {
            return profiles.Keys;
        }
    }

    /// <summary>
    /// Controller profile containing button mappings and settings
    /// </summary>
    [Serializable]
    public class ControllerProfile
    {
        public string ProfileName { get; set; }
        public ControllerType ControllerType { get; set; }
        public Dictionary<string, GameAction> ButtonMappings { get; set; }
        public NavigationSettings NavigationSettings { get; set; }
    }

    /// <summary>
    /// Navigation-specific settings
    /// </summary>
    [Serializable]
    public class NavigationSettings
    {
        public float DeadZone { get; set; } = 0.3f;
        public float RepeatDelay { get; set; } = 0.4f;
        public float RepeatRate { get; set; } = 0.15f;
        public float AnalogSensitivity { get; set; } = 1.0f;
    }

    /// <summary>
    /// User-specific configuration stored in JSON
    /// </summary>
    [Serializable]
    public class UserControllerConfig
    {
        public string ActiveProfile { get; set; }
        public Dictionary<string, GameAction> CustomMappings { get; set; }
        public NavigationSettings NavigationSettings { get; set; }
    }

    /// <summary>
    /// Supported controller types
    /// </summary>
    public enum ControllerType
    {
        None,
        Xbox,
        PlayStation,
        Nintendo,
        Generic
    }

    /// <summary>
    /// Game actions that can be mapped to controller buttons
    /// </summary>
    public enum GameAction
    {
        None,
        // Navigation
        NavigateUp,
        NavigateDown,
        NavigateLeft,
        NavigateRight,
        // Primary Actions
        Confirm,
        Cancel,
        Secondary,
        Tertiary,
        // Tab Navigation
        PreviousTab,
        NextTab,
        // Camera/View
        ZoomIn,
        ZoomOut,
        ToggleView,
        CenterCamera,
        // System
        Menu,
        Pause,
        // Duel-specific
        EndPhase,
        ActivateEffect,
        ViewGraveyard,
        ViewBanished,
        ViewExtraDeck,
        ViewHand,
        ShowCardInfo,
        // Deck Builder
        AddCard,
        RemoveCard,
        SearchCard,
        SortDeck
    }
}
