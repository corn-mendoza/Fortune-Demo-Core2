FROM microsoft/aspnetcore-build:2.0

COPY /Fortune-Teller-Service/ /src/Fortune-Teller-Service
COPY /Fortune-Teller-Models/ /src/Fortune-Teller-Models

RUN cd /src/Fortune-Teller-Service && dotnet publish -o /app

EXPOSE 8080
WORKDIR /app
ENTRYPOINT ["dotnet", "Fortune-Teller-Service.dll"]