# ==========================================
# Stage 1: Build & Publish
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first to maximize Docker layer caching
COPY ["EMS.Domain/EMS.Domain.csproj", "EMS.Domain/"]
COPY ["EMS.Application/EMS.Application.csproj", "EMS.Application/"]
COPY ["EMS.Infrastructure/EMS.Infrastructure.csproj", "EMS.Infrastructure/"]
COPY ["EMS.API/EMS.API.csproj", "EMS.API/"]

# Restore NuGet dependencies
RUN dotnet restore "EMS.API/EMS.API.csproj"

# Copy the rest of the source code
COPY . .

# Build and publish the application in Release mode
WORKDIR "/src/EMS.API"
RUN dotnet publish "EMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ==========================================
# Stage 2: Production Runtime
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install libfontconfig1 required by QuestPDF for PDF generation in Linux
RUN apt-get update \
    && apt-get install -y --no-install-recommends libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*

# Copy compiled application from build stage
COPY --from=build /app/publish .

# Expose port and configure ASP.NET Core for Render / reverse-proxy environments
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

ENTRYPOINT ["dotnet", "EMS.API.dll"]
