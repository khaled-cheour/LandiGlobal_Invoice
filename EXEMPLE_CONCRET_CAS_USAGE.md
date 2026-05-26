/**
 * EXEMPLE CONCRET : CAS D'UTILISATION RÉELLE
 * 
 * Scénario: Une entreprise reçoit une facture PDF, puis upload 150 factures en batch
 * Nous allons suivre ce qui se passe étape par étape.
 */

// ========================================
// SCÉNARIO 1 : EXTRACTION D'UNE FACTURE PDF
// ========================================

/*
ÉTAPE 1: Un user clique sur "Upload Invoice"
         └─ Sélectionne: facture_acme_001.pdf (2MB)

ÉTAPE 2: PDFExtractionService.ExtractInvoiceDataAsync() est appelé
         ├─ Ouvre le PDF
         ├─ Extrait les données OCR/Texte:
         │  ├─ Invoice N°: "INV-ACME-2026-001"
         │  ├─ Date: "22/04/2026"
         │  ├─ Client: "Acme Corporation"
         │  ├─ Email: "billing@acme.com"
         │  ├─ Produits:
         │  │  ├─ PROD-A (Qty: 5, Unit Price: 1000.00, Total: 5000.00)
         │  │  ├─ PROD-B (Qty: 3, Unit Price: 2000.00, Total: 6000.00)
         │  │  └─ PROD-C (Qty: 2, Unit Price: 500.00, Total: 1000.00)
         │  └─ Total Montant: 12,000.00 USD

ÉTAPE 3: Objet créé en mémoire:
*/

var extractedInvoice = new InvoiceHistory
{
    InvoiceNumber = "INV-ACME-2026-001",
    InvoiceDate = new DateTime(2026, 4, 22),
    CustomerName = "Acme Corporation",
    Email = "billing@acme.com",
    TotalAmount = 12000.00m,
    Currency = "USD",
    Status = "Processed",  // À déterminer après validation
    CreatedAt = DateTime.Now,
    Products = new List<InvoiceHistoryProduct>
    {
        new() { ProductNumber = "PROD-A", Quantity = 5, UnitPrice = 1000, TotalAmount = 5000 },
        new() { ProductNumber = "PROD-B", Quantity = 3, UnitPrice = 2000, TotalAmount = 6000 },
        new() { ProductNumber = "PROD-C", Quantity = 2, UnitPrice = 500, TotalAmount = 1000 }
    }
};

/*
ÉTAPE 4: Validation contre les 12 règles
*/

// Règle 1: Email valide?
ValidateResult: "billing@acme.com" ✓ Email valide

// Règle 2: Montant positif?
ValidateResult: 12000.00 ✓ Montant valide (> 0)

// Règle 3: Facture déjà existante?
SELECT COUNT(*) FROM InvoiceHistories WHERE InvoiceNumber = "INV-ACME-2026-001"
ValidateResult: 0 ✓ Facture unique

// Règle 4: Produits valides?
∑(Product.TotalAmount) = 5000 + 6000 + 1000 = 12000 ✓ Total cohérent

// Règle 5: Dates cohérentes?
InvoiceDate (22/04/2026) <= CreatedAt (22/04/2026 10:30) ✓ Cohérent

// Résultat final:
ValidationResult.Success = true
ValidationResult.Errors = []

/*
ÉTAPE 5: Status déterminé
*/
Status = "Processed"  ✓ Car validation réussie

/*
ÉTAPE 6: Insertion en base de données
*/

// Requête SQL:
INSERT INTO InvoiceHistories (
    InvoiceNumber, InvoiceDate, CustomerName, Email, 
    TotalAmount, Currency, Status, CreatedAt
) VALUES (
    'INV-ACME-2026-001', '2026-04-22', 'Acme Corporation', 'billing@acme.com',
    12000.00, 'USD', 'Processed', GETDATE()
);

// Résultat: Id = 1 (première facture)

// Insertion des produits (3 lignes):
INSERT INTO InvoiceHistoryProducts 
    (InvoiceHistoryId, ProductNumber, Quantity, UnitPrice, TotalAmount)
VALUES 
    (1, 'PROD-A', 5, 1000, 5000),
    (1, 'PROD-B', 3, 2000, 6000),
    (1, 'PROD-C', 2, 500, 1000);

