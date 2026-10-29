export interface WatchListItemDto
{
    id?: string
    stockId: string
    addedAt?: string
    stock?: {
        id: string
        tickerSymbol: string
        name: string
        lastPrice: number | null
        currency?: string | null
    }
}