import { InactiveAccountError } from './errors'

const apiUrl = import.meta.env.VITE_API_URL as string

export async function ping(token: string): Promise<void> {
  const response = await fetch(`${apiUrl}/admin/ping`, {
    headers: { Authorization: `Bearer ${token}` },
  })
  if (response.status === 401) throw new InactiveAccountError()
  if (!response.ok) throw new Error(`Ping failed: ${response.status}`)
}
