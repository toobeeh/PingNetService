FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

# grpc
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["tobeh.PingNetService.Server.csproj", "tobeh.PingNetService.Server/"]
RUN dotnet restore "tobeh.PingNetService.Server/tobeh.PingNetService.Server.csproj"
COPY . ./tobeh.PingNetService.Server
WORKDIR "/src/tobeh.PingNetService.Server"
#RUN ls -lR
RUN dotnet build "tobeh.PingNetService.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "tobeh.PingNetService.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

CMD ["sh", "-c", "dotnet tobeh.PingNetService.Server.dll"]