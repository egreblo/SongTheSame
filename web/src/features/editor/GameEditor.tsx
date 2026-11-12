import { useQueryClient } from '@tanstack/react-query'
import { ArrowDown, ArrowUp, Loader2, X } from 'lucide-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { SongCombobox } from '@/features/game/components/SongCombobox'
import { getListMyCustomGamesQueryKey, useCreateCustomGame } from '@/shared/api/generated/custom-games/custom-games'
import type { CustomGameSummary, SongSummary } from '@/shared/api/generated/models'
import { CustomGameLink } from './CustomGameLink'

const ROUNDS = 5

/** Editor: name a game, pick five songs in play order, and get a share link. */
export function GameEditor() {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const [title, setTitle] = useState('')
  const [songs, setSongs] = useState<(SongSummary | null)[]>(Array.from({ length: ROUNDS }, () => null))
  const [created, setCreated] = useState<CustomGameSummary | null>(null)

  const create = useCreateCustomGame({
    mutation: {
      onSuccess: (response) => {
        setCreated(response.data)
        void queryClient.invalidateQueries({ queryKey: getListMyCustomGamesQueryKey() })
      },
      onError: (error: Error) => toast.error(error.message),
    },
  })

  const picked = songs.filter((song): song is SongSummary => song !== null)
  const ready = title.trim().length > 0 && picked.length === ROUNDS

  const setSong = (index: number, song: SongSummary | null) =>
    setSongs((current) => current.map((existing, i) => (i === index ? song : existing)))
  const move = (index: number, delta: number) =>
    setSongs((current) => {
      const next = [...current]
      ;[next[index], next[index + delta]] = [next[index + delta], next[index]]
      return next
    })

  if (created) {
    return (
      <div className="space-y-4">
        <CustomGameLink game={created} />
        <Button
          variant="secondary"
          onClick={() => {
            setCreated(null)
            setTitle('')
            setSongs(Array.from({ length: ROUNDS }, () => null))
          }}
        >
          {t('editor.another')}
        </Button>
      </div>
    )
  }

  return (
    <form
      className="space-y-5"
      onSubmit={(event) => {
        event.preventDefault()
        if (ready) create.mutate({ data: { title, songIds: picked.map((song) => song.id) } })
      }}
    >
      <div className="space-y-2">
        <label htmlFor="game-title" className="text-sm font-medium">
          {t('editor.titleLabel')}
        </label>
        <Input id="game-title" value={title} maxLength={60} placeholder={t('editor.titlePlaceholder')} onChange={(event) => setTitle(event.target.value)} />
      </div>

      <ol className="space-y-3">
        {songs.map((song, index) => (
          <li key={index} className="flex items-center gap-2">
            <span className="grid size-7 shrink-0 place-items-center rounded-full bg-secondary text-xs font-semibold">{index + 1}</span>
            <div className="min-w-0 flex-1">
              <SongCombobox
                value={song}
                onChange={(picked) => setSong(index, picked)}
                excludeIds={picked.map((p) => p.id)}
                disabled={create.isPending}
              />
            </div>
            <div className="flex flex-col">
              <Button type="button" variant="ghost" size="icon-xs" aria-label={t('editor.moveUp')} disabled={index === 0} onClick={() => move(index, -1)}>
                <ArrowUp className="size-3.5" aria-hidden />
              </Button>
              <Button type="button" variant="ghost" size="icon-xs" aria-label={t('editor.moveDown')} disabled={index === ROUNDS - 1} onClick={() => move(index, 1)}>
                <ArrowDown className="size-3.5" aria-hidden />
              </Button>
            </div>
            <Button type="button" variant="ghost" size="icon-xs" aria-label={t('editor.clear')} disabled={!song} onClick={() => setSong(index, null)}>
              <X className="size-3.5" aria-hidden />
            </Button>
          </li>
        ))}
      </ol>

      <Button type="submit" size="lg" className="w-full" disabled={!ready || create.isPending}>
        {create.isPending ? (
          <>
            <Loader2 className="size-4 animate-spin" aria-hidden />
            {t('editor.building')}
          </>
        ) : (
          t('editor.create')
        )}
      </Button>
      {create.isPending && <p className="text-center text-xs text-muted-foreground">{t('editor.buildingHint')}</p>}
    </form>
  )
}
