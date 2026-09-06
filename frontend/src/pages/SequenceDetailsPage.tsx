import { useParams } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { queryKeys } from '../api/queryKeys'
import { robotSequencesApi } from '../api/robotSequencesApi'
import { getErrorMessage } from '../utils/httpError'
import { useState } from 'react'

export const SequenceDetailsPage = () => {
  const { id } = useParams()
  const sequenceId = Number(id)
  const queryClient = useQueryClient()
  const [error, setError] = useState<string | null>(null)

  const { data: sequence } = useQuery({
    queryKey: queryKeys.robotSequences.detail(sequenceId),
    queryFn: () => robotSequencesApi.getSequenceById(sequenceId),
    enabled: Number.isFinite(sequenceId),
  })

  const cancelMutation = useMutation({
    mutationFn: () => robotSequencesApi.cancelSequence(sequenceId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.robotSequences.detail(sequenceId) })
      await queryClient.invalidateQueries({ queryKey: queryKeys.robotSequences.all })
    },
    onError: (mutationError) => setError(getErrorMessage(mutationError)),
  })

  if (!sequence) {
    return <p className="text-sm text-slate-400">Loading sequence...</p>
  }

  return (
    <section className="space-y-4">
      <h2 className="text-xl font-semibold">Sequence #{id}</h2>
      {error && <p className="rounded-md border border-rose-500/30 bg-rose-500/10 p-2 text-sm text-rose-200">{error}</p>}
      <div className="rounded-lg border border-slate-800 bg-slate-900 p-4 text-sm">
        <p>Status: {sequence.status}</p>
        <p>Step: {sequence.currentStep}/{sequence.totalSteps}</p>
        <p>Start: ({sequence.startX}, {sequence.startY})</p>
        <p>Final: ({sequence.finalX ?? '-'}, {sequence.finalY ?? '-'})</p>
        <p>Failure: {sequence.failureReason ?? '-'}</p>
        <button
          type="button"
          disabled={!['Queued', 'Executing'].includes(sequence.status) || cancelMutation.isPending}
          onClick={() => cancelMutation.mutate()}
          className="mt-3 rounded-md border border-amber-500/30 bg-amber-500/10 px-3 py-2 text-xs text-amber-200 disabled:opacity-50"
        >
          Cancel sequence
        </button>
      </div>

      <div className="rounded-lg border border-slate-800 bg-slate-900 p-4">
        <h3 className="mb-2 text-sm font-semibold">Timeline</h3>
        <ol className="space-y-2 text-sm">
          {sequence.items.map((item) => (
            <li key={item.order} className="rounded border border-slate-800 p-2">
              <span className="font-medium">Step {item.order}</span> · {item.commandName} · {item.executed ? 'Executed' : 'Pending'}
            </li>
          ))}
        </ol>
      </div>
    </section>
  )
}
