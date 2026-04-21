export function RegisterExpiredPage() {
  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <div className="bg-white rounded-lg shadow-md max-w-md w-full p-8 text-center">
        <div className="text-amber-500 text-5xl mb-4">⏱</div>
        <h1 className="text-xl font-semibold text-gray-900 mb-3">Registration link expired</h1>
        <p className="text-gray-600">
          This registration link has expired. Please ask an administrator to send you a new
          invitation.
        </p>
      </div>
    </div>
  )
}
