import { createRouter, createWebHistory } from "vue-router"
import StocksView from '../views/StocksView.vue'
import WatchlistsView from '../views/WatchlistsView.vue'

export default createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: '/',
            name: 'market',
            component: StocksView,
        },
        {
            path: '/watchlists',
            name: 'watchlists',
            component: WatchlistsView,
        }
    ]
})