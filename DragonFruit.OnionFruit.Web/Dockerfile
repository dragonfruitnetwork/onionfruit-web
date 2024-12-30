ARG IMAGEVARIANT="alpine"

FROM mcr.microsoft.com/dotnet/aspnet:9.0-${IMAGEVARIANT}
WORKDIR /app

COPY . .
ENV CONFIG_FOLDER_PATH="/config"
ENTRYPOINT ["dotnet", "DragonFruit.OnionFruit.Web.dll"]