import { useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { startCustomSession } from '@/shared/api/generated/custom-games/custom-games'
import {
  startDailySession,
  useGuessSong,
  useSkipClip,
  useSkipSimilarity,
  useSubmitSimilarity,
} from '@/shared/api/generated/daily/daily'
import type { SessionView } from '@/shared/api/generated/models'
import { soundAfterGuess, soundAfterSimilarity, soundAfterSkip, soundAfterSkippedSimilarity } from './gameSounds'

export const sessionQueryKey = (code?: string) => (code ? (['custom-session', code] as const) : (['daily-session'] as const))

/**
 * Loads (or resumes) a game - today's puzzle, or a custom game when a share code is given - and exposes the
 * player actions. Every action returns the complete session, which simply replaces the cached one, so the UI
 * always renders what the server decided. Sound effects play from the same responses.
 */
export function useGameSession(code?: string) {
  const queryClient = useQueryClient()
  const queryKey = sessionQueryKey(code)

  const query = useQuery({
    queryKey,
    queryFn: async () => (code ? (await startCustomSession(code)).data : (await startDailySession()).data),
    staleTime: Infinity,
    retry: 1,
  })

  const replaceSession = (session: SessionView) => queryClient.setQueryData(queryKey, session)
  const onError = (error: Error) => toast.error(error.message)

  const guess = useGuessSong({
    mutation: {
      onSuccess: ({ data }, { round }) => {
        replaceSession(data)
        soundAfterGuess(data, round)
      },
      onError,
    },
  })
  const skip = useSkipClip({
    mutation: {
      onSuccess: ({ data }, { round }) => {
        replaceSession(data)
        soundAfterSkip(data, round)
      },
      onError,
    },
  })
  const similarity = useSubmitSimilarity({
    mutation: {
      onSuccess: ({ data }, { round }) => {
        replaceSession(data)
        soundAfterSimilarity(data, round)
      },
      onError,
    },
  })
  const skipSimilarity = useSkipSimilarity({
    mutation: {
      onSuccess: ({ data }) => {
        replaceSession(data)
        soundAfterSkippedSimilarity(data)
      },
      onError,
    },
  })

  const session = query.data
  const sessionId = session?.sessionId

  return {
    session,
    isLoading: query.isLoading,
    isError: query.isError,
    retry: () => void query.refetch(),
    isBusy: guess.isPending || skip.isPending || similarity.isPending || skipSimilarity.isPending,
    guess: (round: number, songId: string) => {
      if (sessionId) guess.mutate({ sessionId, round, data: { songId } })
    },
    skip: (round: number) => {
      if (sessionId) skip.mutate({ sessionId, round })
    },
    submitSimilarity: (round: number, songId: string) => {
      if (sessionId) similarity.mutate({ sessionId, round, data: { songId } })
    },
    skipSimilarity: (round: number) => {
      if (sessionId) skipSimilarity.mutate({ sessionId, round })
    },
  }
}
