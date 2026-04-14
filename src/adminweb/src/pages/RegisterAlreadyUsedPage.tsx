export function RegisterAlreadyUsedPage() {
  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <div className="bg-white rounded-lg shadow-md max-w-md w-full p-8 text-center">
        <div className="text-green-500 text-5xl mb-4">✓</div>
        <h1 className="text-xl font-semibold text-gray-900 mb-3">Account already activated</h1>
        <p className="text-gray-600">
          This registration link has already been used. Your account is active — please log in
          directly.
        </p>
      </div>
    </div>
  )
}
