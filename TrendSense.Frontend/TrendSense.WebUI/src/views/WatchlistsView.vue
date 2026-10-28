<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { isAuthenticated } from '../services/auth'

import {
  Plus,
  RefreshCw,
  Trash2,
  LockKeyhole,
} from 'lucide-vue-next'

import {
  getWatchlists,
  createWatchlist,
  removeStockFromWatchlist,
} from '../services/watchlists'

import { getStocks } from '../services/stocks'

import type { WatchListDto } from '../types/watchListDto'
import type { StockDto } from '../types/stockDto'

const router = useRouter()

const watchlists = ref<WatchListDto[]>([])
const newListName = ref('')

const loading = ref(false)
const creating = ref(false)
const error = ref('')
const success = ref('')

onMounted(() => {
  void loadWatchlists()
})

async function loadWatchlists() {
  loading.value = true
  error.value = ''
  success.value = ''

  try {
    const lists = await getWatchlists()

    let stocks: StockDto[] = []

    try {
      stocks = await getStocks()
    } catch {
    }

    const stocksById = new Map(
      stocks.map((stock) => [stock.id, stock]),
    )

    watchlists.value = lists.map((list) => ({
      ...list,
      items: (list.items ?? []).map((item) => ({
        ...item,
        stock: item.stock ?? stocksById.get(item.stockId),
      })),
    }))
  } catch (e) {
    error.value =
      e instanceof Error
        ? e.message
        : 'Не удалось загрузить списки.'

    if (!isAuthenticated.value) {
      await router.replace({
        name: 'login',
        query: { redirect: '/watchlists' },
      })
    }
  } finally {
    loading.value = false
  }
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
        : 'Не удалось создать список.'

    if (!isAuthenticated.value) {
      await router.replace({
        name: 'login',
        query: { redirect: '/watchlists' },
      })
    }
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
    await removeStockFromWatchlist(watchlistId, stockId)

    await loadWatchlists()
    success.value = 'Акция удалена из списка.'
  } catch (e) {
    error.value =
      e instanceof Error
        ? e.message
        : 'Не удалось удалить акцию.'

    if (!isAuthenticated.value) {
      await router.replace({
        name: 'login',
        query: { redirect: '/watchlists' },
      })
    }
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
        class="button-secondary"
        type="button"
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

    <div v-if="error" class="notice notice-error">
      {{ error }}
    </div>

    <div v-if="success" class="notice notice-success">
      {{ success }}
    </div>

    <section class="market-section create-list-section">
      <div class="section-heading">
        <div>
          <h2>Создать список</h2>

          <p>
            Например, «Долгосрочные инвестиции».
          </p>
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
          required
        />

        <button
          class="button-primary"
          type="submit"
          :disabled="creating"
        >
          <Plus :size="17" />

          {{
            creating
              ? 'Создание...'
              : 'Создать список'
          }}
        </button>
      </form>
    </section>

    <div v-if="loading && watchlists.length === 0" class="feedback">
      Загружаем списки наблюдения...
    </div>

    <div v-else class="watchlists-grid">
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
                  'Акция недоступна'
                }}
              </strong>

              <small v-if="item.stock?.tickerSymbol && item.stock?.name">
                {{ item.stock.name }}
              </small>

              <small v-if="item.stock?.lastPrice != null">
                {{
                  item.stock.lastPrice.toLocaleString('ru-RU', {
                    maximumFractionDigits: 2,
                  })
                }}
                {{ item.stock.currency ?? '' }}
              </small>
            </div>

            <button
              class="remove-button"
              type="button"
              title="Удалить акцию из списка"
              aria-label="Удалить акцию из списка"
              @click="removeStock(list.id, item.stockId)"
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
      <div class="empty-state-icon">
        <LockKeyhole :size="27" />
      </div>

      <h2>Списков пока нет</h2>

      <p>
        Создай первый список, чтобы группировать
        интересующие тебя акции.
      </p>
    </div>
  </section>
</template>