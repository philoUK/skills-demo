import { useAuth } from 'react-oidc-context'
import { DashboardCard } from '../components/DashboardCard'

export function LandingPage() {
  const auth = useAuth()

  return (
    <div style={{ fontFamily: 'sans-serif' }}>
      <div
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          padding: '16px 24px',
          borderBottom: '1px solid #e5e7eb',
        }}
      >
        <h1 style={{ margin: 0, fontSize: '1.5rem' }}>Admin Dashboard</h1>
        <button
          onClick={() => auth.signoutRedirect()}
          style={{ padding: '8px 16px', cursor: 'pointer' }}
        >
          Sign out
        </button>
      </div>

      <div style={{ padding: '32px 24px' }}>
        <h2 style={{ marginTop: 0, color: '#374151' }}>Overview</h2>
        <div style={{ display: 'flex', gap: '16px', flexWrap: 'wrap' }}>
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
