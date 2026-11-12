import { Copy, LogOut } from 'lucide-react'
import { useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { getListMyLeaguesQueryKey, useGetLeagueStandings, useLeaveLeague } from '@/shared/api/generated/leagues/leagues'
import { useAuthStore } from '@/shared/auth/authStore'

/** Today / 7-day / 30-day comparison of a league's members, plus the invite code and leave button. */
export function LeagueStandingsTable({ leagueId }: { leagueId: string }) {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const userId = useAuthStore((state) => state.user?.id)
  const { data, isLoading, isError } = useGetLeagueStandings(leagueId)
  const leave = useLeaveLeague({
    mutation: {
      onSuccess: async () => {
        await queryClient.invalidateQueries({ queryKey: getListMyLeaguesQueryKey() })
        void navigate('/leagues')
      },
      onError: (error: Error) => toast.error(error.message),
    },
  })

  if (isLoading) return <p className="text-muted-foreground">{t('game.loading')}</p>
  if (isError || !data) return <p className="text-destructive">{t('leagues.loadError')}</p>
  const league = data.data

  const copyCode = async () => {
    await navigator.clipboard.writeText(league.inviteCode)
    toast.success(t('leagues.codeCopied'))
  }

  return (
    <div className="space-y-5">
      <header className="space-y-1">
        <h1 className="text-2xl font-semibold">{league.name}</h1>
        <p className="text-sm text-muted-foreground">{t('leagues.standingsFor', { day: league.today })}</p>
      </header>

      <div className="overflow-x-auto rounded-lg border">
        <table className="w-full text-sm">
          <thead className="bg-muted/50 text-left text-xs text-muted-foreground">
            <tr>
              <th className="px-3 py-2">#</th>
              <th className="px-3 py-2">{t('leagues.player')}</th>
              <th className="px-3 py-2 text-right">{t('leagues.today')}</th>
              <th className="px-3 py-2 text-right">{t('leagues.week')}</th>
              <th className="px-3 py-2 text-right">{t('leagues.month')}</th>
            </tr>
          </thead>
          <tbody>
            {league.rows.map((row, index) => (
              <tr key={row.userId} className={row.userId === userId ? 'bg-primary/10' : undefined}>
                <td className="px-3 py-2 tabular-nums text-muted-foreground">{index + 1}</td>
                <td className="px-3 py-2 font-medium">{row.displayName}</td>
                <td className="px-3 py-2 text-right tabular-nums">{row.todayPoints ?? '—'}</td>
                <td className="px-3 py-2 text-right tabular-nums">{row.weekPoints}</td>
                <td className="px-3 py-2 text-right tabular-nums">{row.monthPoints}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="flex items-center justify-between gap-3 rounded-lg border bg-card p-4">
        <div>
          <p className="text-xs text-muted-foreground">{t('leagues.inviteCode')}</p>
          <p className="font-mono text-xl tracking-widest">{league.inviteCode}</p>
        </div>
        <Button variant="secondary" onClick={() => void copyCode()}>
          <Copy className="size-4" aria-hidden />
          {t('share.copy')}
        </Button>
      </div>

      <Button variant="ghost" className="text-muted-foreground" onClick={() => leave.mutate({ leagueId })}>
        <LogOut className="size-4" aria-hidden />
        {t('leagues.leave')}
      </Button>
    </div>
  )
}
