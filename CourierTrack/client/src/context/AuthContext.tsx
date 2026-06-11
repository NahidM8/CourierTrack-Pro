import React, { createContext, useContext, useEffect, useState } from 'react'
import Cookie from 'js-cookie'
import { User, AuthState } from '@/types'
import { authAPI } from '@/services/api'
import { disconnectSignalR, initSignalRConnection } from '@/services/signalr'

interface AuthContextType extends AuthState {
  login: (email: string, password: string) => Promise<void>
  register: (data: any) => Promise<void>
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

const decodeJwtPayload = (token: string) => {
  const payload = token.split('.')[1]
  if (!payload) {
    throw new Error('Invalid token payload')
  }

  const normalized = payload.replace(/-/g, '+').replace(/_/g, '/')
  const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, '=')
  const json = atob(padded)
  return JSON.parse(json) as Record<string, any>
}

const buildUserFromToken = (token: string): User => {
  const payload = decodeJwtPayload(token)
  const id = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || ''
  const email = payload.email || ''
  const name = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || ''
  const roleClaim = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || ''
  const normalizedRole = String(roleClaim).toUpperCase()

  let role: User['role'] = 'CUSTOMER'
  if (normalizedRole.includes('ADMIN')) {
    role = 'ADMIN'
  } else if (normalizedRole.includes('COURIER')) {
    role = 'COURIER'
  }

  return {
    id,
    email,
    name,
    phone: '',
    role,
    isActive: true,
    createdAt: new Date().toISOString(),
  }
}

const mapRoleToApiValue = (role: User['role']) => {
  switch (role) {
    case 'COURIER':
      return 1
    case 'ADMIN':
      return 2
    default:
      return 0
  }
}

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [authState, setAuthState] = useState<AuthState>({
    user: null,
    token: null,
    isAuthenticated: false,
    isLoading: true,
  })

  // Initialize auth from stored token
  useEffect(() => {
    const initAuth = async () => {
      const token = Cookie.get('authToken')
      if (token) {
        try {
          const user = buildUserFromToken(token)
          setAuthState({
            user,
            token,
            isAuthenticated: true,
            isLoading: false,
          })
          await initSignalRConnection()
        } catch {
          Cookie.remove('authToken')
          setAuthState({
            user: null,
            token: null,
            isAuthenticated: false,
            isLoading: false,
          })
        }
      } else {
        setAuthState((prev) => ({
          ...prev,
          isLoading: false,
        }))
      }
    }

    initAuth()
  }, [])

  const login = async (email: string, password: string) => {
    setAuthState((prev) => ({ ...prev, isLoading: true }))
    try {
      const response = await authAPI.login(email, password)
      const { accessToken } = response.data
      const user = buildUserFromToken(accessToken)
      Cookie.set('authToken', accessToken, { expires: 7, secure: true, sameSite: 'strict' })
      setAuthState({
        user,
        token: accessToken,
        isAuthenticated: true,
        isLoading: false,
      })
      await initSignalRConnection()
    } catch (error) {
      setAuthState({
        user: null,
        token: null,
        isAuthenticated: false,
        isLoading: false,
      })
      throw error
    }
  }

  const register = async (data: any) => {
    setAuthState((prev) => ({ ...prev, isLoading: true }))
    try {
      const response = await authAPI.register({
        fullName: data.name,
        email: data.email,
        password: data.password,
        phoneNumber: data.phone,
        role: mapRoleToApiValue(data.role),
        vehicleType: 0,
      })
      const { accessToken } = response.data
      const user = buildUserFromToken(accessToken)
      Cookie.set('authToken', accessToken, { expires: 7, secure: true, sameSite: 'strict' })
      setAuthState({
        user,
        token: accessToken,
        isAuthenticated: true,
        isLoading: false,
      })
      await initSignalRConnection()
    } catch (error) {
      setAuthState({
        user: null,
        token: null,
        isAuthenticated: false,
        isLoading: false,
      })
      throw error
    }
  }

  const logout = async () => {
    try {
      await authAPI.logout()
    } finally {
      await disconnectSignalR()
      Cookie.remove('authToken')
      setAuthState({
        user: null,
        token: null,
        isAuthenticated: false,
        isLoading: false,
      })
    }
  }

  return (
    <AuthContext.Provider
      value={{
        ...authState,
        login,
        register,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export const useAuth = () => {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider')
  }
  return context
}
