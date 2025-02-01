<script setup lang="ts">
import firebaseService from './services/firebase.service';
import useAuthStore from './store/auth';

const auth = useAuthStore();
</script>

<template>
  <div
    id="app"
    data-testid="app"
    class="min-h-screen flex flex-col justify-start items-center"
  >
    <div data-testid="content" class="container px-4">
      <div data-testid="logo" class="my-4">
        <router-link to="/">
          <img
            class="mx-auto md:mx-0 w-72"
            src="/assets/img/vsmoddb-logo-s.png"
          />
        </router-link>
      </div>
      <nav
        data-testid="nav"
        class="navigation flex flex-col md:flex-row md:gap-2 text-white rounded-t"
      >
        <nav-item to="/">Home</nav-item>
        <nav-item to="/mods">Mods</nav-item>
        <nav-item v-if="auth.isLoggedIn" to="/submit">Submit a mod</nav-item>
        <nav-item to="https://wiki.vintagestory.at/Troubleshooting_Mods"
          >Mod troubleshooting</nav-item
        >
      </nav>
      <div class="content-inner px-4 py-2">
        <NuxtPage />
      </div>
      <footer
        class="mt-2 py-1 px-2 flex flex-row text-sm text-neutral-800 bg-white/80"
      >
        <span>
          This site is not affiliated with Vintage Story or Anego Studios |
          Built by Ilan |
          <NuxtLink
            to="https://mods.vintagestory.at"
            class="underline hover:text-neutral-900"
          >
            Vintage Story Mod DB
          </NuxtLink>
        </span>
      </footer>
    </div>
  </div>
</template>

<style>
@reference '@/main.css';

#app {
  background-image: url('/assets/img/background.jpg');
  background-attachment: fixed;
  background-size: cover;
}

.navigation {
  font-size: 125%;
  width: 100%;
  background: rgb(31, 26, 21);
  background: linear-gradient(
    0deg,
    rgba(104, 80, 55, 0.65) 0%,
    rgba(179, 154, 121, 0.65) 100%
  );
}

.content-inner {
  background-color: rgba(255, 252, 244, 0.98);
}

a.link-blue {
  @apply transition-colors;
  @apply text-blue-600 hover:text-blue-700;
  @apply visited:text-purple-700;
  @apply cursor-pointer underline;
}

a.link {
  @apply hover:text-gray-800 underline;
}

a.link-dotted {
  @apply underline decoration-dotted;
}
</style>
