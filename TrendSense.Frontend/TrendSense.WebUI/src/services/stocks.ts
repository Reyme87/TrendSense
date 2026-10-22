import type { StockDto } from '../types/stockDto'

export async function getStocks(): Promise<StockDto[]> {
    const response = await fetch('/api/v1/Stocks')

    if (!response.ok)
    {
        throw new Error(`Не удалось загрузить акции. HTTP ${response.status}`)
    }

    return (await response.json()) as StockDto[]
}