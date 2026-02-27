# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.sln .
COPY src/Chaalbaaz.API/*.csproj ./src/Chaalbaaz.API/
COPY src/Chaalbaaz.Core/*.csproj ./src/Chaalbaaz.Core/
COPY src/Chaalbaaz.Application/*.csproj ./src/Chaalbaaz.Application/
COPY src/Chaalbaaz.Infrastructure/*.csproj ./src/Chaalbaaz.Infrastructure/

RUN dotnet restore

COPY . .
RUN dotnet publish src/Chaalbaaz.API/Chaalbaaz.API.csproj -c Release -o /out

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /out .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Chaalbaaz.API.dll"]
