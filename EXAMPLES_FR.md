# Exemples Concrets d'Utilisation

## Cas d'Usage 1: Facture Classique avec Produits Valides

### Données du PDF Original:
```
Invoice Number: S55-26030038
Invoice Date: 23/03/2026
Purchase Order: BSPO26-0004093
Customer: BlueStar Europe Distribution B.V.
Email: ap-eu@eu.bluestarinc.com
Phone: +34 91 744 44 60
Billing Address: Koninginnegracht 19, Den Haag, South Holland, Netherlands
Ship to: Weijerbeemd 12, 5651GN EINDHOVEN, The Netherlands
Ship From: Wanlida Industrial Zone, Jingcheng town, Nanjing, Zhangzhou, Fujian, China

Products:
  Product Number: 6008M-00--
  Description: (not much detail in PDF)
  Quantity: 110
  Unit Price: 162.50
  HS Code: 8470501000
```

### Données Extraites et Enrichies:
```json
{
  "invoiceInfo": {
    "invoiceNumber": "S55-26030038",
    "invoiceDate": "23/03/2026",
    "purchaseOrderNumber": "BSPO26-0004093",
    "paymentTerm": "45 days after invoice",
    "incoterms": "CIP",
    "countryOfOrigin": "CHINA"
  },
  "customerInfo": {
    "customerName": "BlueStar Europe Distribution B.V.",
    "email": "ap-eu@eu.bluestarinc.com",
    "phone": "+34 91 744 44 60",
    "billingAddress": "Koninginnegracht 19, Den Haag, South Holland, Netherlands",
    "shipToAddress": "Weijerbeemd 12, 5651GN EINDHOVEN, The Netherlands",
    "shipFromAddress": "Wanlida Industrial Zone, Jingcheng town, Nanjing, Zhangzhou, Fujian, 363600, China",
    "vat": "NL852024551B01",
    "eori": "NL852024551"
  },
  "products": [
    {
      "productNumber": "6008M-00--",
      "familyName": "M20",
      "productDescription": "M20 --2-Years total - Extended Warranty",
      "platform": "Warranty",
      "model": "M20 --2-Years total - Extended Warranty",
      "mainDisplay": "-",
      "secondDisplay": "-",
      "quantity": 110,
      "unitPrice": 162.50,
      "totalAmount": 17875.00,
      "paymentType": "-",
      "memoryPlan": "-",
      "g4": "-",
      "gms": "-",
      "hsCode": "8470501000"
    }
  ],
  "totalAmount": 17875.00,
  "currency": "EUR"
}
```

---

## Cas d'Usage 2: Facture Complexe avec Produits Variés

### Données Brutes du PDF:
```
Products:
  6008M-00--    110    162.50    17,875.00    8470501000
  8530M-00--    50     1,200.00  60,000.00    8470501000
  BM14000026    200    45.00     9,000.00     8470501000
```

### Résultats Après Enrichissement:

```json
{
  "products": [
    {
      "productNumber": "6008M-00--",
      "familyName": "M20",
      "productDescription": "M20 --2-Years total - Extended Warranty",
      "platform": "Warranty",
      "model": "M20 --2-Years total - Extended Warranty",
      "quantity": 110,
      "unitPrice": 162.50,
      "totalAmount": 17875.00,
      "hsCode": "8470501000"
    },
    {
      "productNumber": "8530M-00--",
      "familyName": "C20Lite (with FHD)",
      "productDescription": "ECR - 4GB+32GB,  Dual Display 15\" FHD+ 10,1\" STD CFD",
      "platform": "Android 13",
      "model": "ECR - 4GB+32GB,  Dual Display 15\" FHD+ 10,1\" STD CFD (Disp. only)",
      "mainDisplay": "15,6\"",
      "secondDisplay": "10,1\"",
      "paymentType": "-",
      "memoryPlan": "4+32",
      "g4": "-",
      "gms": "-",
      "quantity": 50,
      "unitPrice": 1200.00,
      "totalAmount": 60000.00,
      "hsCode": "8470501000"
    },
    {
      "productNumber": "BM14000026",
      "familyName": "M20SE",
      "productDescription": "TPU Silicon case",
      "platform": "Accessory",
      "model": "TPU Silicon case",
      "mainDisplay": "-",
      "secondDisplay": "-",
      "paymentType": "-",
      "memoryPlan": "-",
      "g4": "-",
      "gms": "-",
      "quantity": 200,
      "unitPrice": 45.00,
      "totalAmount": 9000.00,
      "hsCode": "8470501000"
    }
  ],
  "totalAmount": 86875.00
}
```

---

