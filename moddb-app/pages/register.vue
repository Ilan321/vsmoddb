<script setup lang="ts">
import type { StartAccountLinkResponse } from '~/models/responses/account/StartAccountLinkResponse';

useTitle('Link your account');

definePageMeta({
  middleware: ['anonymous-only']
});

const username = ref('');
const email = ref('');
const token = ref('');
const tokenUrl = ref('');
const password = ref('');
const step = ref(1);

const stepOneErrors = ref<string[]>([]);
const stepOneLoading = ref(false);

const stepTwoErrors = ref<string[]>([]);
const stepTwoLoading = ref(false);

const stepThreeErrors = ref<string[]>([]);
const stepThreeLoading = ref(false);
const stepThreeSuccess = ref(false);

async function onStepOneSubmit() {
  if (stepOneLoading.value) {
    return;
  }

  if (step.value !== 1) {
    return;
  }

  stepOneErrors.value = [];

  if (!username.value || !email.value) {
    stepOneErrors.value = ['Please enter a valid username and email address'];

    return;
  }

  try {
    stepOneLoading.value = true;

    const response = await $fetch<StartAccountLinkResponse>(
      '/api/v1/account/link/start',
      {
        method: 'POST',
        body: {
          username: username.value,
          email: email.value
        }
      }
    );

    token.value = response.linkToken;
    tokenUrl.value = response.url;
    step.value = 2;
  } catch (error: any) {
    if (error?.statusCode === 500) {
      stepOneErrors.value = [
        'An error occurred while trying to link your account. Please try again later.'
      ];

      return;
    }

    if (error?.statusCode === 400 && error?.data?.errorCode) {
      const map = {
        ACCOUNT_ALREADY_EXISTS: 'An account with this username already exists'
      } as Record<string, string>;

      stepOneErrors.value = [
        map[error.data.errorCode] ||
          'An error occurred while trying to link your account. Please try again later.'
      ];

      return;
    }

    console.log({ error });
  } finally {
    stepOneLoading.value = false;
  }
}

async function onStepTwoSubmit() {
  if (step.value !== 2) {
    return;
  }

  if (stepTwoLoading.value) {
    return;
  }

  stepTwoErrors.value = [];

  try {
    stepTwoLoading.value = true;

    await $fetch('/api/v1/account/link/verify', {
      method: 'POST',
      body: {
        linkToken: token.value
      }
    });

    step.value = 3;
  } catch (error: any) {
    if (error?.statusCode >= 500) {
      stepOneErrors.value = [
        'An error occurred while trying to link your account. Please try again later.'
      ];

      return;
    }

    if (error?.statusCode === 400) {
      const errorMap = {
        INVALID_LINK_REQUEST:
          'This validation token is invalid. Please refresh the page and try again.',
        LINK_REQUEST_EXPIRED:
          'This validation token has expired. Please refresh the page and try again.',
        LINK_VERIFICATION_FAILED:
          'Could not find a verification comment on the ModDB page. Please make sure you have posted the token correctly.'
      };
    }

    console.log({ error });
  } finally {
    stepTwoLoading.value = false;
  }
}

async function onStepThreeSubmit() {
  if (step.value !== 3) {
    return;
  }

  if (stepThreeLoading.value) {
    return;
  }

  stepThreeErrors.value = [];

  try {
    stepThreeLoading.value = true;

    await $fetch('/api/v1/account/link/set-password', {
      method: 'POST',
      body: {
        password: password.value,
        linkToken: token.value
      }
    });

    // Show success, then redirect to the home page

    stepThreeSuccess.value = true;
    step.value = 4;
  } catch (error: any) {
    console.log({ error });
  } finally {
    stepThreeLoading.value = false;
  }
}
</script>

