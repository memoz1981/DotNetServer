# Base OS: Ubuntu-based .NET 8 runtime
FROM ubuntu/dotnet-runtime:8.0-24.04_stable

# Set the user to root (default is root, but we specify it explicitly here)
USER root

# Set working directory
WORKDIR /app

# Copy published app from host to container
COPY ./publish .

# Set entry point to your app's DLL
ENTRYPOINT ["dotnet", "DotNetServer.Debug.dll"]
