using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Polly;
using ShoppingCart.ShoppingCart;

/* Apiary: Create new API endpoint, called 'products'.
 * In the editor, paste a json response, like this:
 *
# Polls API

Polls is a simple API allowing consumers to view polls and vote in them.

## Questions Collection [/questions]

### List All Questions [GET]

+ Response 200 (application/json)

        [
            {
                "productId": "1",
                "productName": "Basic t-shirt",
                "productDescription": "a quiet t-shirt",
                "price": { "amount" : 40, "currency": "eur" },
                "attributes" : [
                {
                    "sizes": [ "s", "m", "l"],
                    "colors": ["red", "blue", "green"]
                }]
            },
            {
                "productId": "2",
                "productName": "Fancy shirt",
                "productDescription": "a loud t-shirt",
                "price": { "amount" : 50, "currency": "eur" },
                "attributes" : [
                {
                    "sizes": [ "s", "m", "l", "xl"],
                    "colors": ["ALL", "Batique"]
                }]
            }
        ]
 * The API endpoint URL is for example
 * http://private-bb9b4e-products100.apiary-mock.com/questions, where
 * question is from above ([/questions])
 */
namespace ShoppingCart
{
    public class ProductCatalogueClient : IProductCatalogueClient
    {
        private static readonly Policy ExponentialRetryPolicy = Policy.Handle<Exception>()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)), (ex, _) => Console.WriteLine(ex.ToString()));

        private const string ProductCatalogueBaseUrl = @"http://private-bb9b4e-products100.apiary-mock.com";

        private const string GetProductPathTemplate = "/products?productIds=[{0}]";

        public Task<IEnumerable<ShoppingCartItem>> GetShoppingCartItems(int[] productCatalogueIds) =>
            ExponentialRetryPolicy
                .ExecuteAsync(async () => await GetItemsFromCatalogueService(productCatalogueIds)
                                              .ConfigureAwait(false));

        private static async Task<IEnumerable<ShoppingCartItem>> GetItemsFromCatalogueService(int[] productCatalogueIds)
        {
            var response = await RequestProductFromProductCatalogue(productCatalogueIds)
                               .ConfigureAwait(false);
            return await ConvertToShoppingCartItems(response)
                       .ConfigureAwait(false);
        }

        private static async Task<HttpResponseMessage> RequestProductFromProductCatalogue(int[] productCatalogueIds)
        {
            var productsResource = string.Format(GetProductPathTemplate, string.Join(",", productCatalogueIds));
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(ProductCatalogueBaseUrl);
                return await httpClient.GetAsync(productsResource)
                           .ConfigureAwait(false);
            }
        }

        private static async Task<IEnumerable<ShoppingCartItem>> ConvertToShoppingCartItems(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            var products =
                JsonConvert.DeserializeObject<List<ProductCatalogueProduct>>(await response.Content.ReadAsStringAsync()
                                                                                 .ConfigureAwait(false));
            return
                products
                    .Select(p => new ShoppingCartItem(
                                int.Parse(p.ProductId),
                                p.ProductName,
                                p.ProductDescription,
                                p.Price
                            ));
        }

        private class ProductCatalogueProduct
        {
            public string ProductId { get; set; }
            public string ProductName { get; set; }
            public string ProductDescription { get; set; }
            public Money Price { get; set; }
        }
    }
}