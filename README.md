# dotnet-package-cache-gen

Tiny tool to generate cache files for .NET SDK projects.

This toos helps you to setup a docker-based builds for .NET Core projects with proper caching.

## How to use

1. Add the following lines to your build script:

  ```shell
  docker run --rm -v $(pwd):/source itglobal/dotnet-package-cache-gen > ./build/package-cache.xml
  ```

  This command will generate Nuget package cache file from projects in current directory
  and write it into `./build/package-cache.xml` file.

  It's important to run this command before running "docker build", otherwise your build will probably fail.

  `./build/package-cache.xml` file should be gitignored but not dockerignored.

2. Add the following lines to your Dockerfile:

   ```Dockerfile
   FROM your-base-image

   # Copy cache file and run "dotnet restore" against it
   COPY ./build/package-cache.xml /build-tmp/
   RUN dotnet restore -v q /build-tmp/package-cache.xml /nologo && rm -rf /build-tmp

   # Everything above this line will be cached
   # Docker will not re-run "dotnet restore" command unless list of referenced nuget packages,
   # their versions or target frameworks is changed

   # Rest of your dockerfile...
   COPY . /src
   RUN dotnet publish
   ```

## [License](LICENSE)

MIT
