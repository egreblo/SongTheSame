import { useTranslation } from 'react-i18next'
import { useListMyCustomGames } from '@/shared/api/generated/custom-games/custom-games'
import { CustomGameLink } from './CustomGameLink'

/** The games the signed-in player has already created. */
export function MyGames() {
  const { t } = useTranslation()
  const { data } = useListMyCustomGames()
  const games = data?.data ?? []
  if (games.length === 0) return null

  return (
    <section className="space-y-3">
      <h2 className="font-semibold">{t('editor.myGames')}</h2>
      {games.map((game) => (
        <CustomGameLink key={game.code} game={game} />
      ))}
    </section>
  )
}
