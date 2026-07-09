ARG IMAGEVARIANT="alpine"

FROM mcr.microsoft.com/dotnet/runtime:10.0-${IMAGEVARIANT}
WORKDIR /app

COPY . .
ENTRYPOINT ["dotnet", "DragonFruit.OnionFruit.Web.Worker.dll"]