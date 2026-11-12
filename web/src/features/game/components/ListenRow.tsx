import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import type { SongSummary } from '@/shared/api/generated/models'
import { getSongPreview } from '@/shared/api/generated/songs/songs'
import type { SnippetPlayer } from '@/shared/audio/useSnippetPlayer'
import { formatTime } from '@/shared/lib/formatTime'
import { SnippetButton } from './SnippetButton'
import { SongCover } from './SongCover'

/** Resolves a fresh preview URL for a catalog song, telling the player when none is available. */
export function previewResolver(songId: string, onMissing: () => void): () => Promise<string | null> {
  return async () => {
    try {
      return (await getSongPreview(songId)).data.url
    } catch {
      onMissing()
      return null
    }
  }
}

interface ListenRowProps {
  /** Unique within the page, so one shared player can tell which row is playing. */
  id: string
  song: SongSummary
  caption?: string
  /** Where in the preview the interesting part begins; unknown for old puzzles, so playback starts at 0. */
  startSeconds: number | null | undefined
  durationSeconds: number
  player: SnippetPlayer
  /** Overrides the preview lookup, for rows whose audio URL is already known. */
  resolveUrl?: () => Promise<string | null>
  trailing?: React.ReactNode
}

/** A song with a play button that starts at the part that matches best, plus where that part is. */
export function ListenRow({ id, song, caption, startSeconds, durationSeconds, player, resolveUrl, trailing }: ListenRowProps) {
  const { t } = useTranslation()
  const active = player.activeKey === id
  const start = startSeconds ?? 0
  const resolver = resolveUrl ?? previewResolver(song.id, () => toast.error(t('game.noPreview')))

  return (
    <li className="flex items-center gap-3 rounded-md bg-muted/40 px-3 py-2">
      <SongCover url={song.coverUrl} size="sm" />
      <div className="min-w-0 flex-1">
        <p className="truncate text-sm">
          {song.title} <span className="text-muted-foreground">· {song.artist}</span>
        </p>
        <p className="text-xs text-muted-foreground">
          {caption ? `${caption} · ` : ''}
          {startSeconds != null ? t('game.mostSimilarAt', { time: formatTime(start) }) : t('game.fromStart')}
        </p>
      </div>
      {trailing}
      <SnippetButton
        active={active}
        loading={active && player.loading}
        progress={player.progress}
        label={t('game.playPart')}
        onToggle={() => (active ? player.stop() : player.play(id, resolver, start, durationSeconds))}
      />
    </li>
  )
}
