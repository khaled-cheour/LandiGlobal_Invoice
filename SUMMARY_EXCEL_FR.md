# 📊 RÉSUMÉ - Extraction & Transformation Excel (LAN / STOCK / SELLSOUT)

## 🎯 Objectif

Fournir la documentation synthétique et actionnable pour le module d'extraction/transform Excel (types d'import : `LAN`, `STOCK`, `SELLSOUT`). Ce document décrit les problèmes couverts, les solutions implémentées, les colonnes ajoutées, les règles de transformation, les exemples, les tests et les étapes opérationnelles.

---

## 🔍 Problèmes traités

1. En-têtes et formats hétérogènes (noms de colonnes variables)
2. Valeurs numériques mal formatées (virgules, espaces, signes)
3. Absence de colonnes cibles attendues par le flux métier
4. Données produit non harmonisées (SKU divergents)
5. Besoin d'enrichissement (FamilyName, HSCode, etc.)
6. Lignes vides / données corrompues

---

## ✅ Solutions mises en place

- Normalisation d'en-têtes flexibles (reconnaissance de variantes : `Item No`/`SKU`/`Product Number`).
- Conversion décimale robuste (`,` → `.` ; suppression d'espaces non numériques).
- Mapping configurable via fichier de mapping Excel (colonne `Item No` obligatoire) et mapping statique par défaut.
- Ajout automatique des colonnes cibles requises par type d'import.
- Enrichissement produit via `ProductCatalog` (si disponible) : `FamilyName`, `HSCode`, `Model`, `Platform`.
- Règles métier par import type (LAN/STOCK/SELLSOUT) et rejet avec logging des lignes invalides.

---

## 📦 Colonnes ajoutées / Générées (par défaut)

Colonnes communes générées :
- `ProductNumber` (normalisé)
- `ImportType` (LAN / STOCK / SELLSOUT)
- `ProcessedAt` (UTC timestamp)

Colonnes LAN (ajoutées) :
- `FamilyName`, `ProductDescription`, `Quantity`, `UnitPrice`, `TotalAmount`

Colonnes STOCK (ajoutées) :
- `WarehouseCode`, `StockQuantity`, `AvailableToSell`, `ReorderLevel`, `SafetyStock` (optionnel)

Colonnes SELLSOUT (ajoutées) :
- `QuantitySold`, `UnitPrice`, `SalesAmount`, `Period` (YYYY-MM)

---

## Règles de transformation principales

- Normaliser les headers (cas insensible, trim, variantes reconnues).
- Nettoyer les valeurs (supprimer non-digits pour champs numériques, remplacer `,` par `.`).
- Calculer `TotalAmount` ou `Quantity` si l’un des deux est manquant et l’autre est présent.
- Arrondir montants à 2 décimales.
- Rejeter les lignes sans `ProductNumber` valide (log avec raison).
- Enrichir via mapping priorité : fichier de mapping fourni → mapping statique → ProductCatalog lookup.
- Ne pas écraser `ProductDescription` source sauf option `forceCatalog`.

---

## 🔗 Mapping (fichier recommandé)

Structure recommandée pour le fichier de mapping (feuille 1) :
- `Item No` | `FamilyName` | `Family` | `WarehouseCode` | `DefaultReorderLevel` | `Notes`

Règles :
- Le service recherche `Item No` dans la feuille de mapping et applique valeurs présentes.
- Les colonnes du mapping sont optionnelles : seules celles présentes sont appliquées.

---

## 🧪 Exemples concrets

### Exemple LAN (entrée)
```
Item No, Description, Qty, Unit Price, Invoice Number, Date, Store
6008M-00--, Warranty ext, 110, 162.50, INV-2026-001, 2026-04-21, STORE1
```
### Sortie attendue
```
ProductNumber, FamilyName, ProductDescription, Quantity, UnitPrice, TotalAmount, ImportType, ProcessedAt
6008M-00--, M20, Warranty ext (catalog), 110, 162.50, 17875.00, LAN, 2026-04-22T09:30:00Z
```

### Exemple STOCK (entrée)
```
SKU, Warehouse, StockQty, OnOrder, ReorderLevel
6008M-00--, WH1, 200, 50, 20
```
### Sortie attendue
```
ProductNumber, WarehouseCode, StockQuantity, AvailableToSell, ReorderLevel, ImportType, ProcessedAt
6008M-00--, WH1, 200, 200, 20, STOCK, 2026-04-22T09:30:00Z
```

### Exemple SELLSOUT (entrée)
```
SKU, SoldQty, SaleDate, RetailPrice, POS
6008M-00--, 15, 2026-04-01, 162.50, STORE1
```
### Sortie attendue
```
ProductNumber, QuantitySold, UnitPrice, SalesAmount, Period, ImportType, ProcessedAt
6008M-00--, 15, 162.50, 2437.50, 2026-04, SELLSOUT, 2026-04-22T09:30:00Z
```

---

## 🔍 Tests & Validation

- Unit tests : parser d’en-têtes, normalisation décimale, calcul `TotalAmount`, mapping.
- Integration tests : upload d’un fichier d’échantillon par `ImportType` et comparaison avec le fichier attendu.
- Tests manuels : fichiers avec virgules comme séparateur décimal, en-têtes variantes, mapping incomplet.

---

## 🛠️ Fichiers / Code impactés

- `Controllers/ExcelTransformController.cs`
- `Services/ExcelTransformService.cs`
- `Models/ExcelTransformModels.cs`
- `Views/ExcelTransform/*`

---

## ▶️ Mode opératoire (exécution locale)

1. Restaurer et builder :

```bash
dotnet restore
dotnet build
```

2. Lancer l'application :

```bash
dotnet run
```

3. Ouvrir l'UI Excel Transform et uploader les fichiers d'exemple.

---

## 🔮 Prochaines recommandations

1. Fournir 2-3 fichiers d’exemple par `ImportType` pour tests automatisés.
2. Ajouter UI de mapping dynamique pour lier colonnes source → cibles.
3. Persister les mappings par client.
4. Exporter les lignes rejetées (CSV) pour correction manuelle.

---

## Conclusion

Le module d'extraction Excel est prêt à traiter les imports `LAN`, `STOCK` et `SELLSOUT` en normalisant les formats, en ajoutant les colonnes métier nécessaires et en appliquant l'enrichissement produit. Le fichier `DOCUMENTATION_EXCEL_EXTRACTION.md` contient les détails d'implémentation; ce résumé sert de guide opérationnel rapide.
