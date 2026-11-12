import { Users } from 'lucide-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { useQueryClient } from '@tanstack/react-query'
import { getListMyLeaguesQueryKey, useCreateLeague, useJoinLeague, useListMyLeagues } from '@/shared/api/generated/leagues/leagues'

/** The player's leagues, plus forms to create a league or join one with an invite code. */
export function LeagueList() {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const [name, setName] = useState('')
  const [code, setCode] = useState('')
  const leagues = useListMyLeagues()

  const refresh = () => queryClient.invalidateQueries({ queryKey: getListMyLeaguesQueryKey() })
  const create = useCreateLeague({
    mutation: {
      onSuccess: () => {
        setName('')
        void refresh()
      },
      onError: (error: Error) => toast.error(error.message),
    },
  })
  const join = useJoinLeague({
    mutation: {
      onSuccess: () => {
        setCode('')
        void refresh()
      },
      onError: (error: Error) => toast.error(error.message),
    },
  })

  return (
    <div className="space-y-6">
      <ul className="space-y-2">
        {leagues.data?.data.map((league) => (
          <li key={league.id}>
            <Link to={`/leagues/${league.id}`} className="flex items-center gap-3 rounded-lg border bg-card p-4 transition-colors hover:bg-secondary">
              <Users className="size-5 text-primary" aria-hidden />
              <span className="min-w-0 flex-1 truncate font-medium">{league.name}</span>
              <span className="text-sm text-muted-foreground">{t('leagues.members', { count: league.memberCount })}</span>
            </Link>
          </li>
        ))}
        {leagues.data?.data.length === 0 && <li className="text-sm text-muted-foreground">{t('leagues.empty')}</li>}
      </ul>

      <form
        className="space-y-2"
        onSubmit={(event) => {
          event.preventDefault()
          create.mutate({ data: { name } })
        }}
      >
        <h2 className="font-semibold">{t('leagues.create')}</h2>
        <div className="flex gap-2">
          <Input value={name} maxLength={40} placeholder={t('leagues.namePlaceholder')} onChange={(event) => setName(event.target.value)} />
          <Button type="submit" disabled={!name.trim() || create.isPending}>
            {t('leagues.createButton')}
          </Button>
        </div>
      </form>

      <form
        className="space-y-2"
        onSubmit={(event) => {
          event.preventDefault()
          join.mutate({ data: { inviteCode: code } })
        }}
      >
        <h2 className="font-semibold">{t('leagues.join')}</h2>
        <div className="flex gap-2">
          <Input value={code} maxLength={12} placeholder={t('leagues.codePlaceholder')} onChange={(event) => setCode(event.target.value)} />
          <Button type="submit" variant="secondary" disabled={!code.trim() || join.isPending}>
            {t('leagues.joinButton')}
          </Button>
        </div>
      </form>
    </div>
  )
}
