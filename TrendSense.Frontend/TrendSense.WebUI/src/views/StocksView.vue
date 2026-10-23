<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import {
    ArrowDownRight,
    ArrowUpRight,
    RefreshCw,
    Search,
    TrendingUp
} from 'lucide-vue-next'

import { getStocks } from '../services/stocks';
import type { StockDto } from '../types/stockDto';

const stocks = ref<StockDto[]>([])
const search = ref('')
const loading = ref(false)
const error = ref('')
const filteredStocks = computed(() => {
  const query = search.value.trim().toLowerCase()

  if (!query) {
    return stocks.value
  }

  return stocks.value.filter(
    (stock) =>
      stock.tickerSymbol.toLowerCase().includes(query) ||
      stock.name.toLowerCase().includes(query),
  )
})

const risingStocks = computed(
  () =>
    stocks.value.filter(
      (stock) => (stock.dayChangePercent ?? 0) > 0,
    ).length,
)

const fallingStocks = computed(
  () =>
    stocks.value.filter(
      (stock) => (stock.dayChangePercent ?? 0) < 0,
    ).length,
)

async function loadStocks() {
  loading.value = true
  error.value = ''

  try {
    stocks.value = await getStocks()
  } catch (e) {
    error.value =
      e instanceof Error
        ? e.message
        : 'Произошла ошибка при загрузке данных'
  } finally {
    loading.value = false
  }
}

function formatPrice(
  value: number | null,
  currency: string | null,
): string {
  if (value === null) {
    return '—'
  }

  const code = currency || 'RUB'

  try {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: code,
      maximumFractionDigits: 2,
    }).format(value)
  } catch {
    return `${value.toLocaleString('ru-RU')} ${code}`
  }
}

function formatChange(value: number | null): string {
  if (value === null) {
    return '—'
  }

  const formatted = value.toLocaleString('ru-RU', {
    maximumFractionDigits: 2,
    minimumFractionDigits: 2,
  })

  return value > 0 ? `+${formatted}` : formatted
}

function formatPercent(value: number | null): string {
  if (value === null) {
    return '—'
  }

  const formatted = value.toLocaleString('ru-RU', {
    maximumFractionDigits: 2,
    minimumFractionDigits: 2,
  })

  return `${value > 0 ? '+' : ''}${formatted}%`
}

onMounted(loadStocks)
</script>

<template>
  <section class="page-container">
    <div class="page-heading">
      <div>
        <div class="eyebrow">
          <TrendingUp :size="15" />
          MARKET OVERVIEW
        </div>

        <h1>Обзор рынка</h1>

        <p class="page-description">
          Следи за котировками и изменениями стоимости инструментов.
        </p>
      </div>

      <button
        class="button-secondary"
        type="button"
        :disabled="loading"
        @click="loadStocks"
      >
        <RefreshCw :size="17" :class="{ spinning: loading }" />
        Обновить
      </button>
    </div>

    <div class="stats-grid">
      <article class="stat-card">
        <div class="stat-label">Инструменты в базе</div>
        <div class="stat-value">{{ stocks.length }}</div>
        <div class="stat-note">Доступны для просмотра</div>
      </article>

      <article class="stat-card">
        <div class="stat-label">Растут</div>
        <div class="stat-value positive">{{ risingStocks }}</div>
        <div class="stat-note">
          <ArrowUpRight :size="15" />
          Положительное дневное изменение
        </div>
      </article>

      <article class="stat-card">
        <div class="stat-label">Снижаются</div>
        <div class="stat-value negative">{{ fallingStocks }}</div>
        <div class="stat-note">
          <ArrowDownRight :size="15" />
          Отрицательное дневное изменение
        </div>
      </article>
    </div>

    <section class="market-section">
      <div class="section-heading">
        <div>
          <h2>Рыночные инструменты</h2>
          <p>
            Найдено: {{ filteredStocks.length }}
          </p>
        </div>

        <label class="search-box">
          <Search :size="18" />

          <input
            v-model="search"
            type="search"
            placeholder="Тикер или название..."
            aria-label="Поиск по акциям"
          />
        </label>
      </div>

      <div v-if="error" class="feedback error-feedback">
        <strong>Не удалось загрузить котировки</strong>
        <p>{{ error }}</p>
        <button class="button-secondary" @click="loadStocks">
          Повторить
        </button>
      </div>

      <div v-else-if="loading && stocks.length === 0" class="feedback">
        Загружаем инструменты...
      </div>

      <div
        v-else-if="filteredStocks.length === 0"
        class="feedback"
      >
        {{ search ? 'Ничего не найдено.' : 'В базе пока нет акций.' }}
      </div>

      <div v-else class="table-wrapper">
        <table class="stocks-table">
          <thead>
            <tr>
              <th>ИНСТРУМЕНТ</th>
              <th>БИРЖА</th>
              <th>ПОСЛЕДНЯЯ ЦЕНА</th>
              <th>ИЗМЕНЕНИЕ</th>
              <th>ИЗМЕНЕНИЕ, %</th>
            </tr>
          </thead>

          <tbody>
            <tr
              v-for="stock in filteredStocks"
              :key="stock.id"
            >
              <td>
                <div class="stock-cell">
                  <div class="stock-symbol">
                    {{ stock.tickerSymbol.slice(0, 2) }}
                  </div>

                  <div>
                    <strong class="ticker">
                      {{ stock.tickerSymbol }}
                    </strong>

                    <div class="stock-name">
                      {{ stock.name }}
                    </div>
                  </div>
                </div>
              </td>

              <td>
                <span class="exchange-tag">
                  {{ stock.exchange || '—' }}
                </span>
              </td>

              <td class="price-cell">
                {{ formatPrice(stock.lastPrice, stock.currency) }}
              </td>

              <td
                :class="
                  (stock.dayChange ?? 0) >= 0
                    ? 'positive'
                    : 'negative'
                "
              >
                {{ formatChange(stock.dayChange) }}
              </td>

              <td>
                <span
                  class="change-badge"
                  :class="
                    (stock.dayChangePercent ?? 0) >= 0
                      ? 'change-positive'
                      : 'change-negative'
                  "
                >
                  {{
                    formatPercent(stock.dayChangePercent)
                  }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="table-footer">
        <span>Данные из TrendSense API</span>
        <span>{{ loading ? 'Обновление...' : ' ' }}</span>
      </div>
    </section>
  </section>
</template>