using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

#if (!UNITY_ANDROID && !UNITY_IOS && !UNITY_STANDALONE_LINUX) || UNITY_EDITOR
using UnityEngine.InputSystem.Switch;
#endif

namespace MDPro3.ControllerSupport
{
    /// <summary>
    /// Central controller manager that handles gamepad detection, input processing,
    /// and action dispatching for the MDPro3 controller support mod.
    /// </summary>
    public class ControllerManager : MonoBehaviour
    {
        public static ControllerManager Instance { get; private set; }

        // Controller state
        public static bool IsControllerConnected => Gamepad.current != null;
        public static ControllerType DetectedControllerType { get; private set; } = ControllerType.None;

        // Events
        public static event Action<ControllerType> OnControllerConnected;
        public static event Action OnControllerDisconnected;
        public static event Action<GameAction> OnGameActionTriggered;

        // Input state tracking
        private Gamepad currentGamepad;
        private bool wasConnected;

        // Navigation timing
        private float leftStickMoveTime;
        private float rightStickMoveTime;
        private Vector2 lastLeftStickInput;
        private Vector2 lastRightStickInput;

        // Duel-specific state
        public static bool IsDuelMode { get; set; }
        public static bool IsDeckBuilderMode { get; set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize controller config
            ControllerConfig.Initialize();
        }

        private void Start()
        {
            // Subscribe to controller connection events
            InputSystem.onDeviceChange += OnDeviceChange;
            
            // Check initial controller state
            CheckControllerConnection();
        }

        private void OnDestroy()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
        }

        private void Update()
        {
            if (!IsControllerConnected) return;

            // Monitor controller connection
            if (currentGamepad != Gamepad.current)
            {
                CheckControllerConnection();
            }

            // Process controller input
            ProcessControllerInput();
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (device is Gamepad)
            {
                switch (change)
                {
                    case InputDeviceChange.Added:
                    case InputDeviceChange.Reconnected:
                        CheckControllerConnection();
                        break;
                    case InputDeviceChange.Removed:
                    case InputDeviceChange.Disconnected:
                        if (Gamepad.current == null)
                        {
                            DetectedControllerType = ControllerType.None;
                            OnControllerDisconnected?.Invoke();
                        }
                        break;
                }
            }
        }

        private void CheckControllerConnection()
        {
            currentGamepad = Gamepad.current;
            
            if (currentGamepad != null)
            {
                DetectedControllerType = DetectControllerType(currentGamepad);
                ControllerConfig.SetProfileByType(DetectedControllerType);
                
                if (!wasConnected)
                {
                    wasConnected = true;
                    OnControllerConnected?.Invoke(DetectedControllerType);
                    Debug.Log($"[ControllerManager] Controller connected: {DetectedControllerType}");
                }
            }
            else
            {
                if (wasConnected)
                {
                    wasConnected = false;
                    DetectedControllerType = ControllerType.None;
                    OnControllerDisconnected?.Invoke();
                    Debug.Log("[ControllerManager] Controller disconnected");
                }
            }
        }

        private ControllerType DetectControllerType(Gamepad gamepad)
        {
            if (gamepad is DualShockGamepad)
                return ControllerType.PlayStation;

#if (!UNITY_ANDROID && !UNITY_IOS && !UNITY_STANDALONE_LINUX) || UNITY_EDITOR
            if (gamepad is SwitchProControllerHID)
                return ControllerType.Nintendo;
#endif

            // Check by name for other controllers
            var name = gamepad.name.ToLower();
            if (name.Contains("playstation") || name.Contains("dualshock") || name.Contains("dualsense"))
                return ControllerType.PlayStation;
            if (name.Contains("switch") || name.Contains("nintendo") || name.Contains("pro controller"))
                return ControllerType.Nintendo;

            return ControllerType.Xbox; // Default to Xbox layout
        }

        private void ProcessControllerInput()
        {
            if (currentGamepad == null) return;

            var profile = ControllerConfig.CurrentProfile;
            if (profile == null) return;

            // Process button inputs
            ProcessButtonInput("buttonSouth", currentGamepad.buttonSouth);
            ProcessButtonInput("buttonEast", currentGamepad.buttonEast);
            ProcessButtonInput("buttonWest", currentGamepad.buttonWest);
            ProcessButtonInput("buttonNorth", currentGamepad.buttonNorth);
            ProcessButtonInput("leftShoulder", currentGamepad.leftShoulder);
            ProcessButtonInput("rightShoulder", currentGamepad.rightShoulder);
            ProcessButtonInput("leftTrigger", currentGamepad.leftTrigger);
            ProcessButtonInput("rightTrigger", currentGamepad.rightTrigger);
            ProcessButtonInput("select", currentGamepad.selectButton);
            ProcessButtonInput("start", currentGamepad.startButton);
            ProcessButtonInput("leftStickPress", currentGamepad.leftStickButton);
            ProcessButtonInput("rightStickPress", currentGamepad.rightStickButton);

            // Process D-pad input
            ProcessDPadInput();

            // Process analog stick navigation
            ProcessAnalogNavigation();
        }

