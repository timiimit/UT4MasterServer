# UT4MasterServer
This is a reimplementation of EpicGames servers just enough to be able to serve as a master server for Unreal Tournament pre-alpha. There is still lots to do. Anyone is welcome to help out. Reach us on [Discord Server](https://discord.gg/2DaCWkK).

## Contribution
There is not much documentation on how to get started. I will create issues that need resolving as time goes on, but there are also a bunch of `// TODO` comments which tell what is still missing.

## Required packages for running with Visual Studio 2022
 - .NET desktop development
 - Container development tools

## Running & debugging with Visual Studio 2022
 - Install docker
 - Install Visual Studio 2022
 - Set docker-compose as the Startup Project
 - Start Debugging
 
## Running without Visual Studio 2022
There is no documentation as of yet, but you should use `docker-compose` command. Feel free to fill-in this info in a PR.

## How to make UT4 connect to new master server?
 - Use `Files/Engine.ini` in this repository as a template
 - Replace all occurances of `<PUT_MASTER_SERVER_DOMAIN_HERE>` with the domain of master server you want to connect to
 - Copy the content and append all of it to your local Engine.ini file

## How to track statistics on new master server?
It is not possible to do this without editing the client. UT4UU will try to address this in the future.

## I really don't want to use UT4UU, but I want master server to track my statistics!
A workaround would be for the admin of master server to issue you a self-signed certificate. You can then install this certificate so that it is trusted by your machine. Finally you can add an entry into your `hosts` file for domain `datarouter.ol.epicgames.com`.
