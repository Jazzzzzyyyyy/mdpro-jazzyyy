# Installing Controller Support Mod for MDPro3

## Download

Download the latest release from the Releases page:
- `ControllerSupport.dll` - The mod DLL
- `Controller/` folder - Configuration files

## Installation Steps

### Step 1: Install the DLL

Copy `ControllerSupport.dll` to one of these locations in your MDPro3 folder:

**Option A: Plugins folder (Recommended)**
```
MDPro3/
└── MDPro3_Data/
    └── Managed/
        └── ControllerSupport.dll  ← Place here
```

**Option B: If using a mod loader (BepInEx)**
```
MDPro3/
└── BepInEx/
    └── plugins/
        └── ControllerSupport.dll  ← Place here
```

### Step 2: Install Configuration Files

Copy the `Controller` folder to your MDPro3 `Data` directory:

```
MDPro3/
└── Data/
    └── Controller/
        ├── controller_config.json
        ├── xbox_profile.json
        └── playstation_profile.json
```

### Step 3: Launch the Game

1. Connect your controller before or after launching MDPro3
2. The mod will automatically detect your controller
3. Navigate menus, deck builder, and duels with your gamepad!

## Folder Structure After Installation

```
MDPro3/
├── MDPro3.exe
├── MDPro3_Data/
│   └── Managed/
│       ├── ControllerSupport.dll  ← Mod DLL
│       └── ... (other game DLLs)
└── Data/
    └── Controller/
        ├── controller_config.json  ← Your settings
        ├── xbox_profile.json
        └── playstation_profile.json
```

## Configuration

Edit `Data/Controller/controller_config.json` to customize:
- Button mappings
- Dead zone settings
- Navigation repeat rate
- Controller profile (Xbox/PlayStation/Nintendo)

See [CONTROLLER_MOD_README.md](../../CONTROLLER_MOD_README.md) for full configuration options.

## Troubleshooting

### Controller Not Detected
1. Ensure the controller is connected before launching
2. Check if the DLL is in the correct location
3. Verify the game loads the DLL (check logs)

### Buttons Not Working Correctly
1. Check `controller_config.json` exists in `Data/Controller/`
2. Try resetting to default by deleting the config file
3. Manually set your controller profile in the config

### Game Crashes on Startup
1. Ensure you have the correct version of the DLL for your MDPro3 version
2. Check that all required game DLLs are present
3. Look for error logs in the MDPro3 folder

## Uninstallation

1. Delete `MDPro3_Data/Managed/ControllerSupport.dll`
2. (Optional) Delete `Data/Controller/` folder to remove config files
