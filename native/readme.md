# OnionFruit Web Worker Native Library

[![GitHub Actions NuGet](https://img.shields.io/badge/GitHub_Actions-blue?logo=nuget&logoColor=blue&label=NuGet)](https://github.com/dragonfruitnetwork/onionfruit-web/pkgs/nuget/DragonFruit.OnionFruit.Web.Worker.Native)
[![Build Worker Native Libs](https://github.com/dragonfruitnetwork/onionfruit-web/actions/workflows/build-native.yml/badge.svg)](https://github.com/dragonfruitnetwork/onionfruit-web/actions/workflows/build-native.yml)

### Overview
This is a project that processes a list of networks (from location.db) and return the IP address ranges in order, grouped by country code.
It's used by the main worker to effectively replicate the ranges produced by the Tor CLI tool.

- `/src` holds the main interop library, written in Rust
- `/nuget` holds a .NET project used as a container for the native libraries

### Packages
Packages are built on-demand by GitHub Actions for the platforms listed below:

- Linux (GNU/musl; x64 and aarch64)
- Windows (MSVC; x64 and arm64)
- macOS (Universal library)

The package can be integrated in a project by adding a `NuGet.config` to the solution root. For added security, it's recommended to restrict the package source to a specific feed:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="dragonfruit" value="https://nuget.pkg.github.com/dragonfruitnetwork/index.json" />
  </packageSources>
  <allowedPackageSources>
    <add key="DragonFruit.OnionFruit.Web.Worker.Native" value="dragonfruit" />
  </allowedPackageSources>
</configuration>
```

For example usage refer to:
- [NativeMethods](https://github.com/dragonfruitnetwork/onionfruit-web/blob/vnext/DragonFruit.OnionFruit.Web.Worker/Native/NativeMethods.cs) for public method declarations
- [NativeStructs](https://github.com/dragonfruitnetwork/onionfruit-web/blob/vnext/DragonFruit.OnionFruit.Web.Worker/Native/NativeStructs.cs) for struct formats
- [LocationDbSource](https://github.com/dragonfruitnetwork/onionfruit-web/blob/vnext/DragonFruit.OnionFruit.Web.Worker/Sources/LocationDbSource.cs) for usage example

### License
This library is licenced under Apache-2.0.
Portions of this code are derived from the [Tor location.db CLI tool](https://gitlab.torproject.org/tpo/core/tor/-/tree/main/scripts/maint/geoip/geoip-db-tool) by Nick Mathewson <nickm@torproject.org>