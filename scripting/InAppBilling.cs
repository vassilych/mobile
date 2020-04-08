using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;
using SplitAndMerge;

namespace scripting
{
    public class PurchaseFunction2 : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name, true);

            string strAction = args[0].AsString();
            string productId = args[1].AsString();

            InAppBilling.RegisterCallbacks(strAction);
            InAppBilling.PurchaseItem(productId);

            return Variable.EmptyInstance;
        }
    }
    public class RestoreFunction2 : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);
            string strAction = args[0].AsString();

            for (int i = 1; i < args.Count; i++)
            {
                string productId = Utils.GetSafeString(args, i);
                InAppBilling.AddProductId(productId);
            }

            InAppBilling.RegisterCallbacks(strAction);
            InAppBilling.Restore();

            return Variable.EmptyInstance;
        }
    }
    public class ProductIdDescriptionFunction2 : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name, true);

            string productId = args[0].AsString();

            string description = InAppBilling.GetDescription(productId);

            return new Variable(description);
        }
    }

    public class InAppBilling
    {
        public static Action<string> OnIAPOK;
        public static Action<string> OnIAPError;

        public static int IAP_REQUEST = 1001;

        static string m_key;
        static bool m_pricesLoaded;
        // The product we are currently buying. If null, then we are in the restore mode.
        static string m_buyingProductId;

        static HashSet<string> m_availableProducts = new HashSet<string>();
        static HashSet<string> m_purchasedProducts = new HashSet<string>();
        static Dictionary<string, string> m_id2Description = new Dictionary<string, string>();

        static bool s_errorConnecting;
        static string s_prevAction;

        public static void  RegisterCallbacks(string strAction)
        {
            OnIAPOK = null;
            OnIAPError = null;

            InAppBilling.OnIAPOK += (productIds) =>
            {
                UIVariable.GetAction(strAction, "", productIds);
            };
            InAppBilling.OnIAPError += (errorStr) =>
            {
                UIVariable.GetAction(strAction, errorStr, "");
            };
            s_prevAction = strAction;
        }

        public static void AddProductId(string productId)
        {
            m_availableProducts.Add(productId); // Hasn't been purchased
        }
        static bool ConnectionFailed()
        {
            s_errorConnecting = true;
            OnIAPError?.Invoke("Couldn't connect to the App Store");
            return false;
        }
        public static string GetDescription(string productId)
        {
            string desciption;
            if (!m_id2Description.TryGetValue(productId, out desciption))
            {
                desciption = productId;
            }
            return desciption;
        }
        static bool ProductPurchased(string productId)
        {
            m_purchasedProducts.Add(productId);
            OnIAPOK?.Invoke(productId);
            return true;
        }
        public async static Task GetInventory(string productId = "")
        {
            if (!string.IsNullOrEmpty(productId) && m_id2Description.ContainsKey(productId))
            {
                return;
            }
            var billing = CrossInAppBilling.Current;
            try
            {
                var connected = await billing.ConnectAsync();
                if (!connected)
                {
                    ConnectionFailed();
                    return;
                }

                var items = await billing.GetProductInfoAsync(ItemType.InAppPurchase,
                                                              m_availableProducts.ToArray());
                if (items == null)
                {
                    return;
                }
                foreach (var item in items)
                {
                    Console.WriteLine("Got {0}: [{1}] {2} {3}",
                            item.ProductId, item.Description, item.LocalizedPrice, item.Name);
                    m_id2Description[item.ProductId] = item.Name;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting AppStore data: " + ex.Message);
                OnIAPError?.Invoke(ex.Message);
            }
            finally
            {
                await billing.DisconnectAsync();
            }
        }
        public static async Task<bool> PurchaseItem(string productId, string payload = "notused")
        {
            s_errorConnecting = false;
            await GetInventory(productId);

            bool alreadyPurchased = await WasItemPurchased(productId);
            if (alreadyPurchased)
            {
                OnIAPOK?.Invoke(productId);
                return true;
            }
            if (s_errorConnecting)
            {
                return false;
            }

            var billing = CrossInAppBilling.Current;
            try
            {
                var connected = await billing.ConnectAsync();
                if (!connected)
                {
                    return ConnectionFailed();
                }

                var purchase = await billing.PurchaseAsync(productId,
                    ItemType.InAppPurchase, payload);

                //possibility that a null came through.
                if (purchase == null)
                {
                    OnIAPError?.Invoke("Couldn't purchase " + productId);
                    return false;
                }
                else if (purchase.State == PurchaseState.Purchased)
                {
                    ProductPurchased(productId);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                OnIAPError?.Invoke(ex.Message);
            }
            finally
            {
                await billing.DisconnectAsync();
            }
            return false;
        }

        public static async Task Restore()
        {
            s_errorConnecting = false;
            await GetInventory();

            string restored = "";
            foreach (string productId in m_availableProducts)
            {
                bool purchased = await WasItemPurchased(productId);
                if (s_errorConnecting)
                {
                    return;
                }
                if (purchased)
                {
                    if (!string.IsNullOrEmpty(restored))
                    {
                        restored += ",";
                    }
                    restored += productId;
                }
            }

            OnIAPOK?.Invoke(restored);
        }

        static async Task<bool> WasItemPurchased(string productId)
        {
            if (m_purchasedProducts.Contains(productId))
            {
                return true;
            }
            var billing = CrossInAppBilling.Current;
            try
            {
                var connected = await billing.ConnectAsync();
                if (!connected)
                {
                    return ConnectionFailed();
                }

                //check purchases
                var purchases = await billing.GetPurchasesAsync(ItemType.InAppPurchase);

                //check for null just incase
                if (purchases?.Any(p => p.ProductId == productId) ?? false)
                {
                    m_purchasedProducts.Add(productId);
                    return true;
                }
                else
                {
                    //no purchases found
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                s_errorConnecting = true;
                OnIAPError?.Invoke(ex.Message);
            }
            finally
            {
                await billing.DisconnectAsync();
            }

            return false;
        }


#if __ANDROID__
    public static void OnIAPCallback(int requestCode, Android.App.Result resultCode, Android.Content.Intent data)
    {
      // For Plugin.InAppBilling.
      try {
        Plugin.InAppBilling.InAppBillingImplementation.HandleActivityResult(requestCode, resultCode, data);
      } catch (Exception exc) {
        Console.WriteLine("Got exception IAP: {0}:", exc);
      }
    }
#endif
    }
}
