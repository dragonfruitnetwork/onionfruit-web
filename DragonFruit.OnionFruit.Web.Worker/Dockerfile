ARG VERSION=2023.1
ARG VARIANT=alpine

FROM mcr.microsoft.com/dotnet/sdk:7.0-${VARIANT} AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish -p:Version=${VERSION} /p:UseAppHost=false DragonFruit.OnionFruit.Web.Worker

FROM mcr.microsoft.com/dotnet/runtime:7.0-${VARIANT} AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DragonFruit.OnionFruit.Web.Worker.dll"]
