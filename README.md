# UT4MasterServer

This is a reimplementation of EpicGames servers, just enough to be able to serve as a master server for Unreal Tournament pre-alpha. There is still lots to do. Anyone is welcome to help out. Reach us on [Discord Server](https://discord.gg/2DaCWkK).

## Contribution

There is not much documentation on how to get started. I will create issues that need resolving as time goes on, but there are also a bunch of `// TODO` comments which tell what is still missing.

## Required packages for running with Visual Studio 2022

- .NET desktop development
- Container development tools

## Running & debugging with Visual Studio 2022

1. Install Docker
2. Install Visual Studio 2022
3. Set docker-compose as the Startup Project
4. Start Debugging

## Running without Visual Studio 2022

There is no documentation yet, but you should use `docker-compose` command. Feel free to fill-in this info in a PR.

## How to make UT4 connect to new master server?

1. Use `Files/Engine.ini` in this repository as a template
2. Replace all occurrences of `<PUT_MASTER_SERVER_DOMAIN_HERE>` with the domain of master server you want to connect to
3. Copy the content and append all of it to your local `Engine.ini` file
    - Windows path: `C:\Users\your_username\Documents\UnrealTournament\Saved\Config\WindowsNoEditor`
4. Register new account in `POST account/api/create/account` endpoint
5. Now you have 2 options:
   1. Option 1 **(recommended)**:
      1. Add `UnrealTournament` parameter to your UT4 shortcut:
      Example:

      ```text
      "D:\Epic Games\UnrealTournament\Engine\Binaries\Win64\UE4-Win64-Shipping.exe" UnrealTournament
      ```

      2. After starting game, you should see login window. Enter your credentials and you should be able to play.
   2. Option 2:
      1. Get `access_token` from `POST account/api/oauth/token` with `grant_type: password`
      2. `GET account/api/oauth/exchange` with Bearer authorization (`access_token` from `POST account/api/oauth/token`)
      3. Add parameters to your UT4 shortcut and replace `your_password_code` with `code` from `GET account/api/oauth/exchange`:

      ```text
      "D:\Epic Games\UnrealTournament\Engine\Binaries\Win64\UE4-Win64-Shipping.exe" UnrealTournament -AUTH_LOGIN=unused -AUTH_PASSWORD=your_password_code -AUTH_TYPE=exchangecode
      ```

      NOTE: You need to repeat point *b* and *c* if you want to login again.

## How to track statistics on new master server?

It is not possible to do this without editing the client. UT4UU will try to address this in the future.

## I really don't want to use UT4UU, but I want master server to track my statistics

A workaround would be for the admin of master server to issue you a self-signed certificate. You can then install this certificate so that it is trusted by your machine. Finally, you can add an entry into your `hosts` file for domain `datarouter.ol.epicgames.com`.
