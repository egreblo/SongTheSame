import { Home, RotateCw } from 'lucide-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router'
import { Button } from '@/components/ui/button'
import { Phase1Panel } from '@/features/game/components/Phase1Panel'
import { Phase2Panel } from '@/features/game/components/Phase2Panel'
import { RoundProgress } from '@/features/game/components/RoundProgress'
import { RoundResultCard } from '@/features/game/components/RoundResultCard'
import { useGameSession } from '@/features/game/useGameSession'
import { ResultsPanel } from '@/features/results/ResultsPanel'
import { siteUrl } from '@/shared/lib/siteUrl'
import { SoundToggle } from '@/shared/ui/SoundToggle'
import { useGetCustomGame } from '@/shared/api/generated/custom-games/custom-games'
import type { RoundView } from '@/shared/api/generated/models'
import { RoundStatus } from '@/shared/api/generated/models'

function firstOpenRound(rounds: RoundView[]): number {
  return rounds.find((round) => round.status !== RoundStatus.Done)?.index ?? Math.max(...rounds.map((round) => round.index))
}

export function GamePage() {
  const { t } = useTranslation()
  const { code } = useParams()
  const game = useGameSession(code)
  const customGame = useGetCustomGame(code ?? '', { query: { enabled: Boolean(code) } })
  const [seenResults, setSeenResults] = useState(false)
  const [viewIndex, setViewIndex] = useState<number | null>(null)
  const { session } = game

  if (game.isLoading) return <Centered>{t('game.loading')}</Centered>
  if (game.isError || !session) {
    return (
      <Centered>
        <p>{t('game.loadError')}</p>
        <Button onClick={game.retry}>
          <RotateCw className="size-4" aria-hidden />
          {t('game.retry')}
        </Button>
      </Centered>
    )
  }

  const index = viewIndex ?? firstOpenRound(session.rounds)
  const round = session.rounds.find((candidate) => candidate.index === index) ?? session.rounds[0]
  // Pin the viewed round on the first action, so the round's result stays on screen after it is finished.
  const act = (action: () => void) => {
    setViewIndex(index)
    action()
  }

  // A finished game opens on the results; one finished just now shows the last round's score first.
  const showResults = session.completed && (seenResults || viewIndex === null)

  return (
    <main className="mx-auto flex min-h-dvh w-full max-w-md flex-col gap-8 px-4 py-6">
      <nav className="flex items-center justify-between">
        <Button variant="ghost" size="icon" aria-label={t('app.home')} nativeButton={false} render={<Link to="/" />}>
          <Home className="size-5" aria-hidden />
        </Button>
        <h1 className="max-w-48 truncate text-xl font-semibold">{customGame.data?.data.title ?? t('app.title')}</h1>
        <SoundToggle />
      </nav>

      {showResults ? (
        <ResultsPanel session={session} shareUrl={code ? siteUrl(`g/${code}`) : siteUrl()} />
      ) : (
        <>
          <RoundProgress rounds={session.rounds} viewIndex={index} totalPoints={session.totalPoints} />
          {round.status === RoundStatus.Phase1 && (
            <Phase1Panel
              key={`p1-${round.index}`}
              round={round}
              busy={game.isBusy}
              onGuess={(songId) => act(() => game.guess(round.index, songId))}
              onSkip={() => act(() => game.skip(round.index))}
            />
          )}
          {round.status === RoundStatus.Phase2 && (
            <Phase2Panel
              key={`p2-${round.index}`}
              round={round}
              busy={game.isBusy}
              onSubmit={(songId) => act(() => game.submitSimilarity(round.index, songId))}
              onSkip={() => act(() => game.skipSimilarity(round.index))}
            />
          )}
          {round.status === RoundStatus.Done && (
            <RoundResultCard
              key={`done-${round.index}`}
              sessionId={session.sessionId}
              round={round}
              isLastRound={index === session.rounds.length - 1}
              onNext={() => {
                if (index === session.rounds.length - 1) setSeenResults(true)
                else setViewIndex(index + 1)
              }}
            />
          )}
        </>
      )}
    </main>
  )
}

function Centered({ children }: { children: React.ReactNode }) {
  return <main className="mx-auto flex min-h-dvh max-w-md flex-col items-center justify-center gap-4 px-4 text-center">{children}</main>
}
