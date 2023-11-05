ARG IMAGE_VARIANT="7.0-alpine"
ARG ENTRYPOINT_FILENAME="DragonFruit.OnionFruit.Web.dll"

FROM mcr.microsoft.com/dotnet/runtime:${IMAGE_VARIANT}
WORKDIR /app

COPY . .
ENV CONFIG_FOLDER_PATH="./config"
ENTRYPOINT ["dotnet", "${ENTRYPOINT_FILENAME}"]