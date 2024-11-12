$linqProject = "${PSScriptRoot}\..\src\AsyncCollections.Linq"
$outputPath = "${PSScriptRoot}\..\publish"

if (Test-Path -Path $outputPath) {
	Remove-Item -Path "${outputPath}\*"
}

dotnet pack $linqProject --output $outputPath