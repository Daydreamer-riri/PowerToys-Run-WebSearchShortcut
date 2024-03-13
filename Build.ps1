$ErrorActionPreference = "Stop"

[xml]$xml = Get-Content -Path "$PSScriptRoot\Directory.Build.Props"
$version = $xml.Project.PropertyGroup.Version

foreach ($platform in "ARM64", "x64")
{
    if (Test-Path -Path "$PSScriptRoot\Community.PowerToys.Run.Plugin.EdgeFavorite\bin")
    {
        Remove-Item -Path "$PSScriptRoot\Community.PowerToys.Run.Plugin.EdgeFavorite\bin\*" -Recurse
    }

    dotnet build $PSScriptRoot\Community.PowerToys.Run.Plugin.EdgeFavorite.sln -c Release /p:Platform=$platform

    Remove-Item -Path "$PSScriptRoot\Community.PowerToys.Run.Plugin.EdgeFavorite\bin\*" -Recurse -Include *.xml, *.pdb, PowerToys.*, Wox.*
    Rename-Item -Path "$PSScriptRoot\Community.PowerToys.Run.Plugin.EdgeFavorite\bin\$platform\Release" -NewName "EdgeFavorite"

    Compress-Archive -Path "$PSScriptRoot\Community.PowerToys.Run.Plugin.EdgeFavorite\bin\$platform\EdgeFavorite" -DestinationPath "$PSScriptRoot\EdgeFavorite-$version-$platform.zip"
}
