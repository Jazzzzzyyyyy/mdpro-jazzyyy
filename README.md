## MDPro3

A new version of YGOPro in Unity with MasterDuel Assets.

Unity version: 6000.0.10f1

### Building for macOS

MDPro3 includes an automated build script that handles dependency fetching and Unity building.

**Quick Start:**
```bash
# Run the build script
./build-macos.sh
```

**Options:**
- `--force` - Force re-download of all dependencies (useful if downloads were incomplete)
- `--skip-deps` - Skip dependency fetching (if you've already downloaded them)
- `--skip-build` - Only fetch dependencies without building
- `--unity-path PATH` - Specify Unity executable path if not in default location

**Examples:**
```bash
# Force re-download dependencies
./build-macos.sh --force

# Only fetch dependencies
./build-macos.sh --skip-build

# Use specific Unity version
./build-macos.sh --unity-path /Applications/Unity/Hub/Editor/6000.0.10f1/Unity.app/Contents/MacOS/Unity
```

The script will:
1. Automatically fetch external dependencies (Platforms, HD-Arts, Closeup, Sound)
2. Create proper symlinks in `Assets/StreamingAssets`
3. Build the macOS application using Unity
4. Output the build to `build/StandaloneOSX/MDPro3.app`

**Note:** External dependencies are cached in the `external/` directory to avoid re-downloading on subsequent builds.

### Other required folders

* Platforms: https://code.mycard.moe/sherry_chaos/mdpro3-assetbundles
* Picture/Art: https://code.mycard.moe/mycard/hd-arts
* Picture/Closeup: https://code.mycard.moe/mycard/ygopro2-closeup
* Picture/DIY: You can find it from the released MDPro3
* Sound: https://code.moenext.com/mycard/mdpro3-sound

### Tools

* DumpShaders: Used for replacement during built-in assets packaging.
* YGO Classes: Used to compile dependencies from YGOPro in this project.
* Translations: python scripts used to split translation.csv to translation.conf.
* QuickBMS: decrypt IDS\_ITEM.bytes(names of items in Master Duel) and IDS\_ITEMDESC.bytes(descriptions of items in Master Duel);

### For Contributors:

* If you want to edit in-game translations, please visit https://docs.google.com/spreadsheets/d/1BbSTxgobDqLyHL7De6uFSqfGiK9WIH3no5fQPW2Xwls/edit?usp=sharing.
* If you want to edit bot.conf(Windbot), please edit it on YGOMobile(https://github.com/fallenstardust/YGOMobile-cn-ko-en), this project copy these files from it.

### Unity Notes:

Unity 6000.32f1 Crashes when loading the following Assetbundles:
fxp\_HL\_EXdeck\_001 (Material 2020.3.48f1)
PlayableGuide\_C001\_Far
PlayableGuide\_C001\_Far\_Mat13
PlayableGuide\_C001\_Near
PlayableGuide\_C001\_Near\_Mat13

