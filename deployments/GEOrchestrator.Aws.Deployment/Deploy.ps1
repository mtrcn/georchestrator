dotnet publish ../../src/GEOrchestrator.TaskManager/GEOrchestrator.TaskManager.csproj -c Release -o ../release/task-manager-release
Compress-Archive -Force -Path ../release/task-manager-release/* -DestinationPath ../release/task-manager-release/release.zip
dotnet publish ../../src/GEOrchestrator.WorkflowManager/GEOrchestrator.WorkflowManager.csproj -c Release  -o ../release/workflow-manager-release
Compress-Archive  -Force -Path ../release/workflow-manager-release/* -DestinationPath ../release/workflow-manager-release/release.zip
pulumi up