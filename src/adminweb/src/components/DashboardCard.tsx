import { Link } from 'react-router-dom'

interface DashboardCardProps {
  icon: string
  title: string
  description: string
  link: string
}

export function DashboardCard({ icon, title, description, link }: DashboardCardProps) {
  return (
    <Link to={link} className="no-underline text-inherit block w-60">
      <div className="border border-gray-300 rounded-lg p-6 hover:shadow-md transition-shadow duration-200">
        <div className="text-4xl mb-3">{icon}</div>
        <h3 className="m-0 mb-2 text-lg font-semibold">{title}</h3>
        <p className="text-gray-500 text-sm">{description}</p>
      </div>
    </Link>
  )
}
