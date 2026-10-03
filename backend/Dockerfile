# Stage 1: Base Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 5158
ENV ASPNETCORE_URLS=http://+:5158;http://+:8080

# Stage 2: SDK Build & Restore
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first for optimized Docker layer caching
COPY ["ServiceRequestManagementSystem.API/ServiceRequestManagementSystem.API.csproj", "ServiceRequestManagementSystem.API/"]
RUN dotnet restore "ServiceRequestManagementSystem.API/ServiceRequestManagementSystem.API.csproj"

# Copy remaining source code and build
COPY . .
WORKDIR "/src/ServiceRequestManagementSystem.API"
RUN dotnet build "ServiceRequestManagementSystem.API.csproj" -c Release -o /app/build

# Stage 3: Publish
FROM build AS publish
RUN dotnet publish "ServiceRequestManagementSystem.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final Production Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServiceRequestManagementSystem.API.dll"]
