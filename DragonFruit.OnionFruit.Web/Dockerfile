FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine
WORKDIR /app

COPY . .
ARG ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "DragonFruit.OnionFruit.Web.dll"]
