# SongTheSame – plan rada

Dnevna glazbena igra u stilu Wordlea: 5 pjesama dnevno, 2 faze po pjesmi, rezultat 1–100 po rundi.

## 1. Odabrane odluke

| Područje | Odluka |
|---|---|
| Audio izvor | Deezer API (30 s previewi, bez auth-a), iTunes Search API kao rezervni izvor |
| Frontend | React + TypeScript + Vite |
| Backend | ASP.NET Core (.NET 10 LTS), Clean Architecture |
| Baza + Auth | Supabase (Postgres + Google/email/Apple prijava), API validira Supabase JWT |
| Hosting API-ja | Container na Google Cloud Run (preporuka) ili Render/Fly.io |
| Analizator | Chroma (pitch-class) značajke + subsequence DTW + tolerancija transpozicije, na serveru |

**Napomena o hostingu:** Fly.io za nove račune više nema pravi besplatni tier, a Render free uspavljuje servis. Cloud Run ima velik besplatni mjesečni limit i scale-to-zero. Cold start nije problem jer su dnevne zagonetke pripremljene unaprijed. Provjeriti trenutne uvjete u Sprintu 0.

## 2. Pravila igre (kako ih shvaćam)

**Faza 1 – pogodi pjesmu**
- Gumb Play pušta isječak pjesme: 1 s → 2 s → 4 s → 7 s → 11 s → 16 s (inkrementi su konfigurabilni).
- Svaki pogrešan pogodak ili skip otključava duži isječak. Pogodak se bira autocompleteom iz kataloga.
- Bodovi za fazu 1 ovise o broju korištenih pokušaja.

**Faza 2 – pronađi najsličniju pjesmu**
- Igrač vidi/čuje kratki „ciljni segment” (~6 s) pogođene pjesme i može ga puštati neograničeno.
- Upisuje drugu pjesmu za koju misli da u sebi ima najsličniji dio. Server vraća sličnost 1–100.
- Ista pjesma ne smije se upisati (potvrđeno), inače bi uvijek bilo 100.
- Igrač ima **1 pokušaj** u fazi 2 (potvrđeno). Bodovi: faza 1 do 50, faza 2 do 50.

**Kraj dana:** zbroj 5 rundi, usporedba s ostalim igračima (percentil), share (emoji tekst + slika + Web Share API), report rundi.

## 3. Kako radi analizator (srž projekta)

Audio se **ne analizira u runtimeu**. Sve se računa unaprijed.

1. **Ingest** (offline job): za svaku pjesmu u katalogu dohvati preview → dekodiraj u mono 22.05 kHz → STFT → **chroma vektori (12 pitch classova) ~10 okvira/s** → spremi samo značajke (~3 KB po pjesmi). Audio se ne pohranjuje.
2. **Ciljni segment:** niz od ~60 chroma okvira (6 s) iz pogođene pjesme (ručno ili automatski odabran najkarakterističniji dio).
3. **Usporedba s pogođenom pjesmom X:** subsequence DTW ciljnog segmenta protiv svih pozicija u X (300 okvira).
   - **Tempo:** DTW dopušta rastezanje/sažimanje.
   - **Tonalitet:** isto za svih 12 cirkularnih rotacija chroma vektora, uzme se najbolja.
   - Cijena: ~60×300×12 ≈ 216k ćelija po usporedbi, milisekunde.
4. **Normalizacija:** sirova udaljenost → skala 1–100 preko kalibracijske krivulje (percentili udaljenosti na slučajnim parovima iz kataloga). Isti pjesma s drugim izvedbom/coverom treba dati 90+, nasumični par 10–30.
5. **Kasnije nadogradnje:** dodati ritmičku/MFCC komponentu, ili ONNX embeddinge (CLAP/MERT), iza istog `ISimilarityScorer` sučelja.

