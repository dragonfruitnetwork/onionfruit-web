FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled

ENV ASPNETCORE_ENVIRONMENT=Production

WORKDIR /app
COPY . .

ENTRYPOINT ["dotnet", "DragonFruit.OnionFruit.AssetGenerator.Worker.dll"]