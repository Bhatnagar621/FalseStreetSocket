# Use official .NET 8 image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Copy the project files
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["FalseStreetSocket.csproj", "./"]
RUN dotnet restore "FalseStreetSocket.csproj"

COPY . .
RUN dotnet publish "FalseStreetSocket.csproj" -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FalseStreetSocket.dll"]
