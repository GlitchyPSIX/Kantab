# GLITCHYPSI Kantab
customizable hand overlay companion app for artist streamers

Compatibility status:

-------
OS | Status
---|---
Windows | ✅ Mouse Cursor (works with WinTab)*
Linux | ❌ (planned through OpenTabletDriver)
macOS | ❌ (planned through OpenTabletDriver, but no Mac to test on)

\* Windows Ink support will be provided with OpenTabletDriver and/or a browser extension in the future.

## Requirements (using Kantab)
[.NET Runtime 8.0 or greater.](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

## Installation
Head over to Releases and download the 7z of the latest version. Extract the compressed file and run `KantabDesktop.exe`.

The program is portable! It however creates a ShellLink in Windows which allows it to have a Notification icon and a Start Menu entry.

## Usage
When you open the Control Panel and start the server, check out the Quickstart guide for help and tips.

## Caveats
Currently, Windows Ink is not supported. Applications using Windows Ink will send no input to Kantab. Disable Windows Ink in applications to get mouse positioning.

Current implementation supports no pressure for WinTab. This is due to implementation challenges. However, it's possible for somebody to be able to make a Relay side-application that tells Kantab where the pen is, what tilt it has and what pressure it has at any given time, giving full functionality.

Linux and macOS are currently unsupported pending an OpenTabletDriver plugin, which would be able to easily relay pen information to Kantab. This however means replacing your existing driver, which may or may not be desirable.

## Building from source
This project is composed of two parts, the Server (Kantab/Kantab.GUI) and the actual HTML views (Kantab.HTML) that are used to display the Constructs and QuickStart pages.

You need the following to build Kantab from source:
- Visual Studio 2022 and .NET 8 SDK
- NodeJS v20 or greater (preferably LTS)

For the Visual Studio part of the project, you just build as usual. In the output directory, create a folder called `Views`.

For the NodeJS part, use `yarn install` to install the necessary packages. After that run the `build` script using ``yarn run build``. This will create a folder called `dist` in the project folder. Copy the contents of that folder to the `Views` folder in the Server's output directory.

## Licenses

- Kantab Server Library and Control Panel GUI
  - GNU LGPL-3.0
- Kantab QuickStart, except graphical assets
  - Creative Commons Attribution-NonCommercial-ShareAlike CC-BY-NC-SA 4.0 International
- Curly assets, Curly's character and design, Kantab Logo
  - Belongs to me, GlitchyPSI. Do not claim her design as yours and if you were to use her design for anything commercial you must talk with me first.