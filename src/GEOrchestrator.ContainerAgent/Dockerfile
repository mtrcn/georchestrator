FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS base

# Set working directory
WORKDIR /src

# Copy only the project file(s) and restore dependencies
COPY src/GEOrchestrator.ContainerAgent/*.csproj ./GEOrchestrator.ContainerAgent/
RUN dotnet restore "GEOrchestrator.ContainerAgent/GEOrchestrator.ContainerAgent.csproj"

# Copy the rest of the source code
COPY src/ ./

WORKDIR /src/GEOrchestrator.ContainerAgent/

RUN dotnet publish "GEOrchestrator.ContainerAgent.csproj" \
    --runtime linux-x64 \
    -c Release \
    -o /out \
    -p:PublishSingleFile=true \
    -p:DebugType=None \
    -p:PublishTrimmed=true \
    -p:TrimMode=Link \
    --self-contained true

# build runtime image
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine
WORKDIR /function_runner
COPY --from=base /out ./