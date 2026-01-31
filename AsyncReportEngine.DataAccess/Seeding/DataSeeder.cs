using AsyncReportEngine.DataAccess.Context;
using AsyncReportEngine.Shared.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace AsyncReportEngine.DataAccess.Seeding;

public static class DataSeeder
{
    public static async Task SeedAsync(ReportDbContext context)
    {
        if (await context.Orders.AnyAsync())
        {
            return;
        }

        var shippingMethods = new List<ShippingMethod>
        {
            new() { Name = "Standard Delivery", BasePrice = 50, EstimatedDays = 5 },
            new() { Name = "Express Courier", BasePrice = 150, EstimatedDays = 1 },
            new() { Name = "Post Office", BasePrice = 30, EstimatedDays = 7 }
        };
        if (!context.ShippingMethods.Any())
        {
            await context.ShippingMethods.AddRangeAsync(shippingMethods);
            await context.SaveChangesAsync();
        }
        else
        {
            shippingMethods = await context.ShippingMethods.ToListAsync();
        }

        var categoryFaker = new Faker<Category>()
            .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0]);

        var categories = categoryFaker.Generate(20);
        if (!context.Categories.Any())
        {
            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
        else
        {
            categories = await context.Categories.ToListAsync();
        }

        var supplierFaker = new Faker<Supplier>()
            .RuleFor(s => s.CompanyName, f => f.Company.CompanyName())
            .RuleFor(s => s.ContactPerson, f => f.Name.FullName())
            .RuleFor(s => s.Email, f => f.Internet.Email());

        var suppliers = supplierFaker.Generate(50);
        if (!context.Suppliers.Any())
        {
            await context.Suppliers.AddRangeAsync(suppliers);
            await context.SaveChangesAsync();
        }
        else
        {
            suppliers = await context.Suppliers.ToListAsync();
        }

        var productFaker = new Faker<Product>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => decimal.Parse(f.Commerce.Price(10, 500)))
            .RuleFor(p => p.SKU, f => f.Commerce.Ean13())
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.CategoryId, f => f.PickRandom(categories).Id)
            .RuleFor(p => p.SupplierId, f => f.PickRandom(suppliers).Id);

        var products = productFaker.Generate(1000);
        if (!context.Products.Any())
        {
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            var reviewFaker = new Faker<ProductReview>()
                .RuleFor(r => r.Rating, f => f.Random.Int(1, 5))
                .RuleFor(r => r.Comment, f => f.Rant.Review())
                .RuleFor(r => r.CreatedAt, f => f.Date.Past(2));

            var reviews = new List<ProductReview>();
            foreach (var p in products)
            {
                if (new Random().NextDouble() > 0.7)
                {
                    var pReviews = reviewFaker.Generate(new Random().Next(1, 5));
                    pReviews.ForEach(r => r.ProductId = p.Id);
                }
            }
        }
        else
        {
            products = await context.Products.ToListAsync();
        }

        var addressFaker = new Faker<Address>()
            .RuleFor(a => a.Country, f => f.Address.Country())
            .RuleFor(a => a.City, f => f.Address.City())
            .RuleFor(a => a.Street, f => f.Address.StreetAddress())
            .RuleFor(a => a.ZipCode, f => f.Address.ZipCode());

        var customerFaker = new Faker<Customer>()
            .RuleFor(c => c.FirstName, f => f.Name.FirstName())
            .RuleFor(c => c.LastName, f => f.Name.LastName())
            .RuleFor(c => c.Email, f => f.Internet.Email())
            .RuleFor(c => c.Addresses, f => addressFaker.Generate(1));

        var customers = customerFaker.Generate(2000);
        if (!context.Customers.Any())
        {
            await context.Customers.AddRangeAsync(customers);
            await context.SaveChangesAsync();
        }
        else
        {
            customers = await context.Customers.ToListAsync();
        }

        var random = new Random();
        int totalOrders = 100000;
        int batchSize = 5000;

        for (int i = 0; i < totalOrders; i += batchSize)
        {
            var ordersBatch = new List<Order>();

            for (int j = 0; j < batchSize; j++)
            {
                var customer = customers[random.Next(customers.Count)];
                var shipping = shippingMethods[random.Next(shippingMethods.Count)];

                var order = new Order
                {
                    OrderDate = new Faker().Date.Past(2),
                    CustomerId = customer.Id,
                    ShippingMethodId = shipping.Id,
                    Items = new List<OrderItem>(),
                    Transactions = new List<PaymentTransaction>()
                };

                int itemsCount = random.Next(1, 6);
                decimal totalAmount = 0;

                for (int k = 0; k < itemsCount; k++)
                {
                    var product = products[random.Next(products.Count)];
                    var qty = random.Next(1, 4);
                    var price = product.Price;

                    order.Items.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = qty,
                        UnitPrice = price
                    });

                    totalAmount += price * qty;
                }

                totalAmount += shipping.BasePrice;
                order.TotalAmount = totalAmount;

                order.Transactions.Add(new PaymentTransaction
                {
                    TransactionDate = order.OrderDate.AddMinutes(random.Next(1, 60)),
                    Amount = totalAmount,
                    Status = "Success"
                });

                ordersBatch.Add(order);
            }

            await context.Orders.AddRangeAsync(ordersBatch);
            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();
        }

    }
}