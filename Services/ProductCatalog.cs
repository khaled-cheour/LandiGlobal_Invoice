using System.Collections.Generic;
using System.Linq;

namespace LandiGlobalTemplate.Services
{
    public class ProductData
    {
        public string ProductNumber { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string MainDisplay { get; set; } = string.Empty;
        public string SecondDisplay { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public string MemoryPlan { get; set; } = string.Empty;
        public string G4 { get; set; } = string.Empty;
        public string GMS { get; set; } = string.Empty;
        public string HSCode { get; set; } = string.Empty;
    }

    public class ProductCatalog
    {
        private static readonly List<ProductData> Products = new()
        {
            // M10SE Series
            new ProductData { ProductNumber = "53209-00--", FamilyName = "M10SE", Platform = "Android 13", Model = "4G(CAT4)/EAU+WiFi(2.4G/5G)+Bluetooth+GPS&BEIDOU+3GB/32GB+5inch FW+3000mAH+0.3MP+5MP+2SIM+SD Card+NFC+5V/2A(multi-Plug)+EU+AM+UK+Android 13.0", MainDisplay = "5\"", SecondDisplay = "-", PaymentType = "SoftPOS", MemoryPlan = "3+32", G4 = "4G", GMS = "", HSCode = "8471309000" },
            new ProductData { ProductNumber = "5320A-00--", FamilyName = "M10SE", Platform = "Android 13", Model = "4G(CAT4)/EAU+WiFi(2.4G/5G)+Bluetooth+GPS&BEIDOU+3GB/32GB+5inch FW+3000mAH+LANDI Scanner+0.3MP+5MP+2SIM+SD Card+NFC+5V/2A(multi-Plug)+EU+AM+UK+Android 13.0", MainDisplay = "5\"", SecondDisplay = "-", PaymentType = "SoftPOS", MemoryPlan = "3+32", G4 = "4G", GMS = "", HSCode = "8471309000" },
            new ProductData { ProductNumber = "5320D-00--", FamilyName = "M10SE", Platform = "Android 14", Model = "4G(CAT4)/EAU, GMS, 3GB+32GB, 5MP+0.3MP, 1SIM", MainDisplay = "5\"", SecondDisplay = "-", PaymentType = "SoftPOS", MemoryPlan = "3+32", G4 = "4G", GMS = "", HSCode = "8471309000" },
            new ProductData { ProductNumber = "53207-00--", FamilyName = "M10SE", Platform = "Android 14", Model = "4G(CAT4)/EAU, GMS, 3GB+32GB, 5MP+0.3MP, 1SIM, LANDI Scanner", MainDisplay = "5\"", SecondDisplay = "-", PaymentType = "SoftPOS", MemoryPlan = "3+32", G4 = "4G", GMS = "GMS", HSCode = "8471309000" },

            // M20SE Series
            new ProductData { ProductNumber = "53004-00--", FamilyName = "M20SE", Platform = "Android 13", Model = "4G(CAT4)/EAU, 3GB+32GB, 5MP+2MP, 2SIM+2SAM, EU/UK/UL AC Cable", MainDisplay = "6,5\"", SecondDisplay = "-", PaymentType = "SoftPOS", MemoryPlan = "3+32", G4 = "4G", GMS = "GMS", HSCode = "8470501000" },
            new ProductData { ProductNumber = "53006-00--", FamilyName = "M20SE", Platform = "Android 13", Model = "4G(CAT4)/EAU, 3GB+32GB, 5MP+2MP, 2SIM+2SAM, LANDI Scanner, EU/UK/UL AC Cable", MainDisplay = "6,5\"", SecondDisplay = "-", PaymentType = "SoftPOS", MemoryPlan = "3+32", G4 = "4G", GMS = "GMS", HSCode = "8470501000" },
            new ProductData { ProductNumber = "53001-00--", FamilyName = "M20SE", Platform = "Android 13", Model = "4G(CAT4)/EAU, 3GB+32GB, 5MP+2MP, 2SIM+2SAM, Honeywell Scanner, EU/UK/UL AC Cable", MainDisplay = "6,5\"", SecondDisplay = "-", PaymentType = "SoftPOS", MemoryPlan = "3+32", G4 = "4G", GMS = "GMS", HSCode = "8470501000" },
            new ProductData { ProductNumber = "53007-00--", FamilyName = "M20SE", Platform = "Android 13", Model = "4G(CAT4)/EAU, 4GB+64GB, 5MP+2MP, 2SIM+2SAM, EU/UK/UL AC Cable", MainDisplay = "6,5\"", SecondDisplay = "-", PaymentType = "SoftPOS", MemoryPlan = "4+64", G4 = "4G", GMS = "GMS", HSCode = "8470501000" },
            new ProductData { ProductNumber = "53008-00--", FamilyName = "M20SE", Platform = "Android 13", Model = "4G(CAT4)/EAU, 4GB+64GB, 5MP+2MP, 2SIM+2SAM, LANDI Scanner, EU/UK/UL AC Cable", MainDisplay = "6,5\"", SecondDisplay = "-", PaymentType = "SoftPOS", MemoryPlan = "4+64", G4 = "4G", GMS = "GMS", HSCode = "8470501000" },
            new ProductData { ProductNumber = "53009-00--", FamilyName = "M20SE", Platform = "Android 13", Model = "4G(CAT4)/EAU, 4GB+64GB, 5MP+2MP, 2SIM+2SAM, Honeywell Scanner, EU/UK/UL AC Cable", MainDisplay = "6,5\"", SecondDisplay = "-", PaymentType = "SoftPOS", MemoryPlan = "4+64", G4 = "4G", GMS = "GMS", HSCode = "8470501000" },

            // M20 Series
            new ProductData { ProductNumber = "53103-00--", FamilyName = "M20", Platform = "Android 13", Model = "4G(CAT4)/EAU, 3GB+32GB, 5MP+2MP, 2SIM+2SAM, Printer, EU/UK/UL AC Cable", MainDisplay = "6,5\"", SecondDisplay = "-", PaymentType = "SoftPOS", MemoryPlan = "3+32", G4 = "4G", GMS = "GMS", HSCode = "8470501000" },
            new ProductData { ProductNumber = "53105-00--", FamilyName = "M20", Platform = "Android 13", Model = "4G(CAT4)/EAU, 3GB+32GB, 5MP+2MP, 2SIM+2SAM, Printer, LANDI Scanner, EU/UK/UL AC Cable", MainDisplay = "6,5\"", SecondDisplay = "-", PaymentType = "SoftPOS", MemoryPlan = "3+32", G4 = "4G", GMS = "GMS", HSCode = "8470501000" },
            new ProductData { ProductNumber = "53101-00--", FamilyName = "M20", Platform = "Android 13", Model = "4G(CAT4)/EAU, 3GB+32GB, 5MP+2MP, 2SIM+2SAM, Printer, Honeywell Scanner, EU/UK/UL AC Cable", MainDisplay = "6,5\"", SecondDisplay = "-", PaymentType = "SoftPOS", MemoryPlan = "3+32", G4 = "4G", GMS = "GMS", HSCode = "8470501000" },
            new ProductData { ProductNumber = "53106-00--", FamilyName = "M20", Platform = "Android 13", Model = "4G(CAT4)/EAU, 4GB+64GB, 5MP+2MP, 2SIM+2SAM, Printer, EU/UK/UL AC Cable", MainDisplay = "6,5\"", SecondDisplay = "-", PaymentType = "SoftPOS", MemoryPlan = "4+64", G4 = "4G", GMS = "GMS", HSCode = "8470501000" },
            new ProductData { ProductNumber = "53107-00--", FamilyName = "M20", Platform = "Android 13", Model = "4G(CAT4)/EAU, 4GB+64GB, 5MP+2MP, 2SIM+2SAM, Printer, LANDI Scanner, EU/UK/UL AC Cable", MainDisplay = "6,5\"", SecondDisplay = "-", PaymentType = "SoftPOS", MemoryPlan = "4+64", G4 = "4G", GMS = "GMS", HSCode = "8470501000" },
            new ProductData { ProductNumber = "53108-00--", FamilyName = "M20", Platform = "Android 13", Model = "4G(CAT4)/EAU, 4GB+64GB, 5MP+2MP, 2SIM+2SAM, Printer, Honeywell Scanner, EU/UK/UL AC Cable", MainDisplay = "6,5\"", SecondDisplay = "-", PaymentType = "SoftPOS", MemoryPlan = "4+64", G4 = "4G", GMS = "GMS", HSCode = "8470501000" },

            // Accessories - M20SE/M20
            new ProductData { ProductNumber = "72401-00--", FamilyName = "M20SE/M20", Platform = "Accessory", Model = "1+1 Charging Dock (EU Plug)", MainDisplay = "-", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "-", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "72402-00--", FamilyName = "M20SE/M20", Platform = "Accessory", Model = "Charging only dock", MainDisplay = "-", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "-", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "72403-00--", FamilyName = "M20SE/M20", Platform = "Accessory", Model = "Communication Dock (EU Plug)", MainDisplay = "-", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "-", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "72409-00--", FamilyName = "M20SE/M20", Platform = "Accessory", Model = "Communication Dock (UK Plug)", MainDisplay = "-", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "-", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "72406-00--", FamilyName = "M20SE/M20", Platform = "Accessory", Model = "1+1 Charging Dock (UK Plug)", MainDisplay = "-", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "-", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "XS07000008", FamilyName = "M20SE/M20", Platform = "Accessory", Model = "Temper glass screen protector for 6.517\"", MainDisplay = "-", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "-", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "BM14000026", FamilyName = "M20SE", Platform = "Accessory", Model = "TPU Silicon case", MainDisplay = "-", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "-", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "BL08000001", FamilyName = "M20", Platform = "Accessory", Model = "Hand Strap for M20", MainDisplay = "-", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "-", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "BL07000010", FamilyName = "M20SE", Platform = "Accessory", Model = "Wrist Strap for M20SE", MainDisplay = "-", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "-", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "72501-00--", FamilyName = "M20SE/M20", Platform = "Accessory", Model = "4-pack battery charger", MainDisplay = "-", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "-", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "PC02000247", FamilyName = "M20SE/M20", Platform = "Accessory", Model = "Spare Battery M20/M20SE (3500mAh/7.2V)", MainDisplay = "-", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "-", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "72601-00--", FamilyName = "M10SE", Platform = "Accessory", Model = "1+1 Charging Dock (EU Plug)", MainDisplay = "-", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "-", G4 = "-", GMS = "-", HSCode = "8471309000" },

            // C20Lite Series
            new ProductData { ProductNumber = "8530L-00--", FamilyName = "C20Lite (with FHD)", Platform = "Android 13", Model = "ECR - 4GB+32GB,  Single Display FHD Screen, USB Type-C, EU AC Cable", MainDisplay = "15,6\"", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "4+32", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "8530M-00--", FamilyName = "C20Lite (with FHD)", Platform = "Android 13", Model = "ECR - 4GB+32GB,  Dual Display 15\" FHD+ 10,1\" STD CFD (Disp. only), USB Type-C, EUAC", MainDisplay = "15,6\"", SecondDisplay = "10,1\"", PaymentType = "-", MemoryPlan = "4+32", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "85304-00--", FamilyName = "C20Lite (FHD)", Platform = "Android 13", Model = "ECR - 4GB+32GB, Single Display FHD Screen", MainDisplay = "15,6\"", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "4+32", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "85305-00--", FamilyName = "C20Lite (FHD)", Platform = "Android 13", Model = "ECR - 4GB+32GB,  Dual Display 15\" FHD+ 10,1\" STD CFD (Display only)", MainDisplay = "15,6\"", SecondDisplay = "10,1\"", PaymentType = "-", MemoryPlan = "4+32", G4 = "-", GMS = "-", HSCode = "8470501000" },

            // C20SE Series
            new ProductData { ProductNumber = "85401-00--", FamilyName = "C20SE", Platform = "Android 13", Model = "ECR - 4GB+32GB,  Single Display?EU AC Cable", MainDisplay = "15,6\"", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "4+32", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "85404-00--", FamilyName = "C20SE", Platform = "Android 13", Model = "ECR - 4GB+32GB,  Dual Display 15\" + 10,1\" PREMIUM CFD (Touch + QR + NFC), EU AC Cable", MainDisplay = "15,6\"", SecondDisplay = "10,1\"", PaymentType = "SoftPOS", MemoryPlan = "4+32", G4 = "-", GMS = "-", HSCode = "8470501000" },

            // C20Pro-GMS Series
            new ProductData { ProductNumber = "8561R-00--", FamilyName = "C20Pro-GMS", Platform = "Android 13", Model = "ECR - 4GB+64GB,  80mm Printer - Single Display, EU AC Cable", MainDisplay = "15,6\"", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "4+64", G4 = "-", GMS = "GMS", HSCode = "8470501000" },
            new ProductData { ProductNumber = "8561S-00--", FamilyName = "C20Pro-GMS", Platform = "Android 13", Model = "ECR - 4GB+64GB,  80mm Printer - Dual Display 15\" + 10,1\" PREMIUM CFD (Touch + QR + NFC)+Attached, EU AC Cable", MainDisplay = "15,6\"", SecondDisplay = "10,1\"", PaymentType = "SoftPOS", MemoryPlan = "4+64", G4 = "-", GMS = "GMS", HSCode = "8470501000" },
            new ProductData { ProductNumber = "8561T-00--", FamilyName = "C20Pro-GMS", Platform = "Android 13", Model = "ECR - 8GB+128GB,  80mm Printer - Single Display, EU AC Cable", MainDisplay = "15,6\"", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "8+128", G4 = "-", GMS = "GMS", HSCode = "8470501000" },
            new ProductData { ProductNumber = "8561U-00--", FamilyName = "C20Pro-GMS", Platform = "Android 13", Model = "P-C20Pro-S2M8E97A1-A-GM", MainDisplay = "15,6\"", SecondDisplay = "10,1\"", PaymentType = "SoftPOS", MemoryPlan = "8+128", G4 = "-", GMS = "GMS", HSCode = "8470501000" },
            new ProductData { ProductNumber = "8561V-00--", FamilyName = "C20Pro(4G)-GMS", Platform = "Android 13", Model = "ECR - 4GB+64GB,  80mm Printer, 4G-EAU - Single Display, EU AC Cable", MainDisplay = "15,6\"", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "4+64", G4 = "4G", GMS = "GMS", HSCode = "8470501000" },
            new ProductData { ProductNumber = "8561Y-00--", FamilyName = "C20Pro(4G)-GMS", Platform = "Android 13", Model = "ECR - 4GB+64GB,  80mm Printer, 4G-EAU - Dual Display 15\" + 10,1\" PREMIUM CFD (Touch + QR + NFC)+Attached, EU AC Cable", MainDisplay = "15,6\"", SecondDisplay = "10,1\"", PaymentType = "SoftPOS", MemoryPlan = "4+64", G4 = "4G", GMS = "GMS", HSCode = "8470501000" },
            new ProductData { ProductNumber = "8561W-00--", FamilyName = "C20Pro(4G)-GMS", Platform = "Android 13", Model = "ECR - 8GB+128GB,  80mm Printer, 4G-EAU - Single Display, EU AC Cable", MainDisplay = "15,6\"", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "8+128", G4 = "4G", GMS = "GMS", HSCode = "8470501000" },
            new ProductData { ProductNumber = "8561X-00--", FamilyName = "C20Pro(4G)-GMS", Platform = "Android 13", Model = "ECR - 8GB+128GB,  80mm Printer, 4G-EAU - Dual Display 15\" + 10,1\" PREMIUM CFD (Touch + QR + NFC)+Attached, EU AC Cable", MainDisplay = "15,6\"", SecondDisplay = "10,1\"", PaymentType = "SoftPOS", MemoryPlan = "8+128", G4 = "4G", GMS = "GMS", HSCode = "8470501000" },

            // C20ProSE-GMS Series
            new ProductData { ProductNumber = "8591F-00--", FamilyName = "C20ProSE-GMS", Platform = "Android 13", Model = "ECR - 4GB+64GB?Single Display 15\" , EU AC Cable", MainDisplay = "15,6\"", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "4+64", G4 = "-", GMS = "GMS", HSCode = "8470501000" },
            new ProductData { ProductNumber = "8591G-00--", FamilyName = "C20ProSE-GMS", Platform = "Android 13", Model = "ECR - 4GB+64GB, Dual Display 15\" + 10,1\" PREMIUM CFD (Touch + QR + NFC)+Attached, EU AC Cable", MainDisplay = "15,6\"", SecondDisplay = "10,1\"", PaymentType = "SoftPOS", MemoryPlan = "4+64", G4 = "-", GMS = "GMS", HSCode = "8470501000" },
            new ProductData { ProductNumber = "8591J-00--", FamilyName = "C20ProSE-GMS", Platform = "Android 13", Model = "ECR - 8GB+128GB?Single Display 15\" , EU AC Cable", MainDisplay = "15,6\"", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "8+128", G4 = "-", GMS = "GMS", HSCode = "8470501000" },
            new ProductData { ProductNumber = "8591K-00--", FamilyName = "C20ProSE-GMS", Platform = "Android 13", Model = "ECR - 8GB+128GB, Dual Display 15\" + 10,1\" PREMIUM CFD (Touch + QR + NFC)+Attached, EU AC Cable", MainDisplay = "15,6\"", SecondDisplay = "10,1\"", PaymentType = "SoftPOS", MemoryPlan = "8+128", G4 = "-", GMS = "GMS", HSCode = "8470501000" },
            new ProductData { ProductNumber = "8591S-00--", FamilyName = "C20ProSE-GMS", Platform = "Android 13", Model = "ECR - 4GB+64GB, Dual Display 15\" + 15\" FHD+2Touch+NFC+Attached, EU AC Cable, GMS", MainDisplay = "15,6\"", SecondDisplay = "15,6\"", PaymentType = "SoftPOS", MemoryPlan = "4+64", G4 = "-", GMS = "GMS", HSCode = "8470501000" },

            // C20ProSE Series
            new ProductData { ProductNumber = "85908-00--", FamilyName = "C20ProSE", Platform = "Android 13", Model = "ECR - 4GB+64GB, Dual Display 15\" + 10,1\" PREMIUM CFD (Touch + QR + NFC)+Attached, EU AC Cable", MainDisplay = "15,6\"", SecondDisplay = "10,1\"", PaymentType = "SoftPOS", MemoryPlan = "4+64", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "8590B-00--", FamilyName = "C20ProSE", Platform = "Android 13", Model = "ECR - 8GB+128GB, Dual Display 15\" + 10,1\" PREMIUM CFD (Touch + QR + NFC)+Attached, EU AC Cable", MainDisplay = "15,6\"", SecondDisplay = "10,1\"", PaymentType = "SoftPOS", MemoryPlan = "8+128", G4 = "-", GMS = "-", HSCode = "8470501000" },

            // C20Pro Series
            new ProductData { ProductNumber = "8560Q-00--", FamilyName = "C20Pro", Platform = "Android 13", Model = "ECR - 8GB+128GB,  80mm Printer - Dual Display 15\" + 10,1\" PREMIUM CFD (Touch + QR + NFC)+Attached", MainDisplay = "15,6\"", SecondDisplay = "10,1\"", PaymentType = "SoftPOS", MemoryPlan = "8+128", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "8560L-00--", FamilyName = "C20Pro (4G)", Platform = "Android 13", Model = "ECR - 8GB+128GB,  80mm Printer, 4G - Dual Display 15\" + 10,1\" PREMIUM CFD (Touch + QR + NFC)+Attached", MainDisplay = "15,6\"", SecondDisplay = "10,1\"", PaymentType = "SoftPOS", MemoryPlan = "8+128", G4 = "4G", GMS = "-", HSCode = "8470501000" },

            // C20DS Series
            new ProductData { ProductNumber = "86305-00--", FamilyName = "C20DS", Platform = "Android 13", Model = "KDS 4GB+32GB+POE+EU AC Cable", MainDisplay = "15,6\"", SecondDisplay = "-", PaymentType = "SoftPOS", MemoryPlan = "4+32", G4 = "-", GMS = "-", HSCode = "8470501000" },

            // Windows Series - CX20SE
            new ProductData { ProductNumber = "86201-00--", FamilyName = "Cx20SE", Platform = "Windows", Model = "ECR - i3, 8GB+256GB - Single Display, EU AC Cable, OS pre-installed /No license", MainDisplay = "15,6\"", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "8+256", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "86202-00--", FamilyName = "Cx20SE", Platform = "Windows", Model = "ECR - i3, 8GB+256GB  - Dual Display 15\" + 10,1\" STD CFD (Display only), EU AC Cable, OS pre-installed /No license", MainDisplay = "15,6\"", SecondDisplay = "10,1\"", PaymentType = "-", MemoryPlan = "8+256", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "8620K-00--", FamilyName = "Cx20SE", Platform = "Windows", Model = "ECR - i3, 8GB+256GB  - Dual Display 15\" + 10,1\" Premium CFD (touch+2MP), EU AC Cable, OS pre-installed /No license", MainDisplay = "15,6\"", SecondDisplay = "10,1\"", PaymentType = "-", MemoryPlan = "8+256", G4 = "-", GMS = "-", HSCode = "8470501000" },

            // Windows Series - CX20LiteSE
            new ProductData { ProductNumber = "86601-00--", FamilyName = "Cx20LiteSE", Platform = "Windows", Model = "ECR - N97, 8GB+256GB - Single Display, EU AC Cable, OS pre-installed /No license", MainDisplay = "15,6\"", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "8+256", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "86602-00--", FamilyName = "Cx20LiteSE", Platform = "Windows", Model = "ECR - N97, 8GB+256GB - Dual Display 15\" + 10,1\" STD CFD (Display only), EU AC Cable, OS pre-installed /No license", MainDisplay = "15,6\"", SecondDisplay = "10,1\"", PaymentType = "-", MemoryPlan = "8+256", G4 = "-", GMS = "-", HSCode = "8470501000" },

            // Windows Series - CX20
            new ProductData { ProductNumber = "85801-00--", FamilyName = "Cx20", Platform = "Windows", Model = "ECR - i3, 8GB+256GB, 80mm Printer - Single Display, EU AC Cable, OS pre-installed /No license", MainDisplay = "15,6\"", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "8+256", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "85802-00--", FamilyName = "Cx20", Platform = "Windows", Model = "ECR - i3, 8GB+256GB, 80mm Printer - Dual Display 15\" + 10,1\" STD CFD (Display only), EU AC Cable, OS pre-installed /No license", MainDisplay = "15,6\"", SecondDisplay = "10,1\"", PaymentType = "-", MemoryPlan = "8+256", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "85803-00--", FamilyName = "Cx20", Platform = "Windows", Model = "ECR - i3, 8GB+256GB, 80mm Printer - Dual Display 15\" + 10,1\" Premium CFD (touch+2MP), EU AC Cable, OS pre-installed /No license", MainDisplay = "15,6\"", SecondDisplay = "10,1\"", PaymentType = "-", MemoryPlan = "8+256", G4 = "-", GMS = "-", HSCode = "8470501000" },

            // Windows Series - CX20Lite
            new ProductData { ProductNumber = "86501-00--", FamilyName = "Cx20Lite", Platform = "Windows", Model = "ECR - N97, 8GB+256GB, 80mm Printer - Single Display, EU AC Cable, OS pre-installed /No license", MainDisplay = "15,6\"", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "8+256", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "86502-00--", FamilyName = "Cx20Lite", Platform = "Windows", Model = "ECR - N97, 8GB+256GB, 80mm Printer - Dual Display 15\" + 10,1\" STD CFD (Display only), EU AC Cable, OS pre-installed /No license", MainDisplay = "15,6\"", SecondDisplay = "10,1\"", PaymentType = "-", MemoryPlan = "8+256", G4 = "-", GMS = "-", HSCode = "8470501000" },

            // Warranty Products (valid products but warranty-related)
            new ProductData { ProductNumber = "6008M-00--", FamilyName = "M20", Platform = "Warranty", Model = "M20 --2-Years total - Extended Warranty", MainDisplay = "-", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "-", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "6008N-00--", FamilyName = "M20", Platform = "Warranty", Model = "M20 --3-Years total - Extended Warranty", MainDisplay = "-", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "-", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "6008T-00--", FamilyName = "M20SE", Platform = "Warranty", Model = "M20SE--2-Years total - Extended Warranty", MainDisplay = "-", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "-", G4 = "-", GMS = "-", HSCode = "8470501000" },
            new ProductData { ProductNumber = "6008U-00--", FamilyName = "M20SE", Platform = "Warranty", Model = "M20SE--3-Years total - Extended Warranty", MainDisplay = "-", SecondDisplay = "-", PaymentType = "-", MemoryPlan = "-", G4 = "-", GMS = "-", HSCode = "8470501000" },
        };

        /// <summary>
        /// Recherche un produit par numéro
        /// </summary>
        public static ProductData GetProduct(string productNumber)
        {
            if (string.IsNullOrWhiteSpace(productNumber))
                return null;

            return Products.FirstOrDefault(p => 
                p.ProductNumber.Equals(productNumber.Trim(), System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Valide si un numéro de produit est valide (format accepté)
        /// </summary>
        public static bool IsValidProductNumber(string productNumber)
        {
            if (string.IsNullOrWhiteSpace(productNumber))
                return false;

            productNumber = productNumber.Trim();

            // Valider le format: XXXXX-00-- ou codes alphanumériques valides
            // Accepter les formats commençant par chiffres ou lettres
            if (productNumber.Length < 3)
                return false;

            // Vérifier si le produit existe dans le catalogue
            return Products.Any(p => p.ProductNumber.Equals(productNumber, System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Enrichit un objet ProductInformation avec les détails du catalogue
        /// </summary>
        public static void EnrichProductInfo(Models.ProductInformation productInfo)
        {
            if (productInfo == null)
                return;

            var catalogProduct = GetProduct(productInfo.ProductNumber);
            if (catalogProduct != null)
            {
                productInfo.FamilyName = catalogProduct.FamilyName;
                productInfo.Platform = catalogProduct.Platform;
                productInfo.Model = catalogProduct.Model;
                productInfo.ProductDescription = catalogProduct.Model; // Full description from catalog
                productInfo.MainDisplay = catalogProduct.MainDisplay;
                productInfo.SecondDisplay = catalogProduct.SecondDisplay;
                productInfo.PaymentType = catalogProduct.PaymentType;
                productInfo.MemoryPlan = catalogProduct.MemoryPlan;
                productInfo.G4 = catalogProduct.G4;
                productInfo.GMS = catalogProduct.GMS;
                productInfo.HSCode = catalogProduct.HSCode;
            }
        }
    }
}
