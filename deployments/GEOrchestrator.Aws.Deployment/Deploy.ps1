dotnet publish ../../src/GEOrchestrator.Api/GEOrchestrator.Api.csproj -c Release -o ../release/api-release
Compress-Archive -Force -Path ../release/api-release/* -DestinationPath ../release/api-release/release.zip
pulumi up