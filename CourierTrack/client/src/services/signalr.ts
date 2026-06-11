import * as signalR from '@microsoft/signalr'
import Cookie from 'js-cookie'

const HUB_URL = import.meta.env.VITE_SIGNALR_HUB_URL || '/hubs/tracking'

const HUB_EVENTS = {
  courierLocationUpdated: 'CourierLocationUpdated',
  orderStatusUpdated: 'OrderStatusUpdated',
  newOrderAssigned: 'NewOrderAssigned',
  orderPickedUp: 'OrderPickedUp',
} as const

type HubEvent = (typeof HUB_EVENTS)[keyof typeof HUB_EVENTS]
type EventCallback = (data: unknown) => void

let connection: signalR.HubConnection | null = null
let connectPromise: Promise<signalR.HubConnection | null> | null = null
const pendingListeners = new Map<HubEvent, Set<EventCallback>>()

const registerPendingListener = (event: HubEvent, callback: EventCallback) => {
  if (!pendingListeners.has(event)) {
    pendingListeners.set(event, new Set())
  }
  pendingListeners.get(event)!.add(callback)
}

const attachListener = (event: HubEvent, callback: EventCallback) => {
  if (connection) {
    connection.on(event, callback)
  }
  registerPendingListener(event, callback)
}

const applyPendingListeners = () => {
  if (!connection) return

  pendingListeners.forEach((callbacks, event) => {
    callbacks.forEach((callback) => {
      connection!.off(event, callback)
      connection!.on(event, callback)
    })
  })
}

export const initSignalRConnection = async (): Promise<signalR.HubConnection | null> => {
  const token = Cookie.get('authToken')
  if (!token) {
    return null
  }

  if (connection?.state === signalR.HubConnectionState.Connected) {
    return connection
  }

  if (connectPromise) {
    return connectPromise
  }

  connectPromise = (async () => {
    if (connection) {
      await connection.stop().catch(() => undefined)
      connection = null
    }

    const urlWithToken = `${HUB_URL}?access_token=${encodeURIComponent(token)}`

    connection = new signalR.HubConnectionBuilder()
      .withUrl(urlWithToken, {
        withCredentials: true,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 15000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    applyPendingListeners()

    try {
      await connection.start()
      return connection
    } catch (err) {
      console.error('SignalR connection error:', err)
      connection = null
      return null
    } finally {
      connectPromise = null
    }
  })()

  return connectPromise
}

export const getConnection = (): signalR.HubConnection | null => connection

export const disconnectSignalR = async () => {
  connectPromise = null
  if (connection) {
    await connection.stop()
    connection = null
  }
}

const invokeHubMethod = async (method: string, ...args: unknown[]) => {
  const hub = await initSignalRConnection()
  if (!hub || hub.state !== signalR.HubConnectionState.Connected) {
    return
  }

  await hub.invoke(method, ...args)
}

export const joinOrderGroup = async (orderId: string) => {
  await invokeHubMethod('JoinOrderGroup', orderId)
}

export const leaveOrderGroup = async (orderId: string) => {
  await invokeHubMethod('LeaveOrderGroup', orderId)
}

export const joinCourierGroup = async (courierId: string) => {
  await invokeHubMethod('JoinCourierGroup', courierId)
}

export const leaveCourierGroup = async (courierId: string) => {
  await invokeHubMethod('LeaveCourierGroup', courierId)
}

export const joinAdminGroup = async () => {
  await invokeHubMethod('JoinAdminGroup')
}

export const leaveAdminGroup = async () => {
  await invokeHubMethod('LeaveAdminGroup')
}

export const onLocationUpdate = (callback: (data: any) => void) => {
  attachListener(HUB_EVENTS.courierLocationUpdated, callback)
}

export const onOrderNotification = (callback: (data: any) => void) => {
  attachListener(HUB_EVENTS.newOrderAssigned, callback)
}

export const onOrderStatusChange = (callback: (data: any) => void) => {
  attachListener(HUB_EVENTS.orderStatusUpdated, callback)
}

export const onOrderPickedUp = (callback: (data: any) => void) => {
  attachListener(HUB_EVENTS.orderPickedUp, callback)
}
