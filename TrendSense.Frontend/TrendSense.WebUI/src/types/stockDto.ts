export interface StockDto
{
    id: string,
    tickerSymbol: string,
    name: string,
    currency: string | null,
    exchange: string | null,
    lastPrice: number | null,
    dayChange: number | null,
    dayChangePercent: number | null,
    updatedAt: string | null
}