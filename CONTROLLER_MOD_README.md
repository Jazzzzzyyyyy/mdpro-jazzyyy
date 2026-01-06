# MDPro3 Controller Support Mod

A comprehensive controller support system for MDPro3, enabling full gamepad navigation and control across all game modes.

## 📋 Table of Contents

- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Supported Controllers](#supported-controllers)
- [Button Mapping](#button-mapping)
- [Usage Guide](#usage-guide)
- [Configuration](#configuration)
- [Troubleshooting](#troubleshooting)
- [Technical Details](#technical-details)

## ✨ Features

- **Full Gamepad Support**: Navigate all menus, deck builder, and duel gameplay with a controller
- **Auto-Detection**: Automatically detects controller type (Xbox, PlayStation, Nintendo) and applies appropriate button labels
- **Customizable Mapping**: JSON-based configuration for custom button mappings
- **Visual Feedback**: Cursor highlighting and selection indicators for controller navigation
- **Haptic Feedback**: Controller vibration for confirmations, cancellations, and navigation
- **Multiple Profiles**: Pre-configured profiles for Xbox, PlayStation, and Nintendo controllers

## 📦 Requirements

- MDPro3 (Unity 6000.0.10f1+)
- Unity Input System package (included in MDPro3)
- Supported gamepad controller

## 🔧 Installation

### For Players (Pre-built)

1. Download the latest release from the Releases page
2. Copy the `ControllerSupport` folder to `MDPro3/Assets/Scripts/MDPro3/`
3. Copy `Controller` folder to `MDPro3/Data/`
4. Launch MDPro3 - controller support will be automatically enabled

### For Developers

1. Clone or copy the source files to your MDPro3 project
2. Ensure the Unity Input System package is installed
3. Add `ControllerManager` to a persistent GameObject in your scene
4. The system will automatically initialize on startup

## 🎮 Supported Controllers

| Controller | Status | Auto-Detect |
|------------|--------|-------------|
| Xbox Series X/S | ✅ Full Support | Yes |
| Xbox One | ✅ Full Support | Yes |
| Xbox 360 | ✅ Full Support | Yes |
| PlayStation 5 (DualSense) | ✅ Full Support | Yes |
| PlayStation 4 (DualShock 4) | ✅ Full Support | Yes |
| Nintendo Switch Pro | ✅ Full Support | Yes |
| Generic XInput | ✅ Full Support | Defaults to Xbox |

## 🕹️ Button Mapping

### Xbox Controller

| Button | Menu | Deck Builder | Duel |
|--------|------|--------------|------|
| A | Confirm | Add Card | Confirm/Select |
| B | Cancel/Back | Remove Card | Cancel |
| X | Secondary | Show Filters | Secondary Action |
| Y | Tertiary | Search | Show Card Info |
| LB | Previous Tab | Previous Tab | Previous Tab |
| RB | Next Tab | Next Tab | Next Tab |
| LT | - | Card Menu | View Options |
| RT | - | Switch Region | Zoom |
| View | - | Save Deck | Timing |
| Menu | Menu | Sub-menu | Pause |
| D-Pad | Navigate | Navigate | Navigate Field |
| Left Stick | Navigate | Navigate | Navigate |
| Right Stick | - | - | Camera |

### PlayStation Controller

| Button | Menu | Deck Builder | Duel |
|--------|------|--------------|------|
| × | Confirm | Add Card | Confirm/Select |
| ○ | Cancel/Back | Remove Card | Cancel |
| □ | Secondary | Show Filters | Secondary Action |
| △ | Tertiary | Search | Show Card Info |
| L1 | Previous Tab | Previous Tab | Previous Tab |
| R1 | Next Tab | Next Tab | Next Tab |
| L2 | - | Card Menu | View Options |
| R2 | - | Switch Region | Zoom |
| Share/Create | - | Save Deck | Timing |
| Options | Menu | Sub-menu | Pause |
| D-Pad | Navigate | Navigate | Navigate Field |
| Left Stick | Navigate | Navigate | Navigate |
| Right Stick | - | - | Camera |

## 📖 Usage Guide

### Menu Navigation

1. Use **D-Pad** or **Left Stick** to navigate between menu items
2. Press **A/×** to confirm selection
3. Press **B/○** to go back/cancel
4. Use **LB/L1** and **RB/R1** to switch between tabs

### Deck Builder

1. **Navigation**:
   - D-Pad/Left Stick: Move between cards
   - RT/R2: Switch between deck view and collection
   - LB/RB: Switch tabs/filters

2. **Card Operations**:
   - A/×: Add selected card to deck
   - B/○: Remove selected card from deck
   - X/□: Open filter menu
   - Y/△: Activate search input

3. **Deck Management**:
   - View/Share: Save deck
   - Menu/Options: Open sub-menu
   - LT/L2: Show card action menu

### Duel Gameplay

1. **Field Navigation**:
   - D-Pad/Left Stick: Navigate zones and cards
   - Up/Down: Move between your field and opponent's field
   - Navigate to hand by pressing Down from your spell/trap zone

2. **Card Selection**:
   - A/×: Select card/confirm action
   - B/○: Cancel current selection
   - Y/△: View card details

3. **View Options** (Hold LT/L2):
   - D-Pad Down: View Graveyard
   - D-Pad Up: View Banished
   - D-Pad Left: View Extra Deck
   - D-Pad Right: View Hand

4. **Phase Control**:
   - Menu/Options: End Phase prompt
   - View/Share: Toggle chain timing

## ⚙️ Configuration

### Configuration File Location

```
MDPro3/Data/Controller/controller_config.json
```

### Configuration Options

```json
{
  "ActiveProfile": "Xbox",
  "CustomMappings": {
    "buttonSouth": "Confirm",
    "buttonEast": "Cancel",
    // ... other mappings
  },
  "NavigationSettings": {
    "DeadZone": 0.3,
    "RepeatDelay": 0.4,
    "RepeatRate": 0.15,
    "AnalogSensitivity": 1.0
  }
}
```

### Navigation Settings

| Setting | Default | Description |
|---------|---------|-------------|
| DeadZone | 0.3 | Minimum stick deflection to register input |
| RepeatDelay | 0.4 | Initial delay before repeat navigation (seconds) |
| RepeatRate | 0.15 | Time between repeat navigation events (seconds) |
| AnalogSensitivity | 1.0 | Analog stick sensitivity multiplier |

### Available Actions

- Navigation: `NavigateUp`, `NavigateDown`, `NavigateLeft`, `NavigateRight`
- Primary: `Confirm`, `Cancel`, `Secondary`, `Tertiary`
- Tabs: `PreviousTab`, `NextTab`
- View: `ZoomIn`, `ZoomOut`, `ToggleView`, `CenterCamera`
- System: `Menu`, `Pause`
- Duel: `EndPhase`, `ViewGraveyard`, `ViewBanished`, `ViewExtraDeck`, `ViewHand`
- Deck Builder: `AddCard`, `RemoveCard`, `SearchCard`, `SortDeck`

## 🔍 Troubleshooting

### Controller Not Detected

1. Ensure the controller is properly connected before launching the game
2. For Bluetooth controllers, make sure they're paired
3. Try unplugging and reconnecting the controller
4. Check if the controller works in other applications

### Wrong Button Labels

1. The system auto-detects controller type on connection
2. If labels are incorrect, manually set the profile in `controller_config.json`
3. Restart the game after changing the profile

### Input Lag or Double Inputs

1. Adjust `DeadZone` setting (increase if accidental inputs occur)
2. Adjust `RepeatDelay` and `RepeatRate` for navigation timing
3. Ensure you're not using third-party controller software that might conflict

### Haptic Feedback Not Working

1. Check if "Rumble" is enabled in game settings
2. Some controllers may not support haptic feedback
3. Ensure controller batteries are charged

### Configuration Not Saving

1. Ensure the `Data/Controller` folder exists
2. Check file permissions on the configuration file
3. Verify JSON syntax is correct

## 🛠️ Technical Details

### Architecture

The controller support system consists of the following components:

1. **ControllerConfig**: JSON-based configuration handling
2. **ControllerManager**: Central input processing and event dispatching
3. **ControllerMenuNavigation**: Menu navigation support with visual feedback
4. **ControllerDuelSupport**: Duel-specific navigation and actions
5. **ControllerDeckBuilderSupport**: Deck builder navigation and operations

### Integration Points

- Integrates with Unity's Input System (`com.unity.inputsystem`)
- Works alongside existing `UserInput` system
- Supports the existing `Servant` UI architecture

### Events

The system exposes the following events for custom integration:

```csharp
// Controller connection events
ControllerManager.OnControllerConnected += (type) => { };
ControllerManager.OnControllerDisconnected += () => { };

// Action events
ControllerManager.OnGameActionTriggered += (action) => { };
```

## 📝 License

This mod is provided as-is for use with MDPro3. Please respect the original MDPro3 license and terms of use.

## 🤝 Contributing

Contributions are welcome! Please submit issues and pull requests to help improve controller support.

## 📞 Support

For issues and questions:
1. Check the Troubleshooting section above
2. Search existing issues on GitHub
3. Create a new issue with:
   - Controller type
   - Operating system
   - Detailed description of the problem
   - Steps to reproduce
