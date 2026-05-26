# Guide simple pour le client : ajouter 3 colonnes (Semaine, Famille, Nom produit)

Ce document explique simplement ce que nous allons faire pour vos fichiers Excel LAN, STOCK et SELLS OUT. Pas besoin de connaissances techniques.

Objectif :
- Prendre votre fichier Excel existant
- Ajouter 3 colonnes manquantes : `Week` (semaine), `Family Name` (nom de la famille produit) et `Product Name` (nom du produit)
- Retourner un nouveau fichier prêt à être utilisé

Étapes pour l'utilisateur :
1. Ouvrir la page "Import Excel" dans l'application.
2. Choisir le type de fichier : LAN, STOCK ou SELLS OUT.
3. Sélectionner votre fichier Excel (ou CSV).
4. (Optionnel) Joindre un fichier de correspondance (mapping) si vous avez une table `Item No. -> Family Name`.
5. Cliquer sur "Traiter".
6. L'application va afficher un tableau avec un aperçu des données et un bouton "Télécharger" pour récupérer le nouveau fichier.

Ce que l'application ajoute automatiquement :
- `Week` : la semaine correspondant à la date principale du fichier (par exemple, date d'expédition ou date de rapport). Si la date est manquante, la semaine actuelle est utilisée.
- `Family Name` : valeur cherchée dans votre fichier de correspondance, sinon dans notre catalogue interne si disponible.
- `Product Name` : nom de produit associé (si disponible), sinon vide.

Exemples d'entrée / sortie (très simple) :

- Exemple LAN (entrée) :
  No., Name, Order Date, Item No., Description, Quantity
  123, Client A, 2026-04-21, 6008M-00--, Warranty ext, 110

- Exemple LAN (sortie) :
  Week, No., Name, Order Date, Item No., Family Name, Product Name, Description, Quantity
  2026-16, 123, Client A, 2026-04-21, 6008M-00--, Warranty, M20 Warranty, Warranty ext, 110

Fichier de correspondance (`Mapping`) :
- Si vous avez un fichier Excel/CSV avec 3 colonnes : `Item No.`, `Product Family`, `Family Name`, joignez-le au moment de l'upload.
- Exemple simple de mapping :
  Item No.,Product Family,Family Name
  6008M-00--,Warranty,M20 Warranty

Attention et bonnes pratiques :
- Gardez la première ligne avec les en-têtes (titres des colonnes).
- Envoyez si possible le fichier sans colonnes fusionnées ni formules compliquées.
- Si une ligne n'a pas d'Item No., elle sera marquée pour vérification et non enrichie automatiquement.

Résultat final :
- Un fichier Excel (.xlsx) identique au vôtre mais avec 3 colonnes ajoutées et prêtes à être utilisées par vos équipes.

Support :
- Si vous voulez, nous pouvons préparer un petit fichier d'exemple pour chaque type (LAN, STOCK, SELLS OUT) que vous pourrez ouvrir pour vérifier le format.



