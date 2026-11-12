import { Check, ChevronsUpDown, Music } from 'lucide-react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList } from '@/components/ui/command'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'
import { useSearchSongs } from '@/shared/api/generated/songs/songs'
import type { SongSummary } from '@/shared/api/generated/models'
import { SongCover } from './SongCover'

interface SongComboboxProps {
  value: SongSummary | null
  onChange: (song: SongSummary) => void
  /** Songs that must not be offered (already guessed, or the answer of phase 1). */
  excludeIds?: string[]
  disabled?: boolean
}

const MIN_QUERY_LENGTH = 2
const DEBOUNCE_MS = 250

function useDebounced<T>(value: T, delayMs: number): T {
  const [debounced, setDebounced] = useState(value)
  useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delayMs)
    return () => clearTimeout(timer)
  }, [value, delayMs])
  return debounced
}

/** Type-ahead song picker backed by the catalog search endpoint. */
export function SongCombobox({ value, onChange, excludeIds = [], disabled }: SongComboboxProps) {
  const { t } = useTranslation()
  const [open, setOpen] = useState(false)
  const [text, setText] = useState('')
  const query = useDebounced(text.trim(), DEBOUNCE_MS)

  const { data, isFetching } = useSearchSongs(
    { q: query, limit: 8 },
    { query: { enabled: query.length >= MIN_QUERY_LENGTH, staleTime: 60_000 } },
  )
  const songs = (data?.data ?? []).filter((song) => !excludeIds.includes(song.id))

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger
        render={
          <Button variant="outline" role="combobox" aria-expanded={open} disabled={disabled} className="h-12 w-full justify-between" />
        }
      >
        {value ? (
          <span className="flex min-w-0 items-center gap-3">
            <SongCover url={value.coverUrl} size="sm" />
            <span className="truncate">
              <span className="font-medium">{value.title}</span>
              <span className="text-muted-foreground"> · {value.artist}</span>
            </span>
          </span>
        ) : (
          <span className="flex items-center gap-2 text-muted-foreground">
            <Music className="size-4" aria-hidden />
            {t('game.searchPlaceholder')}
          </span>
        )}
        <ChevronsUpDown className="size-4 shrink-0 opacity-50" aria-hidden />
      </PopoverTrigger>
      <PopoverContent className="w-(--anchor-width) min-w-72 p-0" align="start">
        <Command shouldFilter={false}>
          <CommandInput value={text} onValueChange={setText} placeholder={t('game.searchHint')} />
          <CommandList>
            {query.length < MIN_QUERY_LENGTH ? (
              <p className="p-4 text-sm text-muted-foreground">{t('game.searchMinChars')}</p>
            ) : (
              <CommandEmpty>{isFetching ? t('game.searching') : t('game.noResults')}</CommandEmpty>
            )}
            <CommandGroup>
              {songs.map((song) => (
                <CommandItem
                  key={song.id}
                  value={song.id}
                  onSelect={() => {
                    onChange(song)
                    setOpen(false)
                  }}
                >
                  <SongCover url={song.coverUrl} size="sm" />
                  <span className="min-w-0 flex-1 truncate">
                    <span className="font-medium">{song.title}</span>
                    <span className="text-muted-foreground"> · {song.artist}</span>
                  </span>
                  {value?.id === song.id && <Check className="size-4" aria-hidden />}
                </CommandItem>
              ))}
            </CommandGroup>
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  )
}
