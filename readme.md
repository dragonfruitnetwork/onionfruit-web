# OnionFruit.Status
[![Build Status](https://travis-ci.com/dragonfruitnetwork/OnionFruit.Status.svg?branch=master)](https://travis-ci.com/dragonfruitnetwork/OnionFruit.Status)
The Tor Relay Information Lookup system to supersede the current version in OnionFruit™ Connect `5.1` (`2019.10x`)

## What does it do?
Similar to [https://torstatus.blutmagie.de/](https://torstatus.blutmagie.de/), `OnionFruit.Status` uses the Onionoo API to get details on all the publicly available nodes. It will then deserialize the JSON into an easy-to-integrate model. The `HTTPClient` is already integrated, but the user will need to catch the errors.

## How to use it
It's easy. You can download our copy from NuGet or if you need the bits that are commented out, download and paste into your project, Import the `.csproj` then find the `RelayInfo.cs` file and start uncommenting. If you need some reference, you can find the raw datasheet at [https://metrics.torproject.org/onionoo.html#details](https://metrics.torproject.org/onionoo.html#details)
Check out the test project's `Program.cs` to see how to use it.

## There seems to be a lot of green space on here...
This app could __techincally__ do the bridge nodes (which can be sent as well), but we are using it to get a list of countries for in/out traffic and to display info on our new site (DragonFruit Yuna), so we don't need this. You'll find that large sections are commented out. They do work, just (as stated above) we don't need the functionality. It also makes the app faster if we use abstraction.

## License
This is one of the open-source components used by OnionFruit™ Connect. It is licensed under the `MIT` Licence.