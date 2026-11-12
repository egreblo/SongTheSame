import { AppNav } from './AppNav'
import { SiteFooter } from './SiteFooter'

/** Common page frame: narrow centred column with the top navigation and footer. */
export function PageShell({ children }: { children: React.ReactNode }) {
  return (
    <main className="mx-auto flex min-h-dvh w-full max-w-md flex-col gap-6 px-4 py-6">
      <AppNav />
      {children}
      <SiteFooter />
    </main>
  )
}
