/**
 * GUIDE COMPLET : FLUX DE DONNÉES - EXTRACTION → STOCKAGE → AFFICHAGE
 * 
 * Ce document explique comment les factures sont extraites, validées, stockées,
 * et affichées dans le Dashboard, Analytics et Recherche.
 */

// ========================================
// 1. EXTRACTION DE FICHIER INVOICE (PDF/CSV/Excel)
// ========================================

/*
ÉTAPE 1: L'utilisateur upload un fichier (PDF, CSV ou Excel)
         ↓
ÉTAPE 2: File upload handler dans InvoiceController.cs
         ↓
ÉTAPE 3: Extraction des données selon le format:
         - PDF: PDFExtractionService.cs
         - CSV/Excel: Parser personnalisé
         ↓
ÉTAPE 4: Données extraites = Objet InvoiceHistory + List<InvoiceHistoryProduct>
*/

// Exemple d'objet InvoiceHistory créé après extraction:
class InvoiceHistory {
    int Id;                          // Clé primaire
    string InvoiceNumber;            // "INV-2024-001"
    DateTime InvoiceDate;            // Date de la facture
    string CustomerName;             // "Acme Corp"
    string Email;                    // "contact@acme.com"
    decimal TotalAmount;             // 15,500.00
    string Currency;                 // "USD"
    string Status;                   // "Processed", "Draft", "Error"
    DateTime CreatedAt;              // Timestamp automatique
    List<InvoiceHistoryProduct> Products;  // Produits de la facture
}

// ========================================
// 2. VALIDATION DES DONNÉES
// ========================================

/*
ÉTAPE 5: Les données extraites passent par InvoiceValidationService
         ↓
ÉTAPE 6: Validation contre 12+ règles:
         - Email valide? ✓
         - Montant positif? ✓
         - Facture déjà existante? ✓
         - Produits valides? ✓
         - Dates cohérentes? ✓
         ↓
ÉTAPE 7: Si VALIDE → Status = "Processed"
         Si ERREUR → Status = "Error" + ErrorMessage
         Si INCOMPLET → Status = "Draft"
*/

// Exemple de validation:
public class InvoiceValidationService {
    public async Task<ValidationResult> ValidateInvoiceAsync(InvoiceHistory invoice)
    {
        var errors = new List<string>();
        
        // Règle 1: Email valide
        if (!IsValidEmail(invoice.Email))
            errors.Add("Email invalide");
        
        // Règle 2: Montant positif
        if (invoice.TotalAmount <= 0)
            errors.Add("Le montant doit être positif");
        
        // Règle 3: Facture unique
        var exists = await _context.InvoiceHistories
            .AnyAsync(i => i.InvoiceNumber == invoice.InvoiceNumber);
        if (exists)
            errors.Add("Facture déjà existante");
        
        // ... plus de règles
        
        return new ValidationResult 
        { 
            Success = errors.Count == 0,
            Errors = errors
        };
    }
}

// ========================================
// 3. STOCKAGE DANS LA BASE DE DONNÉES
// ========================================

/*
ÉTAPE 8: Si VALIDATION réussie:
         - InvoiceHistory → Table [InvoiceHistories]
         - InvoiceHistoryProduct[] → Table [InvoiceHistoryProducts]
         - Statut mis à jour
         - CreatedAt = DateTime.Now
         ↓
ÉTAPE 9: Base de données SQL Server (LocalDB):
         ├── InvoiceHistories
         │   ├── Id (1, 2, 3, ...)
         │   ├── InvoiceNumber ("INV-001", "INV-002", ...)
         │   ├── CustomerName ("Acme", "Widget Corp", ...)
         │   ├── TotalAmount (15500.00, 8200.50, ...)
         │   ├── Status ("Processed", "Draft", "Error")
         │   ├── CreatedAt (2026-04-22 10:30:00, ...)
         │   └── Products (Navigation → InvoiceHistoryProducts)
         │
         └── InvoiceHistoryProducts
             ├── Id
             ├── InvoiceHistoryId (Clé étrangère)
             ├── ProductNumber ("PROD-001", ...)
             ├── Quantity (5, 10, ...)
             ├── UnitPrice (100.00, 200.00, ...)
             └── TotalAmount (500.00, 2000.00, ...)
*/

// ========================================
// 4. AFFICHAGE DANS LE DASHBOARD
// ========================================

