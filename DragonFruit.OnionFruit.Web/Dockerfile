FROM mcr.microsoft.com/dotnet/aspnet:11.0-resolute-chiseled

ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

WORKDIR /app
COPY . .

EXPOSE 8080
ENTRYPOINT ["dotnet", "DragonFruit.OnionFruit.Web.dll"]