import { HubConnectionBuilder, HttpTransportType, LogLevel } from '@microsoft/signalr'
import { apiBaseUrl } from '../api/client'

export const robotHubPath = '/hubs/robot'

export const createRobotHubConnection = () => {
  return new HubConnectionBuilder()
    .withUrl(`${apiBaseUrl}${robotHubPath}`, {
      withCredentials: true,
      transport: HttpTransportType.WebSockets | HttpTransportType.LongPolling,
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()
}

export const robotHubEvents = {
  commandUpdated: 'RobotCommandUpdated',
  positionUpdated: 'RobotPositionUpdated',
  sequenceUpdated: 'RobotSequenceUpdated',
} as const

export const robotHubMethods = {
  subscribeToCommand: 'SubscribeToCommand',
  unsubscribeFromCommand: 'UnsubscribeFromCommand',
  subscribeToSequence: 'SubscribeToSequence',
  unsubscribeFromSequence: 'UnsubscribeFromSequence',
} as const