        private void ProcessButtonInput(string buttonName, ButtonControl button)
        {
            if (button.wasPressedThisFrame)
            {
                var action = ControllerConfig.GetAction(buttonName);
                if (action != GameAction.None)
                {
                    TriggerAction(action);
                }
            }
        }

        private void ProcessDPadInput()
        {
            var dpad = currentGamepad.dpad;
            
            if (dpad.up.wasPressedThisFrame)
                TriggerAction(ControllerConfig.GetAction("dpadUp"));
            if (dpad.down.wasPressedThisFrame)
                TriggerAction(ControllerConfig.GetAction("dpadDown"));
            if (dpad.left.wasPressedThisFrame)
                TriggerAction(ControllerConfig.GetAction("dpadLeft"));
            if (dpad.right.wasPressedThisFrame)
                TriggerAction(ControllerConfig.GetAction("dpadRight"));
        }

        private void ProcessAnalogNavigation()
        {
            var settings = ControllerConfig.CurrentProfile?.NavigationSettings;
            if (settings == null) return;

            var leftStick = currentGamepad.leftStick.ReadValue();
            var deadZone = settings.DeadZone;

            // Process left stick with repeat rate
            if (leftStick.magnitude > deadZone)
            {
                if (leftStickMoveTime <= 0f || leftStickMoveTime > settings.RepeatDelay)
                {
                    if (leftStickMoveTime > settings.RepeatDelay)
                        leftStickMoveTime -= settings.RepeatRate;

                    // Determine primary direction
                    if (Mathf.Abs(leftStick.y) > Mathf.Abs(leftStick.x))
                    {
                        if (leftStick.y > deadZone)
                            TriggerAction(GameAction.NavigateUp);
                        else if (leftStick.y < -deadZone)
                            TriggerAction(GameAction.NavigateDown);
                    }
                    else
                    {
                        if (leftStick.x > deadZone)
                            TriggerAction(GameAction.NavigateRight);
                        else if (leftStick.x < -deadZone)
                            TriggerAction(GameAction.NavigateLeft);
                    }
                }

                leftStickMoveTime += Time.unscaledDeltaTime;
            }
            else
            {
                leftStickMoveTime = 0f;
            }

            lastLeftStickInput = leftStick;
        }

        private void TriggerAction(GameAction action)
        {
            if (action == GameAction.None) return;

            OnGameActionTriggered?.Invoke(action);
            
            // Handle context-specific actions
            if (IsDuelMode)
            {
                HandleDuelAction(action);
            }
            else if (IsDeckBuilderMode)
            {
                HandleDeckBuilderAction(action);
            }
        }

        private void HandleDuelAction(GameAction action)
        {
            // Duel-specific action handling is managed through events
            // This allows the duel system to subscribe and respond appropriately
        }

        private void HandleDeckBuilderAction(GameAction action)
        {
            // Deck builder specific action handling is managed through events
            // This allows the deck editor to subscribe and respond appropriately
        }

        /// <summary>
        /// Trigger haptic feedback on the controller
        /// </summary>
        public void TriggerHapticFeedback(float lowFrequency, float highFrequency, float duration)
        {
            if (currentGamepad == null) return;
            
            if (!MDPro3.Config.GetBool("Rumble", true)) return;

            currentGamepad.SetMotorSpeeds(lowFrequency, highFrequency);
            StartCoroutine(StopHapticAfterDelay(duration));
        }

        private System.Collections.IEnumerator StopHapticAfterDelay(float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            currentGamepad?.SetMotorSpeeds(0f, 0f);
        }

        /// <summary>
        /// Light haptic feedback for UI navigation
        /// </summary>
        public void HapticNavigate()
        {
            TriggerHapticFeedback(0.05f, 0.1f, 0.05f);
        }

        /// <summary>
        /// Medium haptic feedback for confirmations
        /// </summary>
        public void HapticConfirm()
        {
            TriggerHapticFeedback(0.2f, 0.4f, 0.1f);
        }