<template>
  <div class="register pb-4">
    <h2 class="text-xl mb-4">Link your ModDB account</h2>
    <p class="mb-2">Linking your ModDB account is done in 3 steps:</p>
    <ul class="max-w-xl mb-4">
      <li>
        <p>Step 1: Enter your ModDB username and email address</p>
        <p class="text-sm">
          Tip: Make sure the username you enter is exactly the same as your
          username on the official ModDB.
        </p>
      </li>
      <li>
        <p>Step 2: Prove ownership of your ModDB account</p>
        <p class="text-sm">
          You will receive a one-time verification token. You will need to copy
          this token, and post it as a comment on a specific ModDB verification
          page. That comment will be used to prove you are the owner of the
          username you've input.
        </p>
      </li>
      <li>
        <p>Step 3: Verify your account and add a password</p>
        <p class="text-sm">
          Once you've linked your account, your account here on the Unofficial
          ModDB will be created. You will be prompted to add a password to your
          account, so you can login in the future.
        </p>
      </li>
    </ul>
    <div class="mt-4">
      <h3 class="text-lg mb-2">
        Step 1: Enter your ModDB username and email address
      </h3>
      <form
        class="flex flex-col gap-2 lg:max-w-xl"
        @submit.prevent="onStepOneSubmit"
      >
        <v-input
          v-model="username"
          :disabled="step > 1 || stepOneLoading"
          label="ModDB username"
        />
        <v-input
          v-model="email"
          :disabled="step > 1 || stepOneLoading"
          label="Email address"
          type="email"
        />
        <span v-for="error of stepOneErrors" class="text-red-700 text-sm">
          {{ error }}
        </span>
        <v-button
          :disabled="step > 1 || stepOneLoading"
          :loading="stepOneLoading"
          type="submit"
          >Continue</v-button
        >
      </form>
    </div>
    <fade-transition>
      <div v-if="step >= 2" class="mt-4">
        <h3 class="text-lg mb-2">
          Step 2: Prove ownership of your ModDB account
        </h3>
        <ol class="mb-2">
          <li>
            <p>
              Copy your verification token: <code>{{ token }}</code>
            </p>
          </li>
          <li>
            <p>
              Go to the
              <a class="link-blue" :href="tokenUrl"
                >account verification page</a
              >
              on the official ModDB.
            </p>
          </li>
          <li>
            <p>
              Post the verification token as a comment on the verification page.
            </p>
          </li>
          <li>
            <p>
              Verify your account:
              <v-button
                :disabled="stepTwoLoading || step !== 2"
                @click="onStepTwoSubmit"
                >Verify now</v-button
              >
            </p>
          </li>
        </ol>
        <span v-for="error of stepTwoErrors" class="text-red-700 text-sm">
          {{ error }}
        </span>
      </div>
    </fade-transition>
    <fade-transition>
      <div v-if="step >= 3" class="max-w-xl mt-4">
        <h3 class="text-lg mb-2">Step 3: Add a password to your account</h3>
        <p>
          Your account has been linked successfully. Please add a password to
          your account:
        </p>
        <form
          class="mt-2 flex flex-col md:flex-row gap-4 md:gap-2 md:items-end"
          @submit.prevent="onStepThreeSubmit"
        >
          <input
            class="hidden"
            :model-value="username"
            autocomplete="username"
          />
          <v-input
            v-model="password"
            class="md:grow"
            label="New password"
            autocomplete="new-password"
            type="password"
          />
          <v-button
            :loading="stepThreeLoading"
            :disabled="stepThreeLoading || step !== 3"
            class="h-9"
            type="submit"
            >Update your password</v-button
          >
        </form>
      </div>
    </fade-transition>
    <fade-transition>
      <div v-if="step >= 4" class="max-w-xl mt-4">
        <h3 class="text-lg mb-2">Step 4: Success!</h3>
        <p>
          You've linked and reset your account password successfully. You can
          now delete your comment if you wish, and head to the
          <NuxtLink to="/" class="link-blue">home page</NuxtLink>!
        </p>
      </div>
    </fade-transition>
  </div>
</template>
