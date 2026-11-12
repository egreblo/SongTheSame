import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useEffect, type ReactNode } from 'react'
import { Toaster } from '@/components/ui/sonner'
import { TooltipProvider } from '@/components/ui/tooltip'
import { claimSessions } from '@/shared/api/generated/me/me'
import { startAuthSync } from '@/shared/auth/authStore'

const queryClient = new QueryClient({
  defaultOptions: { queries: { retry: 1, refetchOnWindowFocus: false } },
})

/** After a fresh sign-in: move games played as a guest to the account, then reload everything. */
async function afterSignIn() {
  try {
    await claimSessions()
  } catch {
    // Not fatal: the guest games simply stay with the browser.
  }
  await queryClient.invalidateQueries()
}

export function Providers({ children }: { children: ReactNode }) {
  useEffect(() => startAuthSync(() => void afterSignIn()), [])

  return (
    <QueryClientProvider client={queryClient}>
      <TooltipProvider>{children}</TooltipProvider>
      <Toaster theme="dark" position="top-center" />
    </QueryClientProvider>
  )
}
