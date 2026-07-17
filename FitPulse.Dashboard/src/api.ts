import { API_BASE_URL } from './config'

async function request(path: string, token: string, options: RequestInit = {}) {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
      ...options.headers,
    },
  })

  if (!response.ok) {
    const text = await response.text()
    throw new Error(`${response.status} ${response.statusText}: ${text}`)
  }

  if (response.status === 204) return null
  return response.json()
}

export const api = {
  get: (path: string, token: string) => request(path, token),
  post: (path: string, token: string, body: unknown) =>
    request(path, token, { method: 'POST', body: JSON.stringify(body) }),
  put: (path: string, token: string, body?: unknown) =>
    request(path, token, { method: 'PUT', body: body ? JSON.stringify(body) : undefined }),
}

export function decodePermissions(accessToken: string): string[] {
  const payload = JSON.parse(atob(accessToken.split('.')[1]))
  return payload.permissions ?? []
}

export async function graphql(query: string, token: string) {
  const response = await fetch(`${API_BASE_URL}/graphql`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ query }),
  })

  const json = await response.json()
  if (json.errors) {
    throw new Error(json.errors.map((e: { message: string }) => e.message).join(', '))
  }
  return json.data
}
