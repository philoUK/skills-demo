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
