# Documentation du projet LandiGlobalTemplate

## 1. Contexte

Application web ASP.NET Core destinée à la saisie, l’analyse et l’export de factures/produits pour un usage métier interne.

Cible : utilisateurs métiers de traitement de factures et d’analyse commerciale, avec deux responsabilités principales :
- extraction et enrichissement de données PDF de factures
- extraction / transformation de données depuis Excel/CSV

---

## 2. Architecture fonctionnelle

### Vue d’ensemble

L’application est une application web MVC ASP.NET Core (.NET 10.0) single-tenant.

Elle fonctionne comme outil autonome côté serveur et s’intègre dans l’IT via :
- base de données SQL Server LocalDB (configurée dans `appsettings.json`)
- authentification ASP.NET Identity
- export de données Excel/CSV
- notifications e-mail SMTP via `MailKit`

### Intégration IT

- source : `LandiGlobalTemplate.sln`
- backend : `Program.cs`, `Controllers/`, `Services/`, `Data/`
- UI : `Views/`
- DB : `ApplicationDbContext` + migrations EF Core
- email : configuration SMTP dans `appsettings.json`

---

## 3. Fonctionnalités principales

### 3.1 Extraction PDF

- Fonction : extraction de données métier depuis des factures PDF
- Description : upload d’un fichier PDF, extraction texte via `PDFExtractionService`, mapping vers `CommercialInvoice` / `InvoiceHistory`
- Validation : `InvoiceValidationService` vérifie email, montant, doublons, cohérence
- Enrichissement : enrichissement produit via `ProductCatalog`
- Résultat : stockage en base `InvoiceHistory`, génération éventuellement de PDF enrichi via `PDFGenerationService`

### 3.2 Extraction Excel / CSV

- Fonction : transformation de fichiers Excel/CSV métier
- Description : upload fichier Excel/CSV dans `ExcelTransformController`, transformation via `ExcelTransformService`
- Types : import types `LAN`, `STOCK`, `SELLSOUT`
- Mapping : support de fichier de mapping produit et mapping statique par défaut
- Résultat : génération d’un fichier Excel transformé téléchargeable

### 3.3 Export

- Fonction : export des données stockées
- Description : endpoints API dans `ExportController`
- Sorties : Excel (`.xlsx`) et CSV (`.csv`)
- Cas : export de factures, export de produits, export filtré par statut, client, dates

### 3.4 Authentification

- Fonction : sécurisation de l’accès
- Description : ASP.NET Identity + EF Core
- Rôles créés au démarrage : `Admin`, `Manager`, `User`, `Auditor`
- Composants : `AccountController.cs`, `ApplicationUser.cs`, `Program.cs` (`AddIdentity`, `UseAuthentication`, `UseAuthorization`)

### 3.5 Workflow

- PDF :
  1. Upload PDF
  2. Extraction via `PDFExtractionService`
  3. Validation via `InvoiceValidationService`
  4. Stockage SQL Server
  5. Option : génération PDF enrichi ou export Excel/CSV

- Excel :
  1. Upload Excel/CSV
  2. Lecture et transformation via `ExcelTransformService`
  3. Téléchargement du fichier transformé
  4. Option : utilisation de mapping produit

- Export :
  1. Appel API d’export
  2. Création du fichier via `ExportService`
  3. Retour du fichier au navigateur

### 3.6 Logs et erreurs

- Configuration : `appsettings.json`
- Niveau : `Information` par défaut, `Warning` pour `Microsoft.AspNetCore`
- Implémentation : `ILogger<T>` dans les contrôleurs et services (`ExportController`, `ExcelTransformController`, `InvoiceValidationService`, etc.)
- Emplacement : pas de fichier log spécifique configuré dans le code ; le logging suit le pipeline ASP.NET Core standard (console/hôte)
- Dépannage :
  - vérifier la sortie console/terminal de l’application
  - vérifier les erreurs HTTP affichées par l’application
  - vérifier `appsettings.json` / `appsettings.Development.json`
  - si déployé sur IIS/Kestrel, consulter les logs du serveur hôte ou Event Viewer
  - refaire l’exécution locale avec `dotnet run` pour voir les exceptions

