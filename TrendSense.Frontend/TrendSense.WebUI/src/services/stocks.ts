import type { PriceHistoryDto } from '../types/priceHistory'
import type { StockDto } from '../types/stockDto'
import type { PagedResult } from '../types/pagination'

export async function getStocks(
  page = 1,
  pageSize = 20,
): Promise<PagedResult<StockDto>> {
  const params = new URLSearchParams({
    page: String(page),
    pageSize: String(pageSize),
  })

  const response = await fetch(`/api/v1/Stocks?${params}`)

  if (!response.ok) {
    throw new Error(
      `Не удалось загрузить акции. HTTP ${response.status}`,
    )
  }

  return (await response.json()) as PagedResult<StockDto>
}

export async function getStockById(
  id: string,
): Promise<StockDto> {
  const response = await fetch(
    `/api/v1/Stocks/${encodeURIComponent(id)}`,
  )

  if (!response.ok) {
    throw new Error(
      `Не удалось загрузить акцию. HTTP ${response.status}`,
    )
  }

  return (await response.json()) as StockDto
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