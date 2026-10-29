import type { PriceHistoryDto } from '../types/priceHistory'
import type { StockDto } from '../types/stockDto'

export async function getStocks(): Promise<StockDto[]> 
{
    const response = await fetch('/api/v1/Stocks')

    if (!response.ok)
    {
        throw new Error(`Не удалось загрузить акции. HTTP ${response.status}`)
    }

    return (await response.json()) as StockDto[]
}

export async function getPriceHistory(stockId: string): Promise<PriceHistoryDto[]>
{
    const response = await fetch(`/api/v1/Stocks/${encodeURIComponent(stockId)}/history`)

    if (!response.ok)
    {
        throw new Error(`Не удалось загрузить историю цен. HTTP ${response.status}`)
    }

    return (await response.json()) as PriceHistoryDto[]
}