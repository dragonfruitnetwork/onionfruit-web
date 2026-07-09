ARG IMAGEVARIANT="alpine"

FROM mcr.microsoft.com/dotnet/aspnet:10.0-${IMAGEVARIANT}
WORKDIR /app

COPY . .
ENTRYPOINT ["dotnet", "DragonFruit.OnionFruit.Web.dll"]