---

## 4. Documentation utilisateur

Liens internes du projet :
- `README_DOCUMENTATION.md`
- `GUIDE_TEST_PRATIQUE.md`
- `EXAMPLES_FR.md`
- `DOCUMENTATION_INDEX.md`
- `ARCHITECTURE_FLUX_DONNEES.md`

---

## 5. Architecture technique

### Technology Stack

| Composant | Technologie | Version |
|---|---|---|
| Frontend | ASP.NET Core MVC / Razor Views | .NET 10.0 |
| Backend | ASP.NET Core Web | .NET 10.0 |
| Database | Microsoft SQL Server LocalDB | LocalDB |
| Authentification | ASP.NET Core Identity | 10.0.0 |
| Excel | EPPlus | 8.5.4 |
| Excel / CSV | ClosedXML | 0.104.1 |
| PDF | iText 7 | 7.2.5 |
| Email | MailKit / MimeKit | 4.3.0 |
| OpenAPI | Swashbuckle | 7.0.0 |
| ORM | Entity Framework Core | 10.0.0 |

### Outils et langages

- C#
- .NET 10.0 SDK
- ASP.NET Core
- Entity Framework Core
- SQL Server LocalDB
- Visual Studio / `dotnet` CLI
- MailKit pour SMTP
- OfficeOpenXml (EPPlus)
- iText pour PDF

---

## 6. Source code

Le code est dans la racine du projet :
- solution : `LandiGlobalTemplate.sln`
- projet : `LandiGlobalTemplate.csproj`
- fichiers clés :
  - `Program.cs`
  - `Controllers/InvoiceController.cs`
  - `Controllers/ExcelTransformController.cs`
  - `Controllers/ExportController.cs`
  - `Controllers/SearchController.cs`
  - `Services/PDFExtractionService.cs`
  - `Services/ExcelTransformService.cs`
  - `Services/ExportService.cs`
  - `Services/InvoiceValidationService.cs`
  - `Data/ApplicationDbContext.cs`
  - `Data/InvoiceHistory.cs`
  - `Models/`

Chemin absolu actuel : `c:\Users\khale_i3fpmfj\LandiGlobalTemplate`

---

## 7. Compilation

### Prérequis

- .NET 10.0 SDK installé
- SQL Server LocalDB accessible
- `dotnet` CLI

### Commandes

- restaurer : `dotnet restore`
- compiler : `dotnet build`
- exécuter : `dotnet run`
- publier : `dotnet publish -c Release -o ./publish`

---

## 8. Déploiement / Installation

### Mode de déploiement

Application web hébergée côté serveur, non installée comme application desktop.

### Principes

- publier avec `dotnet publish`
- déployer sur un hôte .NET 10 (Kestrel, IIS, ou container)
- configurer `appsettings.json` pour la base de données et le SMTP
- s’assurer que la base de données SQL Server est accessible
- démarrer via `dotnet LandiGlobalTemplate.dll` ou via IIS

### Points spécifiques

- `appsettings.json` définit `DefaultConnection` pour LocalDB
- `Email:SmtpServer`, `Email:SmtpPort`, `Email:Username`, `Email:Password` doivent être renseignés pour les notifications
- l’application utilise une base de données locale par défaut, mais peut pointer vers un SQL Server hébergé en production

---

## 9. Remarque métier

Les deux métiers sont gérés au sein d’une seule application :
1. Extraction de données factures PDF + enrichissement produit
2. Transformation de données Excel/CSV métier

Chaque métier dispose de sa chaîne de traitement propre, mais ils partagent la même plateforme web, les mêmes services d’authentification, et la même base de données pour le stockage / export.
