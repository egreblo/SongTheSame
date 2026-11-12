import { useCallback, useEffect, useRef, useState } from 'react'

function createAudio(url: string): HTMLAudioElement {
  const audio = new Audio(url)
  audio.preload = 'auto'
  return audio
}

export interface ClipPlayer {
  playing: boolean
  /** 0..1 position within the clip that is currently playing. */
  progress: number
  play: (startSeconds: number, durationSeconds: number) => void
  stop: () => void
}

/** Plays a window (start + duration) of one audio URL and stops by itself when the window ends. */
export function useClipPlayer(url: string | null): ClipPlayer {
  const audioRef = useRef<HTMLAudioElement | null>(null)
  const frameRef = useRef<number | null>(null)
  const [playing, setPlaying] = useState(false)
  const [progress, setProgress] = useState(0)

  const cancelFrame = useCallback(() => {
    if (frameRef.current !== null) cancelAnimationFrame(frameRef.current)
    frameRef.current = null
  }, [])

  const stop = useCallback(() => {
    cancelFrame()
    audioRef.current?.pause()
    setPlaying(false)
    setProgress(0)
  }, [cancelFrame])

  useEffect(() => {
    audioRef.current = null
    return () => {
      cancelFrame()
      audioRef.current?.pause()
      audioRef.current = null
    }
  }, [url, cancelFrame])

  const play = useCallback(
    (startSeconds: number, durationSeconds: number) => {
      if (!url) return
      cancelFrame()
      audioRef.current?.pause()

      const audio = audioRef.current ?? createAudio(url)
      audioRef.current = audio

      const tick = () => {
        const elapsed = audio.currentTime - startSeconds
        if (elapsed >= durationSeconds || audio.ended) {
          stop()
          return
        }
        setProgress(Math.max(0, elapsed / durationSeconds))
        frameRef.current = requestAnimationFrame(tick)
      }

      const begin = () => {
        audio.currentTime = startSeconds
        audio
          .play()
          .then(() => {
            setPlaying(true)
            frameRef.current = requestAnimationFrame(tick)
          })
          .catch(() => setPlaying(false))
      }

      if (audio.readyState >= 1) begin()
      else audio.addEventListener('loadedmetadata', begin, { once: true })
    },
    [url, cancelFrame, stop],
  )

  return { playing, progress, play, stop }
}
