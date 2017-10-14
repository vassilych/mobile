using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.OS;
using Android.Widget;

using Xamarin.InAppBilling;
using Xamarin.InAppBilling.Utilities;

namespace scripting.Droid
{
    public delegate void OnIAP(string result);

    public class IAP
    {
        public static OnIAP IAPError;
        public static OnIAP IAPOK;

        public static int IAP_REQUEST = 1001;

        static string m_key;
        static bool m_pricesLoaded;
        // The product we are currently buying. If null, then we are in the restore mode.
        static string m_buyingProductId;

        static IList<Product> m_availableProducts = null;
        static Dictionary<string, bool> m_products = new Dictionary<string, bool>();
        static InAppBillingServiceConnection m_serviceConnection;

        public static void Init(List<string> keyParts)
        {
            // Re-assemble the bfusticated key (parts are in the reverse order).
            List<int> order = new List<int>();
            for (int i = 0; i < keyParts.Count; i++) {
                order.Add(keyParts.Count - i - 1);
            }
            m_key = Security.Unify(keyParts.ToArray(), order.ToArray());

            // Create a new connection to the Google Play Service
            m_serviceConnection = new InAppBillingServiceConnection(MainActivity.TheView, m_key);
            m_serviceConnection.OnInAppBillingError += (error, message) => {
                PurchaseError(string.Format("{0}: {1}", error, message));
            };
            m_serviceConnection.OnConnected += async () => {
                Console.WriteLine("Connected to IAP!");

                m_serviceConnection.BillingHandler.BuyProductError += (responseCode, sku) => {
                    if (responseCode == BillingResult.ItemAlreadyOwned) {
                        Console.WriteLine("IAP Product already Purchased!");
                        PurchaseDone(sku);
                        return;
                    }
                    PurchaseError(string.Format("IAP BuyProductError {0}: {1}", responseCode, sku));
                };
                m_serviceConnection.BillingHandler.OnGetProductsError += (int responseCode, Bundle ownedItems) => {
                    PurchaseError(string.Format("Getting Products Error {0}", responseCode)); 
                };
                m_serviceConnection.BillingHandler.OnUserCanceled += () => {
                    Console.WriteLine("IAP OnUserCanceled");
                    CancelPurchase();
                };
                m_serviceConnection.BillingHandler.OnPurchaseFailedValidation += (purchase, purchaseData, purchaseSignature) => {
                    Console.WriteLine("IAP OnPurchaseFailedValidation {0} {1} {2}", purchase, purchaseData, purchaseSignature);
                    CancelPurchase();
                };
                m_serviceConnection.BillingHandler.OnProductPurchasedError += (int responseCode, string sku) => {
                    PurchaseError(string.Format("Buying Products Error {0} {1}", responseCode, sku));
                };
                m_serviceConnection.BillingHandler.InAppBillingProcesingError += (message) => {
                    PurchaseError(string.Format("Billing Processing Error: {0}", message));
                };
                m_serviceConnection.BillingHandler.OnPurchaseConsumedError += (int responseCode, string token) => {
                    PurchaseError(string.Format("Purchase Consumption Error {0} {1}", responseCode, token));
                };

                m_serviceConnection.BillingHandler.OnProductPurchased +=
                                       (response, purchase, purchaseData, purchaseSignature) => {
                                           Console.WriteLine("IAP ProductPurchased {0} {1} {2}", purchase, purchaseData, purchaseSignature);
                                           PurchaseDone(purchase.ProductId);
                                       };

                // Load inventory or available products
                await GetInventory();

                AfterConnection();
            };
        }

        public static void AddProductId(string productId)
        {
            m_products[productId] = false; // Hasn't been purchased
        }
        static void PurchaseDone(string productId)
        {
            Console.WriteLine("IAP ProductPurchased: {0}", productId);
            DisconnectFromPlayStore();
            m_products[productId] = true;
            IAPOK?.Invoke(productId);
        }
        static void CancelPurchase()
        {
            m_buyingProductId = null;
        }

        static void AfterConnection()
        {
            // Load any items already purchased
            if (!m_pricesLoaded || m_buyingProductId == null) {
                LoadPurchasedItems();
            }

            if (!string.IsNullOrWhiteSpace(m_buyingProductId)) {
                Purchase(m_buyingProductId);
            }
        }

