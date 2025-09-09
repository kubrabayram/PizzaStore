# 1. Base image olarak .NET 8 SDK kullan
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PizzaStore.csproj", "./"]
RUN dotnet restore "./PizzaStore.csproj"
COPY . .
RUN dotnet publish "PizzaStore.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PizzaStore.dll"]
