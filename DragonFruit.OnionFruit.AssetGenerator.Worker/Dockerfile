FROM mcr.microsoft.com/dotnet/runtime:11.0-resolute-chiseled

ENV ASPNETCORE_ENVIRONMENT=Production

WORKDIR /app
COPY . .

ENTRYPOINT ["dotnet", "DragonFruit.OnionFruit.AssetGenerator.Worker.dll"]