namespace SongTheSame.Spike;

public sealed record SpikeSong(string Key, string Query);

public sealed record LabeledPair(string QueryKey, string ReferenceKey, string Relation);

/// <summary>Hand-picked songs and the relations we expect the analyzer to recognise.</summary>
public static class SpikeCatalog
{
    public static readonly IReadOnlyList<SpikeSong> Songs =
    [
        // Related pairs
        new("queen_under_pressure", "Queen David Bowie Under Pressure"),
        new("vanilla_ice", "Vanilla Ice Ice Ice Baby"),
        new("cohen_hallelujah", "Leonard Cohen Hallelujah"),
        new("buckley_hallelujah", "Jeff Buckley Hallelujah"),
        new("nirvana_smells", "Nirvana Smells Like Teen Spirit"),
        new("boston_more", "Boston More Than a Feeling"),
        new("coldplay_viva", "Coldplay Viva La Vida"),
        new("satriani_fly", "Joe Satriani If I Could Fly"),
        new("cash_hurt", "Johnny Cash Hurt"),
        new("nin_hurt", "Nine Inch Nails Hurt"),
        new("hendrix_watchtower", "Jimi Hendrix All Along the Watchtower"),
        new("dylan_watchtower", "Bob Dylan All Along the Watchtower"),
        new("whitney_iwaly", "Whitney Houston I Will Always Love You"),
        new("dolly_iwaly", "Dolly Parton I Will Always Love You"),
        new("gaynor_survive", "Gloria Gaynor I Will Survive"),
        new("cake_survive", "Cake I Will Survive"),
        new("thicke_blurred", "Robin Thicke Blurred Lines"),
        new("gaye_giveitup", "Marvin Gaye Got to Give It Up"),
        new("eagles_hotel", "Eagles Hotel California"),
        new("gipsy_hotel", "Gipsy Kings Hotel California"),
        new("queen_bohrap", "Queen Bohemian Rhapsody"),
        new("queen_bohrap_live", "Queen Bohemian Rhapsody Live Aid"),

        // Distractors
        new("adele_rolling", "Adele Rolling in the Deep"),
        new("metallica_sandman", "Metallica Enter Sandman"),
        new("daftpunk_lucky", "Daft Punk Get Lucky"),
        new("abba_dancing", "ABBA Dancing Queen"),
        new("eminem_lose", "Eminem Lose Yourself"),
        new("billie_badguy", "Billie Eilish bad guy"),
        new("dualipa_levitating", "Dua Lipa Levitating"),
        new("oliver_cesarica", "Oliver Dragojevic Cesarica"),
        new("parni_jedina", "Parni Valjak Jedina"),
        new("severina_stikla", "Severina Moja stikla"),
        new("beatles_yesterday", "The Beatles Yesterday"),
        new("mj_billiejean", "Michael Jackson Billie Jean"),
        new("weeknd_blinding", "The Weeknd Blinding Lights"),
        new("ed_shape", "Ed Sheeran Shape of You"),
        new("acdc_blackinblack", "AC/DC Back in Black"),
        new("mozart_nachtmusik", "Mozart Eine kleine Nachtmusik"),
        new("floyd_numb", "Pink Floyd Comfortably Numb"),
        new("marley_birds", "Bob Marley Three Little Birds"),
        new("gaga_pokerface", "Lady Gaga Poker Face"),
    ];

    public static readonly IReadOnlyList<LabeledPair> Pairs =
    [
        new("queen_under_pressure", "vanilla_ice", "sample"),
        new("coldplay_viva", "satriani_fly", "plagiarism-claim"),
        new("nirvana_smells", "boston_more", "similar-riff"),
        new("thicke_blurred", "gaye_giveitup", "similar-groove"),
        new("cohen_hallelujah", "buckley_hallelujah", "cover"),
        new("cash_hurt", "nin_hurt", "cover"),
        new("hendrix_watchtower", "dylan_watchtower", "cover"),
        new("whitney_iwaly", "dolly_iwaly", "cover"),
        new("gaynor_survive", "cake_survive", "cover"),
        new("eagles_hotel", "gipsy_hotel", "cover"),
        new("queen_bohrap", "queen_bohrap_live", "alternate-recording"),
    ];
}
