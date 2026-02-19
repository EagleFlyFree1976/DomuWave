# DomuWave Frontend

App Vue 3 per la gestione condominiale.

## Stack

- **Vue 3** + Composition API
- **Vue Router 4** — routing lato client
- **Pinia** — state management
- **Axios** — chiamate HTTP
- **Vite** — build tool

## Setup

```bash
npm install
npm run dev       # avvia su http://localhost:5173
npm run build     # build produzione in /dist
```

## Configurazione API

Il proxy Vite reindirizza `/api/*` → `http://localhost:5000`.  
Modifica `vite.config.js` per cambiare l'URL del backend.

Per l'autenticazione, il token JWT va salvato in `localStorage` con la chiave `token`.  
Il `tenantId` va salvato con la chiave `tenantId`.

```js
localStorage.setItem('token', '<jwt>')
localStorage.setItem('tenantId', '<guid>')
```

## Struttura

```
src/
├── assets/
│   └── main.css          # design system (CSS variables, utility classes)
├── services/
│   └── api.js            # tutti gli endpoint API organizzati per dominio
├── store/
│   └── app.js            # Pinia store (condomini, toast)
├── router/
│   └── index.js          # Vue Router con lazy loading
├── views/
│   ├── DashboardView.vue
│   ├── CondominiView.vue
│   ├── CondominioDetailView.vue
│   ├── UnitaView.vue
│   ├── BudgetView.vue         # tab Budget + Spese
│   ├── RateView.vue           # tab Rate + Quote
│   ├── FornitoriView.vue      # tab Fornitori + Contratti
│   ├── DocumentiView.vue
│   └── ComunicazioniView.vue
└── App.vue                    # layout (sidebar + topbar + toasts)
```

## Endpoint API attesi

| Dominio          | Base path               |
|------------------|-------------------------|
| Condomini        | `/api/condominiums`     |
| Unità            | `/api/realestate-units` |
| Budget           | `/api/budgets`          |
| Spese            | `/api/expenses`         |
| Rate             | `/api/installments`     |
| Quote            | `/api/fees`             |
| Fornitori        | `/api/suppliers`        |
| Contratti        | `/api/supplier-contracts` |
| Documenti        | `/api/documents`        |
| Comunicazioni    | `/api/communications`   |

Adatta i path in `src/services/api.js` ai controller reali del tuo backend.
