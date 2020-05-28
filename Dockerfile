FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine as BUILDER

WORKDIR /app
COPY ./src/dotnet-package-cache-gen.csproj /app/src/dotnet-package-cache-gen.csproj
RUN dotnet restore /app/src/dotnet-package-cache-gen.csproj /nologo
COPY . /app/
RUN dotnet publish -o /out --no-restore /app/src/dotnet-package-cache-gen.csproj /nologo

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-alpine

RUN mkdir -p /app
COPY --from=BUILDER /out/ /app
VOLUME [ "/source" ]
ENTRYPOINT [ "dotnet", "/app/dotnet-package-cache-gen.dll" ]
CMD [ "/source" ]