# 1. Base image olarak .NET 8 runtime kullan
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
# Railway kendi PORT değişkenini set ediyor, biz de dinlemeliyiz
ENV ASPNETCORE_URLS=http://+:${PORT}
EXPOSE 8080

# 2. Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PizzaStore.csproj", "./"]
RUN dotnet restore "./PizzaStore.csproj"
COPY . .
RUN dotnet publish "PizzaStore.csproj" -c Release -o /app/publish

# 3. Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PizzaStore.dll"]

