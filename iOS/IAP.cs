using System;
using System.Collections.Generic;
using System.Linq;

using UIKit;
using StoreKit;
using Foundation;

namespace scripting.iOS
{
    public delegate void OnIAP(string result);

    public class IAP
    {
        public static OnIAP IAPError;
        public static OnIAP    IAPOK;

        static PurchaseManager m_iap;
        static CustomPaymentObserver m_observer;
        static NSObject m_priceObserver, m_requestObserver;

        static Dictionary<string, bool> m_products = new Dictionary<string, bool>();

        static Dictionary<string, string> m_id2Description = new Dictionary<string, string>();

        static bool m_pricesLoaded;

        // The product we are currently buying. If null, then we are in the restore mode.
        static NSString m_buyingProductId;

        public static bool Init()
        {
            if (m_iap != null) {
                return true;
            }
            //m_removeAdsProductId = new NSString(AdsID);
            m_iap      = new PurchaseManager();
            m_observer = new CustomPaymentObserver(m_iap);

            // Call this once upon startup of in-app-purchase activities
            // This also kicks off the TransactionObserver which handles the various communications
            SKPaymentQueue.DefaultQueue.AddTransactionObserver(m_observer);

            return true;
        }

        public static void AddProductId(string productId)
        {
            m_products[productId] = false; // Hasn't been purchased
        }
        public static void MarkPurchased(string productId)
        {
            m_products[productId] = true;
        }
        public static bool ProductPurchased(string productId)
        {
            bool purchased = false;
            m_products.TryGetValue(productId, out purchased);
            return purchased;
        }

        public static void ContinuePurchaseOrRestore(NSDictionary info)
        {
            if (info == null || info.Count == 0) {
                Console.WriteLine("No products found in iTunes");
                IAPError?.Invoke("No products found in iTunes");
                return;
            }

            foreach (var key in info.Keys) {
                var product = (SKProduct)info[key];
                Print(product);
            }

            if (m_buyingProductId == null) {
                m_pricesLoaded = true;
                m_iap.Restore();
            } else if (info.ContainsKey(m_buyingProductId)) {
                m_pricesLoaded = true;
                m_iap.PurchaseProduct(m_buyingProductId);
            }
        }

        public static void Purchase(string productId)
        {
            Init();
            m_buyingProductId = new NSString(productId);

            m_priceObserver = NSNotificationCenter.DefaultCenter.AddObserver(
                PurchaseManager.InAppPurchaseManagerTransactionSucceededNotification,
                (notification) => {
                    // update the buttons after a successful purchase
                    Console.WriteLine("Success Purchasing!");
                    MarkPurchased(productId);
                    IAPOK?.Invoke(productId);
                });

            m_requestObserver = NSNotificationCenter.DefaultCenter.AddObserver(
                PurchaseManager.InAppPurchaseManagerRequestFailedNotification,
                (notification) => {
                    Console.WriteLine("Purchase Request Failed");
                    IAPError?.Invoke(notification.ToString());
                });
            
            SKPaymentQueue.DefaultQueue.AddTransactionObserver(m_observer);
            if (!m_pricesLoaded) {
                m_iap.RequestProductData(m_products);
            } else {
                m_iap.PurchaseProduct(m_buyingProductId);
            }
        }

        public static void Restore()
        {
            Init();
            m_buyingProductId = null;

            SKPaymentQueue.DefaultQueue.AddTransactionObserver(m_observer);
            if (!m_pricesLoaded) {
                m_iap.RequestProductData(m_products);
            } else {
                m_iap.Restore();
            }
        }

        public static string GetDescription(string productID)
        {
            string desc = "";
            m_id2Description.TryGetValue(productID, out desc);
            return desc;
        }
        public static bool RestoreTransaction(SKPaymentQueue queue)
        {
            var count = queue.Transactions.Length;
            foreach (var trans in queue.Transactions) {
                Console.WriteLine(" ** RESTORE {0} ", trans.Payment.ProductIdentifier);
                string restoredID = ProcessRestoredItem(trans);
                if (string.IsNullOrEmpty(restoredID)) {
                    continue;
                }
                MarkPurchased(restoredID);
                IAPOK?.Invoke(restoredID);
                count++;
            }
            if (count == 0) {
                IAPOK?.Invoke("");
                return false;
            }

            return true;
        }

