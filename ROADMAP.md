# ParkingApp — Roadmap

> Ζωντανό έγγραφο. Καταγράφει το όραμα, τι έχει γίνει, και τι απομένει.
> Ενημερώνεται καθώς προχωράμε.

---

## Τι είναι

Ένα **multi-tenant SaaS** για διαχείριση χώρων στάθμευσης (parking). Πολλές εταιρίες-πελάτες
συνυπάρχουν σε μία εφαρμογή και μία βάση, με πλήρη απομόνωση: κάθε εταιρία βλέπει και διαχειρίζεται
**μόνο τα δικά της** δεδομένα. Όλα από τον browser (Blazor WebAssembly), με ξεχωριστό Web API.

## Οι δύο κόσμοι χρήστη

### Owner — διαχειριστικό
- Ορίζει και επεξεργάζεται **branches** (παραρτήματα parking· 0, 1 ή πολλά).
- Ορίζει και επεξεργάζεται **υπαλλήλους**, ή δηλώνει ότι δουλεύει το parking μόνος του.
- Βλέπει **στατιστικά κινήσεων** από τα branches του — ξεχωριστά ανά branch ή συνολικά.

### Employee — λειτουργικό
- Κάνει είσοδο και διαχειρίζεται το ίδιο το parking, ανάλογα με το branch στο οποίο ανήκει.
- Αν ανήκει σε **δύο ή περισσότερα** branches, επιλέγει σε ποιο θα εργαστεί.
- Εκτελεί **κινήσεις parking**: είσοδος → έξοδος → χρέωση.

---

## Κατάσταση (τι έχει γίνει)

| Φέτα | Περιεχόμενο | Κατάσταση |
| --- | --- | --- |
| 1 | **Authentication** — seed owners, login, JWT (role + companyId), protected pages, logout | ✅ Ολοκληρώθηκε (end-to-end) |
| 2α | **Company profile** — `GET /api/companies/me`, tenant-scoped read, όνομα εταιρίας στο UI | ✅ Ολοκληρώθηκε |
| 2β | **Branches** — CRUD backend + UI, tenant isolation με EF Core global query filter | ✅ Ολοκληρώθηκε (end-to-end) |
| 2γ | **Floors** — σελίδα διαχείρισης branch· όροφοι nested κάτω από branch, tenant-isolated | ✅ Ολοκληρώθηκε (end-to-end) |
| 2γ+ | **Spots** — θέσεις μέσα σε κάθε όροφο· η «θέση» γίνεται κανονικό entity | ⏭️ Επόμενο |
| 2δ | **Employees** — ο owner δημιουργεί/διαχειρίζεται λογαριασμούς υπαλλήλων | 🔜 Σχεδιασμένο |
| 3 | **Shifts & Vehicle entries** — βάρδιες + κινήσεις parking (είσοδος/έξοδος/χρέωση) | 🔜 Σχεδιασμένο |
| 4 | **Στατιστικά** — dashboard κινήσεων ανά branch και συνολικά | 🔜 Σχεδιασμένο |

---

## Roadmap (τι απομένει)

### Φάση Α — Λειτουργικός σκελετός (κάθετες φέτες, καθεμία end-to-end)
- **2γ — Floors & Spots:** μέσα σε κάθε branch, ορισμός ορόφων → θέσεων → τύπου θέσης
  (μηχανή / αυτοκίνητο / μεγάλο). Η «θέση» γίνεται entity (`ParkingSpot`), όχι JSON string —
  ώστε να μπορεί να απαντηθεί «ποιες θέσεις υπάρχουν / είναι ελεύθερες».
- **2δ — Employees:** ο owner δημιουργεί λογαριασμούς υπαλλήλων (username + password),
  δεμένους στην εταιρία του και σε ένα ή περισσότερα branches.
- **3 — Shifts & Entries:** ο υπάλληλος ανοίγει/κλείνει βάρδια· καταχωρεί εισόδους/εξόδους
  αυτοκινήτων και χρεώσεις. Εδώ ξαναδουλεύεται το αρχικό `ParkingEntry` ώστε να δένει σωστά
  με branch, spot, employee και shift.

### Φάση Β — Ωράισμα & ταχύτητα (μαζεμένα, συνειδητά)
- **UI:** εισαγωγή component library (π.χ. MudBlazor ή Radzen) για συνεπές, επαγγελματικό look,
  αντικαθιστώντας τα raw HTML/CSS σημεία. Γίνεται ως ξεχωριστό βήμα, όχι μέσα στα features.
- **Performance:** τα δεδομένα σελιδοποιούνται (pagination).
  - **Custom pagination wrapper** — δικό μας, επαναχρησιμοποιήσιμο: `PagedResult<T>` (items +
    total count + page info) στο API, `Skip/Take` **στη βάση** (όχι στη μνήμη), κατανάλωση στον client.
- **Στατιστικά (Φέτα 4):** το dashboard του owner (κινήσεις ανά branch / συνολικά).

### Φάση Γ — Επέκταση & επίδειξη
- **Deployment:** το σύστημα ανεβαίνει live (π.χ. Azure / Docker) με CI — ένα προσβάσιμο link.
- **Δεύτερο API + Android app:** ένα API που τροφοδοτεί μια Android εφαρμογή, η οποία —
  βάσει των διευθύνσεων όλων των branches — δείχνει **runtime διαθεσιμότητα θέσεων** των parking.
  *(Η ακριβής αρχιτεκτονική — ξεχωριστό gateway API vs. απευθείας endpoints — θα αποφασιστεί όταν φτάσουμε.)*
- **Έτοιμο για επίδειξη.**

---

## Αρχιτεκτονικές αρχές & συμβάσεις (κατοχυρωμένες)

- **3 projects:** `Api` (Web API) / `Client` (Blazor WASM) / `Shared` (contracts). Client ↔ HTTP ↔ API.
- **Feature folders**, `ApiResponse<T>` envelope, `ActionResult<T>` typed actions.
- **Consumer** (καθαρή HTTP επικοινωνία) vs **Service** (orchestration/state) — μία ευθύνη ο καθένας.
- **MVVM με manual view-model instantiation** — per-visit state (αποφυγή stale state σε WASM).
- **Tenant isolation** μέσω `ITenantProvider` + EF Core global query filter — δομικό, όχι χειροκίνητο `WHERE`.
- **Guid PKs** με `NEWSEQUENTIALID()`, **seed-based** owners (όχι public register).
- **Secrets** εκτός repo (User Secrets), **Conventional Commits**, **English XML doc comments**.

---

## Working method

Χτίζουμε σε **κάθετες φέτες**: κάθε φέτα ολοκληρωμένη από την οθόνη μέχρι τη βάση, δοκιμασμένη,
πριν ανοίξουμε την επόμενη. Το reasoning προηγείται του κώδικα. «Shipping over starting».

## License

**MIT** — open source, ελεύθερο για χρήση και τροποποίηση.
