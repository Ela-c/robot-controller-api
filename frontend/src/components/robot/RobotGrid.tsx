import clsx from 'clsx'

interface RobotGridProps {
  rows: number
  columns: number
  robotX?: number | null
  robotY?: number | null
  targetX?: number | null
  targetY?: number | null
  onCellClick?: (x: number, y: number) => void
}

export const RobotGrid = ({
  rows,
  columns,
  robotX,
  robotY,
  targetX,
  targetY,
  onCellClick,
}: RobotGridProps) => {
  return (
    <div className="overflow-auto rounded-lg border border-slate-800 bg-slate-950 p-3">
      <div className="mb-2 grid" style={{ gridTemplateColumns: `24px repeat(${columns}, 2rem)` }}>
        <div />
        {Array.from({ length: columns }, (_, x) => (
          <div key={`header-x-${x}`} className="text-center text-xs text-slate-400">{x}</div>
        ))}
      </div>

      {Array.from({ length: rows }, (_, y) => (
        <div key={`row-${y}`} className="grid" style={{ gridTemplateColumns: `24px repeat(${columns}, 2rem)` }}>
          <div className="my-auto text-center text-xs text-slate-400">{y}</div>
          {Array.from({ length: columns }, (_, x) => {
            const isRobot = robotX === x && robotY === y
            const isTarget = targetX === x && targetY === y

            return (
              <button
                key={`cell-${x}-${y}`}
                type="button"
                onClick={() => onCellClick?.(x, y)}
                className={clsx(
                  'm-px flex h-8 w-8 items-center justify-center border text-xs transition',
                  isRobot ? 'border-cyan-300 bg-cyan-400/20 text-cyan-100' : 'border-slate-800 bg-slate-900 text-slate-500 hover:bg-slate-800',
                  isTarget && !isRobot && 'border-amber-400 bg-amber-500/20 text-amber-100',
                )}
                aria-label={`Cell (${x}, ${y})`}
              >
                {isRobot ? '🤖' : isTarget ? '◎' : ''}
              </button>
            )
          })}
        </div>
      ))}
    </div>
  )
}
