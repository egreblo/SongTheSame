import { describe, expect, it } from 'vitest'
import type { RoundView, SessionView } from '@/shared/api/generated/models'
import { RoundStatus } from '@/shared/api/generated/models'
import { buildShareText, phase2Bar } from './buildShareText'

function round(index: number, attempts: number, solved: boolean, score: number): RoundView {
  const song = { id: 's', title: 'Song', artist: 'Artist', coverUrl: null }
  return {
    index,
    status: RoundStatus.Done,
    audioUrl: null,
    attempts,
    maxAttempts: 6,
    unlockedClipSeconds: 1,
    clipSeconds: [1, 2, 4, 7, 11, 16],
    guesses: [],
    reveal: { song, solved, segmentStartSeconds: 3, segmentSeconds: 6 },
    phase2: { guess: song, skipped: false, score, points: Math.round(score / 2), guessMatchStartSeconds: null, bestMatches: [] },
    phase1Points: 0,
    points: 0,
  }
}

describe('buildShareText', () => {
  it('summarises every round without revealing any song', () => {
    const session: SessionView = {
      sessionId: 'id',
      day: '2026-11-13',
      totalPoints: 312,
      maxPoints: 500,
      completed: true,
      rounds: [round(0, 1, true, 100), round(1, 3, true, 60), round(2, 6, false, 1)],
    }

    const text = buildShareText(session, 'https://songthesame.example')

    expect(text).toBe(
      [
        'SongTheSame 2026-11-13',
        '312/500',
        '',
        '1 🎧🟩 🎯🟩🟩🟩🟩🟩 100',
        '2 🎧🟨 🎯🟩🟩🟩⬛⬛ 60',
        '3 🎧🟥 🎯⬛⬛⬛⬛⬛ 1',
        '',
        'https://songthesame.example',
      ].join('\n'),
    )
    expect(text).not.toContain('Artist')
  })

  it('fills squares proportionally to the score', () => {
    expect(phase2Bar(0)).toBe('⬛⬛⬛⬛⬛')
    expect(phase2Bar(100)).toBe('🟩🟩🟩🟩🟩')
  })
})
