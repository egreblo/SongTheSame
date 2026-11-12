import { create } from 'zustand'

const STORAGE_KEY = 'songthesame.soundMuted'

function readMuted(): boolean {
  try {
    return localStorage.getItem(STORAGE_KEY) === '1'
  } catch {
    return false
  }
}

interface SoundState {
  muted: boolean
  toggle: () => void
}

/** Whether game sound effects are muted. Remembered in the browser. */
export const useSoundStore = create<SoundState>((set, get) => ({
  muted: readMuted(),
  toggle: () => {
    const muted = !get().muted
    try {
      localStorage.setItem(STORAGE_KEY, muted ? '1' : '0')
    } catch {
      // Storage unavailable: the setting just lasts for this visit.
    }
    set({ muted })
  },
}))
