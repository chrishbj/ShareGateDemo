FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/ShareGateDemo.Api/ShareGateDemo.Api.csproj", "src/ShareGateDemo.Api/"]
COPY ["src/ShareGateDemo.Shared/ShareGateDemo.Shared.csproj", "src/ShareGateDemo.Shared/"]
RUN dotnet restore "src/ShareGateDemo.Api/ShareGateDemo.Api.csproj"
COPY . .
WORKDIR "/src/src/ShareGateDemo.Api"
RUN dotnet publish "ShareGateDemo.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ShareGateDemo.Api.dll"]