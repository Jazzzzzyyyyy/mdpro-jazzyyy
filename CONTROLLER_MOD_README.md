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
- [Building from Source](#building-from-source)

## ✨ Features

- **Full Gamepad Support**: Navigate all menus, deck builder, and duel gameplay with a controller
- **Auto-Detection**: Automatically detects controller type (Xbox, PlayStation, Nintendo) and applies appropriate button labels
- **Customizable Mapping**: JSON-based configuration for custom button mappings
- **Visual Feedback**: Cursor highlighting and selection indicators for controller navigation
- **Haptic Feedback**: Controller vibration for confirmations, cancellations, and navigation
- **Multiple Profiles**: Pre-configured profiles for Xbox, PlayStation, and Nintendo controllers

## 📦 Requirements

- MDPro3 game installation
- Supported gamepad controller

## 🔧 Installation

### Quick Install (Recommended)

1. **Download** the latest release:
   - `ControllerSupport.dll`
   - `Controller/` folder (config files)

2. **Install the DLL**: Copy `ControllerSupport.dll` to:
   ```
   MDPro3/MDPro3_Data/Managed/ControllerSupport.dll
   ```

3. **Install Config Files**: Copy the `Controller` folder to:
   ```
   MDPro3/Data/Controller/
   ```

4. **Launch MDPro3** and connect your controller!

### Detailed Installation

For step-by-step instructions, see [Mods/ControllerSupport/INSTALL.md](Mods/ControllerSupport/INSTALL.md).

### Folder Structure After Installation

```
MDPro3/
├── MDPro3.exe
├── MDPro3_Data/
│   └── Managed/
│       └── ControllerSupport.dll  ← Mod DLL
└── Data/
    └── Controller/
        ├── controller_config.json  ← Your settings
        ├── xbox_profile.json
        └── playstation_profile.json
```

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

## 🛠️ Building from Source

If you want to build the DLL yourself or modify the source code:

### Source Files Location

The mod source code is in `Mods/ControllerSupport/`:
```
Mods/ControllerSupport/
├── ControllerSupport.csproj    ← Build project
├── BUILD.md                    ← Build instructions
├── INSTALL.md                  ← Installation guide
├── src/                        ← C# source files
│   ├── ControllerConfig.cs
│   ├── ControllerManager.cs
│   ├── ControllerMenuNavigation.cs
│   ├── ControllerDuelSupport.cs
│   └── ControllerDeckBuilderSupport.cs
└── Controller/                 ← Config templates
    ├── controller_config.json
    ├── xbox_profile.json
    └── playstation_profile.json
```

### Building the DLL

1. Install [.NET SDK 6.0+](https://dotnet.microsoft.com/download)
2. Set `GamePath` to your MDPro3 installation folder
3. Run: `dotnet build -c Release`
4. Find `ControllerSupport.dll` in `bin/Release/netstandard2.1/`

For detailed build instructions, see [Mods/ControllerSupport/BUILD.md](Mods/ControllerSupport/BUILD.md).

### Architecture

**Source Code Components**:
1. **ControllerConfig.cs**: JSON-based configuration handling
2. **ControllerManager.cs**: Central input processing and event dispatching
3. **ControllerMenuNavigation.cs**: Menu navigation support with visual feedback
4. **ControllerDuelSupport.cs**: Duel-specific navigation and actions
5. **ControllerDeckBuilderSupport.cs**: Deck builder navigation and operations

**Runtime Files** (for end users):
- `Data/Controller/controller_config.json`: User configuration file
- `Data/Controller/xbox_profile.json`: Xbox button reference
- `Data/Controller/playstation_profile.json`: PlayStation button reference

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