## Cas d'Usage 3: Gestion des Erreurs - Produit Invalide

### Données du PDF:
```
Products:
  53004-00--    100    162.50    16,250.00    8470501000   ✅ VALIDE
  BADPRODUCT    50     100.00    5,000.00     8470501000   ❌ INVALIDE
  72401-00--    10     150.00    1,500.00     8470501000   ✅ VALIDE
```

### Traitement:
```json
{
  "processing": {
    "53004-00--": {
      "status": "VALID",
      "action": "ENRICHED",
      "details": "Product found in catalog and enriched"
    },
    "BADPRODUCT": {
      "status": "INVALID",
      "action": "REJECTED",
      "reason": "Product not found in catalog",
      "message": "This product number does not exist in the system"
    },
    "72401-00--": {
      "status": "VALID",
      "action": "ENRICHED",
      "details": "Product found in catalog and enriched"
    }
  },
  "results": {
    "totalProcessed": 3,
    "valid": 2,
    "invalid": 1,
    "successRate": "66.67%"
  },
  "products": [
    // 53004-00-- enrichi
    // 72401-00-- enrichi
    // BADPRODUCT est omis
  ]
}
```

---

## Cas d'Usage 4: Appel API de Validation

### Request:
```bash
curl -X GET "http://localhost:5206/api/product/validate/53004-00--"
```

### Response:
```json
{
  "isValid": true,
  "productNumber": "53004-00--",
  "product": {
    "familyName": "M20SE",
    "platform": "Android 13",
    "model": "4G(CAT4)/EAU, 3GB+32GB, 5MP+2MP, 2SIM+2SAM, EU/UK/UL AC Cable",
    "mainDisplay": "6,5\"",
    "secondDisplay": "-",
    "paymentType": "SoftPOS",
    "memoryPlan": "3+32",
    "g4": "4G",
    "gms": "GMS",
    "hsCode": "8470501000"
  }
}
```

---

## Cas d'Usage 5: Appel API d'Enrichissement

### Request:
```bash
curl -X POST "http://localhost:5206/api/product/enrich" \
  -H "Content-Type: application/json" \
  -d '{
    "productNumber": "8530M-00--",
    "quantity": 50,
    "unitPrice": 1200.00
  }'
```

### Response:
```json
{
  "success": true,
  "product": {
    "productNumber": "8530M-00--",
    "familyName": "C20Lite (with FHD)",
    "productDescription": null,
    "platform": "Android 13",
    "model": "ECR - 4GB+32GB,  Dual Display 15\" FHD+ 10,1\" STD CFD (Disp. only)",
    "mainDisplay": "15,6\"",
    "secondDisplay": "10,1\"",
    "quantity": 50,
    "unitPrice": 1200.0,
    "totalAmount": 0.0,
    "paymentType": "-",
    "memoryPlan": "4+32",
    "g4": "-",
    "gms": "-",
    "hsCode": "8470501000"
  }
}
```

---

## Cas d'Usage 6: Téléchargement du PDF Enrichi

### Processus:
1. **Upload PDF original** → Extraction
2. **Enrichissement automatique** → Données complètes
3. **Click Download** → Génération PDF enrichi

### PDF Généré Contient:

**Section 1: Invoice Information**
```
┌─────────────────────────────────────────┐
│ INVOICE INFORMATION                     │
├─────────────────────────────────────────┤
│ Invoice Number:         S55-26030038    │
│ Invoice Date:           23/03/2026      │
│ Purchase Order Number:  BSPO26-0004093  │
│ Payment Term:           45 days         │
│ Incoterms:             CIP              │
│ Country of Origin:      CHINA            │
└─────────────────────────────────────────┘
```

**Section 2: Customer Information**
```
┌─────────────────────────────────────────┐
│ CUSTOMER INFORMATION                    │
├─────────────────────────────────────────┤
│ Customer Name:   BlueStar Europe...     │
│ Email:          ap-eu@eu.bluestarinc... │
│ Phone:          +34 91 744 44 60       │
│ Billing Address: Koninginnegracht 19...│
│ Ship To:        Weijerbeemd 12...      │
│ Ship From:      Wanlida Industrial...  │
│ VAT:            NL852024551B01         │
│ EORI:           NL852024551            │
└─────────────────────────────────────────┘
```

