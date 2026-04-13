import { useAuth } from 'react-oidc-context'
import { DashboardCard } from '../components/DashboardCard'

export function LandingPage() {
  const auth = useAuth()

  return (
    <div>
      <div className="flex justify-between items-center px-6 py-4 border-b border-gray-200">
        <h1 className="m-0 text-2xl font-semibold">Admin Dashboard</h1>
        <button
          onClick={() => auth.signoutRedirect()}
          className="px-4 py-2 border border-gray-300 rounded-md bg-white hover:bg-gray-50 cursor-pointer"
        >
          Sign out
        </button>
      </div>

      <div className="px-6 py-8">
        <h2 className="mt-0 mb-4 text-xl font-semibold text-gray-700">Overview</h2>
        <div className="flex gap-4 flex-wrap">
          <DashboardCard
            icon="👥"
            title="Administrators"
            description="Manage administrator accounts and permissions"
            link="/administrators"
          />
        </div>
      </div>
    </div>
  )
}