        public static string ProcessRestoredItem(SKPaymentTransaction transaction)
        {
            string restoredID = transaction.Payment.ProductIdentifier;
            Console.WriteLine(" ** RESTORING {0} ", restoredID);
            bool restored = transaction.TransactionState == SKPaymentTransactionState.Restored;
            if (!restored) {
                return "";
            }
            return IAP.GetDescription(restoredID);
        }

        public static void Print(SKProduct product)
        {
            Console.WriteLine("Product id: {0}", product.ProductIdentifier);
            Console.WriteLine("Product title: {0}", product.LocalizedTitle);
            Console.WriteLine("Product description: {0}", product.LocalizedDescription);
            Console.WriteLine("Product price: {0}", product.Price);

            m_id2Description[product.ProductIdentifier] = product.LocalizedTitle;
        }
    }
    public class PurchaseManager : SKProductsRequestDelegate
    {
        public static readonly NSString InAppPurchaseManagerProductsFetchedNotification = new NSString("InAppPurchaseManagerProductsFetchedNotification");
        public static readonly NSString InAppPurchaseManagerTransactionFailedNotification = new NSString("InAppPurchaseManagerTransactionFailedNotification");
        public static readonly NSString InAppPurchaseManagerTransactionSucceededNotification = new NSString("InAppPurchaseManagerTransactionSucceededNotification");
        public static readonly NSString InAppPurchaseManagerRequestFailedNotification = new NSString("InAppPurchaseManagerRequestFailedNotification");

        protected SKProductsRequest ProductsRequest { get; set; }

        // Verify that the iTunes account can make this purchase for this application
        public bool CanMakePayments()
        {
            return SKPaymentQueue.CanMakePayments;
        }

        // request multiple products at once
        public void RequestProductData(Dictionary<string, bool> productIds)
        {
            NSString[] array = productIds.Keys.Select(pId => (NSString)pId).ToArray();
            NSSet productIdentifiers = NSSet.MakeNSObjectSet<NSString>(array);

            //set up product request for in-app purchase
            ProductsRequest = new SKProductsRequest(productIdentifiers);
            ProductsRequest.Delegate = this; // SKProductsRequestDelegate.ReceivedResponse
            ProductsRequest.Start();
        }

        // received response to RequestProductData - with price,title,description info
        public override void ReceivedResponse(SKProductsRequest request, SKProductsResponse response)
        {
            SKProduct[] products = response.Products;

            NSMutableDictionary userInfo = new NSMutableDictionary();
            //NSDictionary userInfo = new NSDictionary();
            for (int i = 0; i < products.Length; i++) {
                userInfo.Add((NSString)products[i].ProductIdentifier, products[i]);
            }
            NSNotificationCenter.DefaultCenter.PostNotificationName(InAppPurchaseManagerProductsFetchedNotification, this, userInfo);

            Console.WriteLine(" ** ReceivedResponse ** ");
            IAP.ContinuePurchaseOrRestore(userInfo);

            foreach (string invalidProductId in response.InvalidProducts) {
                Console.WriteLine("Invalid product id: {0}", invalidProductId);
            }
        }

        public void PurchaseProduct(string appStoreProductId)
        {
            Console.WriteLine("PurchaseProduct {0}", appStoreProductId);
            SKPayment payment = SKPayment.PaymentWithProduct(appStoreProductId);
            SKPaymentQueue.DefaultQueue.AddPayment(payment);
        }

        public void FailedTransaction(SKPaymentTransaction transaction)
        {
            //SKErrorPaymentCancelled == 2
            Console.WriteLine(" ** FailedTransaction ** {0} {1}",
                              transaction.Error.Code, transaction.Error.LocalizedDescription);
            bool cancelled = transaction.Error.Code == 2;
            if (!cancelled) {
                string msg = "Failed: " + transaction.Error.LocalizedDescription;
                Console.WriteLine(msg);
                IAP.IAPError?.Invoke(transaction.Error.LocalizedDescription);
            }
            FinishTransaction(transaction, false);
        }

        public void CompleteTransaction(SKPaymentTransaction transaction)
        {
            Console.WriteLine("CompleteTransaction {0}", transaction.TransactionIdentifier);
            string productId = transaction.Payment.ProductIdentifier;

            FinishTransaction(transaction, true);
        }

