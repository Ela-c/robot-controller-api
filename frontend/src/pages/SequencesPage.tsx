import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { queryKeys } from '../api/queryKeys'
import { robotSequencesApi } from '../api/robotSequencesApi'
import type { RobotSequenceStatus } from '../types/robot'
import { getErrorMessage } from '../utils/httpError'

export const SequencesPage = () => {
  const queryClient = useQueryClient()
  const [statusFilter, setStatusFilter] = useState<string>('')
  const [error, setError] = useState<string | null>(null)

  const { data: sequences = [] } = useQuery({
    queryKey: [...queryKeys.robotSequences.all, statusFilter],
    queryFn: () =>
      robotSequencesApi.getSequences(statusFilter ? (statusFilter as RobotSequenceStatus) : undefined),
  })

  const cancelMutation = useMutation({
    mutationFn: robotSequencesApi.cancelSequence,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.robotSequences.all })
    },
    onError: (mutationError) => setError(getErrorMessage(mutationError)),
  })

  return (
    <section className="space-y-4">
      <h2 className="text-xl font-semibold">Sequence History</h2>
      {error && <p className="rounded-md border border-rose-500/30 bg-rose-500/10 p-2 text-sm text-rose-200">{error}</p>}

      <div>
        <select value={statusFilter} onChange={(event) => setStatusFilter(event.target.value)} className="rounded-md border border-slate-700 bg-slate-900 px-3 py-2 text-sm">
          <option value="">All statuses</option>
          <option value="Queued">Queued</option>
          <option value="Executing">Executing</option>
          <option value="Completed">Completed</option>
          <option value="Failed">Failed</option>
          <option value="Cancelled">Cancelled</option>
        </select>
      </div>

      <div className="overflow-auto rounded-lg border border-slate-800 bg-slate-900">
        <table className="min-w-full text-sm">
          <thead className="bg-slate-950 text-slate-400">
            <tr>
              <th className="px-3 py-2 text-left">ID</th>
              <th className="px-3 py-2 text-left">Status</th>
              <th className="px-3 py-2 text-left">Progress</th>
              <th className="px-3 py-2 text-left">Created</th>
              <th className="px-3 py-2 text-left">Actions</th>
            </tr>
          </thead>
          <tbody>
            {sequences.map((sequence) => (
              <tr key={sequence.id} className="border-t border-slate-800">
                <td className="px-3 py-2">{sequence.id}</td>
                <td className="px-3 py-2">{sequence.status}</td>
                <td className="px-3 py-2">{sequence.currentStep}/{sequence.totalSteps}</td>
                <td className="px-3 py-2">{new Date(sequence.createdDate).toLocaleString()}</td>
                <td className="px-3 py-2">
                  <div className="flex gap-2">
                    <Link to={`/app/sequences/${sequence.id}`} className="rounded border border-slate-700 px-2 py-1 text-xs hover:bg-slate-800">Open</Link>
                    <button
                      type="button"
                      disabled={!['Queued', 'Executing'].includes(sequence.status)}
                      onClick={() => cancelMutation.mutate(sequence.id)}
                      className="rounded border border-amber-500/30 bg-amber-500/10 px-2 py-1 text-xs text-amber-200 disabled:opacity-40"
                    >
                      Cancel
                    </button>
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
