FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ./AllegroService/AllegroService/AllegroService.csproj ./AllegroService/AllegroService/
RUN dotnet restore ./AllegroService/AllegroService/AllegroService.csproj

COPY . .
RUN dotnet publish ./AllegroService/AllegroService/AllegroService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AllegroService.dll"]
