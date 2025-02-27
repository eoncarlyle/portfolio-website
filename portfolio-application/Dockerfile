FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app

COPY *.fsproj ./

RUN apt install nodejs

RUN dotnet restore

COPY . ./

RUN dotnet publish -c Release -o out

ENTRYPOINT ["dotnet", "out/portfolio-website.App.dll"]
