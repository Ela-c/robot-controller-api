import { useEffect, useRef } from 'react'
import { HubConnectionState, type HubConnection } from '@microsoft/signalr'
import { useRealtime } from '../realtime/RealtimeContext'
import { createRobotHubConnection, robotHubEvents, robotHubMethods } from '../realtime/robotHub'
import type {
  RobotCommandUpdatedEvent,
  RobotPositionUpdatedEvent,
  RobotSequencePositionUpdatedEvent,
  RobotSequenceUpdatedEvent,
} from '../types/signalr'
import { useAuth } from './useAuth'

interface UseRobotHubOptions {
  onCommandUpdated?: (event: RobotCommandUpdatedEvent) => void
  onPositionUpdated?: (event: RobotPositionUpdatedEvent | RobotSequencePositionUpdatedEvent) => void
  onSequenceUpdated?: (event: RobotSequenceUpdatedEvent) => void
}

type CommandHandler = (event: RobotCommandUpdatedEvent) => void
type PositionHandler = (event: RobotPositionUpdatedEvent | RobotSequencePositionUpdatedEvent) => void
type SequenceHandler = (event: RobotSequenceUpdatedEvent) => void

let sharedConnection: HubConnection | null = null
let consumers = 0
let startPromise: Promise<void> | null = null

const commandHandlers = new Set<CommandHandler>()
const positionHandlers = new Set<PositionHandler>()
const sequenceHandlers = new Set<SequenceHandler>()

const getOrCreateConnection = () => {
  if (sharedConnection) {
    return sharedConnection
  }

  const connection = createRobotHubConnection()

  connection.on(robotHubEvents.commandUpdated, (event: RobotCommandUpdatedEvent) => {
    commandHandlers.forEach((handler) => handler(event))
  })

  connection.on(
    robotHubEvents.positionUpdated,
    (event: RobotPositionUpdatedEvent | RobotSequencePositionUpdatedEvent) => {
      positionHandlers.forEach((handler) => handler(event))
    },
  )

  connection.on(robotHubEvents.sequenceUpdated, (event: RobotSequenceUpdatedEvent) => {
    sequenceHandlers.forEach((handler) => handler(event))
  })

  sharedConnection = connection
  return connection
}

export const useRobotHub = (options: UseRobotHubOptions) => {
  const { isAuthenticated } = useAuth()
  const { setStatus } = useRealtime()
  const connectionRef = useRef<HubConnection | null>(null)
  const callbacksRef = useRef(options)

  callbacksRef.current = options

  useEffect(() => {
    if (!isAuthenticated) {
      setStatus('disconnected')
      return
    }

    const connection = getOrCreateConnection()
    connectionRef.current = connection

    consumers += 1

    const commandHandler: CommandHandler = (event) => {
      callbacksRef.current.onCommandUpdated?.(event)
    }
    const positionHandler: PositionHandler = (event) => {
      callbacksRef.current.onPositionUpdated?.(event)
    }
    const sequenceHandler: SequenceHandler = (event) => {
      callbacksRef.current.onSequenceUpdated?.(event)
    }

    commandHandlers.add(commandHandler)
    positionHandlers.add(positionHandler)
    sequenceHandlers.add(sequenceHandler)

    connection.onreconnecting(() => setStatus('reconnecting'))
    connection.onreconnected(() => setStatus('connected'))
    connection.onclose(() => setStatus('disconnected'))

    const connect = async () => {
      setStatus('connecting')
      try {
        await ensureConnected(connection)
        setStatus('connected')
      } catch {
        setStatus('disconnected')
      }
    }

    void connect()

    return () => {
      commandHandlers.delete(commandHandler)
      positionHandlers.delete(positionHandler)
      sequenceHandlers.delete(sequenceHandler)

      consumers -= 1

      if (consumers <= 0) {
        consumers = 0
        void connection.stop()
        sharedConnection = null
      }

      connectionRef.current = null
    }
  }, [isAuthenticated, setStatus])

  const ensureConnected = async (connection: HubConnection) => {
    if (connection.state === HubConnectionState.Connected) {
      return
    }

    setStatus('connecting')

    if (!startPromise) {
      startPromise = connection.start().finally(() => {
        startPromise = null
      })
    }

    await startPromise
    setStatus('connected')
  }

  const subscribeToCommand = async (commandId: number) => {
    if (!connectionRef.current) return false

    try {
      await ensureConnected(connectionRef.current)
      await connectionRef.current.invoke(robotHubMethods.subscribeToCommand, commandId)
      return true
    } catch {
      setStatus('disconnected')
      return false
    }
  }

  const unsubscribeFromCommand = async (commandId: number) => {
    if (!connectionRef.current) return
    if (connectionRef.current.state !== HubConnectionState.Connected) return
    await connectionRef.current.invoke(robotHubMethods.unsubscribeFromCommand, commandId)
  }

  const subscribeToSequence = async (sequenceId: number) => {
    if (!connectionRef.current) return false

    try {
      await ensureConnected(connectionRef.current)
      await connectionRef.current.invoke(robotHubMethods.subscribeToSequence, sequenceId)
      return true
    } catch {
      setStatus('disconnected')
      return false
    }
  }

  const unsubscribeFromSequence = async (sequenceId: number) => {
    if (!connectionRef.current) return
    if (connectionRef.current.state !== HubConnectionState.Connected) return
    await connectionRef.current.invoke(robotHubMethods.unsubscribeFromSequence, sequenceId)
  }

  return {
    subscribeToCommand,
    unsubscribeFromCommand,
    subscribeToSequence,
    unsubscribeFromSequence,
  }
}
