# Documentation : `ExcelTransform` et `ExcelTransform/Upload`

Ce document décrit le point d'entrée `ExcelTransform` (vues) et l'action `Upload` qui gère l'upload et la transformation des fichiers Excel/CSV.

## Endpoints

- GET `/ExcelTransform/` : page d'upload (`ExcelTransformController.Index`) - retourne la vue d'upload.
- POST `/ExcelTransform/Upload` : action `Upload(ExcelTransformViewModel model)` - reçoit le fichier et le mapping, retourne la vue `Result` contenant le `ExcelTransformResult`.
- GET `/ExcelTransform/Download` : action `Download()` - télécharge le fichier transformé stocké en session.

## Modèle attendu (côté serveur)

Type: `LandiGlobalTemplate.Models.ExcelTransformViewModel`

Propriétés :
- `ImportType` (enum `ExcelImportType`): `LAN`, `STOCK`, ou `SELLSOUT`.
- `File` (`IFormFile?`): fichier source (Excel/CSV). Obligatoire pour `Upload`.
- `MappingFile` (`IFormFile?`): fichier de mapping facultatif (Excel) utilisé pour enrichir/mapper les colonnes.
- `Result` (`ExcelTransformResult?`): rempli par le service après transformation (utilisé par la vue `Result`).

## Contrat HTTP pour `POST /ExcelTransform/Upload`

- Content-Type: `multipart/form-data`
- Form fields:
  - `ImportType`: string ou int correspondant à l'enum (`LAN`, `STOCK`, `SELLSOUT`).
  - `File`: le fichier source (champ de type fichier).
  - `MappingFile` (optionnel): le fichier de mapping (champ fichier).

## Validation & comportement serveur

- Si `File` est null ou vide, `ModelState` ajoute une erreur et la vue `Index` est retournée avec message d'erreur.
- `ExcelTransformService.TransformAsync` est appelé pour :
  - lire la première feuille du classeur
  - détecter en-têtes sources
  - appliquer mapping (mapping file fourni → mapping statique)
  - transformer les lignes selon `ImportType`
  - générer un fichier Excel de sortie
- Le résultat (`ExcelTransformResult`) contient : `ImportTypeLabel`, `Headers`, `Rows` (extraites, limitées), `TotalRows`, `FileBytes`.
- Le contrôleur stocke en session :
  - `ExcelTransformBytes` : Base64 du `FileBytes` (clé de session)
  - `ExcelTransformFileName` : nom du fichier de sortie
- L'utilisateur est redirigé vers la vue `Result` qui affiche un aperçu et un bouton de téléchargement.

## Session keys

- `ExcelTransformBytes` : Base64 string du contenu du fichier transformé.
- `ExcelTransformFileName` : nom de fichier utilisé par `/ExcelTransform/Download`.

## Erreurs communes

- "Aucune feuille Excel valide" : feuille vide ou mauvais format.
- Mapping non appliqué : mapping file sans colonne `Item No`.
- Valeurs numériques non convertibles : virgules non normalisées ou texte dans colonnes numériques.

Les erreurs sont journalisées via `ILogger` et retournées dans `ModelState` ou `BadRequest` selon le code.

## Exemple `curl` (upload)

```bash
curl -v -X POST "http://localhost:5000/ExcelTransform/Upload" \
  -F "ImportType=LAN" \
  -F "File=@/path/to/input.xlsx" \
  -F "MappingFile=@/path/to/mapping.xlsx"
```

Remarques :
- Remplacez l'URL par le port où tourne l'app (`dotnet run` affiche l'URL).
- `ImportType` peut être envoyé comme `LAN`, `STOCK`, ou `SELLSOUT`.

## Exemple `curl` (téléchargement après upload via UI)

Après l'upload par UI (ou un appel POST simulé qui stocke la session), télécharger via GET :

```bash
curl -v -X GET "http://localhost:5000/ExcelTransform/Download" -o Transformed.xlsx
```

## Comportement de la vue `Result`

- Affiche `Result.Headers` et un extrait des `Result.Rows` (max 50 lignes affichées).
- Bouton "Download" qui appelle `/ExcelTransform/Download` pour récupérer le fichier binaire.

## Bonnes pratiques pour les fichiers d'entrée

- S'assurer que la première ligne contient les en-têtes (même si noms légèrement différents).
- Pour les nombres : utiliser `.` comme séparateur décimal ou laisser le service normaliser `,` → `.`.
- Éviter les colonnes fusionnées ou formules complexes : utiliser valeurs calculées.
- Fournir un fichier de mapping si les colonnes sources sont non standard.

## Notes d'implémentation (références code)

- Contrôleur : `Controllers/ExcelTransformController.cs` (`Index`, `Upload`, `Download`).
- Service : `Services/ExcelTransformService.cs` (méthode `TransformAsync(ExcelImportType, IFormFile, IFormFile?)`).
- Modèles : `Models/ExcelTransformModels.cs` (`ExcelTransformViewModel`, `ExcelImportType`, `ExcelTransformResult`).

## Tests recommandés

- POST `/ExcelTransform/Upload` avec un fichier `LAN` minimal valide → vérifier `Result.TotalRows` et session.
- POST avec mapping partiel → vérifier que les valeurs mappées s'appliquent.
- POST avec nombres contenant `,` → vérifier conversion.
- POST sans `File` → vérifier erreur de validation.

---

## Fichiers d'exemple fournis

Des fichiers d'exemple CSV sont fournis pour tests rapides et pour que le client puisse voir le format attendu :

- [wwwroot/sample_data/sample_LAN.csv](wwwroot/sample_data/sample_LAN.csv)
- [wwwroot/sample_data/sample_STOCK.csv](wwwroot/sample_data/sample_STOCK.csv)
- [wwwroot/sample_data/sample_SELLSOUT.csv](wwwroot/sample_data/sample_SELLSOUT.csv)

Ces fichiers peuvent être uploadés via l'UI `ExcelTransform` ou envoyés avec `curl` au endpoint `POST /ExcelTransform/Upload`.


Fichier mis à jour : `DOCUMENTATION_EXCELTRANSFORM_UPLOAD.md`.

Si vous voulez, je peux :
- ajouter un tableau détaillé colonne-source → colonne-cible pour chaque `ImportType`, ou
- générer 1 fichier d'exemple par `ImportType` et placer les fichiers dans `wwwroot/sample_data/`.