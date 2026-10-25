<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { ArrowLeft, RefreshCw } from 'lucide-vue-next'
import { Line } from 'vue-chartjs'
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Tooltip,
  Legend,
  Filler,
  type ChartData,
  type ChartOptions,
} from 'chart.js'

import { getStocks, getPriceHistory } from '../services/stocks'
import type { StockDto } from '../types/stockDto'
import type { PriceHistoryDto } from '../types/priceHistory'

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Tooltip,
  Legend,
  Filler,
)

const route = useRoute()

const stock = ref<StockDto | null>(null)
const history = ref<PriceHistoryDto[]>([])
const loading = ref(true)
const error = ref('')

const currency = computed(() => stock.value?.currency || 'RUB')

function formatPrice(value: number | null): string {
  if (value === null) return '—'

  try {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: currency.value,
      maximumFractionDigits: 2,
    }).format(value)
  } catch {
    return `${value.toLocaleString('ru-RU')} ${currency.value}`
  }
}

function formatPercent(value: number | null): string {
  if (value === null) return '—'

  return `${value > 0 ? '+' : ''}${value.toLocaleString('ru-RU', {
    maximumFractionDigits: 2,
    minimumFractionDigits: 2,
  })}%`
}

const chartData = computed<ChartData<'line'>>(() => ({
  labels: history.value.map((item) =>
    new Date(item.recordedAt).toLocaleString('ru-RU', {
      day: '2-digit',
      month: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    }),
  ),
  datasets: [
    {
      label: 'Цена',
      data: history.value.map((item) => item.price),
      borderColor: '#54d6a0',
      backgroundColor: 'rgba(84, 214, 160, 0.10)',
      pointBackgroundColor: '#54d6a0',
      pointBorderColor: '#54d6a0',
      pointRadius: history.value.length > 50 ? 0 : 3,
      pointHoverRadius: 5,
      borderWidth: 2,
      tension: 0.25,
      fill: true,
    },
  ],
}))

const chartOptions: ChartOptions<'line'> = {
  responsive: true,
  maintainAspectRatio: false,
  interaction: {
    intersect: false,
    mode: 'index',
  },
  plugins: {
    legend: {
      display: false,
    },
    tooltip: {
      callbacks: {
        label: (context) =>
          `Цена: ${formatPrice(Number(context.parsed.y))}`,
      },
    },
  },
  scales: {
    x: {
      grid: {
        color: '#253142',
      },
      ticks: {
        color: '#8997aa',
        maxTicksLimit: 8,
      },
    },
    y: {
      grid: {
        color: '#253142',
      },
      ticks: {
        color: '#8997aa',
        callback: (value) =>
          Number(value).toLocaleString('ru-RU'),
      },
    },
  },
}

async function loadDetails() {
  loading.value = true
  error.value = ''

  try {
    const stockId = String(route.params.id)
    const stocks = await getStocks()

    stock.value =
      stocks.find((item) => item.id === stockId) ?? null

    if (!stock.value) {
      throw new Error('Акция не найдена в базе данных')
    }

    history.value = await getPriceHistory(stockId)

    history.value.sort(
      (a, b) =>
        new Date(a.recordedAt).getTime() -
        new Date(b.recordedAt).getTime(),
    )
  } catch (e) {
    error.value =
      e instanceof Error
        ? e.message
        : 'Не удалось загрузить данные акции'
  } finally {
    loading.value = false
  }
}

onMounted(loadDetails)
</script>

<template>
  <section class="page-container">
    <div class="page-heading">
      <div>
        <div class="eyebrow">MARKET INSTRUMENT</div>

        <h1>
          {{ stock?.tickerSymbol ?? 'Карточка акции' }}
        </h1>

        <p class="page-description">
          {{ stock?.name ?? 'Подробная информация об инструменте' }}
        </p>
      </div>

      <div class="detail-actions">
        <RouterLink to="/" class="button-secondary">
          <ArrowLeft :size="17" />
          К рынку
        </RouterLink>

        <button
          class="button-secondary"
          :disabled="loading"
          @click="loadDetails"
        >
          <RefreshCw :size="17" :class="{ spinning: loading }" />
          Обновить
        </button>
      </div>
    </div>

    <div v-if="loading" class="feedback">
      Загружаем информацию об акции...
    </div>

    <div v-else-if="error" class="feedback error-feedback">
      <strong>Не удалось загрузить данные</strong>
      <p>{{ error }}</p>

      <button class="button-secondary" @click="loadDetails">
        Повторить
      </button>
    </div>

    <template v-else-if="stock">
      <div class="stats-grid">
        <article class="stat-card">
          <div class="stat-label">Последняя цена</div>
          <div class="stat-value">
            {{ formatPrice(stock.lastPrice) }}
          </div>
          <div class="stat-note">
            {{ stock.exchange || 'Биржа не указана' }}
          </div>
        </article>

        <article class="stat-card">
          <div class="stat-label">Изменение за день</div>
          <div
            class="stat-value"
            :class="(stock.dayChange ?? 0) >= 0
              ? 'positive'
              : 'negative'"
          >
            {{ stock.dayChange === null
              ? '—'
              : `${stock.dayChange > 0 ? '+' : ''}${stock.dayChange.toLocaleString('ru-RU', { maximumFractionDigits: 2 })}` }}
          </div>
          <div class="stat-note">В валюте инструмента</div>
        </article>

        <article class="stat-card">
          <div class="stat-label">Изменение, %</div>
          <div
            class="stat-value"
            :class="(stock.dayChangePercent ?? 0) >= 0
              ? 'positive'
              : 'negative'"
          >
            {{ formatPercent(stock.dayChangePercent) }}
          </div>
          <div class="stat-note">За торговый день</div>
        </article>
      </div>

      <section class="market-section">
        <div class="section-heading">
          <div>
            <h2>История цены</h2>
            <p>
              Записей в истории: {{ history.length }}
            </p>
          </div>
        </div>

        <div v-if="history.length === 0" class="feedback">
          История цен пока пуста. Дождись записи первых цен
          или проверь работу синхронизации истории.
        </div>

        <div v-else class="price-chart">
          <Line
            :data="chartData"
            :options="chartOptions"
            aria-label="График истории цены акции"
          />
        </div>

        <div class="table-footer">
          <RouterLink to="/">← Вернуться к списку акций</RouterLink>
          <span>TrendSense API</span>
        </div>
      </section>
    </template>
  </section>
</template>