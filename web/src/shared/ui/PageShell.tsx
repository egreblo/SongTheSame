import { AppNav } from './AppNav'

/** Common page frame: narrow centred column with the top navigation. */
export function PageShell({ children }: { children: React.ReactNode }) {
  return (
    <main className="mx-auto flex min-h-dvh w-full max-w-md flex-col gap-6 px-4 py-6">
      <AppNav />
      {children}
    </main>
  )
}
