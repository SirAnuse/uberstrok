using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using UberStrok.Core.Common;
using UberStrok.Core.Views;

namespace UberStrok.WebServices.Core
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ShopWebService : BaseShopWebService
    {
        private readonly static ILog Log = LogManager.GetLogger(typeof(ShopWebService).Name);

        public ShopWebService(WebServiceContext ctx) : base(ctx)
        {
            // Space
        }

        public override BuyItemResult OnBuyItem(int itemId, string authToken, UberStrikeCurrencyType currencyType, BuyingDurationType durationType, UberStrikeItemType itemType, BuyingLocationType marketLocation, BuyingRecommendationType recommendationType)
        {
            var member = Context.Users.GetMember(authToken);
            if (member == null)
            {
                Log.Error("An unidentified AuthToken was passed.");
                return BuyItemResult.InvalidData;
            }

            var cmid = member.PublicProfile.Cmid;
            var inventory = Context.Users.Db.Inventories.Load(member.PublicProfile.Cmid);

            // This is the only thing we need this for now.
            // We only want to search when necessary, as this is rather inefficient.
            var itemAmount = -1;
            if (itemType == UberStrikeItemType.QuickUse)
                itemAmount = GetItemAmount(itemId, itemType);

            inventory.Add(new ItemInventoryView(itemId, null, itemAmount, cmid));

            Context.Users.Db.Inventories.Save(cmid, inventory);
            return BuyItemResult.OK;
        }

        public int GetItemAmount(int itemId, UberStrikeItemType type)
        {
            var shop = Context.Items.GetShop();

            foreach (var i in shop.QuickItems)
                if (i.ID == itemId && i.Prices.Count > 0)
                    return i.Prices.ToArray()[0].Amount;

            return -1;
        }

        public override UberStrikeItemShopClientView OnGetShop()
        {
            return Context.Items.GetShop();
        }
    }
}
