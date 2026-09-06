import { useEffect, useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { mapsApi } from '../api/mapsApi'
import { queryKeys } from '../api/queryKeys'
import { robotCommandsApi } from '../api/robotCommandsApi'
import { robotSequencesApi } from '../api/robotSequencesApi'
import { robotStateApi } from '../api/robotStateApi'
import { useRobotHub } from '../hooks/useRobotHub'
import type {
  MovementDirection,
  RobotCommand,
} from '../types/robotCommand'
import type { RobotSequenceStatus } from '../types/robot'
import type { RobotPositionUpdatedEvent, RobotSequencePositionUpdatedEvent } from '../types/signalr'
import { getErrorMessage } from '../utils/httpError'
import { RobotGrid } from '../components/robot/RobotGrid'

const directionOrder: MovementDirection[] = ['Right', 'Left', 'Down', 'Up']

const isSequencePosition = (
  event: RobotPositionUpdatedEvent | RobotSequencePositionUpdatedEvent,
): event is RobotSequencePositionUpdatedEvent => 'sequenceId' in event

const terminalStatuses = new Set(['Completed', 'Failed', 'Cancelled'])

const sequenceStatusClass: Record<RobotSequenceStatus, string> = {
  Pending: 'border-violet-500/40 bg-violet-500/15 text-violet-200',
  Queued: 'border-blue-500/40 bg-blue-500/15 text-blue-200',
  Executing: 'border-cyan-500/40 bg-cyan-500/15 text-cyan-200',
  Completed: 'border-emerald-500/40 bg-emerald-500/15 text-emerald-200',
  Failed: 'border-rose-500/40 bg-rose-500/15 text-rose-200',
  Cancelled: 'border-amber-500/40 bg-amber-500/15 text-amber-200',
}

export const DashboardPage = () => {
  const queryClient = useQueryClient()
  const [targetX, setTargetX] = useState<number | ''>('')
  const [targetY, setTargetY] = useState<number | ''>('')
  const [placeX, setPlaceX] = useState<number | ''>('')
  const [placeY, setPlaceY] = useState<number | ''>('')
  const [selectedCommandId, setSelectedCommandId] = useState<number | ''>('')
  const [sequenceCommandIds, setSequenceCommandIds] = useState<number[]>([])
  const [activeSequenceId, setActiveSequenceId] = useState<number | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [latestEvents, setLatestEvents] = useState<string[]>([])
  const [livePosition, setLivePosition] = useState<{ x: number | null; y: number | null } | null>(null)
  const [liveSequenceSnapshot, setLiveSequenceSnapshot] = useState<{
    sequenceId: number
    status: RobotSequenceStatus
    currentStep: number
    totalSteps: number
    failureReason?: string | null
  } | null>(null)

  const { data: maps = [] } = useQuery({
    queryKey: queryKeys.maps.all,
    queryFn: mapsApi.getMaps,
  })

  const { data: moveCommands = [] } = useQuery({
    queryKey: queryKeys.robotCommands.moveOnly,
    queryFn: robotCommandsApi.getMoveCommands,
  })

  const { data: robotState } = useQuery({
    queryKey: queryKeys.robotState.current,
    queryFn: robotStateApi.getState,
  })

  const { data: activeSequence } = useQuery({
    queryKey: activeSequenceId ? queryKeys.robotSequences.detail(activeSequenceId) : ['robotSequences', 'detail', 'none'],
    queryFn: () => robotSequencesApi.getSequenceById(activeSequenceId as number),
    enabled: !!activeSequenceId,
    refetchInterval: (query) => {
      const status = query.state.data?.status
      return status === 'Queued' || status === 'Executing' ? 1000 : false
    },
  })

  const selectedMap = useMemo(
    () => maps.find((map) => map.id === robotState?.mapId),
    [maps, robotState?.mapId],
  )

  const moveCommandsByDirection = useMemo(() => {
    const mapping = new Map<MovementDirection, RobotCommand>()
    for (const command of moveCommands) {
      if (command.movementDirection && !mapping.has(command.movementDirection)) {
        mapping.set(command.movementDirection, command)
      }
    }
    return mapping
  }, [moveCommands])

  const selectedSequenceCommands = useMemo(
    () => sequenceCommandIds
      .map((commandId) => moveCommands.find((command) => command.id === commandId))
      .filter((command): command is RobotCommand => !!command),
    [moveCommands, sequenceCommandIds],
  )

  const displayPosition = livePosition
    ?? (robotState?.hasPosition
      ? { x: robotState.x ?? null, y: robotState.y ?? null }
      : { x: null, y: null })

  const currentSequenceStatus = activeSequence && liveSequenceSnapshot?.sequenceId === activeSequence.id
    ? liveSequenceSnapshot.status
    : activeSequence?.status

  const currentSequenceStep = activeSequence && liveSequenceSnapshot?.sequenceId === activeSequence.id
    ? liveSequenceSnapshot.currentStep
    : activeSequence?.currentStep

  const currentSequenceTotalSteps = activeSequence && liveSequenceSnapshot?.sequenceId === activeSequence.id
    ? liveSequenceSnapshot.totalSteps
    : activeSequence?.totalSteps

  const currentSequenceFailureReason = activeSequence && liveSequenceSnapshot?.sequenceId === activeSequence.id
    ? (liveSequenceSnapshot.failureReason ?? activeSequence.failureReason)
    : activeSequence?.failureReason

  const appendEvent = (value: string) => {
    setLatestEvents((existing) => [value, ...existing].slice(0, 8))
  }

  const hub = useRobotHub({
    onCommandUpdated: (event) => {
      appendEvent(`Command #${event.commandId}: ${event.status}`)
    },
    onPositionUpdated: (event) => {
      if (isSequencePosition(event)) {
        setLivePosition({ x: event.x, y: event.y })
        setLiveSequenceSnapshot((existing) => {
          if (existing && existing.sequenceId === event.sequenceId) {
            return {
              ...existing,
              currentStep: event.step,
              totalSteps: event.totalSteps,
            }
          }

          return {
            sequenceId: event.sequenceId,
            status: 'Executing',
            currentStep: event.step,
            totalSteps: event.totalSteps,
            failureReason: null,
          }
        })
        appendEvent(`Sequence #${event.sequenceId}: position (${event.x}, ${event.y})`)
        return
      }

      setLivePosition({ x: event.x, y: event.y })
      appendEvent(`Command #${event.commandId}: position (${event.x}, ${event.y})`)
    },
    onSequenceUpdated: (event) => {
      setLiveSequenceSnapshot({
        sequenceId: event.sequenceId,
        status: event.status,
        currentStep: event.currentStep,
        totalSteps: event.totalSteps,
        failureReason: event.failureReason ?? null,
      })
      appendEvent(`Sequence #${event.sequenceId}: ${event.status} (${event.currentStep}/${event.totalSteps})`)
      queryClient.invalidateQueries({ queryKey: queryKeys.robotSequences.detail(event.sequenceId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.robotSequences.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.robotState.current })
    },
  })

  const selectMapMutation = useMutation({
    mutationFn: robotStateApi.selectMap,
    onSuccess: (state) => {
      queryClient.setQueryData(queryKeys.robotState.current, state)
      setLivePosition(null)
      setError(null)
    },
    onError: (mutationError) => setError(getErrorMessage(mutationError)),
  })

  useEffect(() => {
    const status = activeSequence?.status
    if (!activeSequenceId || !status || !terminalStatuses.has(status)) {
      return
    }

    void queryClient.invalidateQueries({ queryKey: queryKeys.robotState.current })
    void queryClient.invalidateQueries({ queryKey: queryKeys.robotSequences.all })
    void queryClient.invalidateQueries({ queryKey: queryKeys.robotSequences.detail(activeSequenceId) })
  }, [activeSequence?.status, activeSequenceId, queryClient])

  const placeMutation = useMutation({
    mutationFn: robotStateApi.placeRobot,
    onSuccess: (state) => {
      queryClient.setQueryData(queryKeys.robotState.current, state)
      setLivePosition({ x: state.x ?? null, y: state.y ?? null })
      setError(null)
    },
    onError: (mutationError) => setError(getErrorMessage(mutationError)),
  })

  const submitSequenceMutation = useMutation({
    mutationFn: robotSequencesApi.submitSequence,
    onSuccess: async (result) => {
      setActiveSequenceId(result.sequenceId)
      setLiveSequenceSnapshot({
        sequenceId: result.sequenceId,
        status: result.status,
        currentStep: 0,
        totalSteps: result.totalSteps,
        failureReason: null,
      })

      const subscribed = await hub.subscribeToSequence(result.sequenceId)
      if (!subscribed) {
        appendEvent(`Sequence #${result.sequenceId}: realtime subscription pending (will continue polling)`)
      }
      queryClient.invalidateQueries({ queryKey: queryKeys.robotSequences.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.robotSequences.detail(result.sequenceId) })
      setError(null)
    },
    onError: (mutationError) => setError(getErrorMessage(mutationError)),
  })

  const cancelSequenceMutation = useMutation({
    mutationFn: (sequenceId: number) => robotSequencesApi.cancelSequence(sequenceId),
    onSuccess: async () => {
      if (activeSequenceId) {
        await queryClient.invalidateQueries({ queryKey: queryKeys.robotSequences.detail(activeSequenceId) })
      }
    },
    onError: (mutationError) => setError(getErrorMessage(mutationError)),
  })

  const submitSequence = async () => {
    const currentX = displayPosition.x
    const currentY = displayPosition.y

    if (currentX == null || currentY == null) {
      setError('Place the robot before submitting movement.')
      return
    }

    if (sequenceCommandIds.length === 0) {
      setError('Add at least one move command to the sequence.')
      return
    }

    if ((targetX === '' && targetY !== '') || (targetX !== '' && targetY === '')) {
      setError('Set both target coordinates or leave both empty.')
      return
    }

    await submitSequenceMutation.mutateAsync({
      commandIds: sequenceCommandIds,
      targetX: targetX === '' ? undefined : targetX,
      targetY: targetY === '' ? undefined : targetY,
    })
  }

  return (
    <section className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold">Robot Operations Dashboard</h2>
      </div>

      {error && <p className="rounded-md border border-rose-500/30 bg-rose-500/10 p-2 text-sm text-rose-200">{error}</p>}

      <div className="grid gap-4 xl:grid-cols-[2fr_1fr]">
        <div className="space-y-4 rounded-lg border border-slate-800 bg-slate-900 p-4">
          <div className="flex flex-wrap items-end gap-3">
            <div>
              <label className="mb-1 block text-xs text-slate-400">Selected map</label>
              <select
                value={robotState?.mapId ?? ''}
                onChange={(event) => {
                  if (!event.target.value) return
                  selectMapMutation.mutate({ mapId: Number(event.target.value) })
                }}
                className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              >
                <option value="">Select map...</option>
                {maps.map((map) => (
                  <option key={map.id} value={map.id}>
                    {map.name} ({map.rows}×{map.columns})
                  </option>
                ))}
              </select>
            </div>
            <div className="text-xs text-slate-400">Current position: ({displayPosition.x ?? '-'}, {displayPosition.y ?? '-'})</div>
          </div>

          {selectedMap ? (
            <RobotGrid
              rows={selectedMap.rows}
              columns={selectedMap.columns}
              robotX={displayPosition.x}
              robotY={displayPosition.y}
              targetX={targetX === '' ? null : targetX}
              targetY={targetY === '' ? null : targetY}
              onCellClick={(x, y) => {
                setTargetX(x)
                setTargetY(y)
              }}
            />
          ) : (
            <p className="text-sm text-slate-400">Select a map to display the robot grid.</p>
          )}

          <div className="grid gap-3 md:grid-cols-2">
            <div className="rounded-md border border-slate-800 p-3">
              <h3 className="mb-2 text-sm font-semibold">Place robot</h3>
              <div className="flex items-end gap-2">
                <input type="number" value={placeX} onChange={(event) => setPlaceX(event.target.value === '' ? '' : Number(event.target.value))} placeholder="X" className="w-20 rounded-md border border-slate-700 bg-slate-950 px-2 py-1 text-sm" />
                <input type="number" value={placeY} onChange={(event) => setPlaceY(event.target.value === '' ? '' : Number(event.target.value))} placeholder="Y" className="w-20 rounded-md border border-slate-700 bg-slate-950 px-2 py-1 text-sm" />
                <button
                  type="button"
                  disabled={placeMutation.isPending || placeX === '' || placeY === ''}
                  onClick={() => placeMutation.mutate({ x: Number(placeX), y: Number(placeY) })}
                  className="rounded-md bg-cyan-500 px-3 py-1 text-sm font-medium text-slate-950 disabled:opacity-60"
                >
                  {placeMutation.isPending ? 'Placing...' : 'Place'}
                </button>
              </div>
            </div>

            <div className="rounded-md border border-slate-800 p-3">
              <h3 className="mb-2 text-sm font-semibold">Target marker (optional)</h3>
              <div className="flex items-end gap-2">
                <input type="number" value={targetX} onChange={(event) => setTargetX(event.target.value === '' ? '' : Number(event.target.value))} placeholder="Target X" className="w-24 rounded-md border border-slate-700 bg-slate-950 px-2 py-1 text-sm" />
                <input type="number" value={targetY} onChange={(event) => setTargetY(event.target.value === '' ? '' : Number(event.target.value))} placeholder="Target Y" className="w-24 rounded-md border border-slate-700 bg-slate-950 px-2 py-1 text-sm" />
              </div>
              <p className="mt-2 text-xs text-slate-400">This does not auto-generate movements. It only validates where your manual sequence should end.</p>
            </div>

            <div className="rounded-md border border-slate-800 p-3 md:col-span-2">
              <h3 className="mb-2 text-sm font-semibold">Build movement sequence</h3>
              <div className="flex flex-wrap items-end gap-2">
                <select
                  value={selectedCommandId}
                  onChange={(event) => setSelectedCommandId(event.target.value ? Number(event.target.value) : '')}
                  className="min-w-64 rounded-md border border-slate-700 bg-slate-950 px-2 py-1 text-sm"
                >
                  <option value="">Select move command...</option>
                  {moveCommands.map((command) => (
                    <option key={command.id} value={command.id}>
                      #{command.id} {command.name} {command.movementDirection ? `(${command.movementDirection})` : ''}
                    </option>
                  ))}
                </select>
                <button
                  type="button"
                  disabled={selectedCommandId === ''}
                  onClick={() => {
                    if (selectedCommandId === '') {
                      return
                    }
                    setSequenceCommandIds((existing) => [...existing, selectedCommandId])
                    setError(null)
                  }}
                  className="rounded-md bg-cyan-500 px-3 py-1 text-sm font-medium text-slate-950 disabled:opacity-60"
                >
                  Add step
                </button>
                <button
                  type="button"
                  disabled={sequenceCommandIds.length === 0}
                  onClick={() => setSequenceCommandIds([])}
                  className="rounded-md border border-slate-700 px-3 py-1 text-sm text-slate-200 disabled:opacity-60"
                >
                  Clear
                </button>
                <button
                  type="button"
                  disabled={submitSequenceMutation.isPending || sequenceCommandIds.length === 0}
                  onClick={() => void submitSequence()}
                  className="rounded-md bg-emerald-500 px-3 py-1 text-sm font-medium text-slate-950 disabled:opacity-60"
                >
                  {submitSequenceMutation.isPending ? 'Submitting...' : 'Submit Sequence'}
                </button>
              </div>

              <div className="mt-3 space-y-1 text-xs text-slate-300">
                {selectedSequenceCommands.length === 0 ? (
                  <p className="text-slate-400">No steps added yet.</p>
                ) : (
                  selectedSequenceCommands.map((command, index) => (
                    <div key={`${index}-${command.id}`} className="flex items-center justify-between rounded border border-slate-800 px-2 py-1">
                      <span>
                        Step {index + 1}: #{command.id} {command.name} {command.movementDirection ? `(${command.movementDirection})` : ''}
                      </span>
                      <button
                        type="button"
                        onClick={() => setSequenceCommandIds((existing) => existing.filter((_, idx) => idx !== index))}
                        className="rounded border border-rose-500/30 bg-rose-500/10 px-2 py-0.5 text-rose-200"
                      >
                        Remove
                      </button>
                    </div>
                  ))
                )}
              </div>
            </div>
          </div>
        </div>

        <div className="space-y-4">
          <div className="rounded-lg border border-slate-800 bg-slate-900 p-4">
            <h3 className="mb-2 text-sm font-semibold">Available move commands</h3>
            <div className="space-y-2 text-sm">
              {directionOrder.map((direction) => {
                const command = moveCommandsByDirection.get(direction)
                return (
                  <p key={direction} className="text-slate-300">
                    <span className="font-medium text-slate-200">{direction}:</span>{' '}
                    {command ? `${command.name} (#${command.id})` : 'Not configured'}
                  </p>
                )
              })}
              {moveCommands.length === 0 && (
                <p className="text-xs text-amber-300">No move commands are available. Ask an admin to create them.</p>
              )}
            </div>
          </div>

          <div className="rounded-lg border border-slate-800 bg-slate-900 p-4">
            <h3 className="mb-2 text-sm font-semibold">Current Sequence</h3>
            {activeSequence ? (
              <>
                <div className="flex items-center justify-between gap-3">
                  <p className="text-sm text-slate-300">Sequence #{activeSequence.id}</p>
                  <span className={`rounded-md border px-3 py-1 text-sm font-semibold ${sequenceStatusClass[currentSequenceStatus ?? 'Pending']}`}>
                    {currentSequenceStatus}
                  </span>
                </div>
                <p className="mt-2 text-sm text-slate-200">Step {currentSequenceStep ?? 0} / {currentSequenceTotalSteps ?? activeSequence.totalSteps}</p>
                {currentSequenceFailureReason && (
                  <p className="mt-2 rounded border border-rose-500/30 bg-rose-500/10 px-2 py-1 text-sm text-rose-200">
                    Failure reason: {currentSequenceFailureReason}
                  </p>
                )}
                <button
                  type="button"
                  disabled={!['Queued', 'Executing', 'Pending'].includes(currentSequenceStatus ?? activeSequence.status) || cancelSequenceMutation.isPending}
                  onClick={() => activeSequenceId && cancelSequenceMutation.mutate(activeSequenceId)}
                  className="mt-3 w-full rounded-md border border-amber-500/40 bg-amber-500/10 px-3 py-2 text-xs text-amber-200 disabled:opacity-50"
                >
                  Cancel Sequence
                </button>
              </>
            ) : (
              <p className="text-sm text-slate-400">No active sequence selected.</p>
            )}
          </div>

          <div className="rounded-lg border border-slate-800 bg-slate-900 p-4">
            <h3 className="mb-2 text-sm font-semibold">Latest Events</h3>
            {latestEvents.length === 0 ? (
              <p className="text-sm text-slate-400">No events yet.</p>
            ) : (
              <ul className="space-y-1 text-xs text-slate-300">
                {latestEvents.map((event, index) => <li key={`${event}-${index}`}>{event}</li>)}
              </ul>
            )}
          </div>
        </div>
      </div>
    </section>
  )
}
