<template>
  <h1>Instructions for Hub Owners</h1>
  <p>
    Use these instructions to configure a Hub you own or admin to use UT4 Master Server.
  </p>
  <ol>
    <li>Go to <span class="code">{UT4 Server Install Location}\UnrealTournament\Saved\Config\WindowsServer\</span> in
      file explorer</li>
    <li>Open <span class="code">Engine.ini</span> file</li>
    <li>
      Append the following at the bottom of the file:
      <CodeBlock>
        [OnlineSubsystemMcp.BaseServiceMcp]<br/>
        Domain=master-ut4.timiimit.com<br/>
        Protocol=https<br/>
        [OnlineSubsystemMcp.GameServiceMcp]<br/>
        Domain=master-ut4.timiimit.com<br/>
        Protocol=https<br/>
        [OnlineSubsystemMcp.AccountServiceMcp]<br/>
        Domain=master-ut4.timiimit.com<br/>
        Protocol=https<br/>
        [OnlineSubsystemMcp.OnlineFriendsMcp]<br/>
        Domain=master-ut4.timiimit.com<br/>
        Protocol=https<br/>
        [OnlineSubsystemMcp.PersonaServiceMcp]<br/>
        Domain=master-ut4.timiimit.com<br/>
        Protocol=https<br/>
        [OnlineSubsystemMcp.OnlineImageServiceMcp]<br/>
        Domain=master-ut4.timiimit.com<br/>
        Protocol=https<br/>
      </CodeBlock>
    </li>
    <li>Start the server as you normally would</li>
    <li>If you were previously an owner of a trusted hub/server, remove whole <span class="code">[OnlineSubsystemMcp]</span> section from <span class="code">Engine.ini</span></li>
  </ol>
</template>
<script setup lang="ts">
import CodeBlock from '../../components/CodeBlock.vue';
</script>
