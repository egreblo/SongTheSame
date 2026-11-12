import type { RoundView, SessionView } from '@/shared/api/generated/models'

const PHASE2_SQUARES = 5

/** Phase 1 result as one square: green = quick, yellow = medium, orange = slow, red = never got it. */
export function phase1Square(round: RoundView): string {
  if (!round.reveal?.solved) return '🟥'
  if (round.attempts <= 2) return '🟩'
  if (round.attempts <= 4) return '🟨'
  return '🟧'
}

/** Phase 2 similarity score (1-100) as a row of five squares. */
export function phase2Bar(score: number): string {
  const filled = Math.round((score / 100) * PHASE2_SQUARES)
  return '🟩'.repeat(filled) + '⬛'.repeat(PHASE2_SQUARES - filled)
}

/** The spoiler-free, Wordle-style summary players paste into chats. */
export function buildShareText(session: SessionView, url: string): string {
  const lines = session.rounds.map((round, index) => {
    const score = round.phase2?.score ?? 0
    return `${index + 1} 🎧${phase1Square(round)} 🎯${phase2Bar(score)} ${score}`
  })

  return [`SongTheSame ${session.day}`, `${session.totalPoints}/${session.maxPoints}`, '', ...lines, '', url].join('\n')
}
