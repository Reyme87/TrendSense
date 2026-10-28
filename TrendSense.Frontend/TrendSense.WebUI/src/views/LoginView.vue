<script setup lang="ts">
import { computed, ref } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { ChartLine, LockKeyhole, LogIn } from 'lucide-vue-next'
import { login } from '../services/auth'

const email = ref('')
const password = ref('')
const loading = ref(false)
const error = ref('')

const router = useRouter()
const route = useRoute()

const registered = computed(
  () => route.query.registered === '1',
)

async function submitLogin() {
  loading.value = true
  error.value = ''

  try {
    await login(email.value.trim(), password.value)

    const redirect = route.query.redirect

    const destination =
      typeof redirect === 'string' &&
      redirect.startsWith('/') &&
      !redirect.startsWith('//')
        ? redirect
        : '/watchlists'

    await router.replace(destination)
  } catch (e) {
    error.value =
      e instanceof Error ? e.message : 'Не удалось выполнить вход.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <main class="login-page">
    <section class="login-card">
      <RouterLink to="/" class="login-brand">
        <span class="brand-icon">
          <ChartLine :size="23" />
        </span>
        <strong>TrendSense</strong>
      </RouterLink>

      <div class="login-icon">
        <LockKeyhole :size="24" />
      </div>

      <h1>С возвращением</h1>

      <p class="login-description">
        Войди, чтобы управлять своими списками наблюдения.
      </p>

      <p v-if="registered" class="notice notice-success">
        Регистрация завершена. Теперь войди в аккаунт.
      </p>

      <form class="login-form" @submit.prevent="submitLogin">
        <label for="email">Email</label>
        <input
          id="email"
          v-model="email"
          type="email"
          autocomplete="username"
          placeholder="you@example.com"
          required
        />

        <label for="password">Пароль</label>
        <input
          id="password"
          v-model="password"
          type="password"
          autocomplete="current-password"
          placeholder="Введи пароль"
          required
        />

        <p v-if="error" class="notice notice-error">
          {{ error }}
        </p>

        <button
          type="submit"
          class="button-primary login-submit"
          :disabled="loading"
        >
          <LogIn :size="17" />
          {{ loading ? 'Выполняется вход...' : 'Войти' }}
        </button>
      </form>

      <p class="register-prompt">
        Нет аккаунта?
        <RouterLink to="/register">Зарегистрироваться</RouterLink>
      </p>

      <RouterLink to="/" class="login-back">
        Вернуться к рынку
      </RouterLink>
    </section>
  </main>
</template>