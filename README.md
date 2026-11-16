# RetroJ2ME

**A modern desktop launcher for classic Java ME (J2ME) mobile games.**

RetroJ2ME brings the nostalgic era of Java mobile gaming to modern desktop computers through a clean, organized interface and an integrated FreeJ2ME-Plus emulator.

## Features

- **Game Library** – Organize and manage your J2ME games.
- **JAR Import** – Import Java ME games using `.jar` files.
- **Integrated Emulator** – Launch games using FreeJ2ME-Plus.
- **Device Profiles** – Explore classic mobile device profiles.
- **Recently Played** – Access your recently launched games.
- **Settings** – Configure your emulator experience.
- **Bundled Java Runtime** – Windows installer includes a Java runtime.

## Screenshots

Screenshots will be added soon.

## Windows Installation

1. Download `RetroJ2ME-Setup.exe` from the Releases section.
2. Run the installer.
3. Complete the installation.
4. Open RetroJ2ME.
5. Import your `.jar` game files.
6. Select a game and launch it.

## Requirements

- Windows 10 or later (64-bit).
- Java runtime is bundled with the Windows installer.

## Technology Stack

- C#
- .NET 10
- Avalonia UI
- Java
- FreeJ2ME-Plus

## Project Structure

```text
RetroJ2ME/
├── Emulator/
├── config/
├── docs/
├── freej2me_system/
├── packaging/
├── profiles/
├── src/
│   └── RetroJ2ME.Desktop/
│       ├── Assets/
│       ├── Emulator/
│       ├── Models/
│       ├── ViewModels/
│       └── Views/
├── tests/
├── tools/
├── RetroJ2ME.iss
└── README.md
```

## Development

Clone the repository:

```powershell
git clone https://github.com/SAthyEAH2929/RetroJ2ME.git
cd RetroJ2ME
```

Open the solution/project in Visual Studio or build the desktop project using the .NET SDK.

## Emulator Credits

RetroJ2ME integrates the FreeJ2ME-Plus emulator.

- [FreeJ2ME-Plus GitHub Repository](https://github.com/TASEmulators/freej2me-plus)

Please refer to the emulator project's license and attribution requirements.

## License

License information will be added after the project licensing terms are finalized.

---

**RetroJ2ME** – Bringing classic Java mobile gaming back to desktop.