        static void PurchaseError(string errorMsg)
        {
            Console.WriteLine(errorMsg);
            IAPError?.Invoke(errorMsg);
            CancelPurchase();
        }

        public static void Restore()
        {
            m_buyingProductId = null;
            ConnectToPlayStore();
        }

        public static void Purchase(string productId)
        {
            bool alreadyPurchased = false;
            if (m_products.TryGetValue(productId, out alreadyPurchased) &&
                alreadyPurchased) {
                PurchaseDone(productId);
                return;
            }

            m_buyingProductId = productId;
            if (!m_serviceConnection.Connected) {
                ConnectToPlayStore();
                return;
            }

            Product toBuy = null;
            foreach (Product p in m_availableProducts) {
                if (p.ProductId.Equals(productId, StringComparison.OrdinalIgnoreCase)) {
                    toBuy = p;
                    break;
                }
            }
            if (toBuy == null) {
                PurchaseError(string.Format("IAP: No product {0} found in the PlayStore!", productId));
                return;
            }
            Console.WriteLine("IAP: Buying {0}", toBuy);
            m_serviceConnection.BillingHandler.BuyProduct(toBuy);
        }
        static void ConnectToPlayStore()
        {
            // Attempt to connect to the service
            if (m_serviceConnection != null && !m_serviceConnection.Connected) {
                m_serviceConnection.Connect();
            } else {
                AfterConnection();
            }
        }
        static void DisconnectFromPlayStore()
        {
            m_buyingProductId = null;
            if (m_serviceConnection != null && m_serviceConnection.Connected) {
                m_serviceConnection.Disconnect();
            }
        }

        public static void OnIAPCallback(int requestCode, Result resultCode, Intent data)
        {
            // Ask the open service connection's billing handler to process this request
            m_serviceConnection.BillingHandler.HandleActivityResult(requestCode, resultCode, data);
            //Also possible: Use a call back to update the purchased items
            //UpdatePurchasedItems();
        }

        async static Task GetInventory()
        {
            // Ask the open connection's billing handler to return a list of avilable products for the 
            // given list of items.
            // NOTE: We are asking for the Reserved Test Product IDs that allow you to test In-App
            // Billing without actually making a purchase.

            List<string> requestProducts = m_products.Where(p => p.Value == false).Select(p => p.Key).ToList();
            if (m_buyingProductId == null) {
                requestProducts.Add(ReservedTestProductIDs.Purchased);
            }
            m_availableProducts = await m_serviceConnection.BillingHandler.QueryInventoryAsync(
                requestProducts, ItemType.Product);

            // Were any products returned?
            if (m_availableProducts == null) {
                m_availableProducts = new List<Product>();
                Console.WriteLine("No IAP Products Available");
                return;
            }

            // Populate list of available products
            var items = m_availableProducts.Select(p => p.ToString()).ToList();
            Console.WriteLine("Got {0} IAP Products Available: {1}", m_availableProducts.Count,
                              String.Join(",", items));
            foreach (Product p in m_availableProducts) {
                Console.WriteLine("Got {0}: [{1}] for {2} {3} ({4}): [{5}] {6}",
                                  p.ProductId, p.Description, p.Price_Currency_Code, p.Price,
                                  p.Price_Amount_Micros, p.Title, p.Type);
            }

        }

        static void LoadPurchasedItems()
        {
            // Ask the open connection's billing handler to get any purchases
            var purchases = m_serviceConnection.BillingHandler.GetPurchases(ItemType.Product);
            m_pricesLoaded = true;
            if (purchases == null || purchases.Count == 0) {
                Console.WriteLine("No IAP Purchased Products");
                IAPOK?.Invoke("");
                return;
            }
            Console.WriteLine("Got {0} IAP Purchased Products: {1}",
                              purchases.Count, String.Join(",", purchases));
            foreach (Purchase p in purchases) {
                PurchaseDone(p.ProductId);
                if (p.ProductId.Equals("ilanguage_ads_removal", StringComparison.OrdinalIgnoreCase)) {
                    // A Hack to get rid of test purchases: just consume them.
                    bool result = m_serviceConnection.BillingHandler.ConsumePurchase(p);
                    Console.WriteLine("Canceled purchase: {0}", result);
                }
            }
        }
    }
}
