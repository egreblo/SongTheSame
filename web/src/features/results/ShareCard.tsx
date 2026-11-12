import { forwardRef } from 'react'
import type { SessionView } from '@/shared/api/generated/models'
import { phase1Square, phase2Bar } from './buildShareText'

interface ShareCardProps {
  session: SessionView
}

/** The picture players can save or send. Rendered off-screen and captured with html-to-image. */
export const ShareCard = forwardRef<HTMLDivElement, ShareCardProps>(function ShareCard({ session }, ref) {
  return (
    <div
      ref={ref}
      style={{
        width: 540,
        padding: 36,
        borderRadius: 28,
        color: '#f6f1e7',
        fontFamily: "'Space Grotesk Variable', 'Geist Variable', sans-serif",
        background:
          'radial-gradient(420px 300px at 0% 0%, #6d2a73 0%, transparent 70%), radial-gradient(380px 280px at 100% 0%, #8a5a12 0%, transparent 70%), #1b1130',
      }}
    >
      <div style={{ fontSize: 20, opacity: 0.8 }}>SongTheSame · {session.day}</div>
      <div style={{ fontSize: 88, fontWeight: 700, lineHeight: 1.05, color: '#f5b83d' }}>
        {session.totalPoints}
        <span style={{ fontSize: 32, opacity: 0.7, color: '#f6f1e7' }}> / {session.maxPoints}</span>
      </div>
      <div style={{ marginTop: 22, display: 'grid', gap: 10, fontSize: 26 }}>
        {session.rounds.map((round, index) => (
          <div key={round.index} style={{ display: 'flex', gap: 14, alignItems: 'center' }}>
            <span style={{ width: 24, opacity: 0.7 }}>{index + 1}</span>
            <span>🎧{phase1Square(round)}</span>
            <span>🎯{phase2Bar(round.phase2?.score ?? 0)}</span>
            <span style={{ marginLeft: 'auto', fontWeight: 600 }}>{round.phase2?.score ?? 0}</span>
          </div>
        ))}
      </div>
    </div>
  )
})
