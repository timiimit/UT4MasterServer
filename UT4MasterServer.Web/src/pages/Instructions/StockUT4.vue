<template>
  <h1>Instructions for Stock UT4</h1>
  <p>
    Use these instructions to use the UT4 Master Server with a standard Windows
    installation of Unreal Tournament 4 without
    <a href="https://github.com/timiimit/UT4UU-Public/">UT4UU</a> installed.
  </p>
  <ol>
    <li>
      Go to
      <span class="code"
        >{Your Windows
        User}\Documents\UnrealTournament\Saved\Config\WindowsNoEditor\</span
      >
      in file explorer
    </li>
    <li>Open <span class="code">Engine.ini</span> file</li>
    <li>
      Append the following at the bottom of the file:
      <CodeBlock>
        [OnlineSubsystemMcp.BaseServiceMcp]<br />
        Domain={{ masterServerUrl }}<br />
        Protocol=https<br />
        [OnlineSubsystemMcp.GameServiceMcp]<br />
        Domain={{ masterServerUrl }}<br />
        Protocol=https<br />
        [OnlineSubsystemMcp.AccountServiceMcp]<br />
        Domain={{ masterServerUrl }}<br />
        Protocol=https<br />
        [OnlineSubsystemMcp.OnlineFriendsMcp]<br />
        Domain={{ masterServerUrl }}<br />
        Protocol=https<br />
        [OnlineSubsystemMcp.PersonaServiceMcp]<br />
        Domain={{ masterServerUrl }}<br />
        Protocol=https<br />
        [OnlineSubsystemMcp.OnlineImageServiceMcp]<br />
        Domain={{ masterServerUrl }}<br />
        Protocol=https<br />
      </CodeBlock>
    </li>
    <li>
      Go to
      <span class="code">{UT4 Install Location}\Engine\Binaries\Win64\</span>
    </li>
    <li>
      Create a shortcut to <span class="code">UE4-Win64-Shipping.exe</span> on
      your desktop (Right click -> Send to -> Desktop)
    </li>
    <li>Open properties of the shortcut (Right click -> Properties)</li>
    <li>
      In text box target at the very end, add:
      <CodeBlock>
        UnrealTournament -epicapp=UnrealTournamentDev -epicenv=Prod -EpicPortal
      </CodeBlock>
    </li>
    <li>
      Make sure there is space between what was in there before and what you
      added
    </li>
    <li>Use the shortcut to start the game</li>
    <li>
      When prompted to log in, enter your UT4 Master Server username (the one
      you use to log into this site) instead of email
    </li>
    <li>
      You may want to configure your settings again as they are stored in the
      cloud, and they will not be transferred over from Epic's servers
    </li>
  </ol>
  <p>
    To switch back to official Epic's servers, remove all lines that you added
    to <span class="code">Engine.ini</span> file (added lines may have moved).
  </p>
</template>
<script setup lang="ts">
import CodeBlock from '@/components/CodeBlock.vue';

const masterServerUrl = __BACKEND_URL.split('//')[1];
</script>
