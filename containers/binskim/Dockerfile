FROM mcr.microsoft.com/dotnet/sdk:10.0

RUN dotnet tool install --global Microsoft.CodeAnalysis.BinSkim --version 1.9.5

ENV PATH="${PATH}:/root/.dotnet/tools"

WORKDIR /src

HEALTHCHECK NONE

CMD ["binskim", "analyze", "--recurse", "--output", "/output/binskim.json", "--pretty-print", "/src/"]