        /// <summary>
        /// Strong haptic feedback for errors/cancellations
        /// </summary>
        public void HapticCancel()
        {
            TriggerHapticFeedback(0.4f, 0.2f, 0.15f);
        }

        /// <summary>
        /// Get left analog stick value
        /// </summary>
        public Vector2 GetLeftStick()
        {
            return currentGamepad?.leftStick.ReadValue() ?? Vector2.zero;
        }

        /// <summary>
        /// Get right analog stick value
        /// </summary>
        public Vector2 GetRightStick()
        {
            return currentGamepad?.rightStick.ReadValue() ?? Vector2.zero;
        }

        /// <summary>
        /// Check if a specific action button is currently pressed
        /// </summary>
        public bool IsActionPressed(GameAction action)
        {
            if (currentGamepad == null) return false;

            var profile = ControllerConfig.CurrentProfile;
            if (profile == null) return false;

            foreach (var mapping in profile.ButtonMappings)
            {
                if (mapping.Value == action)
                {
                    var button = GetButtonByName(mapping.Key);
                    if (button != null && button.isPressed)
                        return true;
                }
            }
            return false;
        }

        private ButtonControl GetButtonByName(string buttonName)
        {
            if (currentGamepad == null) return null;

            return buttonName switch
            {
                "buttonSouth" => currentGamepad.buttonSouth,
                "buttonEast" => currentGamepad.buttonEast,
                "buttonWest" => currentGamepad.buttonWest,
                "buttonNorth" => currentGamepad.buttonNorth,
                "leftShoulder" => currentGamepad.leftShoulder,
                "rightShoulder" => currentGamepad.rightShoulder,
                "leftTrigger" => currentGamepad.leftTrigger,
                "rightTrigger" => currentGamepad.rightTrigger,
                "select" => currentGamepad.selectButton,
                "start" => currentGamepad.startButton,
                "leftStickPress" => currentGamepad.leftStickButton,
                "rightStickPress" => currentGamepad.rightStickButton,
                _ => null
            };
        }

        /// <summary>
        /// Get controller name for display
        /// </summary>
        public static string GetControllerDisplayName()
        {
            return DetectedControllerType switch
            {
                ControllerType.Xbox => "Xbox Controller",
                ControllerType.PlayStation => "PlayStation Controller",
                ControllerType.Nintendo => "Nintendo Switch Controller",
                ControllerType.Generic => "Generic Controller",
                _ => "No Controller"
            };
        }

        /// <summary>
        /// Get button display name based on controller type
        /// </summary>
        public static string GetButtonDisplayName(string buttonName)
        {
            return DetectedControllerType switch
            {
                ControllerType.PlayStation => GetPlayStationButtonName(buttonName),
                ControllerType.Nintendo => GetNintendoButtonName(buttonName),
                _ => GetXboxButtonName(buttonName)
            };
        }

        private static string GetXboxButtonName(string buttonName)
        {
            return buttonName switch
            {
                "buttonSouth" => "A",
                "buttonEast" => "B",
                "buttonWest" => "X",
                "buttonNorth" => "Y",
                "leftShoulder" => "LB",
                "rightShoulder" => "RB",
                "leftTrigger" => "LT",
                "rightTrigger" => "RT",
                "select" => "View",
                "start" => "Menu",
                "leftStickPress" => "LS",
                "rightStickPress" => "RS",
                _ => buttonName
            };
        }

        private static string GetPlayStationButtonName(string buttonName)
        {
            return buttonName switch
            {
                "buttonSouth" => "×",
                "buttonEast" => "○",
                "buttonWest" => "□",
                "buttonNorth" => "△",
                "leftShoulder" => "L1",
                "rightShoulder" => "R1",
                "leftTrigger" => "L2",
                "rightTrigger" => "R2",
                "select" => "Share",
                "start" => "Options",
                "leftStickPress" => "L3",
                "rightStickPress" => "R3",
                _ => buttonName
            };
        }

        private static string GetNintendoButtonName(string buttonName)
        {
            return buttonName switch
            {
                "buttonSouth" => "B",
                "buttonEast" => "A",
                "buttonWest" => "Y",
                "buttonNorth" => "X",
                "leftShoulder" => "L",
                "rightShoulder" => "R",
                "leftTrigger" => "ZL",
                "rightTrigger" => "ZR",
                "select" => "-",
                "start" => "+",
                "leftStickPress" => "LS",
                "rightStickPress" => "RS",
                _ => buttonName
            };
        }
    }
}
