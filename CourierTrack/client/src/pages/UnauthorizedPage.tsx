import React from 'react'
import { Link } from 'react-router-dom'
import { Lock } from 'lucide-react'

const UnauthorizedPage: React.FC = () => {
  return (
    <div className="flex items-center justify-center min-h-screen bg-slate-50">
      <div className="text-center">
        <Lock className="w-16 h-16 text-red-600 mx-auto mb-4" />
        <h1 className="text-4xl font-bold text-slate-900 mb-2">Yetkisiz Erişim</h1>
        <p className="text-slate-600 mb-8">Bu sayfaya erişim izniniz yok.</p>
        <Link
          to="/"
          className="inline-block px-6 py-3 bg-blue-600 text-white rounded-lg font-semibold hover:bg-blue-700 transition-colors"
        >
          Ana Sayfa
        </Link>
      </div>
    </div>
  )
}

export default UnauthorizedPage
