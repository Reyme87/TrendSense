import type { WatchListDto } from "../types/watchListDto"

const BASE_URL = '/api/v1/WatchLists'

export function getAccessToken(): string
{
    return sessionStorage.getItem('trendsense_access_token') ?? ''
}

export function saveAccessToken(token: string): void
{
    sessionStorage.setItem('trendsense_access_token', token)
}

export function clearAccessToken(): void
{
    sessionStorage.removeItem('trendsense_access_token')
}

async function apiRequest<T>(url: string, init: RequestInit = {}): Promise<T> 
{
    const token = getAccessToken()
    
    if (!token)
    {
        throw new Error('Для работы со списками нужна аутентификация')
    }

    const headers = new Headers(init.headers)
    headers.set('Accept', 'application/json')
    headers.set('Authorization', `Bearer ${token}`)

    if (init.body)
    {
        headers.set('Content-type', 'application/json')
    }

    const response = await fetch(url, {...init, headers})

    if (response.status === 401)
    {
        throw new Error('Сессия истекла или JWT-токен недействителен')
    }

    if (response.status === 403) {
    throw new Error('У тебя нет доступа к этой операции.')
  }

  if (!response.ok) {
    const body = await response.text()
    throw new Error(
      body || `Ошибка API: HTTP ${response.status}`,
    )
  }

  if (response.status === 204) {
    return undefined as T
  }

  const text = await response.text()

  return (text ? JSON.parse(text) : undefined) as T
}

export async function getWatchlists(): Promise<WatchListDto[]> {
  const result = await apiRequest<unknown>(BASE_URL)

  if (Array.isArray(result)) {
    return result as WatchListDto[]
  }

  const data = result as {
    watchLists?: WatchListDto[]
    watchlists?: WatchListDto[]
    items?: WatchListDto[]
    data?: WatchListDto[]
  }

  return (
    data.watchLists ??
    data.watchlists ??
    data.items ??
    data.data ??
    []
  )
}

export async function createWatchlist(
  name: string,
): Promise<void> {
  await apiRequest(BASE_URL, {
    method: 'POST',
    body: JSON.stringify({ name }),
  })
}

export async function addStockToWatchlist(
  watchlistId: string,
  stockId: string,
): Promise<void> {
  await apiRequest(
    `${BASE_URL}/${watchlistId}/stocks`,
    {
      method: 'POST',
      body: JSON.stringify({ watchlistId, stockId }),
    },
  )
}

export async function removeStockFromWatchlist(
  watchlistId: string,
  stockId: string,
): Promise<void> {
  await apiRequest(
    `${BASE_URL}/${watchlistId}/stocks/${stockId}`,
    {
      method: 'DELETE',
    },
  )
}