# dotnet-package-cache-gen

![Docker Pulls](https://img.shields.io/docker/pulls/itglobal/dotnet-package-cache-gen?style=flat-square)
![GitHub](https://img.shields.io/github/license/itglobal/dotnet-package-cache-gen?style=flat-square)

Tiny tool to generate cache files for Docker-powered .NET SDK projects.

This toos helps you to setup a docker-based builds for .NET Core projects with proper Docker image caching.

## When to use it

This tool would be pretty handy if:

* your project is built via Docker
* and your project contains more than one `.csproj` file
* and (optionally) your project uses more than one target framework

If you don't build your project via Docker or you have only one `.csproj` file,
you won't need **dotnet-package-cache-gen**.

## How to use it

1. Add the following lines to your build script:

   ```shell
   docker run --rm -v $(pwd):/source ghcr.io/itglobal/dotnet-package-cache-gen:latest > ./build/package-cache.xml
   ```

   This command will generate Nuget package cache file from projects in current directory
   and write it into `./build/package-cache.xml` file.

   It's important to run this command before running "docker build", otherwise your build will probably fail.

   `./build/package-cache.xml` file should be gitignored but not dockerignored.

   > If you need to get detailed output, add a `-v` or `--verbose` flag to the `docker run` command:
   >
   > ```shell
   > docker run --rm -v $(pwd):/source ghcr.io/itglobal/dotnet-package-cache-gen:latest -v > ./build/package-cache.xml
   > ```

2. Add the following lines to your Dockerfile:

   ```Dockerfile
   FROM your-base-image

   # Copy cache file and run "dotnet restore" against it
   COPY ./build/package-cache.xml /build-tmp/
   RUN dotnet restore -v q /build-tmp/package-cache.xml /nologo /nowarn:msb4011 && rm -rf /build-tmp

   # Everything above this line will be cached
   # Docker will not re-run "dotnet restore" command unless list of referenced nuget packages,
   # their versions or target frameworks is changed

   # Rest of your dockerfile...
   COPY . /src
   RUN dotnet publish
   ```

## How it works

**dotnet-package-cache-gen** scans specified directory recursively for `*.csproj` files,
skipping any `bin` and `obj` subdirectories.

Every found project file is analyzed. Analyzer is able to process the following parts of project file:

* `<TargetFramework>` element
* `<TargetFrameworks>` element
* `<RuntimeIdentifier>` element
* `<RuntimeIdentifiers>` element
* `<Import>` element
* `<Sdk>` element
* `<PackageReference>` element within a plain `<ItemGroup>`
* `<PackageReference>` element within a `<ItemGroup>` with condition.
  The following conditions are supported:

  * `Condition="'$(TargetFramework)' == 'TARGET_FRAMEWORK'"` - specifies `<ItemGroup>` which is applicable only to `TARGET_FRAMEWORK`.
  * `Condition="'$(TargetFramework)' == 'TARGET_FRAMEWORK_1' Or '$(TargetFramework)' == 'TARGET_FRAMEWORK_2'"` - specifies
    `<ItemGroup>` which is applicable only to `TARGET_FRAMEWORK_1` or `TARGET_FRAMEWORK_2`.

Example:

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <!-- Project target frameworks -->
        <TargetFrameworks>net40;net45;netstandard2.0</TargetFrameworks>
    </PropertyGroup>

    <!-- This packages are references when building for any target framework -->
    <ItemGroup>
        <PackageReference Include="Nuget.Package.Id.1" Version="1.0.0" />
    </ItemGroup>

    <!-- This packages are references when building for .NET Framework 4.0 -->
    <ItemGroup Condition="'$(TargetFramework)' == 'net40'">
        <PackageReference Include="Nuget.Package.Id.2" Version="1.0.0" />
    </ItemGroup>

    <!-- This packages are references when building for .NET Framework 4.5 -->
    <ItemGroup Condition="'$(TargetFramework)' == 'net45'">
        <PackageReference Include="Nuget.Package.Id.3" Version="1.0.0" />
    </ItemGroup>

    <!-- This packages are references when building for .NET Standard 2.0 -->
    <ItemGroup Condition="'$(TargetFramework)' == 'netstandard2.0'">
        <PackageReference Include="Nuget.Package.Id.4" Version="1.0.0" />
    </ItemGroup>

    <!-- This packages are references when building for .NET Framework 4.0 or .NET Framework 4.5 -->
    <ItemGroup Condition="'$(TargetFramework)' == 'net40' Or '$(TargetFramework)' == 'net45'">
        <PackageReference Include="Nuget.Package.Id.3" Version="1.0.0" />
    </ItemGroup>
</Project>
```

## [License](LICENSE)

MIT
