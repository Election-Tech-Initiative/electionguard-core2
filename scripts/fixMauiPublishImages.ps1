#fixMauiPublishImages.ps1
param
(
	[Parameter(Mandatory=$true)][string]$StagingDir,	
	[Parameter(Mandatory=$true)][string]$OutputDir
)

#if maui ever fixes this issue, remove this script https://github.com/dotnet/maui/issues/10526

$OutputPath = "$OutputDir\Electionguard.UI"

#Copy only the files we want, but preserving directory structure
Write-Host "Copying file(s)..."
robocopy $StagingDir $OutputDir *.png *.ico *.html *.svg *.sql *.ttf *.otf /s /nfl /ndl

Write-Host "Done."