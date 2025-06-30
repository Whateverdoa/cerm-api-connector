using System.Text.Json.Serialization;

namespace CermApiModule.Tests;

/// <summary>
/// Test data model based on F003ADB6G8.json
/// </summary>
public class OrderTestData
{
    [JsonPropertyName("Description")]
    public string Description { get; set; } = "F003ADB6G8";

    [JsonPropertyName("ReferenceAtCustomer")]
    public string ReferenceAtCustomer { get; set; } = "M45H4C226B";

    [JsonPropertyName("Delivery")]
    public string Delivery { get; set; } = "2025-03-07";

    [JsonPropertyName("Shipment_method")]
    public string ShipmentMethod { get; set; } = "PostNL EPS to Business";

    [JsonPropertyName("OrderQuantity")]
    public int OrderQuantity { get; set; } = 1000;

    [JsonPropertyName("Quantity_per_roll")]
    public string QuantityPerRoll { get; set; } = "";

    [JsonPropertyName("DispenserQuantity")]
    public int DispenserQuantity { get; set; } = 0;

    [JsonPropertyName("Core")]
    public string Core { get; set; } = "";

    [JsonPropertyName("UnitPrice")]
    public decimal UnitPrice { get; set; } = 37.18m;

    [JsonPropertyName("SupplierId")]
    public string SupplierId { get; set; } = "Drukwerkdeal";

    [JsonPropertyName("Name")]
    public string Name { get; set; } = "Roll Stickers";

    [JsonPropertyName("Street")]
    public string Street { get; set; } = "Rue Saint Donat 6";

    [JsonPropertyName("Country")]
    public string Country { get; set; } = "BE";

    [JsonPropertyName("PostalCode")]
    public string PostalCode { get; set; } = "5640";

    [JsonPropertyName("City")]
    public string City { get; set; } = "Mettet";

    [JsonPropertyName("Contacts")]
    public List<ContactTestData> Contacts { get; set; } = new();

    [JsonPropertyName("Width")]
    public string Width { get; set; } = "30,0";

    [JsonPropertyName("Height")]
    public string Height { get; set; } = "30,0";

    [JsonPropertyName("Shape")]
    public string Shape { get; set; } = "Circle";

    [JsonPropertyName("Winding")]
    public int Winding { get; set; } = 1;

    [JsonPropertyName("Radius")]
    public int Radius { get; set; } = 0;

    [JsonPropertyName("Premium_White")]
    public string PremiumWhite { get; set; } = "N";

    [JsonPropertyName("Substrate")]
    public string Substrate { get; set; } = "Adhesive Label Paper White Gloss";

    [JsonPropertyName("Adhesive")]
    public string Adhesive { get; set; } = "Removable Adhesive Glue";

    [JsonPropertyName("Delivery_Addresses")]
    public int DeliveryAddresses { get; set; } = 1;

    [JsonPropertyName("LineComment1")]
    public string LineComment1 { get; set; } = "1";
}

public class ContactTestData
{
    [JsonPropertyName("LastName")]
    public string LastName { get; set; } = "Allard";

    [JsonPropertyName("FirstName")]
    public string FirstName { get; set; } = "Guillaume";

    [JsonPropertyName("Initials")]
    public string Initials { get; set; } = "";

