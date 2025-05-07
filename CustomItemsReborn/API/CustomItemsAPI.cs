namespace CustomItems.API;

using System;
using System.Collections.Generic;
using Exiled.API.Features;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Enums;
using PlayerRoles;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;

public abstract class CustomItemsAPI
{
    public abstract string ItemName { get; }
    public abstract ItemType ItemType { get; }
    public abstract string PickupBroadcast { get; }
    public abstract string ChangeHint { get; }
    public abstract List<ushort> ItemList { get; }

    public static List<ushort> CreatedCustomItems = new List<ushort>();


    #region Hints

    private void PickingUpItem(PickingUpItemEventArgs ev)
    {
        if (ev.Pickup is null || !IsSelectedCustomItem(ev.Pickup.Serial, ItemList) || ev.Pickup.Type != ItemType) return;
        {
            ev.Player.Broadcast(3, PickupBroadcast);
        }
    }

    private void Player_ChangedItem(ChangedItemEventArgs ev)
    {
        if (!(ev.Item is null) && !IsSelectedCustomItem(ev.Item.Serial, ItemList)) ;
                ev.Player.ShowHint(ChangeHint, 5f);
        
    }

    public static bool IsSelectedCustomItem(ushort serial, List<ushort> itemList)
    {
        if (!CreatedCustomItems.Contains(serial) || itemList.Contains(serial))
        {
            return false;
        }
        return true;
    }

    #endregion
}