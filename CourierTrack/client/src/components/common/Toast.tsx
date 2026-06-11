import React, { useEffect, useState } from 'react'
import { CheckCircle, AlertCircle, X } from 'lucide-react'

export type ToastType = 'success' | 'error' | 'info' | 'warning'

interface ToastProps {
  id: string
  message: string
  type: ToastType
  onClose: (id: string) => void
}

const Toast: React.FC<ToastProps> = ({ id, message, type, onClose }) => {
  useEffect(() => {
    const timer = setTimeout(() => onClose(id), 4000)
    return () => clearTimeout(timer)
  }, [id, onClose])

  const bgColor = {
    success: 'bg-green-50 border-green-200',
    error: 'bg-red-50 border-red-200',
    info: 'bg-blue-50 border-blue-200',
    warning: 'bg-amber-50 border-amber-200',
  }[type]

  const textColor = {
    success: 'text-green-800',
    error: 'text-red-800',
    info: 'text-blue-800',
    warning: 'text-amber-800',
  }[type]

  const iconColor = {
    success: 'text-green-600',
    error: 'text-red-600',
    info: 'text-blue-600',
    warning: 'text-amber-600',
  }[type]

  return (
    <div className={`${bgColor} border rounded-lg p-4 flex items-start gap-3 max-w-sm`}>
      {type === 'success' && <CheckCircle className={`${iconColor} w-5 h-5 flex-shrink-0 mt-0.5`} />}
      {type === 'error' && <AlertCircle className={`${iconColor} w-5 h-5 flex-shrink-0 mt-0.5`} />}
      {type === 'info' && <AlertCircle className={`${iconColor} w-5 h-5 flex-shrink-0 mt-0.5`} />}
      {type === 'warning' && <AlertCircle className={`${iconColor} w-5 h-5 flex-shrink-0 mt-0.5`} />}
      <p className={`${textColor} flex-1 text-sm font-medium`}>{message}</p>
      <button
        onClick={() => onClose(id)}
        className={`${textColor} hover:opacity-70 transition-opacity`}
      >
        <X className="w-4 h-4" />
      </button>
    </div>
  )
}

export default Toast
