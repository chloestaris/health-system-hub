FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["HealthHub.API/HealthHub.API.csproj", "HealthHub.API/"]
RUN dotnet restore "HealthHub.API/HealthHub.API.csproj"

# Copy the rest of the code
COPY . .
WORKDIR "/src/HealthHub.API"
RUN dotnet build "HealthHub.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HealthHub.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directory for certificates
RUN mkdir -p /app/certs

EXPOSE 443
ENTRYPOINT ["dotnet", "HealthHub.API.dll"] 