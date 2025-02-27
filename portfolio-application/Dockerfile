FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app

COPY *.fsproj ./

RUN apt-get update && \
    apt-get install -y curl gnupg && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

RUN dotnet restore

COPY . ./

RUN dotnet publish -c Release -o out

ENTRYPOINT ["dotnet", "out/portfolio-website.App.dll"]
