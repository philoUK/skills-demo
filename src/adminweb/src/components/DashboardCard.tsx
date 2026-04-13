import { Link } from 'react-router-dom'

interface DashboardCardProps {
  icon: string
  title: string
  description: string
  link: string
}

export function DashboardCard({ icon, title, description, link }: DashboardCardProps) {
  return (
    <Link
      to={link}
      style={{ textDecoration: 'none', color: 'inherit' }}
    >
      <div
        style={{
          border: '1px solid #d1d5db',
          borderRadius: '8px',
          padding: '24px',
          cursor: 'pointer',
          width: '240px',
          transition: 'box-shadow 0.2s',
        }}
        onMouseEnter={(e) =>
          (e.currentTarget.style.boxShadow = '0 4px 12px rgba(0,0,0,0.1)')
        }
        onMouseLeave={(e) => (e.currentTarget.style.boxShadow = 'none')}
      >
        <div style={{ fontSize: '2.5rem', marginBottom: '12px' }}>{icon}</div>
        <h3 style={{ margin: '0 0 8px', fontSize: '1.1rem' }}>{title}</h3>
        <p style={{ margin: 0, color: '#6b7280', fontSize: '0.9rem' }}>{description}</p>
      </div>
    </Link>
  )
}
