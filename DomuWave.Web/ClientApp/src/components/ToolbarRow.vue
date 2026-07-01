<template>
  <div class="toolbar-row">
    <div class="toolbar-row__left">
      <slot name="left" />
    </div>
    <div class="toolbar-row__right">
      <slot name="right" />
    </div>
  </div>
</template>

<style scoped>
.toolbar-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex-wrap: nowrap;
  margin-bottom: 1rem;
}

.toolbar-row__left {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex: 1;
  min-width: 0;
}

.toolbar-row__right {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-shrink: 0;
}

@media (max-width: 768px) {
  .toolbar-row {
    flex-wrap: wrap;
  }

  .toolbar-row__left {
    flex-wrap: wrap;
    width: 100%;
  }

  .toolbar-row__right {
    width: 100%;
    justify-content: flex-start;
  }
}
</style>
