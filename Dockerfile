FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine as BUILDER

WORKDIR /app
COPY ./src/dotnet-package-cache-gen.csproj /app/src/dotnet-package-cache-gen.csproj
RUN dotnet restore /app/src/dotnet-package-cache-gen.csproj /nologo
COPY . /app/
RUN dotnet publish -o /out --no-restore /app/src/dotnet-package-cache-gen.csproj /nologo

FROM mcr.microsoft.com/dotnet/runtime:7.0-alpine

RUN mkdir -p /app
COPY --from=BUILDER /out/ /app
VOLUME [ "/source" ]
ENTRYPOINT [ "dotnet", "/app/dotnet-package-cache-gen.dll" ]
CMD [ "/source" ]