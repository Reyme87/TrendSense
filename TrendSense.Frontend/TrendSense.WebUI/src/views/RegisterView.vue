<script setup lang="ts">
import { ref } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { ChartLine, UserPlus } from 'lucide-vue-next'
import { register } from '../services/auth'

const userName = ref('')
const email = ref('')
const password = ref('')
const confirmPassword = ref('')

const loading = ref(false)
const error = ref('')

const router = useRouter()

async function submitRegistration() {
  error.value = ''

  if (password.value !== confirmPassword.value) {
    error.value = 'Пароли не совпадают.'
    return
  }

  loading.value = true

  try {
    await register({
      userName: userName.value.trim(),
      email: email.value.trim(),
      password: password.value,
    })

    await router.replace({
      name: 'login',
      query: { registered: '1' },
    })
  } catch (e) {
    error.value =
      e instanceof Error
        ? e.message
        : 'Не удалось зарегистрировать аккаунт.'
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

      <h1>Создать аккаунт</h1>

      <p class="login-description">
        Зарегистрируйся, чтобы создавать собственные списки наблюдения.
      </p>

      <form
        class="login-form"
        @submit.prevent="submitRegistration"
      >
        <label for="userName">Имя пользователя</label>
        <input
          id="userName"
          v-model="userName"
          autocomplete="username"
          required
        />

        <label for="registerEmail">Email</label>
        <input
          id="registerEmail"
          v-model="email"
          type="email"
          autocomplete="email"
          required
        />

        <label for="registerPassword">Пароль</label>
        <input
          id="registerPassword"
          v-model="password"
          type="password"
          autocomplete="new-password"
          required
        />

        <label for="confirmPassword">Повтори пароль</label>
        <input
          id="confirmPassword"
          v-model="confirmPassword"
          type="password"
          autocomplete="new-password"
          required
        />

        <p v-if="error" class="notice notice-error">
          {{ error }}
        </p>

        <button
          class="button-primary login-submit"
          type="submit"
          :disabled="loading"
        >
          <UserPlus :size="17" />
          {{ loading ? 'Регистрация...' : 'Зарегистрироваться' }}
        </button>
      </form>

      <p class="register-prompt">
        Уже есть аккаунт?
        <RouterLink to="/login">Войти</RouterLink>
      </p>
    </section>
  </main>
</template>