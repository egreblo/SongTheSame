import { Disc3 } from 'lucide-react'
import { cn } from '@/lib/utils'

const sizes = { sm: 'size-8', md: 'size-14', lg: 'size-24' } as const

interface SongCoverProps {
  url?: string | null
  size?: keyof typeof sizes
  className?: string
}

export function SongCover({ url, size = 'md', className }: SongCoverProps) {
  const box = cn('shrink-0 rounded-md bg-muted object-cover', sizes[size], className)
  if (!url) {
    return (
      <span className={cn(box, 'grid place-items-center')}>
        <Disc3 className="size-1/2 text-muted-foreground" aria-hidden />
      </span>
    )
  }
  return <img src={url} alt="" loading="lazy" className={box} />
}
