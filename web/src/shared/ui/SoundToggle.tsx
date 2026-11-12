import { Volume2, VolumeX } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'
import { sfx } from '@/shared/audio/sfx'
import { useSoundStore } from '@/shared/audio/useSoundStore'

/** Mutes or unmutes the game sound effects. */
export function SoundToggle() {
  const { t } = useTranslation()
  const { muted, toggle } = useSoundStore()

  return (
    <Button
      variant="ghost"
      size="icon"
      aria-label={muted ? t('sound.unmute') : t('sound.mute')}
      aria-pressed={muted}
      onClick={() => {
        toggle()
        // Confirm with a blip when turning sound on (useSoundStore is already updated here).
        if (muted) sfx.skip()
      }}
    >
      {muted ? <VolumeX className="size-5" aria-hidden /> : <Volume2 className="size-5" aria-hidden />}
    </Button>
  )
}
