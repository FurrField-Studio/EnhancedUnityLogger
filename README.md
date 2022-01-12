# Enhanced Unity Logger

What is Enhanced Unity Logger?

Its enhanced version of [Unity-Logger](https://github.com/Semaeopus/Unity-Logger) made by [Semaeopus](https://github.com/Semaeopus) team (orginal creator is [Harry "harryr0se"](https://github.com/harryr0se)), with changes to logger editor to allow adding new channels and changing channel colors. Its compatible with orginal Unity-Logger so you can easily replace old one with this, but due to this compatibility it requires some code generation to be done, so in Logger Editor window there is a button to generate scripts for channels and script with color for each channel.

We also added support for UPM.

<img align="center" width="559" height="703" src="./Images~/example.png">

```C#
Logger.Log(Channel.AI, "Finding NPC paths");
Logger.Log(Channel.Audio, "Loading audio banks");
Logger.Log(Channel.Loading, "Begin Load");
Logger.Log(Channel.Loading, Priority.Error, "Load failed");
```
---

Enhanced Unity Logger is a simple channel driven logging script for use with Unity.

Due to it's nicely formatted output, it also allows the user to easily distinguish between the different channels.

Provided in this repo is also a simple editor script which allows the user to turn channels on/off, change channel color and add/remove channels, this works both in play mode and whilst editing.

# Installation

- open <kbd>Window/Package Manager</kbd>
- click <kbd>+</kbd>
- click <kbd>Add package from git URL</kbd> or <kbd>Add package by name</kbd>
- Add `https://github.com/FurrField-Studio/EnhancedUnityLogger.git` in Package Manager
