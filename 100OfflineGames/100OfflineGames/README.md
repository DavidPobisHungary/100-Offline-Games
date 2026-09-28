# 100 Offline Games — Unity 5.6.7f1 Project

## Requirements
- Unity **5.6.7f1** (exact version)
- Windows 10 PC
- iOS Build Support module installed in Unity
- Xcode on a Mac (to actually build to iOS)

---

## How to Open
1. Unzip this folder somewhere on your PC (e.g. `C:\Projects\100OfflineGames`)
2. Open **Unity Hub** (or Unity 5.6.7f1 directly)
3. Click **Open Project** and select the `100OfflineGames` folder
4. Wait for Unity to import everything

---

## IMPORTANT: Build All Scenes First!
After Unity opens the project, you MUST run the scene builder:

1. In Unity, look at the **top menu bar**
2. Click **Tools → Build All 100OfflineGames Scenes**
3. Wait — Unity will auto-create all 7 scenes in `Assets/Scenes/`
4. All scenes are also automatically added to **Build Settings**

This creates:
- `MainMenu` — the home screen with game list
- `CookieClicker` — fully playable cookie clicker
- `Game2040` — fully playable 2048 clone
- `Bobluxob` — Roblox-style 3D game
- `MineBloxx` — Minecraft-style game
- `VehicleSimulator` — vehicle selection + physics driving
- `PaperSnake` — Paper.io with bots

---

## iOS Build Instructions
1. Go to **File → Build Settings**
2. Select **iOS** platform, click **Switch Platform**
3. Click **Player Settings**:
   - Bundle ID: `com.yourname.100offlinegames`
   - Target minimum iOS: **7.1**
   - Architecture: **ARMv7** (for iPhone 4)
4. Click **Build** — choose a folder
5. Open the generated `.xcodeproj` on a **Mac with Xcode**
6. Connect iPhone 4 → build & run

---

## Games Included

| # | Game | Status |
|---|------|--------|
| 1 | 🍪 Cookie Clicker | ✅ Fully Playable |
| 2 | 🔢 2040 | ✅ Fully Playable |
| 3 | 🐍 PaperSnake.io | ✅ Fully Playable (vs bots) |
| 4 | 🚗 Vehicle Simulator | ✅ Playable (all vehicles + maps) |
| 5 | 👾 Bobluxob | ✅ Playable (map, NPCs, items, ragdoll) |
| 6 | ⛏ MineBloxx | ✅ Playable (house, furniture, vehicles outside) |

---

## Controls

### Cookie Clicker
- Tap the cookie to get cookies
- Tap the 🛒 Upgrades button to buy upgrades
- Buy all 7 upgrades to win!

### 2040
- **Swipe** on mobile, **WASD / Arrow Keys** on desktop
- Merge tiles to reach 2040!

### PaperSnake.io
- **Swipe** to change direction
- Claim territory by looping back to your zone
- Beat the 4 bots!

### Bobluxob
- **Left joystick** to move, **Jump** button to jump
- Select items from hotbar (tap slots)
- **Tap screen** to use selected item:
  - Sword: melee attack NPCs
  - Rocket Launcher: fire exploding rocket
  - Boombox: play music
  - Wall Builder: build a physics wall
- **Ragdoll** button: toggle ragdoll mode

### MineBloxx
- **WASD** to move, **Mouse** to look
- **Left click** to break blocks / interact with doors & windows
- **Right click** to place selected block
- **Scroll wheel** or **1-9 keys** to change hotbar slot
- Go outside through the **front door**
- Vehicles are parked outside!

### Vehicle Simulator
1. Select a **Vehicle** (Planes, Helis, Cars, Boats)
2. Select a **Map**
3. Hit **Start!**
4. Use **joystick** + throttle buttons to drive/fly

---

## In-Game HUD (shown inside every game)
- **⚙ Settings** — opens settings popup (music, sfx, vibration)
- **💾 Save** — saves your progress
- **✖ Exit** — returns to Main Menu

Portrait mode: bar is at the **top**
Landscape mode: bar is on the **right side**

---

## Notes
- APK Emulator was excluded (impossible on iOS without jailbreak)
- The project targets iPhone 4 / iOS 7.1 — keep graphics simple!
- All scripts are in `Assets/Scripts/`
- Editor tools are in `Assets/Editor/`
