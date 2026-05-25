# About The Project

This project purpose is to pre-create frequent use effect, such as: 
- Multiples coins fly toward a target
- A popup text shows up on the screen to inform player
- An number popup to show the stat changes
- An image show on screen to perform as a vfx

This project use DOTween as the Tween behaviour

# Features

## Item Fly (`ITEM_FLY`)
Spawns and animates a cluster of items (e.g., coins, gems) flying along dynamic Bezier paths from a starting position to a target UI text or transform element.
* **Purpose**: Simulates realistic reward collection/harvesting feedback (e.g. coin collection).
* **Key Features**: 
  * Animates multiple items sequentially using **Bezier paths** (exponential, sine, parabolic, linear, or logarithmic curves).
  * Automatically handles UI resource counters, **incrementing the text value** dynamically as each coin lands.
  * Triggers **scale-punch feedback** on the target element and custom events (`OnItemInteract`, `OnComplete`) when items land.
* **Key Parameters**: `addValue` (total amount), `itemAmount` (spawn count), `startPosition`, `targetText`, `targetInteractTransform`, `itemSprite`, `delayBetweenItems`.

---

## Toast (`TOAST`)
Displays temporary on-screen notification toast messages with customizable styles and layouts.
* **Purpose**: Shows user-friendly feedback, status updates, or brief alerts (e.g., "Level Up!").
* **Key Features**:
  * Utilizes high-contrast **auto-contrast color pairing** for backplate and text.
  * Supports custom fonts, scale multiplier, background visibility, and custom duration.
  * Flexibly positioned via screen-percentage (e.g., `50, 50` for center) or absolute anchored coordinates.
* **Key Parameters**: `message` (content), `textColor`, `textFont`, `customDuration`, `customScale`, `useScreenPercentage`, `screenPercentage`.

---

## Stat Change Text (`STAT_CHANGE_TEXT`)
Animates temporary floating numbers indicating changes in player stats (e.g., `+100 XP`, `-50 HP`).
* **Purpose**: Delivers instant feedback for numeric increments, decrements, or damage numbers.
* **Key Features**:
  * Copies standard layout properties of a target TextMeshProUGUI element automatically.
  * Supports font style customization (boldness, color, size multiplier) and custom offsets.
  * Spawns directly at the screen position of a 3D/2D world object using **world-to-screen conversion**.
  * Fades out smoothly after a designated duration.
* **Key Parameters**: `amount` (value), `additionalIconText` (e.g., `%`, `XP`), `isBold`, `duration`, `rectTransformOffset`, `moveDistance`, `targetObject`.

---

## Popup Image (`POPUP_IMAGE`)
Displays highly customizable 2D popups (e.g., visual achievements, emojis, reward icons) at specified positions.
* **Purpose**: Shows key image prompts or illustrative achievements cleanly.
* **Key Features**:
  * Uses sleek scale-in sequences followed by high-rise fade-out transitions.
  * Positioned using screen percentage, coordinates, or world targets.
  * Allows customizable filter tint colors and duration parameters.
* **Key Parameters**: `sprite`, `useScreenPercentage`, `screenPercentage`, `anchoredPos`, `customFilterColor`, `customDuration`.

---

## Sprite Motion (`SPRITE_MOTION`)
Applies continuous rotation, scaling, or clock-like periodic motions to simple 2D sprites.
* **Purpose**: Automates minor decorative UI animations or moving environmental sprites.
* **Key Features**:
  * Features customizable motion categories: **Clockwise rotation**, **Continuous spin**, or **Pulsing scale**.
  * Adjusts starting sorting layer and order dynamically for proper layering.
  * Recycles itself automatically after a specified lifecycle duration.
* **Key Parameters**: `sprite`, `motionType` (`CLOCK`, `ROTATE`, `SCALE`), `worldSpaceStartPosi`, `customDuration`, `customSortingLayer`, `customSortingOrder`.

# Note
- This method in PrimeTween/Editor/CodeGenerator.cs is commented because error 
```// Assert.IsTrue(System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(methodName), $"Method name is invalid: {methodName}.");```



# INSTALLATION

## UNITY PACKAGE MANAGER
- Step 1: From Unity Editor -> Window -> Package Management -> Package Manager
- Step 2: Press the plus (+) icon in the top-left of Window
- Step 3: Select **Install package from git URL** -> paste the git-URL (https://github.com/NamPhuThuy/UP-AnimateWithScripts.git) and choose Install


## THROUGH GIT
- Step 1: Copy the git-URL (https://github.com/NamPhuThuy/UP-AnimateWithScripts.git)
- Step 2: Open Git CLI and type: git install https://github.com/NamPhuThuy/UP-AnimateWithScripts.git
- Step 3: Enter and wait
- Step 4: Make sure you have install the package TextMeshProUGUI in the Unity Project


## Scripts Documentation

| Script Name | Primary Purpose | Key Public Methods |
|-------------|-----------------|--------------------|
| `AnimationManager.cs` | Main Singleton manager for pooling and playing visual effects | `Play<T>(T args)`, `Preload()`, `Release()` |
| `AnimationBase.cs` | Abstract base class for all pooled animation/VFX elements | `Play<T>(T args)`, `Recycle()`, `EndFast()` |
| `AnimationCatalog.cs` | ScriptableObject defining animation prefabs for the pool | `GetEntry(AnimationType)` |
| `Anim_ItemFly.cs` | Animates items (e.g., coins) flying along a Bezier curve to a UI target | `Play(ItemFlyArgs)` |
| `Anim_ParticleSystem.cs` | Spawns and plays a Unity ParticleSystem from the object pool | `Play(ParticleSystemArgs)` |
| `Anim_PopupImage.cs` | Displays a popup image sprite with customizable duration/filtering | `Play(PopupImageArgs)` |
| `Anim_RewardProgress.cs` | Animates a segmented progress bar filling sequentially | `Play(RewardProgressArgs)` |
| `Anim_ScreenShake.cs` | Applies camera/screen shake using intensity and animation curves | `Play(ScreenShakeArgs)` |
| `Anim_SpriteMotion.cs` | Moves a 2D sprite through world or UI space automatically | `Play(SpriteMotionArgs)` |
| `Anim_StatChangeText.cs` | Animates text representing a stat change (e.g., "+5") | `Play(StatChangeTextArgs)` |
| `Anim_Toast.cs` | Displays a simple temporary notification/toast text message | `Play(ToastArgs)` |
| `IAnimationArgs.cs` | Interfaces and structs defining parameters for each animation type | N/A (Data Structures) |
| `ObjActiveAuto.cs` | Automatically enables/disables a GameObject after a delay | N/A (Inspector Driven) |
| `ObjMoveBetweenAuto.cs` | Automatically pings-pongs an object between a start and end point | N/A (Inspector Driven) |
| `ObjRotateAuto.cs` | Applies continuous rotation to an object automatically | N/A (Inspector Driven) |
| `ObjScaleAuto.cs` | Applies automatic pulsing/scaling to an object over time | N/A (Inspector Driven) |
| `BezierCurveHelper.cs` | Mathematical helper for generating complex Bezier paths | N/A (Static Utils) |
