param ([string] $ApiKey, [string]$Suffix = "")

# versioning info
$VERSION = "$(Get-Date -UFormat "%Y.%m%d").$($env:GITHUB_RUN_NUMBER)$($Suffix)"
$WORKINGDIR = Get-Location

# nuget clear & restore
nuget locals all -clear
nuget restore

# build files
Write-Output "Building OnionFruit.Status Version $VERSION"
dotnet build -c Release /p:PackageVersion=$VERSION /p:Version=$VERSION

# pack into nuget files with the suffix if we have one
Write-Output "Publishing OnionFruit.Status Version $VERSION"
dotnet pack ".\DragonFruit.OnionFruit.Status\DragonFruit.OnionFruit.Status.csproj" -o $WORKINGDIR -c Release -p:PackageVersion=$VERSION

# recursively push all nuget files created
Get-ChildItem -Path $WORKINGDIR -Filter *.nupkg -Recurse -File -Name | ForEach-Object {
    dotnet nuget push $_ --api-key $ApiKey --source https://api.nuget.org/v3/index.json --force-english-output
}
