# Work in Progress!
This project is not ready for end users just yet. Feel free to tinker with it, however.

# X11Overlay
A lightweight X11 desktop overlay for OpenVR / SteamVR.

Primarily made this because I couldn't find a proper overlay for Linux.

A reimplementation of [OVR4X11](https://github.com/galister/OVR4X11) using a lighter toolkit.

Features:
- Access your screens from within OpenVR / SteamVR
- Mouse pointer that supports left/right/middle click, depending on hand orientation
- Customizable keyboard with 2-hand typing
- Notifications system (WIP)
- Watch panel that shows:
  - Local time + 2 customizable time zones
  - Battery states of controllers + all connected trackers (might get cramped with a lot of trackers, though)
  - Volume rocker
  - Toggles for screens / keyboard

# Dependencies

The following libraries are needed:
- libX11.so
- libXtst.so
- libxcb.so
- libxcb-xfixes.so
- libxcb-randr.so
- libxcb-shm.so
- libxcb-xinerama.so
- dotnet >= 6
- [xshm-cap](https://github.com/galister/xshm-cap) (included as binary, feel free to build yourself)

On Arch Linux:
```
pacman -S libx11 libxcb libxtst dotnet
```

# How to Build

The project uses the standard dotnet build pipeline.

```
git clone https://github.com/galister/X11Overlay.git
cd X11Overlay
dotnet build
```

Then, run with:
```
cd bin/debug/net6.0
./X11Overlay
```
(Start SteamVR before running this.)

# SteamVR bindings:
Default bindings are provided for Index Controllers. Some notes to create your own:
- `Click`: keyboard typing and clicking on the screen. set this to your triggers.
- `Grip`: for moving overlays. Recommended: `Grip` input with pressure mode, pressure 70%. Release pressure 50%
- `AltClick`: optional push-to-talk key
- `Pose`: set this to the controller tip
- `Scroll`: set this to your joystick, and choose non-discrete mode

# Pointer

The pointer changes mode depending on the orientation:
- Blue - left click - thumb upwards
- Yellow - right click - palm upwards
- Purple - middle click - backhand upwards
Up is relative to HMD up.

# Grabbing

Simply grab to move screens and the keyboard. Scroll and grab for extra effect, depends on the pointer mode:

- Blue pointer: move on the forward axis (close / far)
- Yellow/Purple pointer: change size

# Keyboard

The default layout is my personal 60% layout, reflecting my real life setup. The layout can be changed via the keyboard.yaml file.

The keyboard also has 3 modes. The keys will change color to indicate the active mode. 

- Blue - regular keyboard
- Yellow - regular with shift
- Purple - alternative layout

The color of the pointer that has remained on the keyboard the longest will determine the color of the keyboard.

# Non-planned Features
- Displaying individual windows (XComposite) as this does not work well when using workspaces (windows getting culled and display black)
- Wayland support (at least until SteamVR runs reliably on Wayland)
- Windows support

# Known Issues
- The project is still in work in progress. I will make a release when I feel comfortable that this is actually useful to people.

# Works Used
- [FreeTypeSharp](https://github.com/ryancheung/FreeTypeSharp), MIT License
- [Godot Engine](https://github.com/godotengine/godot), MIT License
- [Liberation Fonts](https://github.com/liberationfonts/liberation-fonts), SIL Open Font License v1.1
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json), MIT License
- [OBS Studio](https://github.com/obsproject/obs-studio), GPLv2 License
- [OpenVR SDK](https://github.com/ValveSoftware/openvr), BSD-3-Clause license
- [OVRSharp](https://github.com/OVRTools/OVRSharp), MIT License
- [Silk.NET](https://github.com/dotnet/Silk.NET), MIT License
- [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp), Apache v2 License
- [YamlDotNet](SixLabors/ImageSharp), MIT License