**Section 3: Product Information (Tableau enrichi)**
```
┌──────────────────┬────────────┬──────────────┬─────────────┬───────────┐
│ Product Number   │ Family     │ Description  │ Quantity    │ HS Code   │
├──────────────────┼────────────┼──────────────┼─────────────┼───────────┤
│ 6008M-00--       │ M20        │ M20 --2-Years│ 110         │ 8470501000│
│                  │            │ Extended War │             │           │
├──────────────────┼────────────┼──────────────┼─────────────┼───────────┤
│ 8530M-00--       │ C20Lite    │ ECR -        │ 50          │ 8470501000│
│                  │ (with FHD) │ 4GB+32GB... │             │           │
├──────────────────┼────────────┼──────────────┼─────────────┼───────────┤
│ BM14000026       │ M20SE      │ TPU Silicon  │ 200         │ 8470501000│
│                  │            │ case         │             │           │
└──────────────────┴────────────┴──────────────┴─────────────┴───────────┘

Total Amount: EUR 86,875.00
```

---

## Produits Valides (Exemples)

### M10SE Series
- ✅ `53209-00--` M10SE Android 13
- ✅ `5320A-00--` M10SE with Scanner
- ✅ `5320D-00--` M10SE Android 14
- ✅ `53207-00--` M10SE Android 14 with Scanner

### M20SE Series
- ✅ `53004-00--` M20SE Base
- ✅ `53006-00--` M20SE with LANDI Scanner
- ✅ `53001-00--` M20SE with Honeywell Scanner
- ✅ `53007-00--` M20SE 4GB+64GB
- ✅ `53008-00--` M20SE 4GB+64GB with Scanner
- ✅ `53009-00--` M20SE 4GB+64GB with Honeywell

### M20 Series
- ✅ `53103-00--` M20 with Printer
- ✅ `53105-00--` M20 with Printer and Scanner
- ✅ `53101-00--` M20 with Printer and Honeywell
- ✅ `53106-00--` M20 4GB+64GB with Printer
- ✅ `53107-00--` M20 4GB+64GB with Printer and Scanner
- ✅ `53108-00--` M20 4GB+64GB with Printer and Honeywell

### Accessories
- ✅ `72401-00--` 1+1 Charging Dock (EU Plug)
- ✅ `72402-00--` Charging only dock
- ✅ `BM14000026` TPU Silicon case
- ✅ `BL08000001` Hand Strap for M20
- ✅ `XS07000008` Temper glass screen protector

### C20 Series
- ✅ `8530L-00--` C20Lite Single Display
- ✅ `8530M-00--` C20Lite Dual Display
- ✅ `85401-00--` C20SE Single Display
- ✅ `85404-00--` C20SE Dual Display

### Windows Series
- ✅ `86201-00--` Cx20SE i3 Single Display
- ✅ `86202-00--` Cx20SE i3 Dual Display
- ✅ `85801-00--` Cx20 i3 with Printer
- ✅ `86501-00--` Cx20Lite N97 with Printer

### Warranty Products
- ✅ `6008M-00--` M20 2-Years Warranty
- ✅ `6008N-00--` M20 3-Years Warranty
- ✅ `6008T-00--` M20SE 2-Years Warranty
- ✅ `6008U-00--` M20SE 3-Years Warranty

---

## Produits Invalides (Exemples)

### Ces numéros seront REJETÉS:
- ❌ `INVALID123` (pas dans la base)
- ❌ `ABC-00--` (pas dans la base)
- ❌ `00000-00--` (pas dans la base)
- ❌ `XYZ` (format invalide et pas dans la base)
- ❌ `53999-99--` (pas dans la base)

---

## Recommandations d'Utilisation

### ✅ À Faire:
- ✅ Upload des PDFs de factures commerciales
- ✅ Laisser le système enrichir automatiquement
- ✅ Vérifier les données enrichies
- ✅ Télécharger le PDF final enrichi
- ✅ Conserver une trace des factures traitées

### ❌ À Éviter:
- ❌ Saisir manuellement les détails du catalogue
- ❌ Inventer des numéros de produits
- ❌ Modifier les HS Codes enrichis sans raison
- ❌ Télécharger le PDF avant enrichissement complet
- ❌ Créer des doublons de factures

---

## Performance

| Opération | Temps | Notes |
|-----------|-------|-------|
| Upload + Extraction | 2-5s | Dépend du PDF |
| Enrichissement auto | <1ms/produit | 100 produits = <100ms |
| Validation | <1ms | Via API |
| Génération PDF final | 1-3s | Dépend de la taille |
| **Temps Total** | **3-10s** | ✅ Très rapide |

---

## Support

Pour toute question ou problème:
1. Vérifiez que le numéro de produit est valide
2. Utilisez l'API de validation: `/api/product/validate/{productNumber}`
3. Consultez la liste complète des produits valides ci-dessus
4. Contactez le support technique avec les détails de la facture
