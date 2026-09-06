import { useMemo, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { queryKeys } from '../api/queryKeys'
import { robotCommandsApi } from '../api/robotCommandsApi'
import type { MovementDirection } from '../types/robotCommand'
import { getErrorMessage } from '../utils/httpError'
import { useAuth } from '../hooks/useAuth'
import { isAdmin } from '../utils/roles'

export const CommandsPage = () => {
  const queryClient = useQueryClient()
  const { user } = useAuth()
  const admin = isAdmin(user)
  const [filter, setFilter] = useState<'all' | 'move'>('all')
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [isMoveCommand, setIsMoveCommand] = useState(true)
  const [selectedMovementStep, setSelectedMovementStep] = useState<MovementDirection>('Right')
  const [movementDirections, setMovementDirections] = useState<MovementDirection[]>(['Right'])
  const [error, setError] = useState<string | null>(null)

  const allCommandsQuery = useQuery({
    queryKey: queryKeys.robotCommands.all,
    queryFn: robotCommandsApi.getCommands,
  })

  const moveCommandsQuery = useQuery({
    queryKey: queryKeys.robotCommands.moveOnly,
    queryFn: robotCommandsApi.getMoveCommands,
  })

  const commands = useMemo(
    () => (filter === 'all' ? (allCommandsQuery.data ?? []) : (moveCommandsQuery.data ?? [])),
    [allCommandsQuery.data, filter, moveCommandsQuery.data],
  )

  const createMutation = useMutation({
    mutationFn: robotCommandsApi.submitCommand,
    onSuccess: async (result) => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.robotCommands.all })
      await queryClient.invalidateQueries({ queryKey: queryKeys.robotCommands.moveOnly })
      setName('')
      setDescription('')
      setSelectedMovementStep('Right')
      setMovementDirections(['Right'])
      setError(null)
      queryClient.invalidateQueries({ queryKey: queryKeys.robotCommands.detail(result.id) })
    },
    onError: (mutationError) => setError(getErrorMessage(mutationError)),
  })

  const deleteMutation = useMutation({
    mutationFn: robotCommandsApi.deleteCommand,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.robotCommands.all })
      await queryClient.invalidateQueries({ queryKey: queryKeys.robotCommands.moveOnly })
    },
    onError: (mutationError) => setError(getErrorMessage(mutationError)),
  })

  return (
    <section className="space-y-4">
      <h2 className="text-xl font-semibold">Command History</h2>
      {error && <p className="rounded-md border border-rose-500/30 bg-rose-500/10 p-2 text-sm text-rose-200">{error}</p>}

      {admin && (
        <div className="rounded-lg border border-slate-800 bg-slate-900 p-4">
          <h3 className="mb-3 text-sm font-semibold">Create command</h3>
          <div className="grid gap-2 md:grid-cols-4">
            <input value={name} onChange={(event) => setName(event.target.value)} placeholder="Name" className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm" />
            <input value={description} onChange={(event) => setDescription(event.target.value)} placeholder="Description" className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm" />
            <label className="inline-flex items-center gap-2 rounded-md border border-slate-700 px-3 py-2 text-sm">
              <input
                type="checkbox"
                checked={isMoveCommand}
                onChange={(event) => {
                  const checked = event.target.checked
                  setIsMoveCommand(checked)
                  if (checked && movementDirections.length === 0) {
                    setMovementDirections(['Right'])
                  }
                }}
              />
              Move command
            </label>
            <div className="flex items-center gap-2">
              <select
                disabled={!isMoveCommand}
                value={selectedMovementStep}
                onChange={(event) => setSelectedMovementStep(event.target.value as MovementDirection)}
                className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm disabled:opacity-60"
              >
                <option value="Up">Up</option>
                <option value="Down">Down</option>
                <option value="Left">Left</option>
                <option value="Right">Right</option>
              </select>
              <button
                type="button"
                disabled={!isMoveCommand}
                onClick={() => setMovementDirections((existing) => [...existing, selectedMovementStep])}
                className="rounded-md border border-slate-700 px-3 py-2 text-xs text-slate-200 disabled:opacity-60"
              >
                Add step
              </button>
            </div>
          </div>
          {isMoveCommand && (
            <div className="mt-2 space-y-1">
              <p className="text-xs text-slate-400">Command movement sequence</p>
              {movementDirections.map((direction, index) => (
                <div key={`${direction}-${index}`} className="flex items-center justify-between rounded border border-slate-800 px-2 py-1 text-xs text-slate-300">
                  <span>Step {index + 1}: {direction}</span>
                  <button
                    type="button"
                    disabled={movementDirections.length <= 1}
                    onClick={() => setMovementDirections((existing) => existing.filter((_, i) => i !== index))}
                    className="rounded border border-rose-500/30 bg-rose-500/10 px-2 py-0.5 text-rose-200 disabled:opacity-60"
                  >
                    Remove
                  </button>
                </div>
              ))}
            </div>
          )}
          <button
            type="button"
            disabled={!name || createMutation.isPending || (isMoveCommand && movementDirections.length === 0)}
            onClick={() =>
              createMutation.mutate({
                name,
                description,
                isMoveCommand,
                movementDirection: isMoveCommand ? movementDirections[0] : null,
                movementDirections: isMoveCommand ? movementDirections : undefined,
              })
            }
            className="mt-3 rounded-md bg-cyan-500 px-3 py-2 text-sm font-medium text-slate-950 disabled:opacity-60"
          >
            {createMutation.isPending ? 'Submitting...' : 'Submit Command'}
          </button>
        </div>
      )}

      <div className="flex items-center gap-2 text-sm">
        <button type="button" onClick={() => setFilter('all')} className={`rounded-md border px-3 py-1 ${filter === 'all' ? 'border-cyan-500 text-cyan-300' : 'border-slate-700 text-slate-300'}`}>
          All
        </button>
        <button type="button" onClick={() => setFilter('move')} className={`rounded-md border px-3 py-1 ${filter === 'move' ? 'border-cyan-500 text-cyan-300' : 'border-slate-700 text-slate-300'}`}>
          Move only
        </button>
      </div>

      <div className="overflow-auto rounded-lg border border-slate-800 bg-slate-900">
        <table className="min-w-full text-sm">
          <thead className="bg-slate-950 text-slate-400">
            <tr>
              <th className="px-3 py-2 text-left">ID</th>
              <th className="px-3 py-2 text-left">Command</th>
              <th className="px-3 py-2 text-left">Type</th>
              <th className="px-3 py-2 text-left">Created</th>
              <th className="px-3 py-2 text-left">Actions</th>
            </tr>
          </thead>
          <tbody>
            {commands.map((command) => (
              <tr key={command.id} className="border-t border-slate-800">
                <td className="px-3 py-2">{command.id}</td>
                <td className="px-3 py-2">{command.name}</td>
                <td className="px-3 py-2">{command.isMoveCommand ? 'Move' : 'Action'}</td>
                <td className="px-3 py-2">{new Date(command.createdDate).toLocaleString()}</td>
                <td className="px-3 py-2">
                  <div className="flex gap-2">
                    <Link className="rounded border border-slate-700 px-2 py-1 text-xs hover:bg-slate-800" to={`/app/commands/${command.id}`}>
                      Open
                    </Link>
                    {admin && (
                      <button
                        type="button"
                        onClick={() => deleteMutation.mutate(command.id)}
                        className="rounded border border-rose-500/30 bg-rose-500/10 px-2 py-1 text-xs text-rose-200"
                      >
                        Delete
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  )
}
