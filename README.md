<img width="64" height="64" alt="IndiCapsLogo" src="https://github.com/user-attachments/assets/072d4b5a-c9ce-4adc-94b9-24953568442c" />




# IndiCaps

**IndiCaps v1.0.0** — a Caps Lock indicator for Windows.

Every time you press Caps Lock, a small elegant notification appears in the bottom-right corner of the screen, then fades away. The app runs quietly in the background from the notification area — no window, no interference.

Designed and crafted by **Jovatis**.

---

## Features

- Overlay notification on every Caps Lock press (PNG-based design)
- Always on top, but **click-through**: it never steals a click or the focus
- Optional toggle sound
- Settings panel:
  - Language: English / French
  - Indicator on/off
  - Sound on/off
  - Animation (fade) on/off
  - Launch at Windows startup
- Icon in the notification area (hidden icons)
- Single instance (no duplicate on double launch)
- Configuration stored in `%APPDATA%\IndiCaps\config.json`

---

## Installation (plug & play)

Download the `IndiCaps-1.0.0.zip` file from the **GitHub Releases** page. The zip contains:

```
IndiCaps-1.0.0/
├── IndiCaps.exe
└── assets/
    ├── bgon.png
    ├── bgoff.png
    ├── capson.wav
    ├── capsoff.wav
    └── Poppins-Regular.ttf
```

Extract the folder anywhere you like and run `IndiCaps.exe`.

**Important:** the `assets` folder must stay **next to** `IndiCaps.exe` (that's where the app looks for it). As long as it does, you can move `IndiCaps.exe` anywhere. To quit: right-click its icon in the notification area → "Quit".

---

## Building from source

Requirements: [.NET 8 SDK](https://dotnet.microsoft.com/download) (Windows).

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o dist
```

The output is `dist\IndiCaps.exe` (self-contained, no .NET install required, ~68 MB once compressed). Then copy the `assets` folder next to the exe.

---

## Project structure

```
IndiCaps.csproj        .NET 8 project (WinForms, single-file)
assets/                resources (images, sounds, font)
src/                   C# source (app, UI, P/Invoke, settings…)
dist/                  build output (ignored by git)
bin/ · obj/            compilation artifacts (ignored by git)
```

Build output (`bin/`, `obj/`, `dist/`) is **excluded from git** because it is regenerated on every build: the repository only holds source + assets, and the compiled exe is distributed through Releases.

---

## Credits

**IndiCaps** — designed and crafted by **Jovatis** · v1.0.0

Thanks for testing, reporting bugs, and starring the project if you like it.

---

## License note

IndiCaps is MIT-licensed. Assets (images, sounds) remain the property of their authors. The bundled font is **Poppins**, released under the SIL Open Font License — free to use and redistribute, including in commercial applications.
