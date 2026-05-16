# ── Stage 1: Build ──────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy solution file
COPY TLearnBe/*.sln ./

# Copy csproj files
COPY TLearnBe/TLearn.API/*.csproj TLearn.API/
COPY TLearnBe/TLearn.Application/*.csproj TLearn.Application/
COPY TLearnBe/TLearn.Domain/*.csproj TLearn.Domain/
COPY TLearnBe/TLearn.Infrastructure/*.csproj TLearn.Infrastructure/
COPY TLearnBe/TLearn.Common/*.csproj TLearn.Common/

# Restore
RUN dotnet restore TLearnBe.sln

# Copy full source
COPY . .

# Publish
RUN dotnet publish TLearnBe/TLearn.API/TLearn.API.csproj \
    -c Release \
    -o /app/publish

# ── Runtime ─────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "TLearn.API.dll"]