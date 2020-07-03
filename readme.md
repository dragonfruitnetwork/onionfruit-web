# OnionFruit.Status

![CI](https://github.com/dragonfruitnetwork/OnionFruit.Status/workflows/Publish/badge.svg)
[![Nuget](https://img.shields.io/nuget/v/DragonFruit.OnionFruit.Status)](https://www.nuget.org/packages/DragonFruit.OnionFruit.Status)
[![DragonFruit Discord](https://img.shields.io/discord/482528405292843018?label=Discord&style=popout)](https://discord.gg/VA26u5Z)

## Overview

This is the Tor Relay Information Lookup system to supersede the current version in OnionFruit™ Connect `5.1` (`2019.10x`)

### What does it do?
Similar to (the late) [https://torstatus.blutmagie.de/](https://torstatus.blutmagie.de/), `OnionFruit.Status` uses the Onionoo API to get details on all the publicly available nodes. It will then deserialize the JSON into an easy-to-integrate model.

## How to use
It's easy. You can download our copy from NuGet (`DragonFruit.OnionFruit.Status`), create a new `ApiClient` (parameterless or with settings) and then invoke the extension `GetServerInfo` to get the server info.
Check out the test project's `Program.cs` if you're still unsure how to use it.

## License
This is one of the open-source components used by OnionFruit™ Connect. It is licensed under the `MIT` Licence.
