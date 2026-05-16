# ── Stage 1: Build ──────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy solution và csproj trước để tận dụng cache
COPY *.sln ./

# Copy các file csproj
COPY TLearn.API/*.csproj TLearn.API/
COPY TLearn.Application/*.csproj TLearn.Application/
COPY TLearn.Domain/*.csproj TLearn.Domain/
COPY TLearn.Infrastructure/*.csproj TLearn.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy toàn bộ source
COPY . .

# Publish project
RUN dotnet publish TLearn.API/TLearn.API.csproj \
    -c Release \
    -o /app/publish

# ── Stage 2: Runtime ────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

# Copy output từ build stage
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "TLearn.API.dll"]