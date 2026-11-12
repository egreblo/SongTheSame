import { useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { startDailySession, useGuessSong, useSkipClip, useSubmitSimilarity } from '@/shared/api/generated/daily/daily'
import { startCustomSession } from '@/shared/api/generated/custom-games/custom-games'
import type { SessionView } from '@/shared/api/generated/models'

export const sessionQueryKey = (code?: string) => (code ? (['custom-session', code] as const) : (['daily-session'] as const))

/**
 * Loads (or resumes) a game - today's puzzle, or a custom game when a share code is given - and exposes the
 * player actions. Every action returns the complete session, which simply replaces the cached one, so the UI
 * always renders what the server decided.
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

  const replaceSession = (response: { data: SessionView }) => queryClient.setQueryData(queryKey, response.data)
  const showError = (error: Error) => toast.error(error.message)
  const options = { mutation: { onSuccess: replaceSession, onError: showError } }

  const guess = useGuessSong(options)
  const skip = useSkipClip(options)
  const similarity = useSubmitSimilarity(options)

  const session = query.data
  const sessionId = session?.sessionId

  return {
    session,
    isLoading: query.isLoading,
    isError: query.isError,
    retry: () => void query.refetch(),
    isBusy: guess.isPending || skip.isPending || similarity.isPending,
    guess: (round: number, songId: string) => {
      if (sessionId) guess.mutate({ sessionId, round, data: { songId } })
    },
    skip: (round: number) => {
      if (sessionId) skip.mutate({ sessionId, round })
    },
    submitSimilarity: (round: number, songId: string) => {
      if (sessionId) similarity.mutate({ sessionId, round, data: { songId } })
    },
  }
}
