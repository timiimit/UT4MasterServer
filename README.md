# UT4MasterServer
This is the first iteration of epic's server reimplementation just enough to be able to run Unreal Tournament pre-alpha. There is still lots to do. Anyone is welcome to help out. Reach us on [Discord Server](https://discord.gg/2DaCWkK).

## Very basic build instructions
 - Install dotnet 6.0 SDK
 - Use `dotnet` command to build project
 OR
 - Install Visual Studio 2022 with C#
 - Build Project

## How to use
 - Use bat files in `certs` to generate CertificateAuthority and Certificate for SSL (you need to run cmd from Visual Studio in order to have makecert.exe available)
 - Install certificates in `mmc` (Microsoft Management Console)
 - Add the lines in `certs/hosts.txt` to your system's `hosts` file

Run the built application and start the game with UT4UU (not tested with anything else) which can be found [here](https://github.com/timiimit/UT4UU-Public).