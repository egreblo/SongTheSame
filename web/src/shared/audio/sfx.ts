import { useSoundStore } from './useSoundStore'

/**
 * Short sound effects synthesised with the Web Audio API, so the app ships no audio files
 * (and no licensing questions). Everything is silent while muted.
 */

type Waveform = OscillatorType

interface ToneOptions {
  type?: Waveform
  volume?: number
  /** Frequency to glide to over the duration. */
  glideTo?: number
}

let context: AudioContext | null = null

function audioContext(): AudioContext | null {
  if (useSoundStore.getState().muted) return null
  if (!context) {
    const Constructor =
      window.AudioContext ?? (window as typeof window & { webkitAudioContext?: typeof AudioContext }).webkitAudioContext
    if (!Constructor) return null
    context = new Constructor()
  }
  if (context.state === 'suspended') void context.resume()
  return context
}

function tone(ctx: AudioContext, frequency: number, offset: number, duration: number, options: ToneOptions = {}) {
  const { type = 'sine', volume = 0.16, glideTo } = options
  const start = ctx.currentTime + offset
  const oscillator = ctx.createOscillator()
  const gain = ctx.createGain()

  oscillator.type = type
  oscillator.frequency.setValueAtTime(frequency, start)
  if (glideTo) oscillator.frequency.exponentialRampToValueAtTime(glideTo, start + duration)

  gain.gain.setValueAtTime(0.0001, start)
  gain.gain.exponentialRampToValueAtTime(volume, start + 0.015)
  gain.gain.exponentialRampToValueAtTime(0.0001, start + duration)

  oscillator.connect(gain).connect(ctx.destination)
  oscillator.start(start)
  oscillator.stop(start + duration + 0.05)
}

// Note frequencies (Hz).
const C5 = 523.25
const E5 = 659.25
const G5 = 783.99
const C6 = 1046.5
const A4 = 440
const E4 = 329.63

export const sfx = {
  /** Right answer: a bright rising arpeggio. */
  correct() {
    const ctx = audioContext()
    if (!ctx) return
    ;[C5, E5, G5, C6].forEach((note, index) => tone(ctx, note, index * 0.08, 0.28, { type: 'triangle' }))
  },

  /** Wrong guess: a short low buzz. */
  wrong() {
    const ctx = audioContext()
    if (!ctx) return
    tone(ctx, 220, 0, 0.22, { type: 'sawtooth', volume: 0.09, glideTo: 150 })
  },

  /** Skipping to a longer clip: a quick upward swoosh. */
  skip() {
    const ctx = audioContext()
    if (!ctx) return
    tone(ctx, 420, 0, 0.14, { type: 'sine', volume: 0.1, glideTo: 880 })
  },

  /** Out of attempts: three falling notes. */
  fail() {
    const ctx = audioContext()
    if (!ctx) return
    ;[A4, 392, E4 * 0.75].forEach((note, index) => tone(ctx, note, index * 0.17, 0.3, { type: 'triangle', volume: 0.13 }))
  },

  /** Phase 2 result: brighter the better the score. */
  score(score: number) {
    const ctx = audioContext()
    if (!ctx) return
    if (score >= 75) {
      ;[C5, E5, G5].forEach((note) => tone(ctx, note, 0, 0.7, { type: 'triangle', volume: 0.1 }))
      tone(ctx, C6, 0.2, 0.5, { type: 'sine', volume: 0.12 })
      tone(ctx, G5 * 2, 0.34, 0.5, { type: 'sine', volume: 0.08 })
    } else if (score >= 40) {
      tone(ctx, E5, 0, 0.2, { type: 'triangle' })
      tone(ctx, G5, 0.13, 0.32, { type: 'triangle' })
    } else {
      tone(ctx, 300, 0, 0.35, { type: 'triangle', volume: 0.1, glideTo: 220 })
    }
  },

  /** All five rounds done. */
  fanfare() {
    const ctx = audioContext()
    if (!ctx) return
    const melody: [number, number, number][] = [
      [C5, 0, 0.16],
      [C5, 0.17, 0.16],
      [C5, 0.34, 0.16],
      [G5, 0.51, 0.5],
      [E5, 1.05, 0.16],
      [G5, 1.22, 0.7],
    ]
    melody.forEach(([note, offset, duration]) => tone(ctx, note, offset, duration, { type: 'square', volume: 0.07 }))
    tone(ctx, C6, 1.22, 0.9, { type: 'triangle', volume: 0.1 })
  },
}
