import type { SessionView } from '@/shared/api/generated/models'
import { sfx } from '@/shared/audio/sfx'

const FANFARE_DELAY_MS = 900

function roundOf(session: SessionView, roundIndex: number) {
  return session.rounds.find((round) => round.index === roundIndex)
}

/** Sound after a phase 1 guess: cheer, buzz, or the "out of attempts" jingle. */
export function soundAfterGuess(session: SessionView, roundIndex: number): void {
  const reveal = roundOf(session, roundIndex)?.reveal
  if (reveal?.solved) sfx.correct()
  else if (reveal) sfx.fail()
  else sfx.wrong()
}

/** Sound after skipping to a longer clip (or giving up when no clip is left). */
export function soundAfterSkip(session: SessionView, roundIndex: number): void {
  if (roundOf(session, roundIndex)?.reveal) sfx.fail()
  else sfx.skip()
}

/** Sound after phase 2: depends on the score, and the day's end gets a fanfare. */
export function soundAfterSimilarity(session: SessionView, roundIndex: number): void {
  const phase2 = roundOf(session, roundIndex)?.phase2
  if (phase2) sfx.score(phase2.score)
  if (session.completed) setTimeout(() => sfx.fanfare(), FANFARE_DELAY_MS)
}

export function soundAfterSkippedSimilarity(session: SessionView): void {
  sfx.skip()
  if (session.completed) setTimeout(() => sfx.fanfare(), FANFARE_DELAY_MS)
}
