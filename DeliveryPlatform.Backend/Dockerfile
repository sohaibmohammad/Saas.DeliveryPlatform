
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project files
COPY ["DeliveryPlatform.API/DeliveryPlatform.API.csproj", "DeliveryPlatform.API/"]
COPY ["DeliveryPlatform.Application/DeliveryPlatform.Application.csproj", "DeliveryPlatform.Application/"]
COPY ["DeliveryPlatform.Domain/DeliveryPlatform.Domain.csproj", "DeliveryPlatform.Domain/"]
COPY ["DeliveryPlatform.Infrastructure/DeliveryPlatform.Infrastructure.csproj", "DeliveryPlatform.Infrastructure/"]
RUN dotnet restore "DeliveryPlatform.API/DeliveryPlatform.API.csproj"
COPY . .
RUN dotnet publish "DeliveryPlatform.API/DeliveryPlatform.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "DeliveryPlatform.API.dll"]