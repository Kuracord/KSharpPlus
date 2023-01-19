# KSharpPlus
An official .NET wrapper for the Kuracord API, based off [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus), but rewritten for Kuracord.

[![Nightly Build Status](https://github.com/Kuracord/KSharpPlus/actions/workflows/dotnet_nightly.yml/badge.svg?branch=master)](https://github.com/Kuracord/KSharpPlus/actions/workflows/dotnet_nightly.yml)
[![NuGet](https://img.shields.io/nuget/v/KSharpPlus.svg?label=NuGet)](https://nuget.org/packages/KSharpPlus)
[![NuGet Latest Nightly/Prerelease](https://img.shields.io/nuget/vpre/KSharpPlus?color=505050&label=NuGet%20Latest%20Nightly%2FPrerelease)](https://nuget.org/packages/KSharpPlus)

# Installing
You can install the library from following sources:

1. All Nightly versions are available on [Nuget](https://www.nuget.org/packages/KSharpPlus/) as a pre-release. These are cutting-edge versions automatically built from the latest commit in the `master` branch in this repository, and as such always contains the latest changes. If you want to use the latest features on Kuracord, you should use the nightlies.

   Despite the nature of pre-release software, all changes to the library are held under a level of scrutiny; for this library, unstable does not mean bad quality, rather it means that the API can be subject to change without prior notice (to ease rapid iteration) and that consumers of the library should always remain on the latest version available (to immediately get the latest fixes and improvements). You will usually want to use this version.

2. The latest stable release is always available on [NuGet](https://nuget.org/packages/KSharpPlus). Stable versions are released less often, but are guaranteed to not receive any breaking API changes without a major version bump.

   Critical bugfixes in the nightly releases will usually be backported to the latest major stable release, but only after they have passed our soak tests. Additionally, some smaller fixes may be infrastructurally impossible or very difficult to backport without "breaking everything", and as such they will remain only in the nightly release until the next major release. You should evaluate whether or not this version suits your specific needs.

3. The library can be directly referenced from your csproj file. Cloning the repository and referencing the library is as easy as:
    ```
    git clone https://github.com/Kuracord/KSharpPlus.git KSharpPlus-Repo
    ```
    Edit MyProject.csproj and add the following line:
    ```xml
    <ProjectReference Include="../KSharpPlus-Repo/KSharpPlus/KSharpPlus.csproj" />
    ```
    This belongs in the ItemGroup tag with the rest of your dependencies. The library should not be in the same directory or subdirectory as your project.

# Documentation
The documentation for the latest stable version is available at KSharpPlus.github.io.

# Resources

### Tutorials
* [Making your first bot in C#](https://ksharpplus.github.io/articles/basics/bot_account.html).

# Questions?
Come talk to us here:

[![KSharpPlus Chat](https://discord.com/api/guilds/848917384316059718/embed.png?style=banner1)](https://discord.gg/upvavwEnzy)
