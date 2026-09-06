import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { queryKeys } from '../api/queryKeys'
import { usersApi } from '../api/usersApi'
import type { UserRole } from '../types/user'
import { getErrorMessage } from '../utils/httpError'

export const UsersPage = () => {
  const queryClient = useQueryClient()
  const [showAdminsOnly, setShowAdminsOnly] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [role, setRole] = useState<UserRole>('user')
  const [selectedUserId, setSelectedUserId] = useState<number | null>(null)
  const [editFirstName, setEditFirstName] = useState('')
  const [editLastName, setEditLastName] = useState('')
  const [editDescription, setEditDescription] = useState('')
  const [editRole, setEditRole] = useState<UserRole>('user')
  const [editEmail, setEditEmail] = useState('')
  const [editPassword, setEditPassword] = useState('')

  const usersQuery = useQuery({
    queryKey: showAdminsOnly ? queryKeys.users.admins : queryKeys.users.all,
    queryFn: showAdminsOnly ? usersApi.getAdminUsers : usersApi.getUsers,
  })

  const createMutation = useMutation({
    mutationFn: usersApi.createUser,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.users.all })
      await queryClient.invalidateQueries({ queryKey: queryKeys.users.admins })
      setFirstName('')
      setLastName('')
      setEmail('')
      setPassword('')
    },
    onError: (mutationError) => setError(getErrorMessage(mutationError)),
  })

  const deleteMutation = useMutation({
    mutationFn: usersApi.deleteUser,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.users.all })
      await queryClient.invalidateQueries({ queryKey: queryKeys.users.admins })
    },
    onError: (mutationError) => setError(getErrorMessage(mutationError)),
  })

  const users = usersQuery.data ?? []

  const loadUserMutation = useMutation({
    mutationFn: usersApi.getUserById,
    onSuccess: (user) => {
      setSelectedUserId(user.id)
      setEditFirstName(user.firstName)
      setEditLastName(user.lastName)
      setEditDescription(user.description ?? '')
      setEditRole((user.role ?? 'user') as UserRole)
      setEditEmail(user.email)
      setEditPassword('')
      setError(null)
    },
    onError: (mutationError) => setError(getErrorMessage(mutationError)),
  })

  const updateUserMutation = useMutation({
    mutationFn: () => {
      if (!selectedUserId) throw new Error('No user selected')
      return usersApi.updateUser(selectedUserId, {
        firstName: editFirstName,
        lastName: editLastName,
        description: editDescription,
        role: editRole,
      })
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.users.all })
      await queryClient.invalidateQueries({ queryKey: queryKeys.users.admins })
      setError(null)
    },
    onError: (mutationError) => setError(getErrorMessage(mutationError)),
  })

  const updateCredentialsMutation = useMutation({
    mutationFn: () => {
      if (!selectedUserId) throw new Error('No user selected')
      return usersApi.patchUserCredentials(selectedUserId, {
        email: editEmail,
        password: editPassword,
      })
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.users.all })
      await queryClient.invalidateQueries({ queryKey: queryKeys.users.admins })
      setEditPassword('')
      setError(null)
    },
    onError: (mutationError) => setError(getErrorMessage(mutationError)),
  })

  return (
    <section className="space-y-4">
      <h2 className="text-xl font-semibold">User Management</h2>
      {error && <p className="rounded-md border border-rose-500/30 bg-rose-500/10 p-2 text-sm text-rose-200">{error}</p>}

      <div className="rounded-lg border border-slate-800 bg-slate-900 p-4">
        <h3 className="mb-3 text-sm font-semibold">Add user</h3>
        <div className="grid gap-2 md:grid-cols-3">
          <input value={firstName} onChange={(event) => setFirstName(event.target.value)} placeholder="First name" className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm" />
          <input value={lastName} onChange={(event) => setLastName(event.target.value)} placeholder="Last name" className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm" />
          <input value={email} onChange={(event) => setEmail(event.target.value)} placeholder="Email" className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm" />
          <input value={password} onChange={(event) => setPassword(event.target.value)} type="password" placeholder="Password" className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm" />
          <select value={role} onChange={(event) => setRole(event.target.value)} className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm">
            <option value="user">user</option>
            <option value="admin">admin</option>
          </select>
        </div>
        <button
          type="button"
          disabled={createMutation.isPending || !firstName || !lastName || !email || !password}
          onClick={() => createMutation.mutate({ firstName, lastName, email, passwordHash: password, role })}
          className="mt-3 rounded-md bg-cyan-500 px-3 py-2 text-sm font-medium text-slate-950 disabled:opacity-60"
        >
          Create User
        </button>
      </div>

      <div className="flex items-center gap-2">
        <button
          type="button"
          onClick={() => setShowAdminsOnly((current) => !current)}
          className="rounded-md border border-slate-700 px-3 py-1 text-sm hover:bg-slate-800"
        >
          {showAdminsOnly ? 'Show all users' : 'Show admins only'}
        </button>
      </div>

      <div className="overflow-auto rounded-lg border border-slate-800 bg-slate-900">
        <table className="min-w-full text-sm">
          <thead className="bg-slate-950 text-slate-400">
            <tr>
              <th className="px-3 py-2 text-left">Name</th>
              <th className="px-3 py-2 text-left">Email</th>
              <th className="px-3 py-2 text-left">Role</th>
              <th className="px-3 py-2 text-left">Actions</th>
            </tr>
          </thead>
          <tbody>
            {users.map((user) => (
              <tr key={user.id} className="border-t border-slate-800">
                <td className="px-3 py-2">{user.firstName} {user.lastName}</td>
                <td className="px-3 py-2">{user.email}</td>
                <td className="px-3 py-2">{user.role}</td>
                <td className="px-3 py-2">
                  <div className="flex gap-2">
                    <button
                      type="button"
                      onClick={() => loadUserMutation.mutate(user.id)}
                      className="rounded border border-slate-700 px-2 py-1 text-xs hover:bg-slate-800"
                    >
                      Edit
                    </button>
                    <button
                      type="button"
                      onClick={() => deleteMutation.mutate(user.id)}
                      className="rounded border border-rose-500/30 bg-rose-500/10 px-2 py-1 text-xs text-rose-200"
                    >
                      Delete
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {selectedUserId && (
        <div className="rounded-lg border border-slate-800 bg-slate-900 p-4">
          <h3 className="mb-3 text-sm font-semibold">Edit user #{selectedUserId}</h3>
          <div className="grid gap-2 md:grid-cols-2">
            <input value={editFirstName} onChange={(event) => setEditFirstName(event.target.value)} placeholder="First name" className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm" />
            <input value={editLastName} onChange={(event) => setEditLastName(event.target.value)} placeholder="Last name" className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm" />
            <input value={editDescription} onChange={(event) => setEditDescription(event.target.value)} placeholder="Description" className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm" />
            <select value={editRole} onChange={(event) => setEditRole(event.target.value)} className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm">
              <option value="user">user</option>
              <option value="admin">admin</option>
            </select>
          </div>

          <button
            type="button"
            onClick={() => updateUserMutation.mutate()}
            className="mt-3 rounded-md bg-cyan-500 px-3 py-2 text-sm font-medium text-slate-950"
          >
            Save profile fields
          </button>

          <div className="mt-4 grid gap-2 md:grid-cols-2">
            <input value={editEmail} onChange={(event) => setEditEmail(event.target.value)} placeholder="Email" className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm" />
            <input value={editPassword} onChange={(event) => setEditPassword(event.target.value)} type="password" placeholder="New password" className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm" />
          </div>
          <button
            type="button"
            disabled={!editPassword}
            onClick={() => updateCredentialsMutation.mutate()}
            className="mt-3 rounded-md border border-slate-700 px-3 py-2 text-sm hover:bg-slate-800 disabled:opacity-60"
          >
            Save email/password
          </button>
        </div>
      )}
    </section>
  )
}