/*
ÉTAPE 10: Quand l'utilisateur visite /dashboard:
          ↓
ÉTAPE 11: DashboardController.Index() exécuté:
          - Récupère TOUTES les InvoiceHistories depuis DB
          - Calcule les statistiques:
            * Total Factures = COUNT(*)
            * Total Revenu = SUM(TotalAmount)
            * Factures Traitées = COUNT(WHERE Status='Processed')
            * Moyenne = SUM(TotalAmount) / COUNT(*)
          - Top 5 clients = GROUP BY CustomerName
          - 12 derniers mois = GROUP BY Month
          ↓
ÉTAPE 12: Données envoyées à la Vue Dashboard/Index.cshtml
          ↓
ÉTAPE 13: Chart.js affiche les graphiques interactifs
          - Ligne: Revenu par mois
          - Doughnut: Distribution des statuts
          - Table: Top clients
*/

// Code du contrôleur:
[HttpGet]
public async Task<IActionResult> Index()
{
    var invoices = await _context.InvoiceHistories
        .AsNoTracking()
        .Where(i => i.Status == "Processed")
        .ToListAsync();
    
    var stats = new DashboardViewModel
    {
        TotalInvoices = invoices.Count,
        TotalRevenue = invoices.Sum(i => i.TotalAmount),
        AveragePerInvoice = invoices.Count > 0 ? invoices.Sum(i => i.TotalAmount) / invoices.Count : 0,
        ProcessedCount = invoices.Count(i => i.Status == "Processed"),
        // ... plus de stats
    };
    
    return View(stats);
}

// ========================================
// 5. AFFICHAGE DANS ANALYTICS
// ========================================

/*
ÉTAPE 14: Quand l'utilisateur visite /analytics:
          ↓
ÉTAPE 15: AnalyticsController.GetChartData() exécuté (API endpoint)
          ↓
ÉTAPE 16: Les données sont agrégées par:
          - Mois (derniers 12 mois)
          - Statut
          - Client
          - Croissance (30 jours)
          ↓
ÉTAPE 17: JSON retourné à la Vue:
{
  "monthlyRevenue": [
    { "month": "Jan 2026", "revenue": 45000 },
    { "month": "Feb 2026", "revenue": 67500 },
    ...
  ],
  "revenueByStatus": [
    { "status": "Processed", "revenue": 150000, "count": 50 },
    { "status": "Draft", "revenue": 5000, "count": 2 },
    { "status": "Error", "revenue": 0, "count": 1 }
  ],
  "topCustomers": [
    { "customer": "Acme Corp", "email": "acme@example.com", "revenue": 75000, "invoiceCount": 15 },
    { "customer": "Widget Inc", "email": "widget@example.com", "revenue": 45000, "invoiceCount": 9 },
    ...
  ]
}
          ↓
ÉTAPE 18: Chart.js affiche les graphiques avec données en temps réel
*/

// ========================================
// 6. AFFICHAGE DANS LA RECHERCHE AVANCÉE
// ========================================

/*
ÉTAPE 19: Utilisateur visite /search/advanced
          ↓
ÉTAPE 20: Utilisateur remplit les filtres:
          - N° Facture: "INV-001"
          - Client: "Acme"
          - Montant Min: 10000
          - Montant Max: 50000
          - Statut: "Processed"
          - Date début: 2026-01-01
          ↓
ÉTAPE 21: SearchController.Advanced() construit une requête LINQ:
          
          var query = _context.InvoiceHistories.AsNoTracking();
          
          if (!string.IsNullOrEmpty(invoiceNumber))
              query = query.Where(i => i.InvoiceNumber.Contains(invoiceNumber));
          
          if (!string.IsNullOrEmpty(customerName))
              query = query.Where(i => i.CustomerName.Contains(customerName));
          
          if (minAmount.HasValue)
              query = query.Where(i => i.TotalAmount >= minAmount);
          
          if (maxAmount.HasValue)
              query = query.Where(i => i.TotalAmount <= maxAmount);
          
          if (!string.IsNullOrEmpty(status))
              query = query.Where(i => i.Status == status);
          
          if (startDate.HasValue)
              query = query.Where(i => i.CreatedAt >= startDate);
          
          ↓
ÉTAPE 22: La requête s'exécute avec PAGINATION:
          - Page 1, 20 résultats par page
          - Total = 150 factures
          - Pages = 150 / 20 = 8 pages
          ↓
ÉTAPE 23: Résultats affichés dans une table avec pagination
*/

// ========================================
// 7. NOTIFICATIONS EMAIL
// ========================================

