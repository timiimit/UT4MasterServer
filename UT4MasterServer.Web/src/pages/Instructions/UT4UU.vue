<template>
  <h1>Instructions for UT4 with UT4UU</h1>
  <p>Use these instructions if you have the UT4UU plugin installed.</p>
  <ol>
    <li>Open the game</li>
    <li>Open console (~)</li>
    <li>Enter command <span class="code">uu settings</span></li>
    <li>Go to Other tab</li>
    <li>Scroll to the bottom</li>
    <li>In the <span style="font-style:italic">Master Server Domain</span> section, enter
      <CodeBlock>master-ut4.timiimit.com</CodeBlock>
    </li>
    <li>Press APPLY in bottom right of the window</li>
    <li>Close the game</li>
    <li>Start game</li>
    <li>After logging in on this website, get so-called &quot;<span style="font-style:italic">Auth Code</span>&quot;</li>
    <li>Use it in UT4UU's login window to add a new account</li>
    <li>Make sure to select the newly added account (it should be the last in the account list)</li>
  </ol>
  <p>To switch back to official epic's servers clear textbox in <span style="font-style:italic">Master Server Domain</span> section.</p>
</template>
<script setup lang="ts">
import CodeBlock from '../../components/CodeBlock.vue';
</script>
