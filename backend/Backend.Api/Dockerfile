FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Backend.sln ./
COPY Backend.Api/Backend.Api.csproj Backend.Api/
RUN dotnet restore

COPY . .
RUN dotnet publish Backend.Api/Backend.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "Backend.Api.dll"]
