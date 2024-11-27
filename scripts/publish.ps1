$linqProject = "${PSScriptRoot}\..\src\AsyncAssortments\AsyncAssortments.csproj"
$outputPath = "${PSScriptRoot}\..\publish"

if (Test-Path -Path $outputPath) {
	Remove-Item -Path "${outputPath}\*"
}
else {
	New-Item -Path $outputPath -ItemType "directory"
}

dotnet pack $linqProject --output $outputPath
