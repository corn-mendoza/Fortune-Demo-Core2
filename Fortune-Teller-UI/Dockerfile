FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY *.sln ./
COPY Fortune-Teller-UI/Fortune-Teller-UI.csproj Fortune-Teller-UI/
COPY Fortune-Service-Client/Fortune-Service-Client.csproj Fortune-Service-Client/
COPY Fortune-Teller-Models/Fortune-Teller-Models.csproj Fortune-Teller-Models/
COPY Pivotal-Utilities/Pivotal-Utilities.csproj Pivotal-Utilities/
RUN dotnet restore
COPY . .
WORKDIR /src/Fortune-Teller-UI
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Fortune-Teller-UI.dll"]
