interface StatusTimelineProps {
  statuses: string[]
  currentStatus?: string
}

export const StatusTimeline = ({ statuses, currentStatus }: StatusTimelineProps) => {
  const activeIndex = statuses.findIndex((status) => status.toLowerCase() === (currentStatus ?? '').toLowerCase())

  return (
    <ol className="flex flex-wrap gap-2 text-xs">
      {statuses.map((status, index) => {
        const active = activeIndex === index
        const complete = activeIndex > index

        return (
          <li key={status} className="inline-flex items-center gap-2">
            <span
              className={`rounded border px-2 py-1 ${
                complete ? 'border-emerald-500/40 bg-emerald-500/20 text-emerald-200' : active ? 'border-cyan-500/40 bg-cyan-500/20 text-cyan-200' : 'border-slate-700 text-slate-400'
              }`}
            >
              {status}
            </span>
            {index < statuses.length - 1 && <span className="text-slate-600">→</span>}
          </li>
        )
      })}
    </ol>
  )
}
