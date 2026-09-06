import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { mapsApi } from '../api/mapsApi'
import { queryKeys } from '../api/queryKeys'
import { getErrorMessage } from '../utils/httpError'
import { useAuth } from '../hooks/useAuth'
import { isAdmin } from '../utils/roles'

export const MapsPage = () => {
  const queryClient = useQueryClient()
  const { user } = useAuth()
  const admin = isAdmin(user)
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [rows, setRows] = useState<number>(8)
  const [columns, setColumns] = useState<number>(8)
  const [error, setError] = useState<string | null>(null)

  const { data: maps = [] } = useQuery({
    queryKey: queryKeys.maps.all,
    queryFn: mapsApi.getMaps,
  })

  const createMutation = useMutation({
    mutationFn: mapsApi.createMap,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.maps.all })
      setName('')
      setDescription('')
      setRows(8)
      setColumns(8)
      setError(null)
    },
    onError: (mutationError) => setError(getErrorMessage(mutationError)),
  })

  const deleteMutation = useMutation({
    mutationFn: mapsApi.deleteMap,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.maps.all }),
    onError: (mutationError) => setError(getErrorMessage(mutationError)),
  })

  return (
    <section className="space-y-4">
      <h2 className="text-xl font-semibold">Maps</h2>

      {error && <p className="rounded-md border border-rose-500/30 bg-rose-500/10 p-2 text-sm text-rose-200">{error}</p>}

      {admin && (
        <div className="rounded-lg border border-slate-800 bg-slate-900 p-4">
          <h3 className="mb-3 text-sm font-semibold">Create map</h3>
          <div className="grid gap-2 md:grid-cols-4">
            <input value={name} onChange={(event) => setName(event.target.value)} placeholder="Name" className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm" />
            <input value={description} onChange={(event) => setDescription(event.target.value)} placeholder="Description" className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm" />
            <input value={rows} onChange={(event) => setRows(Number(event.target.value))} type="number" min={1} placeholder="Rows" className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm" />
            <input value={columns} onChange={(event) => setColumns(Number(event.target.value))} type="number" min={1} placeholder="Columns" className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm" />
          </div>
          <button
            type="button"
            disabled={createMutation.isPending || !name}
            onClick={() => createMutation.mutate({ name, description, rows, columns })}
            className="mt-3 rounded-md bg-cyan-500 px-3 py-2 text-sm font-medium text-slate-950 disabled:opacity-60"
          >
            {createMutation.isPending ? 'Creating...' : 'Create Map'}
          </button>
        </div>
      )}

      <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
        {maps.map((map) => (
          <article key={map.id} className="rounded-lg border border-slate-800 bg-slate-900 p-4">
            <h3 className="font-semibold text-slate-100">{map.name}</h3>
            <p className="mb-3 text-xs text-slate-400">{map.rows} × {map.columns}</p>
            <p className="mb-3 line-clamp-2 min-h-10 text-sm text-slate-300">{map.description || 'No description'}</p>
            <div className="flex gap-2">
              <Link to={`/app/maps/${map.id}`} className="rounded-md border border-slate-700 px-3 py-1 text-xs hover:bg-slate-800">Open</Link>
              {admin && (
                <button type="button" onClick={() => deleteMutation.mutate(map.id)} className="rounded-md border border-rose-500/30 bg-rose-500/10 px-3 py-1 text-xs text-rose-200">
                  Delete
                </button>
              )}
            </div>
          </article>
        ))}
      </div>
    </section>
  )
}
