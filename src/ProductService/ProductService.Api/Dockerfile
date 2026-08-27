FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY ["Directory.Build.props", "./"]
COPY ["src/ProductService/ProductService.Api/ProductService.Api.csproj", "src/ProductService/ProductService.Api/"]
COPY ["src/ProductService/ProductService.Application/ProductService.Application.csproj", "src/ProductService/ProductService.Application/"]
COPY ["src/ProductService/ProductService.Infrastructure/ProductService.Infrastructure.csproj", "src/ProductService/ProductService.Infrastructure/"]
COPY ["src/ProductService/ProductService.Domain/ProductService.Domain.csproj", "src/ProductService/ProductService.Domain/"]

RUN dotnet restore "src/ProductService/ProductService.Api/ProductService.Api.csproj"

COPY . .
WORKDIR "/src/src/ProductService/ProductService.Api"
RUN dotnet publish "ProductService.Api.csproj" --configuration Release --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ProductService.Api.dll"]
