import React, { useState, useEffect } from 'react'
import { Check, X, Users, Search } from 'lucide-react'
import { adminAPI } from '@/services/api'
import { ToastType } from '@/components/common/Toast'
import Swal from 'sweetalert2'

interface UserManagementPageProps {
  onAddToast?: (message: string, type: ToastType) => void
}

const UserManagementPage: React.FC<UserManagementPageProps> = ({ onAddToast }) => {
  const [users, setUsers] = useState<any[]>([])
  const [loading, setLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState('')

  useEffect(() => {
    fetchUsers()
  }, [])

  const fetchUsers = async () => {
    setLoading(true)
    try {
      const response = await adminAPI.getUsers()
      console.log('Users response:', response)
      if (response.data.isSuccess) {
        setUsers(response.data.data)
      } else if (Array.isArray(response.data)) {
        setUsers(response.data)
      } else if (Array.isArray(response.data.data)) {
        setUsers(response.data.data)
      } else {
        console.warn('Unexpected response format:', response.data)
        setUsers([])
      }
    } catch (err: any) {
      console.error('Error fetching users:', err)
      const errorMsg = err.response?.data?.message || err.message || 'Kullanıcılar yüklenemedi'
      onAddToast?.(errorMsg, 'error')
    } finally {
      setLoading(false)
    }
  }

  const handleToggleStatus = async (id: string, currentStatus: boolean) => {
    const result = await Swal.fire({
      title: 'Durumu Değiştir?',
      text: `Bu kullanıcıyı ${currentStatus ? 'pasife' : 'aktife'} almak istediğinize emin misiniz?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: currentStatus ? '#d33' : '#10b981',
      cancelButtonColor: '#64748b',
      confirmButtonText: 'Evet, Değiştir',
      cancelButtonText: 'İptal',
    })

    if (!result.isConfirmed) return

    // Optimistic UI update
    setUsers((prevUsers) =>
      prevUsers.map((user) =>
        user.id === id ? { ...user, isActive: !currentStatus } : user
      )
    )

    try {
      const response = await adminAPI.updateUserStatus(id, !currentStatus)
      if (response.data.isSuccess) {
        onAddToast?.('Kullanıcı durumu güncellendi', 'success')
      }
    } catch (err) {
      // Revert on error
      setUsers((prevUsers) =>
        prevUsers.map((user) =>
          user.id === id ? { ...user, isActive: currentStatus } : user
        )
      )
      onAddToast?.('Durum güncellenirken hata oluştu', 'error')
    }
  }

  const filteredUsers = Array.isArray(users) 
    ? users.filter(
        (user) =>
          user.name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
          user.email?.toLowerCase().includes(searchTerm.toLowerCase())
      )
    : []

  const getRoleLabel = (role: number | string): string => {
    if (typeof role === 'number') {
      switch (role) {
        case 0:
          return 'Customer'
        case 1:
          return 'Courier'
        case 2:
          return 'Admin'
        default:
          return 'Unknown'
      }
    }
    return String(role)
  }

  const getRoleColor = (role: number | string): string => {
    const roleNum = typeof role === 'number' ? role : (role === 'ADMIN' ? 2 : role === 'COURIER' ? 1 : 0)
    switch (roleNum) {
      case 2:
        return 'bg-purple-100 text-purple-700'
      case 1:
        return 'bg-blue-100 text-blue-700'
      default:
        return 'bg-slate-100 text-slate-700'
    }
  }

  if (loading) {
    return (
      <div className="flex justify-center items-center min-h-[400px]">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    )
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-3xl font-bold text-slate-900 mb-2 whitespace-nowrap">
            <Users className="inline-block mr-2 w-8 h-8 text-blue-600" />
            Kullanıcı Yönetimi
          </h1>
          <p className="text-slate-600">Tüm sistem kullanıcılarını görüntüleyin ve yönetin</p>
        </div>
        <div className="relative w-full sm:w-64">
          <input
            type="text"
            placeholder="Kullanıcı ara..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-10 pr-4 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          />
          <Search className="absolute left-3 top-2.5 h-5 w-5 text-slate-400" />
        </div>
      </div>

      <div className="bg-white rounded-lg border border-slate-200 overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left">
            <thead className="bg-slate-50 border-b border-slate-200">
              <tr>
                <th className="px-6 py-4 text-sm font-semibold text-slate-600">Ad Soyad</th>
                <th className="px-6 py-4 text-sm font-semibold text-slate-600">E-posta / Telefon</th>
                <th className="px-6 py-4 text-sm font-semibold text-slate-600">Rol</th>
                <th className="px-6 py-4 text-sm font-semibold text-slate-600">Kayıt Tarihi</th>
                <th className="px-6 py-4 text-sm font-semibold text-slate-600">Durum</th>
                <th className="px-6 py-4 text-sm font-semibold text-slate-600 text-right">İşlemler</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-200">
              {filteredUsers.map((user) => (
                <tr key={user.id} className="hover:bg-slate-50">
                  <td className="px-6 py-4">
                    <p className="font-medium text-slate-900">{user.name}</p>
                  </td>
                  <td className="px-6 py-4">
                    <p className="text-slate-600">{user.email}</p>
                    <p className="text-sm text-slate-500">{user.phone}</p>
                  </td>
                  <td className="px-6 py-4">
                    <span className={`px-2 py-1 rounded-full text-xs font-semibold ${getRoleColor(user.role)}`}>
                      {getRoleLabel(user.role)}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <span className="text-slate-600">
                      {new Date(user.createdAt).toLocaleDateString('tr-TR')}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <span
                      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                        user.isActive
                          ? 'bg-green-100 text-green-800'
                          : 'bg-red-100 text-red-800'
                      }`}
                    >
                      {user.isActive ? 'Aktif' : 'Pasif'}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-right">
                    <button
                      onClick={() => handleToggleStatus(user.id, user.isActive)}
                      className={`inline-flex items-center justify-center p-2 rounded-lg transition-colors ${
                        user.isActive
                          ? 'text-red-600 hover:bg-red-50'
                          : 'text-green-600 hover:bg-green-50'
                      }`}
                      title={user.isActive ? 'Pasife Al' : 'Aktifleştir'}
                    >
                      {user.isActive ? <X className="w-5 h-5" /> : <Check className="w-5 h-5" />}
                    </button>
                  </td>
                </tr>
              ))}
              {filteredUsers.length === 0 && (
                <tr>
                  <td colSpan={6} className="px-6 py-8 text-center text-slate-500">
                    Kullanıcı bulunamadı.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}

export default UserManagementPage