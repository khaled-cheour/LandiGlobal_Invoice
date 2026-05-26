/**
 * GUIDE DE TEST PRATIQUE
 * 
 * Instructions étape par étape pour tester l'application
 * Avec des valeurs concrètes à copier-coller
 */

// ========================================
// 1. LANCER L'APPLICATION
// ========================================

ÉTAPE 1: Ouvrir terminal dans VS Code
└─ Ctrl + ` (backtick)

ÉTAPE 2: Compiler et lancer
┌────────────────────────────────────────┐
│ $ dotnet build                         │
│ $ dotnet run                           │
└────────────────────────────────────────┘

Résultat attendu:
✓ "Application started. Press Ctrl+C to shut down."
✓ "Now listening on: http://localhost:5206"

ÉTAPE 3: Ouvrir navigateur
Aller à: http://localhost:5206

Vous devez voir:
┌────────────────────────────────────────┐
│ 🏠 Accueil | 📈 Dashboard | 📋 Factures│
│ 📊 Analytics | 🔍 Recherche | ℹ️ À propos│
│                                        │
│ Landi Global Template                  │
│ Commercial Invoice System              │
│                                        │
│ [Login] [Register]                     │
└────────────────────────────────────────┘

// ========================================
// 2. CRÉER UN COMPTE UTILISATEUR
// ========================================

ÉTAPE 1: Cliquer "Register"

ÉTAPE 2: Remplir le formulaire
┌────────────────────────────────────────┐
│ First Name:        Jean               │
│ Last Name:         Dupont             │
│ Email:             jean@example.com   │
│ Phone:             +33612345678       │
│ Password:          P@ssw0rd2024!      │
│ Confirm Password:  P@ssw0rd2024!      │
└────────────────────────────────────────┘

Notes:
- Password doit avoir: MIN 8 chars, 1 majuscule, 1 chiffre, 1 caractère spécial

ÉTAPE 3: Cliquer "Register"

Résultat attendu:
✓ Redirection à login
✓ Message: "Registration successful"

// ========================================
// 3. SE CONNECTER
// ========================================

ÉTAPE 1: Remplir login form
┌────────────────────────────────────────┐
│ Email:    jean@example.com            │
│ Password: P@ssw0rd2024!                │
│ Remember me: ☑ (cocher)               │
└────────────────────────────────────────┘

ÉTAPE 2: Cliquer "Login"

Résultat attendu:
✓ Redirection à Dashboard
✓ Affichage du user "Jean Dupont" en haut à droite

// ========================================
// 4. TESTER DASHBOARD
// ========================================

ÉTAPE 1: Vous êtes automatiquement sur /dashboard

Vérifie:
├─ Title: "Dashboard"
├─ 4 KPI Cards:
│  ├─ "Total Factures: 0"
│  ├─ "Revenu Total: $0.00"
│  ├─ "Factures Traitées: 0"
│  └─ "Erreurs: 0"
├─ Graphique "Revenu Mensuel" (vide car pas de données)
├─ Graphique "Distribution Statuts" (vide)
├─ Tableau "Top 5 Clients" (vide)
└─ Search box + Status filter

Résultat: ✓ Page affichée correctement (0 données = normal)

// ========================================
// 5. CRÉER DES DONNÉES DE TEST
// ========================================

MÉTHODE 1: Direct via SQL (pour tester rapidement)

ÉTAPE 1: Ouvrir SQL Server Management Studio

ÉTAPE 2: Se connecter à:
└─ Server: (localdb)\mssqllocaldb
└─ Database: LandiGlobalTemplateDb

ÉTAPE 3: Copier-coller cette requête SQL:

┌────────────────────────────────────────────────────────────────┐
│ -- Insérer facture de test                                     │
│ INSERT INTO InvoiceHistories (                                 │
│     InvoiceNumber, InvoiceDate, CustomerName, Email,          │
│     TotalAmount, Currency, Status, CreatedAt                  │
│ ) VALUES (                                                     │
│     'INV-TEST-001',                                            │
│     '2026-04-22',                                              │
│     'Acme Corporation',                                        │
│     'contact@acme.com',                                        │
│     15000.00,                                                  │
│     'USD',                                                     │
│     'Processed',                                               │
│     GETDATE()                                                  │
│ );                                                             │
│                                                                 │
│ -- Récupérer l'ID de la facture créée                          │
│ SELECT Id FROM InvoiceHistories WHERE InvoiceNumber = 'INV-TEST-001';│
│ -- Résultat: Id = 1                                            │
│                                                                 │
│ -- Insérer 3 produits                                          │
│ INSERT INTO InvoiceHistoryProducts (                           │
│     InvoiceHistoryId, ProductNumber, Quantity, UnitPrice, TotalAmount│
│ ) VALUES                                                       │
│ (1, 'PROD-A', 5, 1000.00, 5000.00),                           │
│ (1, 'PROD-B', 3, 2000.00, 6000.00),                           │
│ (1, 'PROD-C', 2, 2000.00, 4000.00);                           │
└────────────────────────────────────────────────────────────────┘

ÉTAPE 4: Exécuter (F5)

Résultat: "Rows affected: 1" + "Rows affected: 3"

// ========================================
// 6. VÉRIFIER DASHBOARD MIS À JOUR
// ========================================

ÉTAPE 1: Rafraîchir page Dashboard (F5)

Résultat attendu:
├─ "Total Factures: 1"
├─ "Revenu Total: $15,000.00"
├─ "Factures Traitées: 1"
├─ "Erreurs: 0"
├─ "Moyenne/Facture: $15,000.00"
├─ Graphique "Revenue Mensuel": Bar at April 2026 with $15,000
├─ Tableau "Top 5 Clients":
│  └─ "1. Acme Corporation - $15,000.00"
└─ Search results: "1 result found"

✓ Dashboard reflète les données de la DB!

// ========================================
// 7. TESTER SEARCH AVANCÉE
// ========================================

ÉTAPE 1: Cliquer "Recherche" dans navbar

ÉTAPE 2: Formulaire de recherche
┌────────────────────────────────────────┐
│ N° Facture: INV-TEST                   │
│ Client: Acme                           │
│ Montant Min: 10000                     │
│ Montant Max: 20000                     │
│ Statut: Processed                      │
│ [Rechercher]                           │
└────────────────────────────────────────┘

ÉTAPE 3: Cliquer "Rechercher"

Résultat attendu:
├─ "1 résultats trouvés"
├─ Table affichant:
│  ├─ N° Facture: INV-TEST-001
│  ├─ Client: Acme Corporation
│  ├─ Montant: 15,000.00 USD
│  ├─ Statut: ✓ Traité (vert)
│  └─ Date: 22/04/2026
└─ Pagination: Page 1 of 1

// ========================================
// 8. TESTER ANALYTICS
// ========================================

ÉTAPE 1: Cliquer "Analytics" dans navbar

Résultat attendu:
├─ 4 KPI Cards:
│  ├─ Total Factures: 1
│  ├─ Revenu Total: $15,000.00
│  ├─ Moyenne/Facture: $15,000.00
│  └─ Taux Succès: 100%
├─ Croissance Mensuelle (30 jours): April 2026 = $15,000
├─ Distribution Statuts: 100% Processed (pie chart)
├─ Par Devise: 100% USD
├─ Tableau Top 10 Clients:
│  └─ 1. Acme Corporation - $15,000.00 (1 facture)
└─ Auto-refresh toutes les 5 minutes

✓ Analytics affichent correctement!

// ========================================
// 9. TESTER EXPORT EXCEL
// ========================================

ÉTAPE 1: Cliquer "Export" (depuis Dashboard ou en POST via API)

OPTION A: Via Swagger (test API)
├─ Aller à http://localhost:5206/swagger
├─ Chercher "Export"
├─ Cliquer "Try it out"
├─ Cliquer "Execute"

OPTION B: Via Postman
├─ Method: GET
├─ URL: http://localhost:5206/api/export/invoices/excel
├─ Headers: Authorization: Bearer [token]
├─ Cliquer "Send"

Résultat attendu:
├─ Fichier "invoices.xlsx" téléchargé
├─ Ouvrir le fichier:
│  ├─ Sheet 1 "Factures": Colonnes avec INV-TEST-001, Acme, 15000
│  ├─ Sheet 2 "Produits": 3 produits listés
│  └─ Sheet 3 "Résumé": Statistiques
├─ Formatage: Headers en gras, couleur verte, nombre à 2 décimales
└─ Curseur auto-ajusté

✓ Excel généré correctement!

// ========================================
// 10. TESTER EXPORT CSV
// ========================================

ÉTAPE 1: GET /api/export/invoices/csv

Résultat attendu:
├─ Fichier "invoices.csv" téléchargé
├─ Ouvrir avec Notepad:
│  └─ Contenu:
│     InvoiceNumber,CustomerName,Email,TotalAmount,Currency,Status,CreatedAt
│     INV-TEST-001,Acme Corporation,contact@acme.com,15000,USD,Processed,2026-04-22...

✓ CSV généré correctement!

// ========================================
// 11. AJOUTER PLUS DE DONNÉES DE TEST
// ========================================

Réexécuter la requête SQL d'insertion mais avec autres données:

┌────────────────────────────────────────────────────────────────┐
│ INSERT INTO InvoiceHistories (...) VALUES                      │
│ ('INV-TEST-002', '2026-04-22', 'Widget Inc', 'sales@widget.com', 8500, 'USD', 'Processed', GETDATE()),│
│ ('INV-TEST-003', '2026-04-22', 'Tech Corp', 'billing@tech.com', 22000, 'EUR', 'Processed', GETDATE()),│
│ ('INV-TEST-004', '2026-04-21', 'Premier Ltd', 'info@premier.com', 5000, 'GBP', 'Draft', GETDATE()),│
│ ('INV-TEST-005', '2026-04-20', 'Global Traders', 'contact@global.com', 12000, 'USD', 'Error', GETDATE());│
└────────────────────────────────────────────────────────────────┘

Puis pour chaque facture, insérer ses produits aussi.

// ========================================
// 12. TESTER AVEC 5 FACTURES
// ========================================

Après avoir 5 factures en DB, tester:

✓ Dashboard:
  ├─ Total: 5 factures
  ├─ Revenu: $62,500.00
  ├─ Moyenne: $12,500.00
  └─ Graphiques multi-color

✓ Analytics:
  ├─ 3 mois de données (avril-mai-juin)
  ├─ Distribution: 60% Processed, 20% Draft, 20% Error
  ├─ Top 3 clients
  └─ Taux succès: 60%

✓ Search:
  ├─ Filtrer par "Tech": 1 résultat
  ├─ Filtrer par "EUR": 1 résultat
  ├─ Filtrer par montant 10000-25000: 2 résultats
  └─ Tester pagination

✓ Export:
  ├─ Excel: 5 factures + 15 produits (si 3 chacun)
  └─ CSV: même contenu

// ========================================
// 13. TESTER ERREURS (VALIDATION)
// ========================================

ÉTAPE 1: Insérer une facture INVALIDE

┌────────────────────────────────────────────────────────────────┐
│ INSERT INTO InvoiceHistories (...) VALUES                      │
│ ('INV-ERROR-001', '2026-04-22', 'Bad Corp', 'INVALID_EMAIL', -500, 'USD', 'Error', GETDATE());│
│                                                                 │
│ INSERT INTO InvoiceHistories (...) VALUES                      │
│ ('INV-TEST-001', '2026-04-22', 'Dup Corp', 'dup@example.com', 5000, 'USD', 'Processed', GETDATE());│
│ -- Résultat: Erreur "Duplicate key" (facture déjà existante)  │
└────────────────────────────────────────────────────────────────┘

ÉTAPE 2: Vérifier que Dashboard montre:
├─ ErrorCount: 1 (augmente)
├─ Taux Succès: < 100%
└─ Distribution: 50% Processed, 50% Error (exemple)

✓ Validation et statut d'erreur fonctionnent!

// ========================================
// 14. TESTER AUTHENTIFICATION
// ========================================

ÉTAPE 1: Cliquer "Logout" en haut à droite

Résultat: Redirection à page d'accueil sans login

ÉTAPE 2: Essayer accéder /dashboard directement
URL: http://localhost:5206/dashboard

Résultat:
✓ Redirection à /login (pas autorisé)

ÉTAPE 3: Essayer accéder API sans token
URL: http://localhost:5206/api/export/invoices/excel

Résultat:
✓ Response 401 Unauthorized (protection OK)

ÉTAPE 4: Se reconnecter et tester

// ========================================
// 15. VÉRIFIER LES LOGS
// ========================================

ÉTAPE 1: Vérifier sortie console (où dotnet run)

Résultat attendu:
├─ "info: Microsoft.EntityFrameworkCore.Infrastructure..."
├─ "info: Microsoft.EntityFrameworkCore.Database.Command..."
├─ SQL queries exécutées
└─ "info: Microsoft.AspNetCore.Hosting.Hosting.Starting HTTP..."

ÉTAPE 2: Chercher erreurs (rouge):
- S'il y a d'erreurs, noter les messages

// ========================================
// 16. TESTER BASE DE DONNÉES
// ========================================

ÉTAPE 1: Vérifier la structure

┌────────────────────────────────────────────────────────────────┐
│ -- Vérifier tables créées                                      │
│ SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES               │
│ WHERE TABLE_SCHEMA = 'dbo';                                    │
│                                                                 │
│ -- Résultat attendu:                                           │
│ InvoiceHistories                                               │
│ InvoiceHistoryProducts                                         │
│ AspNetUsers                                                    │
│ AspNetRoles                                                    │
│ AuditLogs                                                      │
│ Dashboards                                                     │
│ __EFMigrationsHistory                                          │
└────────────────────────────────────────────────────────────────┘

ÉTAPE 2: Vérifier les données

┌────────────────────────────────────────────────────────────────┐
│ -- Compter factures                                            │
│ SELECT COUNT(*) as TotalInvoices FROM InvoiceHistories;        │
│ -- Résultat: 5 (ou nombre attendu)                             │
│                                                                 │
│ -- Compter produits                                            │
│ SELECT COUNT(*) as TotalProducts FROM InvoiceHistoryProducts;  │
│ -- Résultat: 15 (ou 5 * 3 si 3 produits chacun)              │
│                                                                 │
│ -- Lister utilisateurs                                         │
│ SELECT UserName, FirstName, LastName FROM AspNetUsers;        │
│ -- Résultat: jean@example.com (user créé)                     │
│                                                                 │
│ -- Lister roles                                                │
│ SELECT Id, Name FROM AspNetRoles;                              │
│ -- Résultat: Admin, Manager, User, Auditor                    │
└────────────────────────────────────────────────────────────────┘

// ========================================
// CHECKLIST DE TEST FINAL
// ========================================

✓ Application lance sans erreurs (http://localhost:5206)
✓ Registration crée un user
✓ Login fonctionne
✓ Dashboard affiche statistics correctes
✓ Analytics affiche graphiques et agrégations
✓ Search avancée filtre correctement
✓ Export Excel génère fichier valide
✓ Export CSV génère fichier valide
✓ Authentification protège les routes (401 sans login)
✓ Base de données contient les données correctes
✓ Pagination fonctionne avec 10+ factures
✓ Aucune erreur en console (sauf warnings)

Si TOUS ✓ → Application PRÊTE POUR PRODUCTION!

// ========================================
// PROCHAINES ÉTAPES APRÈS TEST
// ========================================

1. Déployer à Azure (voir PROCHAINES_ETAPES.md)
2. Configurer email Gmail (voir PROCHAINES_ETAPES.md)
3. Implémenter batch upload (FEATURE PRIORITY)
4. Ajouter tests automatisés
5. Monitorer performance
6. Sécuriser pour production

BON TEST! 🧪
