using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;

namespace scripting
{
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

    public static void AddProductId(string productId)
    {
      m_availableProducts.Add(productId); // Hasn't been purchased
    }
    static bool ConnectionFailed()
    {
      OnIAPError?.Invoke("Couldn't connect to the App Store");
      return false;
    }
    public static string GetDescription(string productId)
    {
      string desciption;
      if (!m_id2Description.TryGetValue(productId, out desciption)) {
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
      if (!string.IsNullOrEmpty(productId) && m_id2Description.ContainsKey(productId)) {
        return;
      }
      var billing = CrossInAppBilling.Current;
      try {
        var connected = await billing.ConnectAsync();
        if (!connected) {
          ConnectionFailed();
          return;
        }

        var items = await billing.GetProductInfoAsync(ItemType.InAppPurchase,
                                                      m_availableProducts.ToArray());
        if (items == null) {
          return;
        }
        foreach (var item in items) {
          Console.WriteLine("Got {0}: [{1}] {2} {3}",
                  item.ProductId, item.Description, item.LocalizedPrice, item.Name);
          m_id2Description[item.ProductId] = item.Name;
          //if (item.)
          //await billing.ConsumePurchaseAsync(item.ProductId, ItemType.InAppPurchase, "");
        }
      } catch (Exception ex) {
        Console.WriteLine("Error getting AppStore data: " + ex.Message);
        OnIAPError?.Invoke(ex.Message);
      } finally {
        await billing.DisconnectAsync();
      }
    }
    public static async Task<bool> PurchaseItem(string productId, string payload)
    {
      await GetInventory(productId);

      bool alreadyPurchased = await WasItemPurchased(productId);
      if (alreadyPurchased) {
        OnIAPOK?.Invoke(productId);
        return true;
      }

      var billing = CrossInAppBilling.Current;
      try {
        var connected = await billing.ConnectAsync();
        if (!connected) {
          return ConnectionFailed();
        }

        var purchase = await billing.PurchaseAsync(productId,
            ItemType.InAppPurchase, payload);

        //possibility that a null came through.
        if (purchase == null) {
          OnIAPError?.Invoke("Couldn't purchase " + productId);
          return false;
        } else if (purchase.State == PurchaseState.Purchased) {
          ProductPurchased(productId);
          return true;
        }
      } catch (Exception ex) {
        Console.WriteLine("Error: " + ex.Message);
        OnIAPError?.Invoke(ex.Message);
      } finally {
        await billing.DisconnectAsync();
      }
      return false;
    }

    public static async Task Restore()
    {
      await GetInventory();

      string restored = "";
      foreach (string productId in m_availableProducts) {
        bool purchased = await WasItemPurchased(productId);
        if (purchased) {
          if (!string.IsNullOrEmpty(restored)) {
            restored += ",";
          }
          restored += productId;
        }
      }

      OnIAPOK?.Invoke(restored);
    }

    static async Task<bool> WasItemPurchased(string productId)
    {
      if (m_purchasedProducts.Contains(productId)) {
        return true;
      }
      var billing = CrossInAppBilling.Current;
      try {
        var connected = await billing.ConnectAsync();
        if (!connected) {
          return ConnectionFailed();
        }

        //check purchases
        var purchases = await billing.GetPurchasesAsync(ItemType.InAppPurchase);

        //check for null just incase
        if (purchases?.Any(p => p.ProductId == productId) ?? false) {
          m_purchasedProducts.Add(productId);
          return true;
        } else {
          //no purchases found
          return false;
        }
      } catch (Exception ex) {
        Console.WriteLine("Error: " + ex);
      } finally {
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
