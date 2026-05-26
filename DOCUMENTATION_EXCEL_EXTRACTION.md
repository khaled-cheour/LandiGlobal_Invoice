# Documentation : Extraction et Transformation Excel (LAN, STOCK, SELLSOUT)

## 1. Contexte

Ce document décrit le traitement des imports Excel/CSV pour les trois types métier pris en charge par l'application : `LAN`, `STOCK`, `SELLSOUT` (SELLS OUT). Il précise les colonnes d'entrée attendues, les colonnes supplémentaires à générer, les règles de transformation, les validations, les exemples et les erreurs courantes.

---

## 2. Vue d'ensemble

- Point d'entrée : `ExcelTransformController.Upload` (contrôleur MVC)
- Service de traitement : `ExcelTransformService.TransformAsync`
- Bibliothèque : EPPlus (OfficeOpenXml)
- Objectif : lire le fichier source, appliquer mappings et règles métier, produire un fichier Excel transformé téléchargeable.

---

## 3. Workflow général

1. L'utilisateur upload un fichier Excel/CSV via l'interface `ExcelTransform`.
2. Le contrôleur envoie le fichier à `ExcelTransformService` avec le paramètre `ImportType` (LAN/ STOCK/ SELLSOUT) et un éventuel fichier de `mapping`.
3. Le service :
   - lit la première feuille
   - identifie les en-têtes sources
   - applique le mapping (fichier de mapping si fourni, sinon mapping statique par défaut)
   - transforme les lignes selon les règles par `ImportType`
   - enrichit les lignes (lookup catalogue si nécessaire)
   - génère un fichier Excel en sortie et stocke le binaire en session pour le téléchargement
4. Résultat : feuille `Transformed` avec en-têtes cibles et lignes transformées.

---

## 4. Règles communes de validation

- Vérifier la présence d'en-têtes minimales requises pour chaque import type.
- Ignorer les lignes entièrement vides.
- Valider les types numériques (quantité, prix) et normaliser séparateurs décimaux (',' → '.')
- Trim et normaliser les chaînes (remove non-printables)
- Journaliser les lignes rejetées et raisons via `ILogger`.

---

## 5. Imports spécifiques et colonnes

Pour chaque `ImportType`, la table ci-dessous décrit :
- Colonnes d'entrée fréquentes
- Colonnes cibles attendues (à ajouter si absent)
- Règles de transformation / enrichissement

### 5.1 LAN (Point de vente / vente locale)

Colonnes source fréquentes:
- `Item No` | `SKU` | `Product Number`
- `Description`
- `Qty` | `Quantity`
- `Unit Price` | `Price`
- `Invoice Number` (optionnel)
- `Date` | `Invoice Date`
- `Store` | `Location`

Colonnes cibles à générer / ajouter:
- `ProductNumber` (normalisé)
- `FamilyName` (lookup via `ProductCatalog` si possible)
- `ProductDescription` (source si fournie sinon catalogue)
- `Quantity` (int)
- `UnitPrice` (decimal, dot decimal)
- `TotalAmount` = `Quantity` * `UnitPrice`
- `ImportType` = `LAN`
- `ProcessedAt` (timestamp)

Règles:
- Si `Quantity` manquante et `TotalAmount` présent, calculer `Quantity` = `TotalAmount` / `UnitPrice` si possible.
- Arrondir `UnitPrice` et `TotalAmount` à 2 décimales.
- Rejeter la ligne si `ProductNumber` absent ou non valide (selon `ProductCatalog` si activé).

### 5.2 STOCK

Colonnes source fréquentes:
- `SKU` | `Item No`
- `Warehouse` | `Store`
- `StockQty` | `Available`
- `OnOrder` | `Incoming`
- `ReorderLevel`
- `LastUpdated`

Colonnes cibles à générer / ajouter:
- `ProductNumber`
- `WarehouseCode`
- `StockQuantity` (int)
- `AvailableToSell` (int)
- `ReorderLevel` (int)
- `SafetyStock` (optionnel, calculé)
- `ImportType` = `STOCK`
- `ProcessedAt`

Règles:
- Normaliser les colonnes numériques à int.
- Calculer `AvailableToSell` = `StockQuantity` - `Reserved` (si colonne réservée fournie) ou `StockQty`.
- Si `ReorderLevel` absent, laisser vide ou utiliser valeur par défaut du mapping.
- Valider que `Warehouse` correspond à un code connu si listé dans mapping.

### 5.3 SELLSOUT (SELLS OUT)

Colonnes source fréquentes:
- `Item` | `SKU`
- `SoldQty` | `QuantitySold`
- `SaleDate` | `Date`
- `RetailPrice` | `UnitPrice`
- `POS` | `Store`

Colonnes cibles à générer / ajouter:
- `ProductNumber`
- `QuantitySold` (int)
- `UnitPrice` (decimal)
- `SalesAmount` = `QuantitySold` * `UnitPrice`
- `ImportType` = `SELLSOUT`
- `Period` (YYYY-MM) calculé depuis `SaleDate`
- `ProcessedAt`

