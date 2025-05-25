#!/bin/bash

# Function to wait for a service to be healthy
wait_for_service() {
    local url=$1
    local max_attempts=${2:-30}
    local delay_seconds=${3:-2}
    local attempts=0
    local is_healthy=false

    echo "Waiting for service at $url to be healthy..."

    while [ "$is_healthy" = false ] && [ $attempts -lt $max_attempts ]; do
        if curl -s "$url" | grep -q '"status":"healthy"'; then
            is_healthy=true
            echo "Service at $url is healthy!"
        else
            attempts=$((attempts + 1))
            echo "Attempt $attempts of $max_attempts: Service not ready yet. Waiting $delay_seconds seconds..."
            sleep $delay_seconds
        fi
    done

    if [ "$is_healthy" = false ]; then
        echo "Service at $url did not become healthy after $max_attempts attempts"
        exit 1
    fi
}

# Build Container Agent
echo "Building Container Agent..."
docker build -f ./src/GEOrchestrator.ContainerAgent/Dockerfile -t function .

# Build Task Images
echo "Building Task Images..."
docker build -t download-dem ./definitions/raster-calculations/containers/download-dem
docker build -t process-dem ./definitions/raster-calculations/containers/process-dem
docker build -t reproject-raster ./definitions/raster-calculations/containers/reproject-raster

# Start Docker Compose Services
echo "Starting Docker Compose Services..."
docker compose -f docker-compose.yml up -d --build

# Wait for services to be healthy
echo "Waiting for API to be healthy..."
wait_for_service "http://localhost:8000/health"  # API

# Create Tasks
echo "Creating Tasks..."
task_files=(
    "definitions/raster-calculations/download-dem.yaml"
    "definitions/raster-calculations/process-dem.yaml"
    "definitions/raster-calculations/reproject.yaml"
)

for task_file in "${task_files[@]}"; do
    curl -X POST "http://localhost:8000/tasks" \
        -H "Content-Type: application/x-yaml" \
        --data-binary @"$task_file"
    echo "Created task from $task_file"
done

# Create Workflow
echo "Creating Workflow..."
curl -X POST "http://localhost:8000/workflows" \
    -H "Content-Type: application/x-yaml" \
    --data-binary @"definitions/raster-calculations/workflow.yaml"
echo "Created workflow from workflow.yaml" 