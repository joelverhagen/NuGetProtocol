$buildDir = Join-Path $PSScriptRoot "build"
$artifactDir = Join-Path $PSScriptRoot "artifacts"
$tempDir = Join-Path $artifactDir "temp"

$sln = (Join-Path $PSScriptRoot "Knapcode.NuGetProtocol.sln")
$nuget = Join-Path $buildDir "nuget.exe"

Function New-Package {
    param($Id)
    
    if (Test-Path $tempDir) {
        Remove-Item $tempDir -Force -Recurse
        New-Item $tempDir -Type Directory | Out-Null
    }

    $nuspec = Join-Path $PSScriptRoot -ChildPath "src" | Join-Path -ChildPath $Id | Join-Path -ChildPath "$Id.nuspec"
    & $nuget pack $nuspec -OutputDirectory $tempDir

    $tempNupkg = Get-ChildItem (Join-Path $tempDir "*.nupkg")
    $nupkg = Join-Path $artifactDir $tempNupkg.Name

    if (-Not (Test-Path $nupkg)) {
        Move-Item $tempNupkg $nupkg
    } else {
        Write-Output "Package $($tempNupkg.Name) has already been packed."
    }

    Remove-Item $tempDir -Force -Recurse
}

if (-Not (Test-Path $buildDir)) {
    New-Item $buildDir -Type Directory | Out-Null
}

if (-Not (Test-Path $nuget)) {
    Invoke-WebRequest "https://dist.nuget.org/win-x86-commandline/v4.0.0-rc4/nuget.exe" -OutFile $nuget
}

msbuild /t:Restore $sln
msbuild /t:Build $sln

if (-Not (Test-Path $artifactDir)) {
    New-Item $artifactDir -Type Directory | Out-Null
}

New-Package "K.Np.FullMetadata"
New-Package "K.Np.Unlisted"
