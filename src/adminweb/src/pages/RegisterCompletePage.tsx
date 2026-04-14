export function RegisterCompletePage() {
  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <div className="bg-white rounded-lg shadow-md max-w-md w-full p-8 text-center">
        <div className="text-blue-500 text-5xl mb-4">✉</div>
        <h1 className="text-xl font-semibold text-gray-900 mb-3">Check your email</h1>
        <p className="text-gray-600">
          Your account has been created. We have sent you an email with a link to set your
          password. Once you have set your password you can log in.
        </p>
      </div>
    </div>
  )
}
