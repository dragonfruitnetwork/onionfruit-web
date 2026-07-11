FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine
WORKDIR /app

COPY . .
ARG DOTNET_Environment=Production

ENTRYPOINT ["dotnet", "DragonFruit.OnionFruit.Web.Worker.dll"]