        public void FinishTransaction(SKPaymentTransaction transaction, bool wasSuccessful = true)
        {
            Console.WriteLine("FinishTransaction {0}", wasSuccessful);
            // remove the transaction from the payment queue.
            // THIS IS IMPORTANT - LET'S APPLE KNOW WE'RE DONE !!!!
            SKPaymentQueue.DefaultQueue.FinishTransaction(transaction); 

            NSDictionary userInfo = new NSDictionary("transaction", transaction);
            var notificationKey = wasSuccessful ? InAppPurchaseManagerTransactionSucceededNotification : InAppPurchaseManagerTransactionFailedNotification;
            NSNotificationCenter.DefaultCenter.PostNotificationName(notificationKey, this, userInfo);
        }

        /// <summary>
        /// Probably could not connect to the App Store (network unavailable?)
        /// </summary>
        public override void RequestFailed(SKRequest request, NSError error)
        {
            Console.WriteLine(" ** RequestFailed ** {0}", error.LocalizedDescription);

            // send out a notification for the failed transaction
            NSDictionary userInfo = new NSDictionary("error", error);
            NSNotificationCenter.DefaultCenter.PostNotificationName(InAppPurchaseManagerRequestFailedNotification, this, userInfo);
            IAP.IAPError?.Invoke(error.LocalizedDescription);
        }

        /// <summary>
        /// Restore any transactions that occurred for this Apple ID, either on
        /// this device or any other logged in with that account.
        /// </summary>
        public void Restore()
        {
            Console.WriteLine(" ** Restore **");
            // theObserver will be notified of when the restored transactions start arriving <- AppStore
            SKPaymentQueue.DefaultQueue.RestoreCompletedTransactions();
        }

        public virtual void RestoreTransaction(SKPaymentTransaction transaction)
        {
            // Restored Transactions always have an 'original transaction' attached
            Console.WriteLine("RestoreTransaction {0}; OriginalTransaction {1}", transaction.TransactionIdentifier, transaction.OriginalTransaction.TransactionIdentifier);
            //string productId = transaction.OriginalTransaction.Payment.ProductIdentifier;
            IAP.ProcessRestoredItem(transaction);
            //UtilsIOS.ShowToast("Restored", UIColor.Green, 5.0f);
            //AppDelegate.Restore(productId);
            FinishTransaction(transaction, true);
        }
    }
    internal class CustomPaymentObserver : SKPaymentTransactionObserver
    {
        private PurchaseManager theManager;

        public CustomPaymentObserver(PurchaseManager manager)
        {
            theManager = manager;
        }

        // called when the transaction status is updated
        public override void UpdatedTransactions(SKPaymentQueue queue, SKPaymentTransaction[] transactions)
        {
            Console.WriteLine("UpdatedTransactions");
            foreach (SKPaymentTransaction transaction in transactions) {
                switch (transaction.TransactionState) {
                    case SKPaymentTransactionState.Purchasing:
                        Console.WriteLine("Purchasing");
                        break;
                    case SKPaymentTransactionState.Purchased:
                        theManager.CompleteTransaction(transaction);
                        break;
                    case SKPaymentTransactionState.Deferred:
                        Console.WriteLine("Deferred");
                        break;
                    case SKPaymentTransactionState.Failed:
                        theManager.FailedTransaction(transaction);
                        break;
                    case SKPaymentTransactionState.Restored:
                        theManager.RestoreTransaction(transaction);
                        break;
                    default:
                        break;
                }
            }
        }

        public override void PaymentQueueRestoreCompletedTransactionsFinished(SKPaymentQueue queue)
        {
            Console.WriteLine(" ** RESTORE PaymentQueueRestoreCompletedTransactionsFinished ");
            IAP.RestoreTransaction(queue);
        }
        public override void RestoreCompletedTransactionsFinished(SKPaymentQueue queue)
        {
            // Restore succeeded
            Console.WriteLine(" ** RESTORE RestoreCompletedTransactionsFinished ");
            IAP.RestoreTransaction(queue);
        }

        public override void RestoreCompletedTransactionsFailedWithError(SKPaymentQueue queue, NSError error)
        {
            Console.WriteLine(" ** RESTORE RestoreCompletedTransactionsFailedWithError " + error.LocalizedDescription);
            IAP.IAPError?.Invoke(error.LocalizedDescription);
        }
    }
}
