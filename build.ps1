$buildDir = Join-Path $PSScriptRoot "build"
$artifactDir = Join-Path $PSScriptRoot "artifacts"

$sln = (Join-Path $PSScriptRoot "Knapcode.NuGetProtocol.sln")
$nuget = Join-Path $buildDir "nuget.exe"

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

$knpaNuspec = Join-Path $PSScriptRoot -ChildPath "src" | Join-Path -ChildPath "K.Np.A" | Join-Path -ChildPath "K.Np.A.nuspec"
& $nuget pack $knpaNuspec -OutputDirectory $artifactDir

$knpbNuspec = Join-Path $PSScriptRoot -ChildPath "src" | Join-Path -ChildPath "K.Np.B" | Join-Path -ChildPath "K.Np.B.nuspec"
& $nuget pack $knpbNuspec -OutputDirectory $artifactDir
