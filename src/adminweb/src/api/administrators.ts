const apiUrl = import.meta.env.VITE_API_URL as string

export interface Administrator {
  id: string
  email: string
  firstName: string
  lastName: string
  status: string
}

export interface ListAdministratorsResponse {
  administrators: Administrator[]
}

export async function listAdministrators(
  token: string,
  search?: string,
): Promise<ListAdministratorsResponse> {
  const url = new URL(`${apiUrl}/admin/administrators`)
  if (search && search.length >= 3) {
    url.searchParams.set('search', search)
  }

  const response = await fetch(url.toString(), {
    headers: { Authorization: `Bearer ${token}` },
  })

  if (!response.ok) {
    throw new Error(`Failed to fetch administrators: ${response.status}`)
  }

  return response.json()
}

export async function deactivateAdministrator(token: string, id: string): Promise<void> {
  const response = await fetch(`${apiUrl}/admin/administrators/${id}/deactivate`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${token}` },
  })

  if (!response.ok) {
    const body = await response.text().catch(() => '')
    throw new Error(body || `Failed to deactivate administrator: ${response.status}`)
  }
}

export async function reactivateAdministrator(token: string, id: string): Promise<void> {
  const response = await fetch(`${apiUrl}/admin/administrators/${id}/reactivate`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${token}` },
  })

  if (!response.ok) {
    const body = await response.text().catch(() => '')
    throw new Error(body || `Failed to reactivate administrator: ${response.status}`)
  }
}

export interface InviteAdministratorRequest {
  firstName: string
  lastName: string
  email: string
}

export async function inviteAdministrator(
  token: string,
  request: InviteAdministratorRequest,
): Promise<void> {
  const response = await fetch(`${apiUrl}/admin/administrators/invite`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  })

  if (!response.ok) {
    const body = await response.text().catch(() => '')
    throw new Error(body || `Failed to invite administrator: ${response.status}`)
  }
}
