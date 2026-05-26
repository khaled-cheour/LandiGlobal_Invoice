using LandiGlobalTemplate.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LandiGlobalTemplate.Services
{
    /// <summary>
    /// Service de validation des factures avec règles métier
    /// </summary>
    public class InvoiceValidationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InvoiceValidationService> _logger;

        public InvoiceValidationService(ApplicationDbContext context, ILogger<InvoiceValidationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Valide une facture selon les règles métier
        /// </summary>
        public async Task<ValidationResult> ValidateInvoiceAsync(InvoiceHistory invoice)
        {
            var errors = new List<string>();

            try
            {
                // Validation 1: Numéro de facture unique
                if (await _context.InvoiceHistories.AnyAsync(x => x.InvoiceNumber == invoice.InvoiceNumber && x.Id != invoice.Id))
                {
                    errors.Add($"Le numéro de facture '{invoice.InvoiceNumber}' est déjà utilisé.");
                }

                // Validation 2: Client requis
                if (string.IsNullOrWhiteSpace(invoice.CustomerName))
                {
                    errors.Add("Le nom du client est requis.");
                }

                // Validation 3: Montant positif
                if (invoice.TotalAmount <= 0)
                {
                    errors.Add("Le montant total doit être positif.");
                }

                // Validation 4: Email valide si fourni
                if (!string.IsNullOrWhiteSpace(invoice.Email) && !IsValidEmail(invoice.Email))
                {
                    errors.Add($"Le format de l'email '{invoice.Email}' est invalide.");
                }

                // Validation 5: Devise valide (ISO 4217)
                var validCurrencies = new[] { "USD", "EUR", "GBP", "CHF", "CAD", "AUD", "JPY", "CNY" };
                if (!validCurrencies.Contains(invoice.Currency))
                {
                    errors.Add($"La devise '{invoice.Currency}' n'est pas supportée. Devises acceptées: {string.Join(", ", validCurrencies)}");
                }

                // Validation 6: Termes de paiement valides
                var validTerms = new[] { "Net 30", "Net 60", "Net 90", "COD", "Prepayment" };
                if (!string.IsNullOrWhiteSpace(invoice.PaymentTerm) && !validTerms.Contains(invoice.PaymentTerm))
                {
                    errors.Add($"Le terme de paiement '{invoice.PaymentTerm}' n'est pas valide.");
                }

                // Validation 7: Incoterms valides
                var validIncoterms = new[] { "EXW", "FOB", "CIF", "CIP", "DDP", "DAP", "FCA", "CPT" };
                if (!string.IsNullOrWhiteSpace(invoice.Incoterms) && !validIncoterms.Contains(invoice.Incoterms))
                {
                    errors.Add($"L'incoterm '{invoice.Incoterms}' n'est pas valide.");
                }

                // Validation 8: Date de facture pas dans le futur
                if (invoice.InvoiceDate > DateTime.UtcNow.Date)
                {
                    errors.Add("La date de facture ne peut pas être dans le futur.");
                }

                // Validation 9: Adresses requises
                if (string.IsNullOrWhiteSpace(invoice.BillingAddress))
                {
                    errors.Add("L'adresse de facturation est requise.");
                }

                // ShipToAddress is optional

                // Validation 10: VAT/EORI format
                if (!string.IsNullOrWhiteSpace(invoice.VAT) && !ValidateVAT(invoice.VAT))
                {
                    errors.Add($"Le numéro de TVA '{invoice.VAT}' n'a pas le bon format.");
                }

                // Validation 11: Produits présents et valides
                if (invoice.ProductCount <= 0)
                {
                    errors.Add("La facture doit contenir au moins un produit.");
                }

                if (invoice.Products != null && invoice.Products.Count > 0)
                {
                    foreach (var product in invoice.Products)
                    {
                        var productErrors = ValidateProduct(product);
                        if (productErrors.Count > 0)
                        {
                            errors.AddRange(productErrors);
                        }
                    }
                }

                // Validation 12: Total des produits correspond
                if (invoice.Products != null && invoice.Products.Count > 0)
                {
                    var totalProducts = invoice.Products.Sum(p => p.TotalAmount);
                    if (Math.Abs(totalProducts - invoice.TotalAmount) > 0.01m) // Tolérance de 0.01
                    {
                        errors.Add($"Le total des produits ({totalProducts}) ne correspond pas au montant de la facture ({invoice.TotalAmount}).");
                    }
                }

                if (errors.Count == 0)
                {
                    invoice.IsValidated = true;
                    return new ValidationResult { IsValid = true };
                }

                return new ValidationResult { IsValid = false, Errors = errors };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation de la facture");
                errors.Add("Une erreur est survenue lors de la validation.");
                return new ValidationResult { IsValid = false, Errors = errors };
            }
        }

        /// <summary>
        /// Valide les informations produit
        /// </summary>
        private List<string> ValidateProduct(InvoiceHistoryProduct product)
        {
            var errors = new List<string>();

            // Produit requis
            if (string.IsNullOrWhiteSpace(product.ProductNumber))
            {
                errors.Add("Le numéro de produit est requis.");
            }

            // Quantité positive
            if (product.Quantity <= 0)
            {
                errors.Add($"La quantité du produit '{product.ProductNumber}' doit être positive.");
            }

            // Prix unitaire positif
            if (product.UnitPrice < 0)
            {
                errors.Add($"Le prix unitaire du produit '{product.ProductNumber}' ne peut pas être négatif.");
            }

            // Total correspond à Quantité * Prix unitaire
            var expectedTotal = product.Quantity * product.UnitPrice;
            if (Math.Abs(expectedTotal - product.TotalAmount) > 0.01m)
            {
                errors.Add($"Le total du produit '{product.ProductNumber}' n'est pas correct. Attendu: {expectedTotal}, Obtenu: {product.TotalAmount}");
            }

            // HS Code valide (format simple)
            if (!string.IsNullOrWhiteSpace(product.HSCode) && !ValidateHSCode(product.HSCode))
            {
                errors.Add($"Le code HS '{product.HSCode}' n'est pas valide (doit être 6 ou 10 chiffres).");
            }

            return errors;
        }

        /// <summary>
        /// Valide le format d'email
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Valide le numéro de TVA
        /// </summary>
        private bool ValidateVAT(string vat)
        {
            // Format simple: alphanumérique, 5-20 caractères
            return System.Text.RegularExpressions.Regex.IsMatch(vat, @"^[A-Z0-9]{5,20}$");
        }

        /// <summary>
        /// Valide le code HS (Harmonized System Code)
        /// </summary>
        private bool ValidateHSCode(string hsCode)
        {
            // Code HS valide: 6 ou 10 chiffres
            return System.Text.RegularExpressions.Regex.IsMatch(hsCode, @"^\d{6}(\d{4})?$");
        }

        /// <summary>
        /// Enrichit les données de la facture
        /// </summary>
        public async Task<InvoiceHistory> EnrichInvoiceDataAsync(InvoiceHistory invoice)
        {
            try
            {
                // Enrichissement 1: Vérifier les factures similaires
                var similarInvoices = await _context.InvoiceHistories
                    .Where(x => x.CustomerName == invoice.CustomerName && x.Id != invoice.Id)
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                // Enrichissement 2: Remplir les valeurs par défaut si manquantes
                if (string.IsNullOrWhiteSpace(invoice.Currency))
                {
                    invoice.Currency = "USD";
                }

                if (string.IsNullOrWhiteSpace(invoice.Status))
                {
                    invoice.Status = "Draft";
                }

                invoice.IsEnriched = true;
                invoice.UpdatedAt = DateTime.UtcNow;

                return invoice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enrichissement des données");
                return invoice;
            }
        }
    }

    /// <summary>
    /// Résultat de validation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();

        public override string ToString()
        {
            if (IsValid)
                return "Validation réussie";
            return $"Erreurs: {string.Join(", ", Errors)}";
        }
    }
}