/*
ÉTAPE 7: Email de notification envoyé
*/
EmailService.SendInvoiceNotificationAsync(extractedInvoice);

// Email reçu par billing@acme.com:
/*
┌─────────────────────────────────────────┐
│ 📧 Nouvelle Facture Créée               │
│                                         │
│ Facture N°: INV-ACME-2026-001          │
│ Client: Acme Corporation                │
│ Montant: $12,000.00                     │
│ Date: 22/04/2026                        │
│ Statut: ✓ Traité                        │
│                                         │
│ Détails Produits:                       │
│ - PROD-A (5 x $1000) = $5,000.00      │
│ - PROD-B (3 x $2000) = $6,000.00      │
│ - PROD-C (2 x $500)  = $1,000.00      │
└─────────────────────────────────────────┘
*/

// ========================================
// SCÉNARIO 2 : VISUALISATION DANS DASHBOARD
// ========================================

/*
Après 1 heure, l'entreprise a reçu 5 factures au total:
- INV-ACME-2026-001: 12,000.00 USD ✓ Processé
- INV-ACME-2026-002: 8,500.00 USD ✓ Processé
- INV-WIDGET-2026-001: 15,000.00 USD ✓ Processé
- INV-TECH-2026-001: 22,000.00 USD ✓ Processé
- INV-ERROR-2026-001: Email invalide ✗ Erreur

User visite /dashboard
*/

// DashboardController.Index() exécuté:

var allInvoices = await _context.InvoiceHistories.ToListAsync();
// Result: 5 factures chargées en mémoire

var statistics = new DashboardViewModel
{
    TotalInvoices = 5,                        // COUNT(*)
    TotalRevenue = 57500.00m,                 // 12000 + 8500 + 15000 + 22000
    ProcessedCount = 4,                       // WHERE Status='Processed'
    ErrorCount = 1,                           // WHERE Status='Error'
    AveragePerInvoice = 11500.00m,           // 57500 / 5
    
    TopCustomers = new[]
    {
        new { Customer = "Tech Corp", Revenue = 22000.00, Invoices = 1 },
        new { Customer = "Acme Corp", Revenue = 12000.00, Invoices = 1 },
        new { Customer = "Widget Inc", Revenue = 15000.00, Invoices = 1 },
        new { Customer = "Acme Corp", Revenue = 8500.00, Invoices = 1 },
    },
    
    MonthlySales = new[]
    {
        new { Month = "April 2026", Amount = 57500.00, Count = 4 }
    }
};

/*
Dashboard affiche:
┌────────────────────────────────────────────────────────────┐
│ 📊 Dashboard Landi Global                                  │
├────────────────────────────────────────────────────────────┤
│                                                            │
│  Factures Total: 5       Revenu Total: $57,500.00         │
│  Traitées: 4             Moyenne/Facture: $11,500.00      │
│  Erreurs: 1              Taux Succès: 80%                 │
│                                                            │
│  📈 Revenu Mensuel:                                        │
│     April 2026: $57,500.00 ▓▓▓▓▓▓▓▓▓▓ 100%              │
│                                                            │
│  🏆 Top 5 Clients:                                         │
│     1. Tech Corp        $22,000.00 (1 facture)           │
│     2. Widget Inc       $15,000.00 (1 facture)           │
│     3. Acme Corp        $12,000.00 (1 facture)           │
│     4. Acme Corp        $8,500.00  (1 facture)           │
│                                                            │
│  🔵 Distribution Statuts:                                  │
│     Processé (80%)      ████████░░                        │
│     Erreur (20%)        ██░░░░░░░░                        │
└────────────────────────────────────────────────────────────┘
*/

// ========================================
// SCÉNARIO 3 : RECHERCHE AVANCÉE
// ========================================

/*
User visite /search/advanced et remplit:
- N° Facture: "ACME"
- Montant Min: 8000
- Montant Max: 15000
- Statut: "Processed"
*/

// SearchController.Advanced() construit la requête:

var query = _context.InvoiceHistories
    .AsNoTracking()
    .Where(i => i.InvoiceNumber.Contains("ACME"))
    .Where(i => i.TotalAmount >= 8000)
    .Where(i => i.TotalAmount <= 15000)
    .Where(i => i.Status == "Processed");