**Sprint 0 spike je obavezan:** prije bilo čega ostalog izgraditi prototip analizatora (konzolna C# aplikacija) i ručno provjeriti ~30 parova (cover, sample, ista progresija akorda, nasumični). Ako rezultati nisu dobri, mijenja se algoritam, a ne igra.

**Poznata ograničenja:** previewi su 30 s (obično sredina pjesme), dakle sličnost se traži samo unutar tih 30 s. Preview URL-ovi mogu isteći, pa ih backend osvježava pri posluživanju. Vokali/produkcija zbunjuju chromu, a CQT ili harmonic-percussive separacija mogu pomoći.

## 4. Arhitektura backenda (Clean Architecture)

```
src/
  SongTheSame.Domain/          # entiteti, value objecti, pravila igre, bez ovisnosti
  SongTheSame.Application/     # use caseovi (handleri), DTO, sučelja (ports), validatori
  SongTheSame.Infrastructure/  # EF Core/Npgsql, Deezer klijent, Supabase JWT, storage
  SongTheSame.Audio/           # chroma, DTW, scorer (čista biblioteka, testabilna)
  SongTheSame.Api/             # minimal API endpointi grupirani po featureu, DI, middleware
  SongTheSame.Ingest/          # CLI/job za punjenje kataloga i značajki
tests/
  *.Domain.Tests, *.Application.Tests, *.Audio.Tests, *.Api.IntegrationTests
```

Pravila koda:
- **Cijeli codebase je na engleskom:** nazivi klasa, metoda, varijabli, commit poruke, komentari, README, API rute, nazivi tablica i migracija. Hrvatski postoji samo u prijevodnim resursima sučelja (`hr` locale file).
- Organizacija po featureu unutar Applicationa (`Features/Daily/GetToday`, `Features/Rounds/SubmitGuess`…), jedna klasa po fileu.
- **Ograničenje veličine fajla** (~200 linija) provodi se `.editorconfig` + CI skriptom.
- Mediator: **ne MediatR** (komercijalan od 2025.); koristiti jednostavna sučelja `IHandler<TRequest,TResponse>` ili `Mediator` source generator.
- Za asserte Shouldly (FluentAssertions je komercijalan).

### Glavni entiteti
`Song`, `SongFeatures`, `DailyPuzzle` (+ 5 `PuzzleRound`), `GameSession`, `RoundResult`, `Profile`, `League`, `LeagueMember`, `CustomGame`, `CustomGameRound`, `RoundReport`.

### Glavni endpointi
`GET /daily` · `GET /songs/search?q=` · `POST /rounds/{id}/guess` (faza 1) · `POST /rounds/{id}/similarity` (faza 2) · `GET /results/{date}/distribution` · `POST /reports` · `/leagues/*` · `/custom-games/*`.

### Anti-cheat i pravednost
- Ishod se računa na serveru; klijent nikad ne dobiva točan odgovor prije kraja runde.
- Audio URL otkriva pjesmu u network tabu. Za MVP prihvaćeno (kao Heardle); kasnije se može proxyati kroz API.
- Dnevna zagonetka je ista za sve prema **UTC datumu** (ili Europe/Zagreb – odluka), generira se tjedan unaprijed.
- Anonimni igrači: stanje u localStorageu, a pri prijavi se spaja s računom.

## 5. Frontend: predloženi alati (da UI ne izgleda generički)

| Namjena | Alat |
|---|---|
| Build, jezik | **Vite + React + TypeScript** |
| Stilovi | **Tailwind CSS v4** + vlastiti design tokens (boje, tipografija) |
| Komponente | **shadcn/ui** (Radix primitivi, kod je tvoj) + **cmdk** (autocomplete/combobox) |
| Animacije | **Motion** (Framer Motion) za prijelaze, otkrivanje rezultata |
| Audio + valni oblik | **wavesurfer.js** (waveform, regije za isječke) uz Web Audio API / **Howler.js** |
| Ikone | **Lucide** |
| Podaci | **TanStack Query** + generirani API klijent (**Orval** ili **openapi-typescript** iz OpenAPI specifikacije .NET-a) |
| Routing | **React Router** ili **TanStack Router** |
| Stanje | **Zustand** |
| Grafovi | **visx** ili Recharts (distribucija rezultata) |
| Share slika | **html-to-image** + **Web Share API** (poruke, mail, WhatsApp, itd.) |
| Efekti | **canvas-confetti**, **Sonner** (toastovi) |
| i18n | **react-i18next** (hr/en) |
| Prijava | **@supabase/supabase-js** |
| Dev kvaliteta | ESLint, Prettier, **Vitest**, **Testing Library**, **Playwright** (e2e), **Storybook** (po želji) |
| Editor pjesama | **dnd-kit** (redoslijed rundi) + wavesurfer regije (odabir isječka) |

**Dizajn:** prije kodiranja odrediti vizualni identitet (paleta, fontovi s Google Fonts, 1–2 ključna ekrana u Figmi ili kao HTML mockup). Tek tada graditi komponente, inače shadcn izgleda generički.

**Zašto React, a ne Blazor:** bolji audio/vizualizacijski ekosustav i gotove komponente. Blazor bi značio više JS interopa upravo oko audija, što je jezgra aplikacije.

## 6. Redoslijed rada (milestones)

**M0 – Spike i pripreme (1 tjedan)**
- Provjeriti Deezer API: dostupnost previewa, istek URL-ova, rate limiti, uvjeti korištenja.
- Prototip analizatora u konzolnoj aplikaciji; ručna evaluacija ~30 parova; odabir kalibracije.
- Odabrati vizualni identitet i napraviti mockupe.
- *Gate:* ako analizator ne daje smislene rezultate, ne ide se dalje.

**Rezultati M0 (prototip u `src/SongTheSame.Audio` + `tools/SongTheSame.Spike`):**
- Deezer: pretraga i preview rade bez ključa, 42/42 pjesama dohvaćeno (uključujući hrvatske). Preview URL-ovi su potpisani i **istječu u roku od nekoliko sati**, pa ih backend mora osvježavati preko `/track/{id}`. Pretraga je nepouzdana kod parnih naslova (npr. „Jedina” → „Jedino ti”), pa katalog treba graditi po ID-u, ne po upitu.
- Tolerancija na tempo i tonalitet radi: transponirana (+2 polutona) i ubrzana/usporena verzija iste pjesme je rank 1 u 15/16 slučajeva (jednom rank 2, jednom rank 4 uz transpoziciju).
- Prva verzija chrome je imala premalo kontrasta (svi parovi 0,01–0,06). Centriranje po okviru podiglo je razlučivost: coverovi dobivaju percentil 86–100 (Hurt, Hallelujah, I Will Always Love You, I Will Survive, Hotel California), a ista pjesma u drugoj snimci 99.
- Slabije: „sample/riff” parovi (Under Pressure ↔ Ice Ice Baby, Nirvana ↔ Boston) i Hendrix ↔ Dylan ostaju oko percentila 40–55. Chroma hvata harmoniju, a ne bas liniju ni ritam.
- Brzina: 1640 usporedbi parova (5 segmenata × 12 transpozicija) za 7 s, što je pogotovo dovoljno za jedan zahtjev po igraču.
- **Gate prolazi uvjetno:** analizator je upotrebljiv za „slična harmonija/melodija”. Za bolje sample/riff pogotke u M3 treba probati: harmonic-percussive separaciju, CQT umjesto STFT-a, te dodatnu bas/ritam značajku.

**M1 – Kostur (GOTOVO osim Supabase projekta i deploya)**
- Solution sa slojevima, CI (GitHub Actions: build, test, lint, provjera veličine fajla), Docker, Supabase projekt, Vite app, OpenAPI → TS klijent.

**M2 – Katalog i ingest (GOTOVO)**
- `Ingest` CLI: Deezer → preview → chroma → baza. Početni katalog od 2–5k pjesama (popularne liste po žanrovima/desetljećima).
- Endpoint za pretragu s autocompleteom (Postgres `pg_trgm`).

**M3 – Analizator kao servis (GOTOVO)**
- `SongTheSame.Audio` s chroma/DTW/scorerom, unit testovi s fixture parovima, kalibracijska krivulja.

**M4 – Dnevna igra (backend) (GOTOVO)**
- Generiranje 5 pjesama po danu (pravila: raznolikost žanra/ere, bez ponavljanja), GameSession, faza 1 i 2, bodovanje.

**M5 – Igra (frontend) (GOTOVO)**
- Player s inkrementima, autocomplete, faza 2, animacije otkrivanja, ekran rezultata, localStorage za anonimne.

**M6 – Rezultati i dijeljenje (GOTOVO)**
- Percentil/distribucija, share tekst i slika, statistika serije (streak).

**M7 – Prijava i spremanje (GOTOVO; čeka Supabase ključeve)**
- Supabase Auth (Google + email, kasnije Apple/Facebook), spajanje anonimnog stanja, profil, povijest.

**M8 – Report (GOTOVO osnovno; pregled prijava je za sada SQL upit)**
- Znak za prijavu runde, tablica `RoundReport`, jednostavan admin pregled; ručna korekcija/ponovni izračun.

**M9 – Lige (GOTOVO)**
- Kreiranje lige, pozivni link, dnevni poredak i ukupni poredak, notifikacije (kasnije).

**M10 – Editor custom igara (GOTOVO; segment se bira automatski)**
- Pretraga kataloga (ili Deezer-a s on-demand ingestom), odabir 5 pjesama, odabir segmenta, dijeljenje linkom, igra se kao dnevna.
- Moderacija i limiti (rate limit, veličina).

**M11 – Polish i lansiranje**
- Pristupačnost, mobilni UX, performanse, SEO/OG tagovi, monitoring (Serilog + Sentry), backup baze, domena.

## 7. Rizici

1. **Pravno:** Deezer/iTunes uvjeti za javnu igru nisu isti kao za osobnu upotrebu. Za javni launch pročitati ToS, ne spremati audio, davati atribuciju i linkove na pjesmu. Alternativa: royalty-free katalog.
2. **Kvaliteta analizatora:** najveći tehnički rizik, zato je M0 gate.
3. **Dostupnost previewa** za neke pjesme/regije, treba filtrirati u ingestu.
4. **Besplatni hosting limiti:** cold start, kvote baze (Supabase free 500 MB, pauzira nakon neaktivnosti). Kasnije pratiti.
5. **Curenje odgovora** kroz network tab.

## 8. Otvorena pitanja za tebe

**Riješeno:**
1. Faza 2: **1 pokušaj**, rezultat 1–100. Po rundi faza 1 do 50 + faza 2 do 50, ukupno do 500 dnevno.
2. Ista pogođena pjesma je **zabranjena** u fazi 2 (isključena iz autocompletea).
3. Sučelje: **hrvatski + engleski** (react-i18next od početka).
4. Katalog: **međunarodni hitovi + regionalna glazba**, početnih 3–5k pjesama.

5. Novi dan: **Europe/Zagreb**.
6. Supabase i Google Cloud računi postoje.
7. Naziv igre: **SongTheSame**.

**Nema otvorenih pitanja.**

## 9. Stanje nakon implementacije

Sve od M0 do M10 je implementirano i testirano. Ono što ostaje za tebe:
- Supabase projekt: connection string, `Supabase__Url`, Google provider i web varijable (vidi README, odjeljak Accounts).
- Pokrenuti `migrate`, `seed`, `calibrate`, `puzzles` na produkcijskoj bazi.
- Deploy API-ja (Cloud Run) i weba (GitHub Pages workflow), te repository secret/varijable za workflowe.
- Prije javnog lansiranja: pročitati Deezerove uvjete korištenja.

Poznata ograničenja i ideje za dalje:
- Analizator najbolje hvata harmoniju; sample/riff parovi su slabiji (harmonic-percussive separacija, CQT, bas značajka).
- Prijave rundi se spremaju u tablicu `round_reports`; admin pregled još ne postoji.
- Editor ne dopušta ručni odabir segmenta (bira se automatski najjasniji dio).
- Waveform prikaz nije implementiran (Deezer CDN i CORS); koristi se vlastiti indikator.
- Dnevne zagonetke trebaju biti unaprijed generirane (workflow `daily-puzzles.yml`), generiranje traje ~20 s po danu.