    [JsonPropertyName("Title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("PhoneNumber")]
    public string PhoneNumber { get; set; } = "+32 485 40 00 96";

    [JsonPropertyName("FaxNumber")]
    public string FaxNumber { get; set; } = "";

    [JsonPropertyName("GSMNumber")]
    public string GSMNumber { get; set; } = "";

    [JsonPropertyName("Email")]
    public string Email { get; set; } = "DWD@drukwerkdeal.nl";

    [JsonPropertyName("Function")]
    public string Function { get; set; } = "";
}

/// <summary>
/// Static class providing test data instances
/// </summary>
public static class TestDataProvider
{
    /// <summary>
    /// Gets the default test order data from F003ADB6G8.json
    /// </summary>
    public static OrderTestData GetDefaultOrderData()
    {
        return new OrderTestData
        {
            Description = "F003ADB6G8",
            ReferenceAtCustomer = "M45H4C226B",
            Delivery = "2025-03-07",
            ShipmentMethod = "PostNL EPS to Business",
            OrderQuantity = 1000,
            QuantityPerRoll = "",
            DispenserQuantity = 0,
            Core = "",
            UnitPrice = 37.18m,
            SupplierId = "Drukwerkdeal",
            Name = "Roll Stickers",
            Street = "Rue Saint Donat 6",
            Country = "BE",
            PostalCode = "5640",
            City = "Mettet",
            Contacts = new List<ContactTestData>
            {
                new ContactTestData
                {
                    LastName = "Allard",
                    FirstName = "Guillaume",
                    Initials = "",
                    Title = "",
                    PhoneNumber = "+32 485 40 00 96",
                    FaxNumber = "",
                    GSMNumber = "",
                    Email = "DWD@drukwerkdeal.nl",
                    Function = ""
                }
            },
            Width = "30,0",
            Height = "30,0",
            Shape = "Circle",
            Winding = 1,
            Radius = 0,
            PremiumWhite = "N",
            Substrate = "Adhesive Label Paper White Gloss",
            Adhesive = "Removable Adhesive Glue",
            DeliveryAddresses = 1,
            LineComment1 = "1"
        };
    }

    /// <summary>
    /// Gets test customer ID for testing
    /// </summary>
    public static string GetTestCustomerId() => "100001"; // Vila Etiketten

    /// <summary>
    /// Gets a unique test identifier for creating test entities
    /// </summary>
    public static string GetUniqueTestId()
    {
        return $"TEST_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8]}";
    }

    /// <summary>
    /// Creates a test address JSON payload for CERM API
    /// </summary>
    public static string CreateAddressJsonPayload(OrderTestData orderData, string? customerId = null)
    {
        var addressData = new
        {
            CustomerId = customerId ?? GetTestCustomerId(),
            Name = orderData.Name,
            Street = orderData.Street,
            PostalCode = orderData.PostalCode,
            City = orderData.City,
            Country = orderData.Country,
            IsDeliveryAddress = true,
            IsInvoiceAddress = false,
            Active = true
        };

        return System.Text.Json.JsonSerializer.Serialize(addressData, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }

    /// <summary>
    /// Creates a test calculation JSON payload for CERM API
    /// </summary>
    public static string CreateCalculationJsonPayload(OrderTestData orderData)
    {
        var calculationData = new
        {
            Description = orderData.Description,
            Reference = orderData.ReferenceAtCustomer,
            Quantity = orderData.OrderQuantity,
            DeliveryDate = orderData.Delivery,
            CustomerId = GetTestCustomerId()
        };

        return System.Text.Json.JsonSerializer.Serialize(calculationData, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }

    /// <summary>
    /// Creates a test product JSON payload for CERM API
    /// </summary>
    public static string CreateProductJsonPayload(OrderTestData orderData, string calculationId)
    {
        var productData = new
        {
            CalculationId = calculationId,
            Name = orderData.Name,
            Description = orderData.Description,
            Quantity = orderData.OrderQuantity,
            UnitPrice = orderData.UnitPrice,
            Width = orderData.Width,
            Height = orderData.Height,
            Shape = orderData.Shape,
            Substrate = orderData.Substrate,
            Adhesive = orderData.Adhesive
        };

        return System.Text.Json.JsonSerializer.Serialize(productData, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }

    /// <summary>
    /// Creates a test sales order JSON payload for CERM API
    /// </summary>
    public static string CreateSalesOrderJsonPayload(OrderTestData orderData, string customerId, string contactId)
    {
        var salesOrderData = new
        {
            CustomerId = customerId,
            ContactId = contactId,
            Reference = orderData.ReferenceAtCustomer,
            Description = orderData.Description,
            DeliveryDate = orderData.Delivery,
            ShipmentMethod = orderData.ShipmentMethod,
            OrderQuantity = orderData.OrderQuantity,
            UnitPrice = orderData.UnitPrice
        };

        return System.Text.Json.JsonSerializer.Serialize(salesOrderData, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }
}
