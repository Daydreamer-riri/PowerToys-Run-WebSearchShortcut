## Install

```ps1
powershell -ExecutionPolicy Bypass -Command "iex ((New-Object System.Net.WebClient).DownloadString('https://raw.githubusercontent.com/Daydreamer-riri/PowerToys-Run-WebSearchShortcut/main/CmdPalWebSearchShortcut/Install.ps1'))"
```

This command will:
1. Download the latest version of the installer script from GitHub
2. Automatically detect your system architecture (ARM64 or x64)
3. Download the appropriate MSIX package for your system
4. Install the package
5. Clean up downloaded files after installation