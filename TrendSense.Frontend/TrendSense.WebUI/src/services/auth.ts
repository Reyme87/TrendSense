import { computed, ref } from 'vue'
import type { AuthResultDto } from '../types/authResultDto'
import type { LoginRequestDto } from '../types/loginRequestDto'
import type { RegisterUserCommand } from '../types/registerUserCommand'

const TOKEN_KEY = 'trendsense_access_token'
const EXPIRES_AT_KEY = 'trendsense_expires_at'
function restoreSession() {
  const token = sessionStorage.getItem(TOKEN_KEY) ?? ''
  const expiresAt = sessionStorage.getItem(EXPIRES_AT_KEY) ?? ''

  if (
    !token ||
    !expiresAt ||
    !Number.isFinite(Date.parse(expiresAt)) ||
    Date.parse(expiresAt) <= Date.now()
  ) {
    sessionStorage.removeItem(TOKEN_KEY)
    sessionStorage.removeItem(EXPIRES_AT_KEY)

    return { token: '', expiresAt: '' }
  }

  return { token, expiresAt }
}

const initialSession = restoreSession()

const accessToken = ref(initialSession.token)
const expiresAt = ref(initialSession.expiresAt)

function hasValidSession(): boolean {
  const expiration = Date.parse(expiresAt.value)

  if (
    !accessToken.value ||
    !Number.isFinite(expiration) ||
    expiration <= Date.now()
  ) {
    clearAccessToken()
    return false
  }

  return true
}

export const isAuthenticated = computed(
  () =>
    Boolean(accessToken.value) &&
    Boolean(expiresAt.value) &&
    Date.parse(expiresAt.value) > Date.now(),
)

export function getAccessToken(): string {
  return hasValidSession() ? accessToken.value : ''
}

export function saveAuthSession(result: AuthResultDto): void {
  const expiration = Date.parse(result.expiresAt)

  if (
    !result.token ||
    !Number.isFinite(expiration) ||
    expiration <= Date.now()
  ) {
    throw new Error('Сервер вернул недействительный токен или срок его действия.')
  }

  accessToken.value = result.token
  expiresAt.value = result.expiresAt

  sessionStorage.setItem(TOKEN_KEY, result.token)
  sessionStorage.setItem(EXPIRES_AT_KEY, result.expiresAt)
}

export function clearAccessToken(): void {
  accessToken.value = ''
  expiresAt.value = ''

  sessionStorage.removeItem(TOKEN_KEY)
  sessionStorage.removeItem(EXPIRES_AT_KEY)
}

async function readResponse(response: Response): Promise<unknown> {
  const text = await response.text()

  if (!text) {
    return null
  }

  try {
    return JSON.parse(text) as unknown
  } catch {
    return text
  }
}

function getErrorMessage(body: unknown, fallback: string): string {
  if (typeof body === 'string' && body.trim()) {
    return body
  }

  if (body && typeof body === 'object') {
    const data = body as Record<string, unknown>

    for (const key of ['detail', 'message', 'title']) {
      if (typeof data[key] === 'string' && data[key]) {
        return data[key]
      }
    }
  }

  return fallback
}

export async function login(
  email: string,
  password: string,
): Promise<void> {
  const request: LoginRequestDto = {
    email,
    password,
  }

  const response = await fetch('/api/v1/Auth/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Accept: 'application/json',
    },
    body: JSON.stringify(request),
  })

  const body = await readResponse(response)

  if (!response.ok) {
    throw new Error(
      getErrorMessage(body, `Ошибка входа: HTTP ${response.status}`),
    )
  }

  if (
    !body ||
    typeof body !== 'object' ||
    !('token' in body) ||
    !('expiresAt' in body)
  ) {
    throw new Error('Ответ сервера не соответствует AuthResultDto.')
  }

  const result = body as AuthResultDto
  saveAuthSession(result)
}

export async function register(
  command: RegisterUserCommand,
): Promise<string> {
  const response = await fetch('/api/v1/Auth/register', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Accept: 'application/json',
    },
    body: JSON.stringify(command),
  })

  const body = await readResponse(response)

  if (!response.ok) {
    throw new Error(
      getErrorMessage(body, `Ошибка регистрации: HTTP ${response.status}`),
    )
  }

  // RegisterUserCommand реализует IRequest<Guid>,
  // поэтому ожидаем идентификатор пользователя.
  if (typeof body !== 'string') {
    throw new Error('Ответ регистрации не содержит Guid пользователя.')
  }

  return body
}

export function hasValidAccessSession(): boolean {
  return hasValidSession()
}