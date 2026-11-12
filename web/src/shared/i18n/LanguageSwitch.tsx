import { Languages } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/components/ui/button'

/** Toggles between the supported languages (Croatian / English). */
export function LanguageSwitch() {
  const { i18n } = useTranslation()
  const current = i18n.resolvedLanguage === 'hr' ? 'hr' : 'en'

  return (
    <Button
      variant="ghost"
      size="sm"
      aria-label="Language"
      onClick={() => void i18n.changeLanguage(current === 'hr' ? 'en' : 'hr')}
    >
      <Languages className="size-4" aria-hidden />
      {current === 'hr' ? 'HR' : 'EN'}
    </Button>
  )
}
