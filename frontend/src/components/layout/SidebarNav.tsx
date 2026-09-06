import { Link, NavLink } from 'react-router-dom'
import { Activity, Cpu, History, Map, Route, Shield, UserCircle } from 'lucide-react'
import { useAuth } from '../../hooks/useAuth'
import { isAdmin } from '../../utils/roles'

const navClass = ({ isActive }: { isActive: boolean }) =>
  `flex items-center gap-2 rounded-md px-3 py-2 text-sm transition ${
    isActive ? 'bg-cyan-500/20 text-cyan-200' : 'text-slate-300 hover:bg-slate-800 hover:text-slate-100'
  }`

export const SidebarNav = () => {
  const { user } = useAuth()
  const admin = isAdmin(user)

  return (
    <aside className="hidden w-64 flex-col border-r border-slate-800 bg-slate-950 px-3 py-4 md:flex">
      <Link to="/app" className="mb-6 px-3 text-lg font-semibold text-cyan-300">
        Robot Operations
      </Link>
      <nav className="space-y-1">
        <NavLink to="/app" end className={navClass}><Cpu size={16} /> Dashboard</NavLink>
        <NavLink to="/app/maps" className={navClass}><Map size={16} /> Maps</NavLink>
        <NavLink to="/app/commands" className={navClass}><History size={16} /> Commands</NavLink>
        <NavLink to="/app/sequences" className={navClass}><Route size={16} /> Sequences</NavLink>
        {admin && <NavLink to="/app/users" className={navClass}><Shield size={16} /> Users</NavLink>}
        <NavLink to="/app/profile" className={navClass}><UserCircle size={16} /> Profile</NavLink>
        <NavLink to="/app/system" className={navClass}><Activity size={16} /> System</NavLink>
      </nav>
    </aside>
  )
}
