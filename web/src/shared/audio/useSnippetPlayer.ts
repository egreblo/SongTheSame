import { useCallback, useEffect, useRef, useState } from 'react'

export interface SnippetPlayer {
  /** Key of the snippet that is loading or playing, if any. */
  activeKey: string | null
  loading: boolean
  /** 0..1 position within the playing snippet. */
  progress: number
  /** Starts a snippet (stopping any other). The URL is resolved lazily, because preview URLs expire. */
  play: (key: string, resolveUrl: () => Promise<string | null>, startSeconds: number, durationSeconds: number) => void
  stop: () => void
}

/** One audio element shared by a list of "play this part" buttons: starting one stops the previous. */
export function useSnippetPlayer(): SnippetPlayer {
  const audioRef = useRef<HTMLAudioElement | null>(null)
  const frameRef = useRef<number | null>(null)
  const requestRef = useRef(0)
  const [activeKey, setActiveKey] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const [progress, setProgress] = useState(0)

  const stop = useCallback(() => {
    requestRef.current += 1
    if (frameRef.current !== null) cancelAnimationFrame(frameRef.current)
    frameRef.current = null
    audioRef.current?.pause()
    audioRef.current = null
    setActiveKey(null)
    setLoading(false)
    setProgress(0)
  }, [])

  useEffect(() => stop, [stop])

  const play = useCallback(
    (key: string, resolveUrl: () => Promise<string | null>, startSeconds: number, durationSeconds: number) => {
      stop()
      const request = requestRef.current
      setActiveKey(key)
      setLoading(true)

      void resolveUrl().then((url) => {
        if (request !== requestRef.current) return
        if (!url) {
          stop()
          return
        }

        const audio = new Audio(url)
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
              if (request !== requestRef.current) return
              setLoading(false)
              frameRef.current = requestAnimationFrame(tick)
            })
            .catch(() => stop())
        }

        if (audio.readyState >= 1) begin()
        else audio.addEventListener('loadedmetadata', begin, { once: true })
      })
    },
    [stop],
  )

  return { activeKey, loading, progress, play, stop }
}