Règles:
- Agréger par `ProductNumber` + `Period` si option d’agrégation activée.
- Normaliser la date en `yyyy-MM-dd`.
- Rejeter les lignes où `QuantitySold` <= 0.

---

## 6. Mapping et fichier de mapping

- Le service accepte un fichier de mapping facultatif (Excel) avec au minimum une colonne `Item No` et éventuellement des colonnes cibles (ex: `FamilyName`, `WarehouseCode`, `DefaultReorderLevel`).
- Si mapping fourni : prioriser valeurs du mapping pour l’enrichissement.
- Sinon : utiliser le mapping statique embarqué (`GetDefaultProductMappings()` dans `ExcelTransformService`).

Structure recommandée pour le fichier de mapping (feuille 1):
- `Item No` | `FamilyName` | `Family` | `WarehouseCode` | `DefaultReorderLevel` | `Notes`

---

## 7. Enrichissement produit

- Si `ProductCatalog` est disponible, tenter d’enrichir `FamilyName`, `HSCode`, `Model`, `Platform`.
- Enrichissement conditionnel : ne pas écraser une `Description` fournie dans le fichier source si elle est non vide, sauf option `forceCatalog` activée.
- Pour les imports `STOCK` et `SELLSOUT`, utiliser l’enrichissement pour harmoniser le `ProductNumber` et la `FamilyName`.

---

## 8. Erreurs courantes & dépannage

- Erreur: "Aucune feuille Excel valide" → vérifier que la feuille n’est pas vide et que la première ligne contient des en-têtes.
- Erreur: valeurs numériques avec virgule → normaliser séparateurs décimaux (service gère "," → ".").
- Lignes rejetées → journalisées via `ILogger`; consulter la console ou logs d’hôte.
- Mapping non appliqué → vérifier que le fichier de mapping utilise exactement l’en-tête `Item No`.

---

## 9. Exemples

### Exemple LAN (entrée) :
```
Item No, Description, Qty, Unit Price, Invoice Number, Date, Store
6008M-00--, Warranty ext, 110, 162.50, INV-2026-001, 2026-04-21, STORE1
```
### Exemple LAN (sortie attendue) :
```
ProductNumber, FamilyName, ProductDescription, Quantity, UnitPrice, TotalAmount, ImportType, ProcessedAt
6008M-00--, M20, Warranty ext (catalog), 110, 162.50, 17875.00, LAN, 2026-04-22T09:30:00Z
```

### Exemple STOCK (entrée) :
```
SKU, Warehouse, StockQty, OnOrder, ReorderLevel
6008M-00--, WH1, 200, 50, 20
```
### Exemple STOCK (sortie attendue) :
```
ProductNumber, WarehouseCode, StockQuantity, AvailableToSell, ReorderLevel, ImportType, ProcessedAt
6008M-00--, WH1, 200, 200, 20, STOCK, 2026-04-22T09:30:00Z
```

### Exemple SELLSOUT (entrée) :
```
SKU, SoldQty, SaleDate, RetailPrice, POS
6008M-00--, 15, 2026-04-01, 162.50, STORE1
```
### Exemple SELLSOUT (sortie attendue) :
```
ProductNumber, QuantitySold, UnitPrice, SalesAmount, Period, ImportType, ProcessedAt
6008M-00--, 15, 162.50, 2437.50, 2026-04, SELLSOUT, 2026-04-22T09:30:00Z
```

---

## 10. Tests & validation

- Cas unitaires : parser d’en-têtes, conversion décimale, calcul `TotalAmount`, mapping.
- Cas d’intégration : upload d’un fichier d’exemple pour chaque `ImportType` et vérification du fichier transformé.
- Tests manuels recommandés : fichiers avec virgules comme séparateur décimal, en-têtes manquantes, lignes vides, mapping partiel.

---

## 11. Fichiers / Code concernés

- `Controllers/ExcelTransformController.cs`
- `Services/ExcelTransformService.cs`
- `Models/ExcelTransformModels.cs` (ou équivalent)
- `Views/ExcelTransform/Index.cshtml`, `Result.cshtml`

---

## 12. Déploiement & exécution

- Le processus est identique au reste de l’application : build et run via `dotnet build` / `dotnet run`.
- Pour tester localement : exécuter l’app et accéder à la page Excel Transform puis uploader les fichiers d’exemple.

---

## 13. Prochaines améliorations suggérées

1. Ajouter une UI de mapping dynamique (drag & drop) pour associer colonnes source → colonnes cibles.
2. Persister le mapping dans la base pour chaque client.
3. Ajouter des rapports d’erreurs téléchargeables (CSV des lignes rejetées).
4. Option d’agrégation côté SELLSOUT (par période/store).
5. Remplacer les mappings statiques par une table `ProductMappings` en base et API CRUD.

---

## 14. Conclusion

Ce document couvre les règles et attentes pour l’import Excel/CSV pour `LAN`, `STOCK` et `SELLSOUT`. Il peut être utilisé tel quel pour guider l’implémentation des colonnes nouvelles et la validation des fichiers d’entrée.
