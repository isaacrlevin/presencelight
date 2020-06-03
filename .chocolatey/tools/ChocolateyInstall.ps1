$ErrorActionPreference = 'Stop';

$toolsDir       = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$fileName       = "$toolsDir\PackagePath"
$version        = "ReplaceVersion"

$WindowsVersion=[Environment]::OSVersion.Version
if ($WindowsVersion.Major -ne "10") {
  throw "This package requires Windows 10."
}

if ((Get-AppxPackage -name 37828IsaacLevin.197278F15330A).Version -Match $version) {
  if($env:ChocolateyForce) {
    # you can't install the same version of an appx package, you need to remove it first
    Write-Host Removing already installed version first.
    Get-AppxPackage -Name 37828IsaacLevin.197278F15330A | Remove-AppxPackage
  } else {
    Write-Host The $version version of PresenceLight is already installed. If you want to reinstall use --force
    return
  }
}

Add-AppxPackage -Path $fileName
