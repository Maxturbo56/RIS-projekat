# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so Docker can cache NuGet packages between builds.
COPY src/AstralNexus.Web/AstralNexus.Web.csproj src/AstralNexus.Web/
RUN dotnet restore src/AstralNexus.Web/AstralNexus.Web.csproj

# Copy the remaining project and publish a Release build.
COPY . .
RUN dotnet publish src/AstralNexus.Web/AstralNexus.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AstralNexus.Web.dll"]
