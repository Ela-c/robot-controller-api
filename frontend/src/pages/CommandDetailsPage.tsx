import { useParams } from 'react-router-dom'
import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { queryKeys } from '../api/queryKeys'
import { robotCommandsApi } from '../api/robotCommandsApi'
import type { MovementDirection } from '../types/robotCommand'
import { getErrorMessage } from '../utils/httpError'

export const CommandDetailsPage = () => {
  const { id } = useParams()
  const commandId = Number(id)
  const queryClient = useQueryClient()

  const [name, setName] = useState<string | null>(null)
  const [description, setDescription] = useState<string | null>(null)
  const [isMoveCommand, setIsMoveCommand] = useState<boolean | null>(null)
  const [selectedMovementStep, setSelectedMovementStep] = useState<MovementDirection>('Right')
  const [movementDirections, setMovementDirections] = useState<MovementDirection[] | null>(null)
  const [error, setError] = useState<string | null>(null)

  const { data: command } = useQuery({
    queryKey: queryKeys.robotCommands.detail(commandId),
    queryFn: () => robotCommandsApi.getCommandById(commandId),
    enabled: Number.isFinite(commandId),
  })

  const effectiveMove = isMoveCommand ?? command?.isMoveCommand ?? false
  const effectiveDirections = movementDirections ?? command?.movementDirections ?? (command?.movementDirection ? [command.movementDirection] : ['Right'])

  const updateMutation = useMutation({
    mutationFn: () =>
      robotCommandsApi.updateCommand(commandId, {
        name: name ?? command?.name ?? '',
        description: description ?? command?.description ?? '',
        isMoveCommand: effectiveMove,
        movementDirection: effectiveMove ? effectiveDirections[0] : null,
        movementDirections: effectiveMove ? effectiveDirections : undefined,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.robotCommands.detail(commandId) })
      await queryClient.invalidateQueries({ queryKey: queryKeys.robotCommands.all })
      await queryClient.invalidateQueries({ queryKey: queryKeys.robotCommands.moveOnly })
      setError(null)
    },
    onError: (mutationError) => setError(getErrorMessage(mutationError)),
  })

  if (!command) {
    return <p className="text-sm text-slate-400">Loading command...</p>
  }

  return (
    <section className="space-y-4">
      <h2 className="text-xl font-semibold">Command #{id}</h2>
      {error && <p className="rounded-md border border-rose-500/30 bg-rose-500/10 p-2 text-sm text-rose-200">{error}</p>}

      <div className="rounded-lg border border-slate-800 bg-slate-900 p-4">
        <p className="mb-1 text-sm text-slate-300">{command.name}</p>
        <p className="text-xs text-slate-400">Type: {command.isMoveCommand ? 'Move command' : 'Action command'}</p>
        <p className="text-xs text-slate-400">Created: {new Date(command.createdDate).toLocaleString()}</p>
      </div>

      <div className="rounded-lg border border-slate-800 bg-slate-900 p-4">
        <h3 className="mb-2 text-sm font-semibold">Edit command definition</h3>
        <div className="grid gap-2 md:grid-cols-2">
          <input value={name ?? command.name} onChange={(event) => setName(event.target.value)} className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm" />
          <input value={description ?? command.description ?? ''} onChange={(event) => setDescription(event.target.value)} className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm" />
          <label className="inline-flex items-center gap-2 rounded-md border border-slate-700 px-3 py-2 text-sm">
            <input
              type="checkbox"
              checked={effectiveMove}
              onChange={(event) => {
                const checked = event.target.checked
                setIsMoveCommand(checked)
                if (checked && (!movementDirections || movementDirections.length === 0)) {
                  setMovementDirections(['Right'])
                }
              }}
            />
            Move command
          </label>
          <div className="flex items-center gap-2">
            <select
              value={selectedMovementStep}
              disabled={!effectiveMove}
              onChange={(event) => setSelectedMovementStep(event.target.value as MovementDirection)}
              className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
            >
              <option value="Up">Up</option>
              <option value="Down">Down</option>
              <option value="Left">Left</option>
              <option value="Right">Right</option>
            </select>
            <button
              type="button"
              disabled={!effectiveMove}
              onClick={() => setMovementDirections((existing) => [...(existing ?? effectiveDirections), selectedMovementStep])}
              className="rounded-md border border-slate-700 px-3 py-2 text-xs text-slate-200 disabled:opacity-60"
            >
              Add step
            </button>
          </div>
        </div>

        {effectiveMove && (
          <div className="mt-3 space-y-1">
            <p className="text-xs text-slate-400">Movement sequence</p>
            {effectiveDirections.map((direction, index) => (
              <div key={`${direction}-${index}`} className="flex items-center justify-between rounded border border-slate-800 px-2 py-1 text-xs text-slate-300">
                <span>Step {index + 1}: {direction}</span>
                <button
                  type="button"
                  disabled={effectiveDirections.length <= 1}
                  onClick={() => setMovementDirections(effectiveDirections.filter((_, i) => i !== index))}
                  className="rounded border border-rose-500/30 bg-rose-500/10 px-2 py-0.5 text-rose-200 disabled:opacity-60"
                >
                  Remove
                </button>
              </div>
            ))}
          </div>
        )}

        <div className="mt-3 flex gap-2">
          <button
            type="button"
            disabled={updateMutation.isPending || (effectiveMove && effectiveDirections.length === 0)}
            onClick={() => updateMutation.mutate()}
            className="rounded-md bg-cyan-500 px-3 py-2 text-sm font-medium text-slate-950 disabled:opacity-60"
          >
            Save
          </button>
        </div>
      </div>
    </section>
  )
}
