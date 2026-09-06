import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useParams } from 'react-router-dom'
import { mapsApi } from '../api/mapsApi'
import { queryKeys } from '../api/queryKeys'
import { getErrorMessage } from '../utils/httpError'

export const MapDetailsPage = () => {
  const params = useParams()
  const mapId = Number(params.id)
  const queryClient = useQueryClient()

  const [name, setName] = useState<string | null>(null)
  const [description, setDescription] = useState<string | null>(null)
  const [rows, setRows] = useState<number | null>(null)
  const [columns, setColumns] = useState<number | null>(null)
  const [coordX, setCoordX] = useState<number | ''>('')
  const [coordY, setCoordY] = useState<number | ''>('')
  const [coordResult, setCoordResult] = useState<boolean | null>(null)
  const [error, setError] = useState<string | null>(null)

  const { data: map, isLoading } = useQuery({
    queryKey: queryKeys.maps.detail(mapId),
    queryFn: () => mapsApi.getMapById(mapId),
    enabled: Number.isFinite(mapId),
  })

  const updateMutation = useMutation({
    mutationFn: () => {
      if (!map) {
        throw new Error('Map not loaded')
      }

      return mapsApi.updateMap(mapId, {
        name: name ?? map.name,
        description: description ?? map.description ?? '',
        rows: rows ?? map.rows,
        columns: columns ?? map.columns,
      })
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.maps.all })
      await queryClient.invalidateQueries({ queryKey: queryKeys.maps.detail(mapId) })
      setError(null)
    },
    onError: (mutationError) => setError(getErrorMessage(mutationError)),
  })

  const checkCoordinateMutation = useMutation({
    mutationFn: () => mapsApi.checkCoordinate(mapId, Number(coordX), Number(coordY)),
    onSuccess: (valid) => {
      setCoordResult(valid)
      setError(null)
    },
    onError: (mutationError) => setError(getErrorMessage(mutationError)),
  })

  if (isLoading) {
    return <p className="text-sm text-slate-400">Loading map...</p>
  }

  if (!map) {
    return (
      <section className="space-y-3">
        <p className="text-sm text-rose-300">Map not found.</p>
        <Link to="/app/maps" className="text-sm text-cyan-300">Back to maps</Link>
      </section>
    )
  }

  return (
    <section className="space-y-4">
      <h2 className="text-xl font-semibold">Map: {map.name}</h2>
      {error && <p className="rounded-md border border-rose-500/30 bg-rose-500/10 p-2 text-sm text-rose-200">{error}</p>}

      <div className="rounded-lg border border-slate-800 bg-slate-900 p-4">
        <h3 className="mb-2 text-sm font-semibold">Edit map</h3>
        <div className="grid gap-2 md:grid-cols-2">
          <input value={name ?? map.name} onChange={(event) => setName(event.target.value)} className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm" />
          <input value={description ?? map.description ?? ''} onChange={(event) => setDescription(event.target.value)} className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm" />
          <input value={rows ?? map.rows} type="number" min={1} onChange={(event) => setRows(Number(event.target.value))} className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm" />
          <input value={columns ?? map.columns} type="number" min={1} onChange={(event) => setColumns(Number(event.target.value))} className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm" />
        </div>
        <button type="button" onClick={() => updateMutation.mutate()} className="mt-3 rounded-md bg-cyan-500 px-3 py-2 text-sm font-medium text-slate-950">
          Save map
        </button>
      </div>

      <div className="rounded-lg border border-slate-800 bg-slate-900 p-4">
        <h3 className="mb-2 text-sm font-semibold">Coordinate check</h3>
        <div className="flex items-end gap-2">
          <input value={coordX} type="number" onChange={(event) => setCoordX(event.target.value === '' ? '' : Number(event.target.value))} placeholder="X" className="w-24 rounded-md border border-slate-700 bg-slate-950 px-2 py-1 text-sm" />
          <input value={coordY} type="number" onChange={(event) => setCoordY(event.target.value === '' ? '' : Number(event.target.value))} placeholder="Y" className="w-24 rounded-md border border-slate-700 bg-slate-950 px-2 py-1 text-sm" />
          <button type="button" onClick={() => checkCoordinateMutation.mutate()} className="rounded-md border border-slate-700 px-3 py-1 text-sm hover:bg-slate-800">Check</button>
        </div>
        {coordResult != null && (
          <p className="mt-2 text-sm text-slate-300">Coordinate ({coordX}, {coordY}) valid: <span className="font-semibold">{coordResult ? 'Yes' : 'No'}</span></p>
        )}
      </div>
    </section>
  )
}
