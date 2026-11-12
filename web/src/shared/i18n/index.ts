import i18n from 'i18next'
import LanguageDetector from 'i18next-browser-languagedetector'
import { initReactI18next } from 'react-i18next'
import en from './locales/en.json'
import hr from './locales/hr.json'

export const supportedLanguages = ['en', 'hr'] as const

void i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: { en: { translation: en }, hr: { translation: hr } },
    fallbackLng: 'en',
    supportedLngs: supportedLanguages,
    interpolation: { escapeValue: false },
  })

export default i18n
