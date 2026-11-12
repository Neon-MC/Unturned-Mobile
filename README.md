# Unturned Mobile

Source code and platform runtime for the official Android mobile port of [Unturned](https://smartlydressedgames.com/unturned/), a free open-world zombie survival sandbox game.

## Platform Features

- **Engine Target**: Unity 2022.3.62f3 (IL2CPP ARM64)
- **Graphics Pipeline**: Vulkan / OpenGLES3 with optimized mobile lighting and procedural Gerstner water
- **Platform Provider**: Native Steamworks integration with GameNetworkingSockets (GNS) UDP transport and Valve SDR Root CAs
- **Input System**: Native multi-touch controls with dynamic virtual joystick and responsive HUD
- **Automated CI**: GitHub Actions IL2CPP Android build pipeline with Library caching

## Getting Started

### Automated Builds (CI)
Pushes to `main` automatically trigger the `Build Unturned Android APK` workflow, producing a release APK available in the GitHub Actions artifacts.

### Local Development
1. Clone this repository
2. Install [Unity Hub](https://unity.com/download)
3. Install the [Unity 2022.3.62f3](https://unity.com/releases/editor/whats-new/2022.3.62f3) editor with **Android Build Support** (NDK & OpenJDK)
4. Open the project in Unity Editor and set target platform to **Android**
5. Open the `Assets/GameStartup.unity` scene
6. Click play!

## Resources

- [Frequently Asked Questions](https://docs.smartlydressedgames.com/en/stable/u3-sdk/faq.html)
- [Source Code Demo: Adding a Heat-Seeking Missile on YouTube](https://youtu.be/CqJnkcWfmEY)
- [Unturned's Modding Documentation](https://docs.smartlydressedgames.com/en/stable/)
