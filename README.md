<a id="readme-top"></a>

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]

<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage-examples">Usage examples</a></li>
    <li><a href="#todo">Roadmap</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
# About The Project

This project purpose is to pre-create frequent use effect, such as: 
- Multiples coins fly toward a target
- A popup text shows up on the screen to inform player
- An number popup to show the stat changes
- An image show on screen to perform as a vfx

This project use PrimeTween as the native Tween behaviour

## Features

### Item Fly

### StatChangeText

### PopupText

# Note
- This method in PrimeTween/Editor/CodeGenerator.cs is commented because error 
```// Assert.IsTrue(System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(methodName), $"Method name is invalid: {methodName}.");```



<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- INSTALLATION -->
## INSTALLATION

### UNITY PACKAGE MANAGER
- Step 1: From Unity Editor -> Window -> Package Management -> Package Manager
- Step 2: Press the plus (+) icon in the top-left of Window
- Step 3: Select **Install package from git URL** -> paste the git-URL (https://github.com/NamPhuThuy/UP-AnimateWithScripts.git) and choose Install


### THROUGH GIT
- Step 1: Copy the git-URL (https://github.com/NamPhuThuy/UP-AnimateWithScripts.git)
- Step 2: Open Git CLI and type: git install https://github.com/NamPhuThuy/UP-AnimateWithScripts.git
- Step 3: Enter and wait
- Step 4: Make sure you have install the package TextMeshProUGUI in the Unity Project


<!-- USAGE EXAMPLES -->
## Usage examples

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- CONTACT -->
## Contact
<div align="left">
  <a href="https://github.com/NamPhuThuy">
    <img src="images/github.gif" width="100" alt="Click to Play Demo">
  </a>

  <a href="https://www.facebook.com/namphuthuy957">
    <img src="images/facebook.gif" width="100" alt="Click to Play Demo">
  </a>
</div>

[![Itch][itch-shield]][itch-url]

<p align="right">(<a href="#readme-top">back to top</a>)</p>


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

<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/NamPhuThuy/UP-AnimateWithScripts.svg?style=for-the-badge
[contributors-url]: https://github.com/NamPhuThuy/UP-AnimateWithScripts/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/NamPhuThuy/UP-AnimateWithScripts.svg?style=for-the-badge
[forks-url]: https://github.com/NamPhuThuy/UP-AnimateWithScripts/network/members
[stars-shield]: https://img.shields.io/github/stars/NamPhuThuy/UP-AnimateWithScripts.svg?style=for-the-badge
[stars-url]: https://github.com/NamPhuThuy/UP-AnimateWithScripts/stargazers
[issues-shield]: https://img.shields.io/github/issues/NamPhuThuy/UP-AnimateWithScripts.svg?style=for-the-badge
[issues-url]: https://github.com/NamPhuThuy/UP-AnimateWithScripts/issues
[license-shield]: https://img.shields.io/github/license/NamPhuThuy/UP-AnimateWithScripts.svg?style=for-the-badge
[license-url]: https://github.com/NamPhuThuy/UP-AnimateWithScripts/blob/main/LICENSE

<!-- Contact -->
[itch-shield]: https://img.shields.io/badge/-itch.io-blue.svg?style=for-the-badge&logo=itch.io&colorB=f5f5f5
[itch-url]: https://namphuthuy.itch.io/


<!-- Mock Up -->
[product-screenshot]: images/avatar_510x540.png

<!-- Tech Stack -->
[Next.js]: https://img.shields.io/badge/next.js-000000?style=for-the-badge&logo=nextdotjs&logoColor=white
[Next-url]: https://nextjs.org/
[React.js]: https://img.shields.io/badge/React-20232A?style=for-the-badge&logo=react&logoColor=61DAFB
[React-url]: https://reactjs.org/
[JQuery.com]: https://img.shields.io/badge/jQuery-0769AD?style=for-the-badge&logo=jquery&logoColor=white
[JQuery-url]: https://jquery.com 
[Unity.com]: https://img.shields.io/badge/Unity-61DBFB?style=for-the-badge&logo=unity&logoColor=white&labelColor=black&color=black
[Unity-url]: https://unity.com/
[CSharp.com]: https://img.shields.io/badge/C%23-61DBFB?style=for-the-badge&logo=c%23&logoColor=white&labelColor=magenta&color=purple

[CSharp-url]: https://learn.microsoft.com/en-us/dotnet/csharp/
