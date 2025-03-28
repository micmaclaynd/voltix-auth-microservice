FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Voltix.AuthMicroservice/Voltix.AuthMicroservice.csproj", "Voltix.AuthMicroservice/"]
RUN dotnet restore "./Voltix.AuthMicroservice/Voltix.AuthMicroservice.csproj"
COPY . .
WORKDIR "/src/Voltix.AuthMicroservice"
RUN dotnet build "./Voltix.AuthMicroservice.csproj" -c $BUILD_CONFIGURATION -o /app/build


FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Voltix.AuthMicroservice.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Voltix.AuthMicroservice.dll"]