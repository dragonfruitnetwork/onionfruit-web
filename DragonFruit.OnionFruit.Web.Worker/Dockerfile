ARG IMAGEVARIANT="alpine"

FROM mcr.microsoft.com/dotnet/runtime:8.0-${IMAGEVARIANT}
WORKDIR /app

COPY . .
ENV CONFIG_FOLDER_PATH="/config"
ENTRYPOINT ["dotnet", "DragonFruit.OnionFruit.Web.Worker.dll"]