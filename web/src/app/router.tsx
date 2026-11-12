import { createBrowserRouter } from 'react-router'
import { AdminPage } from '@/pages/AdminPage'
import { CreateGamePage } from '@/pages/CreateGamePage'
import { GamePage } from '@/pages/GamePage'
import { HomePage } from '@/pages/HomePage'
import { LeagueDetailPage } from '@/pages/LeagueDetailPage'
import { LeaguesPage } from '@/pages/LeaguesPage'
import { PrivacyPage } from '@/pages/PrivacyPage'

export const router = createBrowserRouter(
  [
    { path: '/', element: <HomePage /> },
    { path: '/play', element: <GamePage /> },
    { path: '/g/:code', element: <GamePage /> },
    { path: '/leagues', element: <LeaguesPage /> },
    { path: '/leagues/:leagueId', element: <LeagueDetailPage /> },
    { path: '/create', element: <CreateGamePage /> },
    { path: '/admin', element: <AdminPage /> },
    { path: '/privacy', element: <PrivacyPage /> },
  ],
  { basename: import.meta.env.BASE_URL },
)