// SQL généré:
/*
SELECT * FROM InvoiceHistories
WHERE InvoiceNumber LIKE '%ACME%'
  AND TotalAmount >= 8000
  AND TotalAmount <= 15000
  AND Status = 'Processed'
ORDER BY CreatedAt DESC
*/

// Résultats:
var results = await query.ToListAsync();
/*
Résultats (2 factures):
1. INV-ACME-2026-001: $12,000.00 ✓ Traité
2. INV-ACME-2026-002: $8,500.00  ✓ Traité

Pagination: Page 1/1
*/

// ========================================
// SCÉNARIO 4 : BATCH UPLOAD (150 FACTURES)
// ========================================

/*
L'administrateur upload "factures_batch_april.xlsx" contenant 150 factures

Le fichier contient:
├─ INV-001: $5,000.00, Client: Acme, Email: acme@company.com ✓
├─ INV-002: $7,500.00, Client: Widget, Email: widget@company.com ✓
├─ INV-003: $10,000.00, Client: Tech, Email: INVALID_EMAIL ✗ Email invalide
├─ INV-004: $-500.00, Client: Error, Email: error@company.com ✗ Montant négatif
├─ ... (146 factures de plus)
└─ Etc.

Étape 1: Parser le fichier Excel
*/

var parsedInvoices = ExcelParser.ParseInvoices(file);
// Result: List<InvoiceHistory> avec 150 items

/*
Étape 2: Valider les 150 factures en parallèle (multi-threading)
*/

var validationTasks = parsedInvoices.Select(i => _validationService.ValidateInvoiceAsync(i));
var validationResults = await Task.WhenAll(validationTasks);

// Résultats de validation:
/*
✓ Factures valides: 145 (96.7%)
✗ Factures invalides: 5 (3.3%)
  - INV-003: Email invalide
  - INV-004: Montant négatif
  - INV-045: Facture déjà existante
  - INV-082: Format invalide
  - INV-150: TVA invalide
*/

/*
Étape 3: Insérer en masse les valides (avec transaction)
*/

var validInvoices = parsedInvoices
    .Where((_, idx) => validationResults[idx].Success)
    .ToList();

_context.InvoiceHistories.AddRange(validInvoices);
await _context.SaveChangesAsync();

// SQL exécuté (INSERT en masse):
/*
INSERT INTO InvoiceHistories 
    (InvoiceNumber, CustomerName, Email, TotalAmount, Currency, Status, CreatedAt)
VALUES
    ('INV-001', 'Acme', 'acme@company.com', 5000, 'USD', 'Processed', GETDATE()),
    ('INV-002', 'Widget', 'widget@company.com', 7500, 'USD', 'Processed', GETDATE()),
    ... (145 lignes au total)
*/

/*
Étape 4: Créer rapport de batch
*/

var batchReport = new BatchUploadReport
{
    BatchId = "BATCH-2026-04-22-001",
    FileName = "factures_batch_april.xlsx",
    TotalCount = 150,
    SuccessCount = 145,
    ErrorCount = 5,
    SuccessRate = 96.7m,
    CreatedAt = DateTime.Now,
    Errors = new[]
    {
        "INV-003: Email invalide",
        "INV-004: Montant négatif",
        "INV-045: Facture déjà existante",
        "INV-082: Format invalide",
        "INV-150: TVA invalide"
    }
};

/*
Étape 5: Envoyer email du rapport
*/

EmailService.SendBatchReportAsync(batchReport);

// Email reçu par admin@company.com:
/*
┌────────────────────────────────────────────────┐
│ 📊 Rapport Batch Upload                        │
│                                                │
│ Batch ID: BATCH-2026-04-22-001                 │
│ Fichier: factures_batch_april.xlsx             │
│ Date: 22/04/2026 11:45:00                      │
│                                                │
│ 📈 Résultats:                                   │
│ • Total Factures: 150                          │
│ • Succès: 145 (96.7%) ✓                        │
│ • Erreurs: 5 (3.3%) ✗                          │
│                                                │
│ ❌ Factures en Erreur:                          │
│ • INV-003: Email invalide                      │
│ • INV-004: Montant négatif                     │
│ • INV-045: Facture déjà existante              │
│ • INV-082: Format invalide                     │
│ • INV-150: TVA invalide                        │
│                                                │
│ Les 145 factures valides sont maintenant       │
│ disponibles dans le Dashboard.                 │
│                                                │
│ Veuillez corriger les 5 factures en erreur     │
│ et relancer le batch.                          │
└────────────────────────────────────────────────┘
*/

