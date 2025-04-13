# Dynamically get the latest CmdPal version number
function Get-LatestCmdPalVersion {
    try {
        $releases = Invoke-RestMethod -Uri "https://api.github.com/repos/Daydreamer-riri/PowerToys-Run-WebSearchShortcut/releases" -ErrorAction Stop
        # Debug output
        Write-Host "API Response received. Found $($releases.Count) releases."
        Write-Host "Releases: $($releases | Select-Object -ExpandProperty tag_name)"

        # Find the first tag that starts with CmdPal using direct access
        $fullVersion = $null
        foreach ($release in $releases) {
            if ($release.tag_name -like "CmdPal*") {
                $fullVersion = $release.tag_name
                Write-Host "Found CmdPal release: $fullVersion"
                break  # Stop at first match
            }
        }

        if ($fullVersion) {
            # Extract numeric part (remove "CmdPal" prefix)
            $numericVersion = $fullVersion -replace "CmdPal", ""
            return $numericVersion
        }

        Write-Warning "No CmdPal version tag found, using default version"
        return "0.0.1" # Default version
    }
    catch {
        Write-Warning "Failed to get version information: $_"
        Write-Warning "Using default version"
        return "0.0.1" # Default version
    }
}

# Get the latest version number
$version = Get-LatestCmdPalVersion
$fullTag = "CmdPal$version"

Write-Host "Version: $version (Tag: $fullTag)"

# Detect current system processor architecture
$architecture = [System.Environment]::GetEnvironmentVariable("PROCESSOR_ARCHITECTURE")
$arch = "x64"  # Default architecture

if ($architecture -eq "ARM64") {
    Write-Host "Detected ARM64 architecture"
    $arch = "arm64"
} else {
    Write-Host "Detected x64 architecture"
    $arch = "x64"
}

# Build download and local file paths
$fileName = "WebSearchShortcut_$($version).0_$arch.msix"
$outputPath = Join-Path -Path (Get-Location).Path -ChildPath $fileName
$downloadUrl = "https://github.com/Daydreamer-riri/PowerToys-Run-WebSearchShortcut/releases/download/$fullTag/$fileName"

# Download MSIX file from GitHub
Write-Host "Starting download: $downloadUrl"
Write-Host "Saving to: $outputPath"

try {
    Invoke-WebRequest -Uri $downloadUrl -OutFile $outputPath -ErrorAction Stop
    Write-Host "Download completed"

    # Install the downloaded package
    Write-Host "Installing: v$version"
    Add-AppPackage -Path $outputPath -AllowUnsigned
}
catch {
    Write-Error "Download or installation failed: $_"
    Write-Host "Please check your network connection and try again, or visit GitHub to manually download the latest version"
}
finally {
    # Clean up - remove the downloaded file whether installation succeeded or failed
    if (Test-Path $outputPath) {
        Write-Host "Cleaning up: Removing downloaded MSIX file"
        Remove-Item -Path $outputPath -Force
    }
}