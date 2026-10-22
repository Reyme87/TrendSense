<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink, RouterView, useRoute } from 'vue-router'
import {
  ChartLine,
  LayoutDashboard,
  Star,
  Bell,
  ChevronDown
} from 'lucide-vue-next'

const route = useRoute()

const pageTitle = computed(() => route.name === 'watchlists' ? 'Отслеживаемые списки' : 'Рынок')

const navigation = [
  { label: 'Обзор рынка', to: '/', icon: LayoutDashboard },
  { label: 'Отслеживаемые списки', to: '/watchlists', icon: Star },
]
</script>

<template>
  <div class="app-layout">
    <aside class="sidebar">
      <RouterLink to="/" class="brand">
        <span class="brand-icon">
          <ChartLine :size="23" />
        </span>

        <span>
          <strong>TrendSense</strong>
          <small>MARKET INTELLIGENCE</small>
        </span>
      </RouterLink>

      <div class="nav-caption">РАБОЧЕЕ ПРОСТРАНСТВО</div>

      <nav class="navigation">
        <RouterLink
          v-for="item in navigation"
          :key="item.to"
          :to="item.to"
          class="nav-link"
          exact-active-class="active"
        >
          <component :is="item.icon" :size="19" />
          <span>{{ item.label }}</span>
        </RouterLink>
      </nav>

      <div class="sidebar-bottom">
        <div class="connection-dot"></div>

        <div>
          <strong>TrendSense API</strong>
          <small>Подключение через backend</small>
        </div>
      </div>
    </aside>

    <div class="workspace">
      <header class="topbar">
        <div class="breadcrumb">
          <span>TrendSense</span>
          <span class="breadcrumb-separator">/</span>
          <strong>{{ pageTitle }}</strong>
        </div>

        <div class="topbar-actions">
          <button
            class="icon-button"
            type="button"
            aria-label="Уведомления"
          >
            <Bell :size="19" />
          </button>

          <div class="topbar-divider"></div>

          <div class="profile">
            <div class="avatar">TS</div>

            <div class="profile-info">
              <strong>Пользователь</strong>
              <small>Market analyst</small>
            </div>

            <ChevronDown :size="16" />
          </div>
        </div>
      </header>

      <main class="main-content">
        <RouterView />
      </main>
    </div>
  </div>
</template>
