# ---------- Build stage: compile and publish the API with the .NET 8 SDK ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore packages first (this layer is cached as long as the .csproj does not change)
COPY src/PlaylistApi/PlaylistApi.csproj src/PlaylistApi/
RUN dotnet restore src/PlaylistApi/PlaylistApi.csproj

# Copy the rest of the source code and publish a Release build
COPY . .
RUN dotnet publish src/PlaylistApi/PlaylistApi.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- Runtime stage: small image that only contains the ASP.NET Core runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
# Render passes the port in the PORT variable (Program.cs reads it); 8080 is used when PORT is not set.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PlaylistApi.dll"]
