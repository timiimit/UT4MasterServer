<template>
  <h1>Instructions for UT4 with UT4UU</h1>
  <p>
    Use these instructions if you have the
    <a href="https://github.com/timiimit/UT4UU-Public/" target="_blank"
      >UT4UU</a
    >
    plugin installed.
  </p>
  <ol>
    <li>Open the game</li>
    <li>Open console (~)</li>
    <li>Enter command <span class="code">uu settings</span></li>
    <li>Go to Other tab</li>
    <li>Scroll to the bottom</li>
    <li>
      In the
      <span style="font-style: italic">Master Server Domain</span> section,
      enter:
      <CodeBlock>{{ masterServerUrl }}</CodeBlock>
    </li>
    <li>Press APPLY in bottom right of the window</li>
    <li>Close the game</li>
    <li>Start game</li>
    <li>
      After logging in on this website, get so-called &quot;<span
        style="font-style: italic"
        >Auth Code</span
      >&quot;
    </li>
    <li>Use it in UT4UU's login window to add a new account</li>
    <li>
      Make sure to select the newly added account (it should be the last in the
      account list)
    </li>
    <li>
      You may want to configure your settings again as they are stored in the
      cloud, and they will not be transferred over from Epic's servers
    </li>
  </ol>
  <p>
    To switch back to official Epic's servers, clear text box in
    <span style="font-style: italic">Master Server Domain</span> section.
  </p>
</template>
<script setup lang="ts">
import CodeBlock from '@/components/CodeBlock.vue';

const masterServerUrl = __BACKEND_URL.split('//')[1];
</script>
