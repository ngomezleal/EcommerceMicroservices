FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/OrderService/OrderService.Api/OrderService.Api.csproj", "src/OrderService/OrderService.Api/"]
COPY ["src/OrderService/OrderService.Application/OrderService.Application.csproj", "src/OrderService/OrderService.Application/"]
COPY ["src/OrderService/OrderService.Domain/OrderService.Domain.csproj", "src/OrderService/OrderService.Domain/"]
COPY ["src/OrderService/OrderService.Infrastructure/OrderService.Infrastructure.csproj", "src/OrderService/OrderService.Infrastructure/"]
RUN dotnet restore "src/OrderService/OrderService.Api/OrderService.Api.csproj"
COPY . .
WORKDIR "/src/src/OrderService/OrderService.Api"
RUN dotnet publish "OrderService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderService.Api.dll"]