/*
LES NOTIFICATIONS EMAIL SONT DÉCLENCHÉES À 4 MOMENTS:

1. CRÉATION DE FACTURE
   └─ User upload → Invoice créée → EmailService.SendInvoiceNotificationAsync()
      └─ Email HTML envoyé au client
      └─ Contenu: "Nouvelle facture #{InvoiceNumber} - ${TotalAmount}"

2. VALIDATION RÉUSSIE
   └─ Facture validée → EmailService.SendValidationNotificationAsync()
      └─ Email succès au user
      └─ Contenu: "Facture validée ✓"

3. VALIDATION ÉCHOUÉE
   └─ Facture invalide → EmailService.SendValidationNotificationAsync(errors)
      └─ Email d'erreur au user avec liste des erreurs
      └─ Contenu: "Erreurs: Email invalide, Montant négatif, ..."

4. EXPORT RÉUSSI
   └─ User exporte en Excel/CSV → EmailService.SendExportReportAsync()
      └─ Email avec pièce jointe (fichier Excel/CSV)
      └─ Contenu: "Export terminé - 150 factures"
*/

// Exemple d'implémentation:
public class EmailService {
    public async Task SendInvoiceNotificationAsync(InvoiceHistory invoice)
    {
        var emailBody = $@"
            <h2>Nouvelle Facture Créée</h2>
            <p>Facture N°: {invoice.InvoiceNumber}</p>
            <p>Client: {invoice.CustomerName}</p>
            <p>Montant: ${invoice.TotalAmount:F2}</p>
            <p>Date: {invoice.CreatedAt:dd/MM/yyyy}</p>
        ";
        
        await SendEmailAsync(
            to: invoice.Email,
            subject: $"Facture {invoice.InvoiceNumber}",
            body: emailBody,
            isHtml: true
        );
    }
    
    public async Task SendValidationNotificationAsync(InvoiceHistory invoice, List<string> errors)
    {
        var status = errors.Count == 0 ? "✓ Validée" : "✗ Erreurs détectées";
        
        var emailBody = $@"
            <h2>Notification de Validation</h2>
            <p>Facture N°: {invoice.InvoiceNumber}</p>
            <p>Statut: {status}</p>
            {(errors.Count > 0 ? $"<p>Erreurs:<ul>{string.Join("", errors.Select(e => $"<li>{e}</li>"))}</ul></p>" : "")}
        ";
        
        await SendEmailAsync(
            to: "admin@company.com",
            subject: $"Validation - {invoice.InvoiceNumber}",
            body: emailBody,
            isHtml: true
        );
    }
}

// ========================================
// 8. BATCH UPLOAD (IMPORT EN MASSE)
// ========================================

/*
PROCESSUS BATCH UPLOAD:

1. USER UPLOAD UN FICHIER EXCEL/CSV AVEC 100+ FACTURES
   ├─ Fichier: factures_batch.xlsx
   ├─ Contient: 150 factures
   └─ Format: Colonnes standardisées
      
2. BATCH UPLOAD HANDLER PARSE LE FICHIER
   ├─ Lecture Excel/CSV
   ├─ Extraction de chaque ligne
   └─ Création d'objets InvoiceHistory pour chaque ligne

3. VALIDATION EN MASSE (MULTI-THREADING)
   ├─ Boucle: FOR EACH invoice IN batch
   │   ├─ Exécute ValidateInvoiceAsync()
   │   ├─ Résultat: ✓ ou ✗ avec erreurs
   │   └─ Stocke le résultat dans un rapport
   └─ Résultat final:
       ├─ Valides: 145 factures
       ├─ Erreurs: 5 factures
       └─ Taux de succès: 96.7%

4. STOCKAGE EN MASSE
   ├─ Factures valides → Insérer directement en DB
   │   └─ Status = "Processed"
   ├─ Factures invalides → Stocker avec ErrorMessage
   │   └─ Status = "Error"
   └─ Utilise: DbContext.InvoiceHistories.AddRangeAsync()

5. RAPPORT DE BATCH
   ├─ Créer un BatchReport object:
   {
       BatchId: "BATCH-2026-04-22-001",
       TotalCount: 150,
       SuccessCount: 145,
       ErrorCount: 5,
       SuccessRate: 96.7%,
       CreatedAt: 2026-04-22 10:30:00,
       Errors: [
           "INV-001: Email invalide",
           "INV-002: Montant négatif",
           ...
       ]
   }
   └─ Envoyer par EMAIL à l'administrateur

6. FLUX VISUEL:
   User Upload (150 factures)
         ↓
   Parser Excel/CSV
         ↓
   Valider 150 factures en parallèle
         ↓
   Résultats: 145 ✓ + 5 ✗
         ↓
   INSERT dans DB + Créer rapport
         ↓
   Envoyer email de succès/erreurs
         ↓
   Afficher rapport au user
*/