// ========================================
// SCÉNARIO 5 : VISUALISATION DANS ANALYTICS
// ========================================

/*
Après le batch upload, l'admin visite /analytics
*/

// AnalyticsController.GetChartData() exécuté:

var chartData = new
{
    monthlyRevenue = new[]
    {
        new { month = "Jan 2026", revenue = 25000 },
        new { month = "Feb 2026", revenue = 35000 },
        new { month = "Mar 2026", revenue = 42000 },
        new { month = "Apr 2026", revenue = 897500 },  // Batch boost!
    },
    
    revenueByStatus = new[]
    {
        new { status = "Processed", revenue = 897500, count = 149, percentage = 98.0m },
        new { status = "Draft", revenue = 0, count = 1, percentage = 0.7m },
        new { status = "Error", revenue = 0, count = 1, percentage = 0.7m }
    },
    
    topCustomers = new[]
    {
        new { customer = "Tech Corp", revenue = 150000, invoiceCount = 8 },
        new { customer = "Acme Corp", revenue = 125000, invoiceCount = 15 },
        new { customer = "Widget Inc", revenue = 98000, invoiceCount = 12 },
        new { customer = "Premier Ltd", revenue = 75000, invoiceCount = 7 },
        new { customer = "Global Traders", revenue = 62000, invoiceCount = 5 }
    },
    
    totalTransactions = new
    {
        total = 152,
        processed = 149,
        draft = 1,
        error = 1,
        successRate = 98.0m
    }
};

/*
Analytics affiche:
┌─────────────────────────────────────────────────────────┐
│ 📊 Analytics Avancées                                   │
├─────────────────────────────────────────────────────────┤
│                                                         │
│ Total: 152 factures    Revenu: $897,500.00            │
│ Moyenne: $5,904.61     Taux Succès: 98%               │
│                                                         │
│ 📈 Revenu Mensuel (12 mois):                           │
│    Jan ████     Feb ██████   Mar ███████  Apr ████... │
│    $25k         $35k        $42k         $897k        │
│                                                         │
│ 🏆 Top 5 Clients:                                       │
│ 1. Tech Corp          $150,000.00 (8 factures)        │
│ 2. Acme Corp          $125,000.00 (15 factures)       │
│ 3. Widget Inc         $98,000.00  (12 factures)       │
│ 4. Premier Ltd        $75,000.00  (7 factures)        │
│ 5. Global Traders     $62,000.00  (5 factures)        │
│                                                         │
│ 📌 Distribution Statuts:                                │
│ ✓ Processed: 149 (98%)    ████████████████░           │
│ ⚠ Draft: 1 (0.7%)         █                           │
│ ✗ Error: 1 (0.7%)         █                           │
└─────────────────────────────────────────────────────────┘
*/

// ========================================
// RÉSUMÉ : CE QUI S'EST PASSÉ EN 1 HEURE
// ========================================

/*
Chronologie complète:

10:00 - User upload 1 facture PDF
        └─ Extraction → Validation → Stockage (1 enregistrement)
        └─ Email notification envoyé

10:30 - Admin upload 150 factures Excel (batch)
        └─ Parsing → Validation parallèle → Stockage (145 enregistrements)
        └─ Rapport batch → Email avec détails

10:45 - Dashboard mis à jour automatiquement
        └─ TotalInvoices: 152 (inclut le 1 du matin + 150 batch + 1 erreur)
        └─ TotalRevenue: $897,500.00
        └─ Top clients: Maintenant 5 à la place de 3

11:00 - Analytics mis à jour
        └─ Revenu mensuel avril: Augmente énormément!
        └─ Top clients: Nouveaux clients du batch

11:15 - User utilise Recherche Avancée
        └─ Peut filtrer par client, montant, date, statut
        └─ Résultats instantanés

POINT CLÉS:
✓ Dashboard = Vue en temps réel de la BD
✓ Analytics = Agrégations de la BD
✓ Recherche = Filtrage de la BD
✓ Email = Notifications automatiques déclenchées
✓ Batch = Traitement en masse avec rapport
*/
