import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'

export function SiteFooter() {
  const { t } = useTranslation()

  return (
    <footer className="mt-auto flex justify-center gap-4 pt-6 text-xs text-muted-foreground">
      <Link to="/privacy" className="hover:text-foreground">
        {t('privacy.title')}
      </Link>
      <a href="https://github.com/egreblo/SongTheSame/issues" className="hover:text-foreground" target="_blank" rel="noreferrer">
        {t('footer.feedback')}
      </a>
    </footer>
  )
}
