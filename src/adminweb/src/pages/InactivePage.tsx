export function InactivePage() {
  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <div className="bg-white rounded-lg shadow-md max-w-md w-full p-8 text-center">
        <div className="text-slate-400 text-5xl mb-4">🔒</div>
        <h1 className="text-xl font-semibold text-gray-900 mb-3">Account inactive</h1>
        <p className="text-gray-600">
          Your administrator account has been deactivated. Please contact another administrator to
          reactivate your account.
        </p>
      </div>
    </div>
  )
}
