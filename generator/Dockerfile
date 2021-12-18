FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
COPY . .
ARG Version
RUN dotnet publish "./generator/DragonFruit.OnionFruit.Tools.CountryList.csproj" -c Release -o /app/publish /p:AssemblyVersion=$Version

FROM mcr.microsoft.com/dotnet/runtime:6.0-focal AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV OUTPUT_FILE=/output/countries.json
ENTRYPOINT ["dotnet", "DragonFruit.OnionFruit.Tools.CountryList.dll"]