ARG IMAGE_VARIANT="7.0-alpine"

FROM mcr.microsoft.com/dotnet/runtime:${IMAGE_VARIANT}
WORKDIR /app

COPY . .
ENV CONFIG_FOLDER_PATH="./config"
ENTRYPOINT ["dotnet", "DragonFruit.OnionFruit.Web.Worker.dll"]