// Exemple simplifié:
public async Task<BatchUploadResult> ProcessBatchUploadAsync(IFormFile file)
{
    var result = new BatchUploadResult();
    
    // 1. Parser le fichier
    var invoices = await ParseExcelOrCsvAsync(file);
    result.TotalCount = invoices.Count;
    
    // 2. Valider en masse
    var validationTasks = invoices.Select(i => ValidateInvoiceAsync(i));
    var validationResults = await Task.WhenAll(validationTasks);
    
    // 3. Séparer les valides des invalides
    var validInvoices = invoices.Where((i, idx) => validationResults[idx].Success).ToList();
    var invalidInvoices = invoices.Where((i, idx) => !validationResults[idx].Success).ToList();
    
    // 4. Insérer les valides en DB
    _context.InvoiceHistories.AddRange(validInvoices);
    await _context.SaveChangesAsync();
    
    // 5. Créer rapport
    result.SuccessCount = validInvoices.Count;
    result.ErrorCount = invalidInvoices.Count;
    result.SuccessRate = (validInvoices.Count * 100) / invoices.Count;
    
    // 6. Envoyer email
    await _emailService.SendBatchReportAsync(result);
    
    return result;
}

// ========================================
// 9. RÉSUMÉ DU FLUX COMPLET
// ========================================

/*
CYCLE COMPLET: EXTRACTION → VALIDATION → STOCKAGE → AFFICHAGE

┌─────────────────────────────────────────────────────────────────┐
│ 1. USER UPLOAD FILE (PDF, CSV, Excel)                           │
└──────────────────┬──────────────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────────────────────────────┐
│ 2. EXTRACTION (PDFExtractionService / Parser CSV/Excel)         │
│    Résultat: InvoiceHistory + InvoiceHistoryProduct[]           │
└──────────────────┬──────────────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────────────────────────────┐
│ 3. VALIDATION (InvoiceValidationService)                        │
│    ✓ Valide → Status = "Processed"                              │
│    ✗ Invalide → Status = "Error"                                │
└──────────────────┬──────────────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────────────────────────────┐
│ 4. STOCKAGE (SQL Server LocalDB)                                │
│    Tables:                                                       │
│    ├── InvoiceHistories                                          │
│    ├── InvoiceHistoryProducts                                    │
│    ├── AuditLogs (traçabilité)                                   │
│    └── Dashboard (statistiques en cache)                         │
└──────────────────┬──────────────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────────────────────────────┐
│ 5. AFFICHAGE (3 canaux)                                         │
│    ├── Dashboard (/dashboard)                                    │
│    │   └─ KPIs + Graphiques Chart.js                            │
│    ├── Analytics (/analytics)                                    │
│    │   └─ Statistiques avancées + Top clients                   │
│    └── Recherche (/search/advanced)                             │
│        └─ Filtrage + Pagination + Résultats                    │
└──────────────────┬──────────────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────────────────────────────┐
│ 6. NOTIFICATIONS EMAIL                                          │
│    ├── Création facture → Email client                          │
│    ├── Validation succès → Email admin                          │
│    ├── Validation erreurs → Email avec détails                  │
│    └── Export terminé → Email avec fichier                      │
└─────────────────────────────────────────────────────────────────┘
*/

// ========================================
// 10. POINTS CLÉS À RETENIR
// ========================================

/*
✓ BASE DE DONNÉES = SOURCE DE VÉRITÉ
  - Toutes les données viennent de la BD
  - Dashboard, Analytics, Recherche lisent la même BD
  - Les changements dans la BD se reflètent partout

✓ VALIDATION CENTRALISÉE
  - Avant stockage = validation stricte
  - Erreurs capturées avec détails
  - Rapports d'erreurs envoyés par email

✓ NOTIFICATIONS EMAIL AUTOMATIQUES
  - Création → Email
  - Validation → Email
  - Export → Email avec pièce jointe
  - Batch → Email avec rapport

✓ BATCH UPLOAD = TRAITEMENT EN MASSE
  - Parse le fichier
  - Valide 150+ factures en parallèle
  - Stocke les valides, rapporte les erreurs
  - Envoie rapport par email

✓ PERFORMANCE
  - Recherche = Requête SQL filtrée
  - Analytics = Agrégations SQL (GROUP BY)
  - Dashboard = Requêtes avec AsNoTracking() (lecture seule)
*/
