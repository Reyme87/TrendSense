import type { WatchListItemDto } from "./watchListItemDto"

export interface WatchListDto
{
    id: string
    name: string
    items: WatchListItemDto[]
    createdAt?: string
}