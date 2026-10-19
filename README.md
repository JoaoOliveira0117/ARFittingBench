# AR Fitting Bench

🇧🇷 [Leia em português](README.pt-BR.md)

A mobile augmented reality app (Android and iOS) where the user carries a virtual part to a socket and aligns the two. The workbench is anchored on a real surface, like a table or the floor. The part follows the phone (or touch gestures), and the socket changes color as the part gets closer. It turns green once the part is within the position and angle tolerances.

Built with **Unity 6 + AR Foundation** (ARCore on Android, ARKit on iOS).

---

## Table of contents

1. [How the app works](#how-the-app-works)
2. [Requirements](#requirements)
3. [Installation (Windows and macOS)](#installation-windows-and-macos)
4. [Opening the project](#opening-the-project)
5. [Testing in the Editor (XR Simulation)](#testing-in-the-editor-xr-simulation)
6. [Building for Android](#building-for-android)
7. [Building for iOS](#building-for-ios)
8. [Project structure](#project-structure)
9. [Configuration](#configuration)
10. [Troubleshooting](#troubleshooting)

---

## How the app works

1. The camera looks for flat surfaces. Detected surfaces are highlighted in blue.
2. Tap a surface to place the **bench**. It is held in place by an AR anchor and stays there even if you walk away.
3. The bench shows an L-shaped **part** (blue) and a **socket** (the negative of the L). Their positions and rotations are randomized every round.
4. Tap the part to pick it up. Move it to the socket and rotate it to line it up. Tap again to drop it.
5. While you move the part, the socket goes from red to yellow to light green as you get closer. When the part is within the tolerances (2 cm and 10° by default), the socket lights up **green** and a new round starts.

The three **control modes** use the same task, objects and tolerances. Only the way the part moves changes:

| Mode | How the part moves |
|---|---|
| **Celular** (phone) | The part stays 30 cm in front of the phone and follows its position and rotation 1:1. |
| **Celular ×2** (phone ×2) | Same as above, but the phone's movement is multiplied by 2. |
| **Tela** (screen) | Drag one finger to slide the part over the bench. Twist two fingers to rotate it. |

Each round's result is saved to a CSV file on the device (see [Configuration](#configuration)).

> The in-app text is in Portuguese (pt-BR).

---

## Requirements

### Exact versions used by the project

| Component | Version |
|---|---|
| **Unity Editor** | **6000.4.0f1** (Unity 6.4), changeset `8cf496087c8f` |
| AR Foundation | 6.4.3 |
| Google ARCore XR Plugin | 6.4.3 |
| Apple ARKit XR Plugin | 6.4.3 |
| XR Plug-in Management | 4.5.2 |
| XR Core Utilities | 2.5.3 |
| Universal Render Pipeline (URP) | 17.4.0 |
| Input System | 1.19.0 |
| Unity UI (uGUI) | 2.0.0 |

Unity downloads all packages automatically from `Packages/manifest.json` / `Packages/packages-lock.json` the first time you open the project. You don't have to install them by hand.

> ⚠️ Use **exactly Unity 6000.4.0f1**. Other versions may upgrade the packages and break the AR setup.

### Software per target

| You want to… | On Windows | On macOS |
|---|---|---|
| Open and edit the project | Unity Hub + Unity 6000.4.0f1 | Unity Hub + Unity 6000.4.0f1 |
| Build for **Android** | ✅ Unity modules: *Android Build Support*, *OpenJDK*, *Android SDK & NDK Tools* | ✅ same modules |
| Build for **iOS** | ⚠️ Can only **export** the Xcode project (module *iOS Build Support*). Compiling and installing **needs a Mac with Xcode**. | ✅ *iOS Build Support* module + **Xcode** (tested with Xcode 26.6 on macOS 26.5) |
| Version control | [Git](https://git-scm.com/downloads) | Git (`xcode-select --install` or Homebrew) |

### Devices

- **Android**: Android **10 (API 29)** or newer, ARM64, on the [list of ARCore-supported devices](https://developers.google.com/ar/devices). *Google Play Services for AR* is installed automatically from the Play Store on first launch.
- **iOS**: iPhone or iPad with **iOS/iPadOS 15.0** or newer and an **A9 chip or newer** (iPhone 6s or later; ARKit doesn't run on the iPad Air 2 or iPad mini 4).
- **iOS signing**: an Apple ID. A free account ("Personal Team") is enough to install on your own iPhone. A paid Apple Developer Program account is only needed for TestFlight or the App Store.

---

## Installation (Windows and macOS)

### 1. Git

- **Windows**: install [Git for Windows](https://git-scm.com/download/win). Keep the default option *"Checkout Windows-style, commit Unix-style line endings"*. The repository's `.gitattributes` keeps Unity files consistent across systems.
- **macOS**: run `xcode-select --install` in Terminal, or `brew install git`.

### 2. Unity Hub

Download it from <https://unity.com/download> and sign in with your Unity account. The free **Unity Personal** license is enough.

### 3. Unity 6000.4.0f1 with the build modules

**Option A: through Unity Hub (recommended)**

1. Open this link in your browser to install the right version: `unityhub://6000.4.0f1/8cf496087c8f`
   (or go to Unity Hub → **Installs** → **Install Editor** → **Archive** → *download archive*, and look for **6000.4.0f1**).
2. In the module selection screen, check:
   - **Android Build Support**, including the sub-items **OpenJDK** and **Android SDK & NDK Tools**
   - **iOS Build Support**
   - (Windows) **Microsoft Visual Studio Community** if you don't have a C# editor yet
3. Accept the Android SDK license and wait for the install to finish.

If the Editor is already installed without these modules: Unity Hub → **Installs** → ⚙️ next to 6000.4.0f1 → **Add modules**.

**Option B: from the command line (Unity CLI)**

With the [Unity CLI](https://unity.com/download) installed:

```bash
unity install 6000.4.0f1 --module android ios --childModules --yes --accept-eula
```

If the Editor is already installed, add only the modules:

```bash
unity install-modules --editor-version 6000.4.0f1 --module android ios --childModules --accept-eula
```

`--childModules` also installs **OpenJDK** and **Android SDK & NDK Tools**.

> Use the **JDK, SDK and NDK installed by Unity**. Don't point Unity at a separate Android Studio SDK unless you know the exact versions it needs.

### 4. Xcode (macOS only, to build for iOS)

1. Install **Xcode** from the Mac App Store.
2. Open it once, accept the license and let it install the extra components.
3. In **Xcode → Settings → Components**, make sure the **iOS** platform is installed.
4. In **Xcode → Settings → Accounts**, add your Apple ID.

---

## Opening the project

```bash
git clone https://github.com/<your-user>/ARFittingBench.git
```

> **Windows:** clone to a short path **without spaces or accents**, for example `C:\dev\ARFittingBench`. Paths like `C:\Users\João\Área de Trabalho\...` often break the Android build (Gradle).

1. Unity Hub → **Projects** → **Add** → **Add project from disk** → pick the cloned folder.
2. Open it with **6000.4.0f1**.
3. The first import takes a few minutes, because the `Library/` folder (several GB) is built locally and is not in the repository.
4. Open the scene **`Assets/ARFittingBench/Scenes/ARFittingBench.unity`**. It is already first in the build list.

---

## Testing in the Editor (XR Simulation)

You can test the app without a phone, using AR Foundation's simulated environment. It is already enabled for the *Standalone* (Windows/Mac) target.

1. In **File → Build Profiles**, make sure the active platform is **Windows/Mac/Linux**. XR Simulation only runs on that platform.
2. Open the `ARFittingBench` scene and press **Play**.
3. Move the simulated camera: hold the **right mouse button** in the Game view and use **W A S D** (move) and **Q E** (down/up). The environment is scanned as the camera moves, and surfaces appear in blue.
4. **Click** a surface to place the bench, then click the part to pick it up.
5. To pick another environment: **Window → XR → AR Foundation → XR Environment**.

> Mouse clicks only reach the game while the **Game view has focus**.

---

## Building for Android

Works the same on **Windows and macOS**.

### Phone setup

1. On the phone, enable **Developer options** (Settings → About phone → tap *Build number* 7 times).
2. Turn on **USB debugging** and connect the phone with a cable. Accept the "Allow USB debugging?" prompt.
3. **Windows**: some manufacturers (Samsung, Motorola, Xiaomi…) need the manufacturer's USB driver, or the *Google USB Driver*, for the phone to show up.

### Build

1. **File → Build Profiles** → select **Android** → **Switch Platform**. The first switch reimports assets and takes a few minutes.
2. Check that the `ARFittingBench` scene is in **Scene List**.
3. (Optional) Run **AR Fitting Bench → Configurar Android e iOS** from the menu bar. It reapplies the AR settings: ARCore enabled, min API 29, IL2CPP, ARM64, portrait.
4. Check **Edit → Project Settings → XR Plug-in Management → Project Validation** (Android tab). It should show no errors.
5. Click **Build And Run** to install straight on the connected phone, or **Build** to generate an `.apk`. Save it in the `Builds/` folder, which is git-ignored.

To install an existing `.apk` by hand:

```bash
adb install -r Builds/ARFittingBench.apk
```

(`adb` lives in the SDK installed by Unity: `<Unity install>/Editor/Data/PlaybackEngines/AndroidPlayer/SDK/platform-tools/` on Windows, or `/Applications/Unity/Hub/Editor/6000.4.0f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/` on macOS.)

### Android settings already in the project

| Setting | Value |
|---|---|
| Package name | `com.arfittingbench.app` |
| Minimum API Level | 29 (Android 10). Required by ARCore with Vulkan when AR is "Required". |
| Target API Level | Automatic (highest installed) |
| Scripting Backend | IL2CPP |
| Architectures | ARM64 |
| Graphics APIs | Vulkan, OpenGLES3 |
| ARCore | Enabled, Requirement = *Required* |

---

## Building for iOS

### On macOS

1. **File → Build Profiles** → **iOS** → **Switch Platform**.
2. (Optional) Run **AR Fitting Bench → Configurar Android e iOS** and check **Project Validation** (iOS tab).
3. Click **Build** and choose an empty folder, such as `Builds/iOS`. Unity generates an **Xcode project**.
4. Open `Builds/iOS/Unity-iPhone.xcodeproj` in Xcode.
5. Select the **Unity-iPhone** target → **Signing & Capabilities**:
   - check **Automatically manage signing**;
   - under **Team**, pick your Apple ID (Personal Team works);
   - if Xcode says the bundle identifier is taken, change `com.arfittingbench.app` to something unique, like `com.yourname.arfittingbench`.
6. Connect the iPhone, select it as the destination at the top of Xcode, and press **▶ Run**.
7. On the iPhone (first time only):
   - iOS 16+: **Settings → Privacy & Security → Developer Mode** → turn it on and restart;
   - **Settings → General → VPN & Device Management** → trust your developer certificate.

> To skip editing the bundle ID in Xcode on every build, change it in Unity: **Edit → Project Settings → Player → iOS → Other Settings → Bundle Identifier**.

Command-line check (no signing), useful to confirm the project compiles:

```bash
cd Builds/iOS
xcodebuild -project Unity-iPhone.xcodeproj -scheme Unity-iPhone -configuration Release \
  -sdk iphoneos -destination 'generic/platform=iOS' \
  CODE_SIGNING_ALLOWED=NO build
```

### On Windows

Windows **cannot compile or sign** iOS apps, because that needs Xcode, which only runs on macOS. What you can do:

1. Install the **iOS Build Support** module and generate the Xcode project (**Build Profiles → iOS → Switch Platform → Build**).
2. Copy the generated folder to a Mac and follow steps 4–7 of the macOS section.
   - Prefer copying with **git**, **AirDrop**, or a network share. If you use a `.zip` made on Windows, run this on the Mac before building:
     ```bash
     find Builds/iOS -name "*.sh" -exec chmod +x {} +
     ```
3. No Mac available: **Unity Build Automation** (Unity's cloud build) can compile iOS from this repository. It requires configuring signing certificates in Unity Cloud.

### iOS settings already in the project

| Setting | Value |
|---|---|
| Bundle Identifier | `com.arfittingbench.app` |
| Target minimum iOS Version | 15.0 |
| Camera Usage Description | "A câmera é usada para a realidade aumentada." |
| ARKit | Enabled, Requirement = *Required* |

---

## Project structure

```
Assets/ARFittingBench/
├── Scenes/ARFittingBench.unity     Main scene (AR Session, XR Origin, UI)
├── Scripts/
│   ├── BancadaPlacer.cs            Plane detection + tap to anchor the bench
│   ├── Bancada.cs                  Randomization, color feedback, tolerances, rounds
│   ├── MovimentoPeca.cs            The single part-movement script (reads ControleConfig)
│   ├── ControleConfig.cs           Config: distance in front of the phone, multiplier, source
│   ├── InterfaceBancada.cs         Instructions, mode buttons, round info
│   ├── RegistroResultados.cs       Writes results to CSV
│   └── Toque.cs                    Tap detection (touch on phone, mouse in Editor)
├── Editor/BancadaSetup.cs          "AR Fitting Bench" menu: rebuilds scene/prefabs/settings
├── Config/                         The 3 control modes (ScriptableObjects)
├── Prefabs/                        Bancada (bench + part + socket) and SuperficieDetectada
└── Materials/
Assets/XR/                          XR Plug-in Management, ARCore and ARKit settings
Assets/Settings/                    URP settings (Mobile_RPAsset is used on phones)
```

### "AR Fitting Bench" menu (Unity menu bar)

| Item | What it does |
|---|---|
| **Criar Cena Completa** | Rebuilds materials, mode configs, prefabs and the scene from scratch, then applies the platform settings. Overwrites the scene and prefabs. |
| **Configurar Android e iOS** | Applies only the player and XR settings: name, bundle ID, ARCore/ARKit enabled, Android API 29, IL2CPP/ARM64, portrait. |

---

## Configuration

### Control modes

Each mode is an asset in `Assets/ARFittingBench/Config/` with three values:

| Field | Meaning |
|---|---|
| `distanciaFrente` | Distance (m) in front of the phone where the part stays while held |
| `multiplicador` | Multiplier applied to movement (phone displacement or screen drag) |
| `origem` | `Celular` (follows the phone) or `Tela` (touch gestures) |

To add a mode: right-click in the Project window → **Create → AR Fitting Bench → Configuração de Controle**. Then add it to the `modos` list and a button to `botoesModo` on the **Jogo** object (`InterfaceBancada` component).

### Tolerances and round

On the `Assets/ARFittingBench/Prefabs/Bancada.prefab` prefab (`Bancada` component): `toleranciaPosicao` (m), `toleranciaAngulo` (°), area size, minimum distance between part and socket, colors and the pause between rounds.

### Results (CSV)

Every round appends a line to `resultados_bancada.csv` with: date/time, mode, source, multiplier, distance, round, time (s), position error (cm) and angle error (°).

| Where it runs | File location |
|---|---|
| Android | `/sdcard/Android/data/com.arfittingbench.app/files/` (`adb pull` that path) |
| iOS | App container. In Xcode: **Window → Devices and Simulators** → app → ⚙️ → **Download Container** |
| Editor (Windows) | `%USERPROFILE%\AppData\LocalLow\ARFittingBench\AR Fitting Bench\` |
| Editor (macOS) | `~/Library/Application Support/ARFittingBench/AR Fitting Bench/` |

```bash
adb pull /sdcard/Android/data/com.arfittingbench.app/files/resultados_bancada.csv
```

---

## Troubleshooting

### Opening the project

| Problem | Fix |
|---|---|
| Hub asks to **upgrade/change version** | Install and use **6000.4.0f1** exactly (see [Installation](#3-unity-600040f1-with-the-build-modules)). Don't accept an upgrade. |
| The Editor opens in **Safe Mode** | There are compile errors. Check the Console. It is usually a package that failed to download: check your internet connection and use **Window → Package Manager → ⟳**. |
| Package resolve error with `com.unity.pipeline` (`-exp`) or `com.unity.ai.assistant` (`-pre`) | These are **optional** editor tools (automation/AI) and are not used by the app. Remove the lines from `Packages/manifest.json` and reopen the project. |
| **Pink/magenta** materials | URP is not assigned. **Edit → Project Settings → Graphics** must use `PC_RPAsset`, and **Quality** must use `Mobile_RPAsset` for Android/iOS. |
| Hub error "*No valid Unity Editor license*" | Unity Hub → ⚙️ **Preferences → Licenses → Add** → *Get a free personal license*. |
| Very slow first import | Expected: `Library/` is rebuilt locally. Don't copy `Library/` between Windows and macOS. Delete it if the project acts strangely after switching machines. |

### Android build

| Problem | Fix |
|---|---|
| "*Android SDK/JDK/NDK not found*" | Install the modules through Unity Hub (Android Build Support + OpenJDK + SDK & NDK Tools). In **Preferences → External Tools**, check the *"…installed with Unity"* boxes. |
| **Gradle build failed** | 1) Project path with spaces/accents or too long (Windows): move it to `C:\dev\ARFittingBench`. 2) Delete `Library/Bee` and try again. 3) Read the first `error:` in the Console, not the last one. |
| `Filename too long` / `MAX_PATH` (Windows) | Use a short path, or enable long paths: `git config --system core.longpaths true` and the Windows *LongPathsEnabled* policy. |
| Validation error "*ARCore requires … API Level 29*" | Run **AR Fitting Bench → Configurar Android e iOS**, or set **Player → Android → Minimum API Level = 29**. |
| Phone doesn't show up in *Run Device* | Check USB debugging, accept the RSA prompt on the phone, try another cable/port, install the manufacturer's USB driver (Windows). Run `adb devices` to check. |
| `INSTALL_FAILED_UPDATE_INCOMPATIBLE` | An older version signed with another key is already installed. Uninstall it from the phone (`adb uninstall com.arfittingbench.app`). |
| App opens with "*This device does not support AR*" / asks for *Google Play Services for AR* | The device is not ARCore-certified, or the service needs installing/updating from the Play Store. |
| **Black screen** (no camera) | Allow camera access (Settings → Apps → AR Fitting Bench → Permissions). Make sure the active Quality level on Android is **Mobile** (its renderer has the *AR Background Renderer Feature*). |

### iOS build

| Problem | Fix |
|---|---|
| "*Signing for 'Unity-iPhone' requires a development team*" | Signing & Capabilities → pick a **Team**. |
| "*Failed to register bundle identifier*" / "*No profiles for … were found*" | The bundle ID is taken by someone else. Change it to something unique (`com.yourname.arfittingbench`). |
| "*iOS X.Y is not installed*" / device "*not available*" | Xcode → Settings → Components → install the iOS platform matching the iPhone's iOS. Update Xcode if the iPhone is newer. |
| "*Untrusted Developer*" when opening the app | iPhone: Settings → General → VPN & Device Management → trust the certificate. |
| "*Developer Mode disabled*" | iPhone (iOS 16+): Settings → Privacy & Security → Developer Mode → on, then restart. |
| `Permission denied` in a build *Run Script* (`process_symbols.sh`, `process_symbols_il2cpp.sh`) | The project was copied from Windows and lost the execute permission. On the Mac: `chmod +x` on those `.sh` files (see [iOS on Windows](#on-windows)). |
| Free account: app stops opening after 7 days | Personal Team profiles expire in 7 days. Run it from Xcode again. |
| App closes as soon as it opens (camera) | The camera permission was denied. Settings → AR Fitting Bench → Camera. |

### Usage

| Problem | Fix |
|---|---|
| No surface is detected | Good lighting, textured surfaces (avoid plain white tables or glass), move the phone slowly for a few seconds. |
| The part "jumps" when picked up | Expected in *Celular* modes: it moves to `distanciaFrente` in front of the phone. Lower that value in the mode asset. |
| The bench is in a bad spot | Tap **Reposicionar bancada** to choose a surface again. |
| Clicks don't work in the Editor | Click inside the **Game** view to give it focus. Test on the *Windows/Mac/Linux* platform (XR Simulation). |
