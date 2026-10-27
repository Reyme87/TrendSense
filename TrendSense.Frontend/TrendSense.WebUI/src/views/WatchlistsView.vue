<script setup lang="ts">
import { onMounted, ref } from 'vue'
import {
  Plus,
  RefreshCw,
  Trash2,
  LogOut,
  LockKeyhole,
} from 'lucide-vue-next'

import {
  getWatchlists,
  createWatchlist,
  removeStockFromWatchlist,
  getAccessToken,
  saveAccessToken,
  clearAccessToken,
} from '../services/watchlists'

import type { WatchListDto } from '../types/watchListDto'

const watchlists = ref<WatchListDto[]>([])
const tokenInput = ref('')
const newListName = ref('')

const connected = ref(false)
const loading = ref(false)
const creating = ref(false)
const error = ref('')
const success = ref('')

onMounted(async () => {
  connected.value = Boolean(getAccessToken())

  if (connected.value) {
    await loadWatchlists()
  }
})

async function loadWatchlists() {
  loading.value = true
  error.value = ''
  success.value = ''

  try {
    watchlists.value = await getWatchlists()
  } catch (e) {
    error.value =
      e instanceof Error
        ? e.message
        : 'Не удалось загрузить списки'
  } finally {
    loading.value = false
  }
}

async function connect() {
  const token = tokenInput.value.trim()

  if (!token) {
    error.value = 'Введи JWT-токен.'
    return
  }

  saveAccessToken(token)
  connected.value = true

  await loadWatchlists()
}

function disconnect() {
  clearAccessToken()
  connected.value = false
  tokenInput.value = ''
  watchlists.value = []
  error.value = ''
  success.value = ''
}

async function createList() {
  const name = newListName.value.trim()

  if (!name) {
    error.value = 'Введи название списка.'
    return
  }

  creating.value = true
  error.value = ''
  success.value = ''

  try {
    await createWatchlist(name)
    newListName.value = ''

    await loadWatchlists()
    success.value = 'Список создан.'
  } catch (e) {
    error.value =
      e instanceof Error
        ? e.message
        : 'Не удалось создать список'
  } finally {
    creating.value = false
  }
}

async function removeStock(
  watchlistId: string,
  stockId: string,
) {
  error.value = ''
  success.value = ''

  try {
    await removeStockFromWatchlist(
      watchlistId,
      stockId,
    )

    await loadWatchlists()
    success.value = 'Акция удалена из списка.'
  } catch (e) {
    error.value =
      e instanceof Error
        ? e.message
        : 'Не удалось удалить акцию'
  }
}
</script>

<template>
  <section class="page-container">
    <div class="page-heading">
      <div>
        <div class="eyebrow">YOUR WORKSPACE</div>
        <h1>Списки наблюдения</h1>
        <p class="page-description">
          Управляй инструментами, за которыми следишь.
        </p>
      </div>

      <button
        v-if="connected"
        class="button-secondary"
        :disabled="loading"
        @click="loadWatchlists"
      >
        <RefreshCw
          :size="17"
          :class="{ spinning: loading }"
        />
        Обновить
      </button>
    </div>

    <section class="market-section auth-section">
      <div class="section-heading">
        <div>
          <h2>Подключение к API</h2>
          <p>
            Для работы со списками нужна авторизация.
          </p>
        </div>

        <button
          v-if="connected"
          class="button-secondary"
          @click="disconnect"
        >
          <LogOut :size="16" />
          Отключиться
        </button>
      </div>

      <form
        v-if="!connected"
        class="watchlist-form token-form"
        @submit.prevent="connect"
      >
        <LockKeyhole :size="18" />

        <input
          v-model="tokenInput"
          type="password"
          autocomplete="off"
          placeholder="Вставь JWT access token"
          aria-label="JWT access token"
        />

        <button class="button-primary" type="submit">
          Подключить
        </button>
      </form>

      <div v-else class="connected-label">
        <span class="connection-dot"></span>
        JWT-токен сохранён в сессии браузера
      </div>
    </section>

    <div v-if="error" class="notice notice-error">
      {{ error }}
    </div>

    <div v-if="success" class="notice notice-success">
      {{ success }}
    </div>

    <template v-if="connected">
      <section class="market-section create-list-section">
        <div class="section-heading">
          <div>
            <h2>Создать список</h2>
            <p>Например, «Долгосрочные инвестиции».</p>
          </div>
        </div>

        <form
          class="watchlist-form"
          @submit.prevent="createList"
        >
          <input
            v-model="newListName"
            maxlength="100"
            placeholder="Название нового списка"
            aria-label="Название списка"
          />

          <button
            class="button-primary"
            type="submit"
            :disabled="creating"
          >
            <Plus :size="17" />
            {{ creating ? 'Создание...' : 'Создать список' }}
          </button>
        </form>
      </section>

      <div class="watchlists-grid">
        <article
          v-for="list in watchlists"
          :key="list.id"
          class="watchlist-card"
        >
          <div class="watchlist-card-header">
            <div>
              <h2>{{ list.name }}</h2>
              <p>
                {{ list.items?.length ?? 0 }} инструментов
              </p>
            </div>

            <span class="watchlist-icon">
              <LockKeyhole :size="19" />
            </span>
          </div>

          <div
            v-if="!list.items?.length"
            class="watchlist-empty"
          >
            В этом списке пока нет акций.
            Добавь инструмент со страницы акции.
          </div>

          <div v-else class="watchlist-items">
            <div
              v-for="item in list.items"
              :key="item.id ?? item.stockId"
              class="watchlist-item"
            >
              <div class="watchlist-stock-info">
                <strong>
                  {{
                    item.stock?.tickerSymbol ??
                    item.stock?.name ??
                    item.stockId
                  }}
                </strong>

                <small v-if="item.stock?.name">
                  {{ item.stock.name }}
                </small>
              </div>

              <button
                class="remove-button"
                type="button"
                title="Удалить акцию из списка"
                aria-label="Удалить акцию из списка"
                @click="
                  removeStock(list.id, item.stockId)
                "
              >
                <Trash2 :size="16" />
              </button>
            </div>
          </div>
        </article>
      </div>

      <div
        v-if="!loading && watchlists.length === 0"
        class="empty-state"
      >
        <h2>Списков пока нет</h2>
        <p>
          Создай первый список, чтобы группировать интересующие акции.
        </p>
      </div>
    </template>
  </section>
</template>