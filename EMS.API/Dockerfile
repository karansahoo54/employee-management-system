# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files for cached layer restore
COPY ["EMS.Domain/EMS.Domain.csproj", "EMS.Domain/"]
COPY ["EMS.Infrastructure/EMS.Infrastructure.csproj", "EMS.Infrastructure/"]
COPY ["EMS.Application/EMS.Application.csproj", "EMS.Application/"]
COPY ["EMS.API/EMS.API.csproj", "EMS.API/"]

RUN dotnet restore "EMS.API/EMS.API.csproj"

# Copy full source and build
COPY . .
WORKDIR "/src/EMS.API"
RUN dotnet publish "EMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Production Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EMS.API.dll"]
