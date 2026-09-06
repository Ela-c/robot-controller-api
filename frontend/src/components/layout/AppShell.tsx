import { Outlet } from 'react-router-dom'
import { SidebarNav } from './SidebarNav'
import { TopBar } from './TopBar'
import { StatusBar } from './StatusBar'
import { useRobotHub } from '../../hooks/useRobotHub'

export const AppShell = () => {
  useRobotHub({})

  return (
    <div className="flex h-screen flex-col bg-slate-900 text-slate-100">
      <TopBar />
      <div className="flex min-h-0 flex-1">
        <SidebarNav />
        <main className="min-h-0 flex-1 overflow-auto p-4 md:p-6">
          <Outlet />
        </main>
      </div>
      <StatusBar />
    </div>
  )
}
