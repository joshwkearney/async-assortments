$linqProject = "${PSScriptRoot}\..\src\AsyncLinq\AsyncLinq.csproj"
$outputPath = "${PSScriptRoot}\..\publish"

if (Test-Path -Path $outputPath) {
	Remove-Item -Path "${outputPath}\*"
}

dotnet pack $linqProject --output $outputPath