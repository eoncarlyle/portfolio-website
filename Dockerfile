FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app

COPY PortfolioWebsite.slnx ./
COPY portfolio-application/*.fsproj ./portfolio-application/
COPY shiki-fsharp/*.fsproj ./shiki-fsharp/

RUN dotnet restore PortfolioWebsite.slnx

COPY portfolio-application/ ./portfolio-application/
COPY shiki-fsharp/ ./shiki-fsharp/

RUN dotnet publish portfolio-application/portfolio-website.fsproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/out ./

ENTRYPOINT ["dotnet", "portfolio-website.App.dll"]
