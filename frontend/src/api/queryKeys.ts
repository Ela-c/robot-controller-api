export const queryKeys = {
  auth: {
    me: ['auth', 'me'] as const,
  },
  users: {
    all: ['users'] as const,
    admins: ['users', 'admins'] as const,
    detail: (id: number) => ['users', id] as const,
  },
  maps: {
    all: ['maps'] as const,
    squares: ['maps', 'squares'] as const,
    detail: (id: number) => ['maps', id] as const,
  },
  robotCommands: {
    all: ['robotCommands'] as const,
    moveOnly: ['robotCommands', 'moveOnly'] as const,
    detail: (id: number) => ['robotCommands', id] as const,
  },
  robotState: {
    current: ['robotState'] as const,
  },
  robotSequences: {
    all: ['robotSequences'] as const,
    detail: (id: number) => ['robotSequences', id] as const,
  },
}
