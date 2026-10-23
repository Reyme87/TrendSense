<script setup lang="ts">
import { Star } from 'lucide-vue-next'
</script>

<template>
  <section class="page-container">
    <div class="page-heading">
      <div>
        <div class="eyebrow">YOUR WORKSPACE</div>
        <h1>Списки наблюдения</h1>
        <p class="page-description">
          Собирай интересующие тебя инструменты в одном месте.
        </p>
      </div>
    </div>

    <div class="empty-state">
      <div class="empty-state-icon">
        <Star :size="27" />
      </div>

      <h2>Твои watchlists будут здесь</h2>

      <p>
        На следующем этапе подключим существующий API
        списков наблюдения.
      </p>
    </div>
  </section>
</template>