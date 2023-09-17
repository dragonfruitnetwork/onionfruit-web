# OnionFruit™ Web
The web-facing components that power OnionFruit&trade; clients

[![DragonFruit Discord](https://img.shields.io/discord/482528405292843018?label=Discord&style=popout)](https://discord.gg/VA26u5Z)

## Overview
This repo holds the server-side components used to provide OnionFruit&trade; clients with geolocation data, alongside Tor connection detection services.

The system is split into two systems:
- The worker process that collects and produces the data required to serve user requests
- The server responsible for serving user-initiated requests for data (designed to scale horizontally independent of the worker process)

### Deployment

WIP - will be completed once the project is more complete.

### License
These libraries and components are licensed under Apache 2.0. Refer to the license file for more info.
