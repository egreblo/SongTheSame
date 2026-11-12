import { useParams } from 'react-router'
import { RequireAccount } from '@/features/auth/RequireAccount'
import { LeagueStandingsTable } from '@/features/leagues/LeagueStandingsTable'
import { PageShell } from '@/shared/ui/PageShell'

export function LeagueDetailPage() {
  const { leagueId } = useParams()

  return (
    <PageShell>
      <RequireAccount>{leagueId && <LeagueStandingsTable leagueId={leagueId} />}</RequireAccount>
    </PageShell>
  )
}
