# Function to wait for a service to be healthy
function Wait-ForService {
    param (
        [string]$Url,
        [int]$MaxAttempts = 30,
        [int]$DelaySeconds = 2
    )
    
    $attempts = 0
    $isHealthy = $false
    
    Write-Host "Waiting for service at $Url to be healthy..."
    
    while (-not $isHealthy -and $attempts -lt $MaxAttempts) {
        try {
            $response = Invoke-RestMethod -Uri $Url -Method Get -ErrorAction Stop
            if ($response.status -eq "healthy") {
                $isHealthy = $true
                Write-Host "Service at $Url is healthy!"
            }
        }
        catch {
            $attempts++
            Write-Host ("Attempt {0} of {1}: Service not ready yet. Waiting {2} seconds..." -f $attempts, $MaxAttempts, $DelaySeconds)
            Start-Sleep -Seconds $DelaySeconds
        }
    }
    
    if (-not $isHealthy) {
        throw "Service at $Url did not become healthy after $MaxAttempts attempts"
    }
}

# Build Container Agent
Write-Host "Building Container Agent..."
docker build -f .\src\GEOrchestrator.ContainerAgent\Dockerfile -t function .

# Build Task Images
Write-Host "Building Task Images..."
docker build -t download-dem ./definitions/raster-calculations/containers/download-dem
docker build -t process-dem ./definitions/raster-calculations/containers/process-dem
docker build -t reproject-raster ./definitions/raster-calculations/containers/reproject-raster

# Start Docker Compose Services
Write-Host "Starting Docker Compose Services..."
docker compose -f docker-compose.yml up -d --build

# Wait for API to be healthy
Write-Host "Waiting for API to be healthy..."
Wait-ForService -Url "http://localhost:8000/health"  # API

# Create Tasks
Write-Host "Creating Tasks..."
$taskFiles = @(
    "definitions/raster-calculations/download-dem.yaml",
    "definitions/raster-calculations/process-dem.yaml",
    "definitions/raster-calculations/reproject.yaml"
)

foreach ($taskFile in $taskFiles) {
    $taskContent = Get-Content -Path $taskFile -Raw
    $response = Invoke-RestMethod -Uri "http://localhost:8000/tasks" -Method Post -Body $taskContent -ContentType "application/x-yaml"
    Write-Host "Created task from $taskFile"
}

# Create Workflow
Write-Host "Creating Workflow..."
$workflowContent = Get-Content -Path "definitions/raster-calculations/workflow.yaml" -Raw
$response = Invoke-RestMethod -Uri "http://localhost:8000/workflows" -Method Post -Body $workflowContent -ContentType "application/x-yaml"
Write-Host "Created workflow from workflow.yaml" 