    
/* Author:      Sklug a.k.a TheGenomeWhisperer
|       	    The following functions are commonly called to be used as a "help"
|
| NOTE:         For common scripting events as part of questing profile behaviors.
| NOTE:     	"ExecuteLua" API function executes "Lua" code language inserted into the C#
| NOTE:     	ALSO!!! This is not a more standardized API with setters and getters, which ultimately would be nice,
| NOTE:	    	but I am writing more focused script functions specifically for the questing profiles as I build them.
|               So, please understand if it is not 100% traditional coding practices.
| Final Note:   Class does not need Static Main as it will be injected into the Rebot.exe through the "Editor"
|               
|               Full Information on actual use in live profiles: http://www.rebot.to/showthread.php?t=4930
|               QH = Q.H. = QuestingHelps
|
|               Last Update: December 17th, 2015
|
*/  
	
public class QH
{
    public static ReBotAPI API;
    public static Fiber<int> Fib;

    // Empty Constructor
    public QH() { }	
		
	// Method:			"AbandonGarrisonFlightQuests(int)"
    // WARNING!!!       Mostly redundant now as full localization class has been built and 100% compatibility has been built into 
    //                  all profiles, but I will leave this here as a legacy method for the off-chance anyone might find some use of
    //                  it, and it might be a method worth emulating for a temporary stop-gap method on future similar circumstances
    //                  when time is limited to expand the Localization class further.
	// What it Does:	Acts as a stop-gap in regards to localization, EU/Russian/Asian script is compatible regardless
	//					of translation by abanding unnecessary quests at the moment to normalize all clients, regardless of languages.
	// Purpose:			Blizzard does not provide an API to match Gossip to Quest ID, so, the String has to be parsed
	//					which presents a problem if using a non-English Client. Thus, by temporarily eliminating
	//					any potential conflicting quests, it leaves the gossip option in the correct position(2)
	//					for ALL languages.  This will be made redundant with full localization of all clients
	//					of which right now we are missing the Asian markets (China, korea specifically)
    // 					Method is a work in progrss... still needs to open Quest window then select the quest
    //					Then abandon.  You cannot abandon a quest unless you have it selected...
    public static IEnumerable<int> AbandonGarrisonFlightQuests(int questToKeep) 
    {
        int[] questArray = {36706,36953,34681,36862,36951,34653,36952,34794,38568,35876};
        
        for (int i = 0; i < questArray.Length; i++)
        {
            if ((questArray[i] != questToKeep) && (API.HasQuest(questArray[i])))
            {
                API.ExecuteLua("local ind = GetQuestLogIndexByID(" + questArray[i] + "); local title = GetQuestLogTitle(ind); SelectQuestLogEntry(ind); SetAbandonQuest(); AbandonQuest();");
                string title = API.ExecuteLua<string>("return title;");
                API.Print("Removing Quest(temporarily) to Leave One Gossip Option at Flightmaster \"" + title + "\"");
                yield return 1000; // Found this to be necessary as often removing quests needs a slight delay.q
            }
        }
    }
    
    // Method:          "AbandonQuest(int)"
    // What it Does:    Abandons any currently obtained quest from the player's questlog.
    // Purpose:         On rare occasions it may be necessary to abandon a quest to re-pick it up, like if a quest "fails."
    //                  Or, say, a player activates the profiles, but has too many quests in his logs and is unable to pickup
    //                  a quest.  This will assist the profile creator in expanding their toolbox for quality of life safeguards.
    public static void AbandonQuest(int questID)
    {
        API.ExecuteLua("local ind = GetQuestLogIndexByID(" + questID + "); local title = GetQuestLogTitle(ind); SelectQuestLogEntry(ind); SetAbandonQuest(); AbandonQuest();");
        string title = API.ExecuteLua<string>("return title;");
        API.Print("The Quest \"" + title + "\" Has Been Removed.");
    }
    
    // Method:          "ArsenalGarrisonAbility(int)"
    // What it does:    Moves to Focus Position, then uses zone ability the given times (max 3).
    // Purpose:         Due to the nature of having to call the Garrison ability multiple times when facing
    //                  challenging rare monsters in the world, it would be more efficient to
    //                  merely require just calling this method. numStrikes should never be more than 3.
    //                  This can be used in Talador or in Tanaan Jungle, assuming proper outpost choice.
    public static IEnumerable<int> ArsenalGarrisonAbility(int numStrikes)
    {
        // Error checking to ensure number is between 1-3
        if (numStrikes <= 0 || numStrikes > 3)
        {
            numStrikes = 3;
        }
        // Do Not even attempt if spell is on cooldown.
        if (RemainingSpellCD(162075) == 0) 
        {
            API.DisableCombat = true;
            while (API.Me.Focus != null && API.Me.Focus.Distance2D > 10)  // This ensure player paths to the focus target until it is 10 yrds or closer.
            {
                API.MoveTo(API.Me.Focus.Position);
                yield return 100;
            }
            for (int i = 0; i < numStrikes; i++)
            {
                if (API.Me.Focus != null) 
                {
                    API.ExecuteLua("DraenorZoneAbilityFrame:Show(); DraenorZoneAbilityFrame.SpellButton:Click()");
                    yield return 100;
                    API.ClickOnTerrain(API.Me.Focus.Position);
                    yield return 1800;
                }
            }
            API.DisableCombat = false;
        }
        else 
        {
            API.Print("Player Wanted to Use the Arsenal Garrison Ability but it was on Cooldown!");
        }
    }
    
    // Method:          "BannerAvailable()"
    // What it Does:    Checks if player has any of the 3 Guild Banners AND the Banner is usuable(not on CD)
    // Purpose:         Helps remove code bloat by offering the boolean check here, before cycling through other Banner methods
    public static bool BannerAvailable()
    {
        if ((API.HasItem(64402) && API.ItemCooldown(64402) == 0) || (API.HasItem(64401) && API.ItemCooldown(64401) == 0)
         || (API.HasItem(64400) && API.ItemCooldown(64400) == 0)) 
        {
            return true;
        }
        return false;
    }

    // Method:          BuyExpPotions(int)
    // What it Does:    Uses the given argument as the number of XP potions to buy from the garrison vendor, inserting Lua to do it.
    // Purpose:         To make it easy to use the method ExpPotionsNeeded() -- Example:  "BuyExpPotions(ExpPotionsNeeded())" whilst
    //                  standing at the correct vendor to purchase them from.  Makes a lot of lines of code very simple when building the profiles.
    public static void BuyExpPotions(int toBuy)
    {
        if (toBuy > 0) 
        {
            string buy = "BuyMerchantItem(24," + toBuy + ")";  // Building LUA script to paste in string form
            API.ExecuteLua(buy);
        }
    }
    
    // Method:          "BuyVendorItemByID(int ID)"
    public static IEnumerable<int> BuyVendorItemByID(int itemToBuy, int howMany)
    {
        // parsing through vendor
        int BuyTwenty = howMany / 20;
        int remainder = howMany % 20;
        string itemID;
        string temp;
        int ID;
        for (int i = 1; i <= API.ExecuteLua<int>("return GetMerchantNumItems()"); i++)
			{
				itemID = API.ExecuteLua<string>("return GetMerchantItemLink(" + i + ");");
				temp = itemID.Substring(itemID.IndexOf(':') + 1);
				itemID = itemID.Substring(itemID.IndexOf(':') + 1, temp.IndexOf(':'));
				ID = int.Parse(itemID);
				if (itemToBuy == ID)
				{
					// j = Multiples of 20
					for (int j = 0; j < BuyTwenty; j++)
					{
						API.ExecuteLua("BuyMerchantItem(" + i + ", 20)");
						yield return 500;
					}
					if (remainder > 0)
					{
						API.ExecuteLua("BuyMerchantItem(" + i + "," + remainder + ")");
						yield return 500;
					}
					yield break;
				}
			}
    }
    
    // Method:          "CTA_GarrisonAbility()"
    // What it Does:     This uses the Frotfire Ridge Garrison ability "Call to Arms" on targeted NPC.
    // Purpose:          Many extremely challenging mobs are made simple with this script. This opens the door to accelerated
    //                   progress due to the higher rewards obtained from defeating more challenging objectives.
    public static IEnumerable<int> CTA_GarrisonAbility() 
    {
        if (RemainingSpellCD(161332) == 0) 
        {
            API.DisableCombat = true;
            while (API.Me.Focus != null && API.Me.Focus.Distance2D > 10.0) 
            {
                API.MoveTo(API.Me.Focus.Position);
                yield return 200;
            }
            // While I wanted to use "/use Garrison Ability" originally, in an ExecuteMacro method, I found that is not viable unless you
            // are ONLY using the English client.  For all languages to be compatible, it needs to execute in Lua Code!
            API.ExecuteLua("DraenorZoneAbilityFrame:Show(); DraenorZoneAbilityFrame.SpellButton:Click()");
            yield return 500;
            API.DisableCombat = false;
        }
        else 
        {
            API.Print("Player Intended to Use the \"Call to Arms\" Garrison Ability, But it Was unfortunately on CD");
            yield break;
        }
    }
    
    // Method:          DestroyKnownToys()
    // What it Does:    If there are any items in the player's inventory that are also TOYS, but the player already has Learned them, it them destroys the item
    // Purpose:         To help with the whole inventory clearing process.  The manual way would be to pick it up, click out of bags, type in "DELETE" for rare or higher items
    //                  This just automates it all.
    public static void DestroyKnownToys()
	{
        Inventory.Refresh();
		int vendorPrice;
		bool soulBound;
		bool isToy;
		bool playerHasToy;
		var ItemList = Inventory.Items;
		// Items to Destroy - Toys that are soulbound, already known, and not vendorable.
		foreach (var item in ItemList)
		{
			// Gear items that have no value
			vendorPrice = item.ItemInfo.VendorPrice;
			soulBound = IsItemSoulbound(item.ItemId);
			isToy = IsItemInInventoryAToy(item.ItemId);
			playerHasToy = API.ExecuteLua<bool>("return PlayerHasToy(" + item.ItemId + ")");
			if (soulBound && vendorPrice == 0 && isToy && playerHasToy)
			{
				// Destroy the item...
				API.Print("Destroying the Following Item:  " + item.Name);
			}
		}
	}

    // Method:          "DisableAddOn(string,bool)"
    // What it Does:    Disables the named addon and reloads the UI (if you so choose)
    // Purpose:         It is recommended to NOT use addons whilst using my profiles as addons can make
    //                  many unseen changes, or UI changes, or overlays, or rename buttons and so on, thus conflicting
    //                  with my script on occasions.  Since I know some people never follow recommendations or maybe
    //                  never bothered to even read them, I will check if a player is using an addon with a known
    //                  conflict and disable it for them.  There are only a small number, and this is ignored if
    //                  the area of conflict has already been surpassed.  It is a minor quality of life thing
    //                  so even those who don't listen get their hand held :)
    //                  It will always be updated as needed when conflicts arise as can happen on major patches.
    //                  It is recommended to be executed in an "initialization" block before proceeding with a profile.
    public static IEnumerable<int> DisableAddOn(string name, bool reload) 
    {
        string luaCall = "DisableAddOn(\"" + name + "\", UnitName(\"player\"))";
        API.ExecuteLua(luaCall);
       
        if (reload)
        {
            Vector3 location = API.Me.Position;
            API.ExecuteLua("ReloadUI()");
            yield return 1500;
            API.Print("On Loading screen...");
        }
    }
    
    // Method:          "DisableAddons(string[])"
    // What it Does:    First, disables all the given addons in the array, based on the given name of the addon (This is based on the [FOLDER NAME] of the addon in /interface/addons/<name>)
    //                  And then after disabling, give the boolean option set to true and the player's UI will reload.  This would typically be done as in most cases a reload is necessary to disable.
    // Purpose:         As with the previous method's description, it is mainly to be able to add a Quality of Life feature to prevent the user's experience from being sub-par by managing their addons
    //                  for them as with certain items there can be certain UI issues that affect macro interactions...
    public static void DisableAddons(string[] addonName, bool reloadUI)
    {
        int numDisabled = 0;
        
        for (int i = 0; i < addonName.Length; i++)
        {
            if (HasAddOnEnabled(addonName[i]))
            {
                numDisabled++;
                API.Print("Disabling " + addonName[i] + " addon!");
                string luaCall = "DisableAddOn(\"" + addonName[i] + "\", UnitName(\"player\"))";
                API.ExecuteLua(luaCall);
            }
        }
        
        if (numDisabled > 0 && reloadUI)
        {
            API.Print("You Have Disabled " + numDisabled + " Addons and Need to Reload.\nReloading Now!");
            API.ExecuteLua("ReloadUI()");
        }
    }
    
    // Method:          "DoGossipOnMatch(string)"
    // What it does:    Identifies which Gossip Option on an NPC is the correct one by matching the string choice and selects it.
    // Purpose:         If a player has many collected quests, certain NPCs can be loaded with multiple gossip options
    //                  thus, it is prudent to first check which matches the intended choice before selecting it.
    //                  Unfortunately, Blizzard has no built-in API to match a quest ID to the given options on an NPC to interact with.
    //                  As a result, this is a workaround "hack" to make my scripting life a little easier.  This is the ONLY method
    //                  in all of my templates that is not "Client" friendly, and as such would need to be re-used for each language.
    //                  All of my profiles have full translations for Eng, German, French, Spanish, Portuguese, Italian, and Russian, but is lacking Asian dialects.
    public static void DoGossipOnMatch(string choice)
    {
        // Initializing Function
        string title = "title0";
        string luaCall = ("local title0,_ = GetGossipOptions(); if title0 ~= nil then return title0 else return \"nil\" end");
        string result = API.ExecuteLua<string>(luaCall);
        // Now Ready to Iterate through All Gossip Options!
        // The reason "1" is used instead of the conventional "0" is because Gossip options start at 1.
        int i = 1;
        string num = "";
        while (result != null)
        {
            if (result.Equals(choice))
            {
                API.Print("Selection Found at Gossip Option " + i + ".");
                API.ExecuteLua("SelectGossipOption(" + i + ");");
                break;
            }
            else
            {
                // This builds the new string to be added into an Lua API call.
                num = i.ToString();
                title = (title.Substring(0,title.Length-1) + num);
                luaCall = ("local " + title + ",_," + luaCall.Substring(6,luaCall.Length-6));
                result = API.ExecuteLua<string>(luaCall);
                i++;
            }
        }
        if (result == null)
        {
            API.Print("Unable to Identify Correct Gossip Option.");
        }
    }
    
    // Method:          "DoFlightMasterGossip()"
    // What it Does:    If talking to a flightmaster, if the flightwindow is not open on interact and you are presented with gossip options, this chooses the correct option.
    // Purpose:         Often Flightmasters have a lot of different Gossip options, like say, unfinished quests,
    //                  which will replace the normal gossip position on the flightmaster.  This finds the gossip option and 
    //                  selects the correct one.  This also brings in compatibility for ALL clients.
    public static void DoFlightMasterGossip()
    {
        // Initializing Function
        string title = "title0";
        string luaCall = ("local title0,_ = GetGossipOptions(); if title0 ~= nil then return title0 else return \"nil\" end");
        string result = API.ExecuteLua<string>(luaCall);
        // Now Ready to Iterate through All Gossip Options!
        // The reason "1" is used instead of the conventional "0" is because Gossip options start at 1.
        int i = 1;
        string num = "";
        while (result != null)
        {
            if (result.Equals("Show me where I can fly.") || result.Equals("Muéstrame adónde puedo ir volando.") || result.Equals("Mostre-me para onde posso voar.") || result.Equals("Wohin kann ich fliegen?") || result.Equals("Muéstrame adónde puedo ir volando.") || result.Equals("Montrez-moi où je peux voler.") || result.Equals("Mostrami dove posso volare.") || result.Equals("Покажи, куда я могу отправиться.") || result.Equals("제가 날아갈 수 있는 곳을 알려 주십시오.") || result.Equals("告訴我可以飛往那些地方。") || result.Equals("告诉我可以飞到哪里去。"))
            {
                API.ExecuteLua("SelectGossipOption(" + i + ");");
                break;
            }
            else
            {
                // This builds the new string to be added into an Lua API call.
                num = i.ToString();
                title = (title.Substring(0,title.Length-1) + num);
                luaCall = ("local " + title + ",_," + luaCall.Substring(6,luaCall.Length-6));
                result = API.ExecuteLua<string>(luaCall);
                i++;
            }
        }
    }
    
    // Method:          "DoMerchantGossip()"
    // What it Does:    Interacts with the appropriate Gossip option on a vendor to open the buy/sell window.
    // Purpose:         If the player is at a Merchant (Refreshment, Vendor, or any generic merchant), at times on first interaction the Merchant may have various
    //                  gossip options rather than opening the buy/sell window immediately.  In these cases, it would be necessary to determine which gossip option would be the
    //                  correct one to open the vendor.  While most of the time it is the first option, I have found a few unique cases where this is not true, and as such
    //                  to encompass all possibilities, this is a universal method to coose the correct option always.
    //                  Example of when to use, after interacting with a merchant:  if (!IsVendorOpen) {DoMerchantGossip()};
    private static void DoMerchantGossip()
	{
		// Initializing Function
		string title = "title0";
		string luaCall = ("local title0,_ = GetGossipOptions(); if title0 ~= nil then return title0 else return \"nil\" end");
		string result = API.ExecuteLua<string>(luaCall);
		// Now Ready to Iterate through All Gossip Options!
		// The reason "1" is used instead of the conventional "0" is because Gossip options start at 1.
		int i = 1;
		string num = "";
		while (result != null)
		{
			if ((result.Equals("Let me browse your goods.") || result.Equals("I want to browse your goods.")) || (result.Equals("Deja que eche un vistazo a tus mercancías.") || result.Equals("Quiero ver tus mercancías.")) || (result.Equals("Deixe-me dar uma olhada nas suas mercadorias.") || result.Equals("Quero ver o que você tem à venda.")) || (result.Equals("Ich möchte ein wenig in Euren Waren stöbern.") || result.Equals("Ich sehe mich nur mal um.")) || (result.Equals("Deja que eche un vistazo a tus mercancías.") || result.Equals("Quiero ver tus mercancías.")) || (result.Equals("Permettez-moi de jeter un œil à vos biens.") || result.Equals("Je voudrais regarder vos articles.")) || (result.Equals("Fammi vedere la tua merce.") || result.Equals("Voglio dare un'occhiata alla tua merce.")) || (result.Equals("Позвольте взглянуть на ваши товары.") || result.Equals("Я хочу посмотреть на ваши товары.")) || (result.Equals("물건을 살펴보고 싶습니다.") || result.Equals("물건을 보고 싶습니다.")) || (result.Equals(" 讓我看看你出售的貨物。") || result.Equals("我想要看看你賣的貨物。")) || (result.Equals("让我看看你出售的货物。") || result.Equals("我想要看看你卖的货物。")))
			{
				API.ExecuteLua("SelectGossipOption(" + i + ");");
				break;
			}
			else
			{
				// This builds the new string to be added into an Lua API call.
				num = i.ToString();
				title = (title.Substring(0, title.Length - 1) + num);
				luaCall = ("local " + title + ",_," + luaCall.Substring(6, luaCall.Length - 6));
				result = API.ExecuteLua<string>(luaCall);
				i++;
			}
		}
	}
    
    // Method:          "ExpPotionsNeeded()"
    // What it Does:    Returns the number of experience potions a player should buy based on their player level.
    // Purpose:         This is mainly to optimize player XP potion buying from the garrison vendor.  Often in a profile that purchases
    //                  potions, it will just be a static amount to purchase.  This is a problem because one, if a player is level 100, that would
    //                  be an unwise thing to do, but furthermore, what if a player was lvl 99, or 98?  Is it still a good idea
    //                  to purchase 10 potions, or whatever amount?  No!!!  Thus, I have a multiplier I use that seems ok.  Basically, for
    //                  whatever remaining levels the players has until 100 (for example, lvl 96 player = 4 remaining levels), I then
    //                  multiple it by 1.4, convert it to an INT to round down to a whole number, and then buy that many potions.
    //                  Also, it will make a consideration, if player is 99, and has the aura still, well it would not be a good idea to buy
    //                  an XP potions for the remaining 10% of experience needed til 100, would it?  So, this algorithm further optimizes
    //                  XP potion buy to be the most efficient and least wasteful as possible.
    public static int ExpPotionsNeeded() 
    {
        // The first block of this method is to prevent unnecessary waste of Garrison resources... Quality of Life Checks for Efficiency.
        // Adding a Quick escape if Player is almost lvl 100... no need to waste resources.
        if (API.Me.Level == 99) 
        {
            float currXP = API.ExecuteLua<float>("return UnitXP(\"player\")");
            float nextLvl = API.ExecuteLua<float>("return UnitXPMax(\"player\")");
            // If player HAS the aura, and there is less than 25% less to level, it will not buy another XP potion wastefully.
            if  (API.Me.HasAura(178119) && currXP/nextLvl > 0.75) 
            {
                API.Print("You are Almost Level 100! No Need to buy any more XP potions... Aura still Active.");
                return 0;
            }
            // If player does NOT have the aura, and has no potions, the player will STILL NOT buy XP potions wastefully if only 15% left to lvl.
            else if (currXP/nextLvl > 0.85) 
            {
                API.Print("You are less than 15% XP From Level 100. Let's Not Waste Any Garrison Resources on Potions!");
                return 0;
            }
        }
        // Now, Calculating How Many Potions a Player at this lvl should buy.
        double num = (100 - (API.Me.Level))*1.4;
        int maxOwn = (int)num;
        int toBuy;
        int currentPotionCount = API.ExecuteLua<int>("local potions = GetItemCount(120182, false, false); return potions;");
        API.Print(currentPotionCount + " XP Potions In Your Possession!");
        
        // Algorithm to determine how many potions to buy based on how many they should AND how many they are even 
        // capable of purchasing based upon Garrison resources
        if (currentPotionCount < maxOwn) 
        {
        	int gResources = API.ExecuteLua<int>("local _, amount = GetCurrencyInfo(824); return amount;");
        	int canBuy = gResources / 100;
        
        	if (canBuy > maxOwn)
        	{
        		canBuy = maxOwn;
        	} 
        	else if (canBuy == 0)
        	{
        		API.Print("Not Enough Resources to Buy XP Potion");
        	}
        	if ((canBuy + currentPotionCount) > maxOwn)
        	{
        		toBuy = canBuy - currentPotionCount;
        	}
        	else
        	{
        		toBuy = canBuy;
        	}
        }
        else 
        {
            API.Print("You Already Have a Sufficient Supply of XP Potions! YAY!");
            toBuy = 0;
        }
        if (toBuy > 0) {
            API.Print("You Should Buy " + toBuy + " XP Potions at Your Garrison!");
        }
        return toBuy;
    }
    
    // Method:          "GetDurability()"
	// What it Does:    Returns the % of character durability as an int (0-100)
    // Purpose:         Could be a good reason to check player's durability before moving on.
	public static int GetDurability()
	{
		int count = 9;
		float durability = 0;
		durability += API.ExecuteLua<float>("local durability,max = GetInventoryItemDurability(1) if durability ~= nil then local result = durability/max; return result else return 1.0 end");
		durability += API.ExecuteLua<float>("local durability,max = GetInventoryItemDurability(3) if durability ~= nil then local result = durability/max; return result else return 1.0 end");
		for (int i = 5; i <= 10; i++)
		{
			durability += API.ExecuteLua<float>("local durability,max = GetInventoryItemDurability(" + i + ") if durability ~= nil then local result = durability/max; return result else return 1.0 end");
		}
		durability += API.ExecuteLua<float>("local durability,max = GetInventoryItemDurability(16) if durability ~= nil then local result = durability/max; return result else return 1.0 end");

		if (API.ExecuteLua<bool>("local durability = GetInventoryItemDurability(17); if durability ~= nil then return true else return false end"))
		{
			durability += API.ExecuteLua<float>("local durability,max = GetInventoryItemDurability(17) local result = durability/max; return result");
			count++;
		}
		durability = durability / count * 100;
		return (int)durability;
	}
    
    // Method:          "GetGarrisonBuildingInfo(int)"
    // What it Does:    Returns a List of 2 objects, an int representing the buildingID that is placed there, and a string representing the name of that building
    //                  If no building is present, it will return ZERO for the buildingID, and "No Building" for the string.
    // Purpose:         Mainly to be used to determine if a Garrison plot position is empty so a building can be placed.
    public static List<object> GetGarrisonBuildingInfo(int plotID)
    {
        int builtID = API.ExecuteLua<int>("local builtID = C_Garrison.GetOwnedBuildingInfo(" + plotID + "); if builtID == nil then return 0 else return builtID end;");
        string buildingName = API.ExecuteLua<string>("local _,name = C_Garrison.GetOwnedBuildingInfo(" + plotID + "); if name ~= nil then return name else return \"No Building\" end;");
        
        List<object> result = new List<object>(){builtID,buildingName};
        return result;
    }
    
    // Method:          "GetGarrisonLevel();
    // What it Does:    Returns the current rank of the player garrison, 1-3
    // Purpose:         When dealing with various pathing at the Garrison, it is important to note that object
    //                  location often varies based on the level and size of the ggarrison. This helps filter it all.
    public static int GetGarrisonLevel()
    {
        return API.ExecuteLua<int>("local level = C_Garrison.GetGarrisonInfo(); return level;");
    }

    // Method:          "GetGarrisonResources()"
    // What it Does:    Returns the amount of Garrison Resources the player has at the given moment.
    // Purpose:         Often you can save a lot of processing time/power by ensuring the player has enough resources
    //                  before wasting time attempting to do something when it would ultimately fail at the last step.
    public static int GetGarrisonResources() 
    {
        return API.ExecuteLua<int>("local _, gResources = GetCurrencyInfo(824); return gResources");
    }
    
    // Method:          "GetItemMinLevel(int)
    // What it Does:    Returns the Minimum Level required of a piece of gear to be equipped.  It returns '0' if there is no minimum.
    // Purpose:         So player can check conditionals on gear for whatever reason. It is commonly to be usedin regards to outdated gear vendoring.
    public static int GetItemMinLevel(int ID)
	{
		return API.ExecuteLua<int>("local _,_,_,_,itemMinLevel = GetItemInfo(" + ID + "); return itemMinLevel;");
	}
    
    // Method:          "GetNumItemsBroken()"
	// What it Does:    Returns if player has any Broken ("RED") items equipped.
    // Purpose:         Could be useful if at a vendor to institute a brief repair.
	public static int GetNumItemsBroken()
	{
		int count = 0;
		if (API.ExecuteLua<float>("local durability,max = GetInventoryItemDurability(1) if durability ~= nil then local result = durability/max; return result else return 1.0 end") < 0.1f)
		{
			count++;
		}
		if (API.ExecuteLua<float>("local durability,max = GetInventoryItemDurability(3) if durability ~= nil then local result = durability/max; return result else return 1.0 end") < 0.1f)
		{
			count++;
		}
		for (int i = 5; i <= 10; i++)
		{
			if (API.ExecuteLua<float>("local durability,max = GetInventoryItemDurability(" + i + ") if durability ~= nil then local result = durability/max; return result else return 1.0 end") < 0.1f)
			{
				count++;
			}
		}
		if (API.ExecuteLua<float>("local durability,max = GetInventoryItemDurability(16) if durability ~= nil then local result = durability/max; return result else return 1.0 end") < 0.1f)
		{
			count++;
		}
		if (API.ExecuteLua<bool>("local durability = GetInventoryItemDurability(17); if durability ~= nil then return true else return false end"))
		{
			if (API.ExecuteLua<float>("local durability,max = GetInventoryItemDurability(17) local result = durability/max; return result") < 0.1f)
			{
				count++;
			}
		}
		return count;
	}
    
    // Method:          "GetPlayerGold()"
    // What it Does:    Returns the amount of Gold a player has (Rounded down to even(int) Gold number)
    // Purpose:         Much like with the Garrison Resources method, this can be useful to ensure player has the means to
    //                  accomplish a task or purchase an item before wasting time attempting.
    public static int GetPlayerGold()
    {
        int money = API.ExecuteLua<int>("return GetMoney()");
        // Since it returns from the Blizz API as copper, the /10000 converts it to Gold. 1xCopper X 100 X 100 = 1.0g
        return (money/10000);
    }
    
    // Method:          "GetPlayerItemLevel()"
    // What it Does:    Returns the numerical value of the player "iLvl" of currently obtained equipped gear.
    // Purpose:         In case of challenging quests, player iLvl can be determined to be a sufficient minimum
    public static int GetPlayerItemLevel()
    {
        return (int)API.ExecuteLua<double>("local _,equipped = GetAverageItemLevel(); return equipped");
    }
    
    // Method:          "GetPlayerProfessions()"
    // What it Does:    Returns a String array of the names of the 2 player professions, or "none" if none in slot
    // Purpose:         To create a localized and friendly method that returns the ENGLISH names of professions,
    //                  regardless of the client used.
    public static string[] GetPlayerProfessions()
    {
        string[] professions = new string[2];
        string prof1 = API.ExecuteLua<string>("local prof1 = GetProfessions(); if prof1 ~= nil then local _,texture = GetProfessionInfo(prof1); return texture; else return \"null\"; end");
        string prof2 = API.ExecuteLua<string>("local _,prof2 = GetProfessions(); if prof2 ~= nil then local _,texture = GetProfessionInfo(prof2); return texture; else return \"null\"; end");
        
        // Parsing the name of the first profession (for localization sake, avoids translation need using filename of icon instead)
        if (!prof1.Equals("null"))
        {
            if (prof1.Substring(prof1.LastIndexOf('\\') + 1, 3).Equals("INV"))
            {
                // Special condition for 2 unique cases where this occurs for ease.
                if (prof1.Substring(prof1.LastIndexOf('\\') + 1, 8).Equals("INV_Misc"))
                {
                    for (int i = 0; i < prof1.Substring(prof1.LastIndexOf('\\') + 10).Length; i++)
                    {
                        if (prof1.Substring(prof1.LastIndexOf('\\') + 10)[i] == '_')
                        {
                            prof1 = prof1.Substring(prof1.LastIndexOf('\\') + 10, i);
                            break;
                        }
                    }
                }
                else
                {
                    prof1 = prof1.Substring(prof1.IndexOf('_') + 1, prof1.Substring(prof1.IndexOf('_') + 1).IndexOf('_'));
                }
            }
            else
            {
                prof1 = prof1.Substring(prof1.LastIndexOf('_') + 1);
            }
            professions[0] = prof1;
        }
        else
        {
            professions[0] = "None";
        }
    
        // Parsing the name of the Second profession
        if (!prof2.Equals("null"))
        {
            if (prof2.Substring(prof2.LastIndexOf('\\') + 1, 3).Equals("INV"))
            {
                // Special condition for 2 unique cases where this occurs for ease.
                if (prof2.Substring(prof2.LastIndexOf('\\') + 1, 8).Equals("INV_Misc"))
                {
                    for (int i = 0; i < prof2.Substring(prof2.LastIndexOf('\\') + 10).Length; i++)
                    {
                        if (prof2.Substring(prof2.LastIndexOf('\\') + 10)[i] == '_')
                        {
                            prof2 = prof2.Substring(prof2.LastIndexOf('\\') + 10, i);
                            break;
                        }
                    }
                }
                else
                {
                    prof2 = prof2.Substring(prof2.IndexOf('_') + 1, prof2.Substring(prof2.IndexOf('_') + 1).IndexOf('_'));
                }
            }
            else
            {
                prof2 = prof2.Substring(prof2.LastIndexOf('_') + 1);
            }
            professions[1] = prof2;
        }
        else
        {
            professions[1] = "None";
        }
        
        return professions;
    }

    // Method:          "GetProfessionBuildingID(string)"
    // What it Does:    This matches the player's profession to the correct building ID within the garrison
    // Purpose:         This is important because each Garrison Building has a corresponding ID.  This will match the
    //                  given profession to its corresponding ID so that the correct matching building can be placed.
    //                  This is best used with GetPlayerProfessions() - Example: int buildingID1 = GetProfessionBuildingID(GetPlayerProfessions()[0])
    public static int GetProfessionBuildingID(string professionName)
    {
        int idNumber;
        switch(professionName) 
        {
            case "Alchemy":
                idNumber = 76;
                break;
            case "Engraving" : 
            	idNumber = 93;
                break;
            case "Engineering":
            	idNumber = 91;
                break;
            case "Gem":
            	idNumber = 96;
                break;
            case "Inscription":
            	idNumber = 95;
                break;
            case "Tailoring": 
            	idNumber = 94;
                break;
            case "BlackSmithing": 
            	idNumber = 60;
                break;
            case "LeatherWorking":
            	idNumber = 90;
                break;
            case "Herbalism":
            	API.Print("Herbalism Has No Corresponding Building to Place. Using Default!");
                idNumber = 0;
                break;
            case "Pick":
            	API.Print("Mining Has No Corresponding Building to Place. Using Default!");
                idNumber = 0;
                break;
            case "Pelt":
            	API.Print("Skinning Has No Corresponding Building to Place. Using Default!");
                idNumber = 0;
                break;
            default:
                API.Print("No Valid Profession Identified...");
                idNumber = 0;
                break;
        }
        return idNumber;    
    }
    
    // Method:          "GTownHallExit()"
    // What it does:    Navigates out of a lvl 2 or 3 Garrison Town Hall (Horde)
    // Purpose:         Rebot has serious Mesh issues when starting a script within a Garrison
    //                  But, even worse, starting within a town hall.  This solves this issue
    //                  by using Click-To-Move actions to navigate out of the town hall successfully.
    //                  This is best used in the "initialization" stage of a script, or to be implemented
    //                  immediately after hearthing to the Garrison.
    public static IEnumerable<int> GTownHallExit()
    {
        int tier = API.ExecuteLua<int>("local level = C_Garrison.GetGarrisonInfo(); return level;");
        string zone = API.ExecuteLua<string>("local zone = GetMinimapZoneText(); return zone;");
        Vector3 location = new Vector3(5559.2f, 4604.8f, 141.7f);
        Vector3 location2 = new Vector3(5562.576f, 4601.484f, 141.7169f);
        Vector3 location3 = new Vector3(5576.729f, 4584.367f, 141.0846f);
        Vector3 location4 = new Vector3(5591.181f, 4569.721f, 136.2159f);
        
        if (API.Me.Distance2DTo(location) < 25 && API.IsInGarrison && (tier == 2 || tier == 3))
        {
            // If I do not disable Flightmaster discovery, then it tries to run to flightmaster BEFORE executing CTM actions
            // which with the lack of a mesh, often results in the player just running helplessly into the wall with mesh errors spamming.
            API.GlobalBotSettings.FlightMasterDiscoverRange = 0.0f;
            while(API.Me.Distance2DTo(location2) > 5)
            {
                API.CTM(location2);
                yield return 100;
            }
            while(API.Me.Distance2DTo(location3) > 5)
            {
                API.CTM(location3);
                yield return 100;
            }
            while(API.Me.Distance2DTo(location4) > 5)
            {
                API.CTM(location4);
                yield return 100;
            }
            API.GlobalBotSettings.FlightMasterDiscoverRange = 75.0f;
        }
        yield break;
    }
    
    // Method:          "HasAddOnEnabled(string)"
    // What it Does:    If a player has not only the addon installed, but turned ON (enabled), this will return true.
    // Purpose:         In determining if an addon should be disabled with a corresponding method, it would be prudent to
    //                  first determine if an addon is even "ON" in the first place.
    public static bool HasAddOnEnabled(string name)
    {
        // Returns null if the addon is either not installed OR not, but it does not determine if it is "ON" or not.
        // It returns the title of the addon you are asking if it has installed.
        string hasAddOn = API.ExecuteLua<string>("local _,title,_,enabled,loadable = GetAddOnInfo(\"" + name + "\"); return title;");
        if (hasAddOn != null)
        {
            // Loadable means it can be turned on without a "ReloadUI()" which can be bad, and enabled means just that, it's enabled.
            // These are just returning the variables derived from the previous Table in Lua.
            bool enabled = API.ExecuteLua<bool>("local _,title,_,enabled,loadable = GetAddOnInfo(\"" + name + "\"); return enabled;");
            string loadable = API.ExecuteLua<string>("local _,title,_,enabled,loadable = GetAddOnInfo(\"" + name + "\"); return loadable;");
            if ((enabled == false && !loadable.Equals("DISABLED")) || enabled == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    
    // Method:          "HasArchaeology()"
    // What it Does:    Returns a boolean if the Archaeology profession is at least lvl 1 or higher.
    // Purpose:         Spires of Arak has many "treasures" only obtainable if the player has trained archaeology.
    //                  This can help determine if the player needs to train in the first place or if it can even collect the object.
    public static bool HasArchaeology() 
    {
       if (API.ExecuteLua<string>("local _,_, archaeology = GetProfessions(); return archaeology;") != null)
       {
           return true;
       }
       return false;
    }
    
    // Method:          "HasGuildBannerAura()"
    // What it Does:    Determines if the player is within 100 yrds of any of the 3 guild banners, thus having its aura.
    // Purpose:         At times it would be unwise to drop a 2nd banner if still in range of another. This adds an additional
    //                  check to avoid wasteful banner use, needlessly putting it on a 15 min cooldown.  Helpful mainly for stop/start situations.
    public static bool HasGuildBannerAura()
    {
        if (API.HasAura(90633) || API.HasAura(90632) || API.HasAura(90631)) 
        {
            return true;
        }
        return false;
    }
    
    // Method:          "HasHiddenAura(int)"
    // What it Does:    Returns a boolean if player currently has the given aura.  Currently, the "API.HasAura()" from the Rebot API
    //                  only checks on the main active Aura buffs, whilst mine now checks against all potential auras.
    // Purpose:         To fill in a necessary, but missing gap in the Rebot API.
    public static bool HasHiddenAura(int spellID)
    {
        foreach (var aura in API.Me.Auras) {
	       if (aura.SpellId == spellID) {
		      return true;
           }	
        }
        return false;
    }
    
    // Method:			"HasRepairMount();
	// What it Does:	Return true if you have any of the Horde repair mounts(Traveler's Tundra, Expedition Yak).
    // Purpose:         Could be useful in a check to get quick access to a vendor if needed...
	public static bool HasHordeRepairMount()
	{
		// Traveler's Tundra or
		if (API.HasMount(61447) || API.HasMount(122708))
		{
			return true;
		}
		return false;
	}
    
    // Method:          "Hasprofession()"
    // What it Does:    Returns a boolean on whether the player has any primary profession or not.
    // Purpose:         There are a few instances with the Garrison where I match professions to their buildings, that first
    //                  determining if the player has existing professions before even bothering to lay the corresponding plots
    //                  would be a prudent decision.  It also opens the door for expansion as this can be used for many circumstances.
    public static bool HasProfession() 
    {
        int prof1 = API.ExecuteLua<int>("local prof1 = GetProfessions(); return prof1");
        int prof2 = API.ExecuteLua<int>("local _,prof2 = GetProfessions(); return prof2");
        
        // If Bother prof1 + prof2 == 0, then it will mean the player has no profession learned at even the first skill lvl 1.
        if (prof1 != 0 || prof2 != 0) 
        {
            return true;
        }
        return false;
    }
    
    
    // Method:          "HearthToGarrison()"
    // What it Does:    Exactly as described, uses the Garrison hearthstone.
    // Purpose:         Extraordinarily useful for speed.  If player needs to pickup a quest that starts in the garrison and they are not there
    //                  by adding this option it will hearth them back, likewise with turning in something.
    //                  This method is invaluable and used incredibly frequently to maximize the efficiency of player scripts and believable player
    //                  behavior.
    public static IEnumerable<int> HearthToGarrison() 
    {
        // Error check to avoid use if flying
        if (API.Me.IsOnTaxi)
        {
            // Waits to use until OFF the IsOnTaxi.
            var check = new Fiber<int>(WaitUntilOffTaxi());
            while (check.Run()) 
            {
                yield return 100;
            }
            yield return 1000;
        }
        
        if (!API.IsInGarrison) 
        {
            // Verifying Garrison hearthstone is not on Cooldown.
            if (API.ItemCooldown(110560) == 0) 
            {
                if (API.ExecuteLua<bool>("local name = GetMerchantItemInfo(1); if name ~= nil then return true else return false end"))
                {
                    API.Print("Player is Interacting With a Vendor. Closing Window Before Attempting to Hearth, lest the Bot Will Attempt to Sell G-Hearthstone!");
                    API.ExecuteLua("CloseMerchant()");
                    yield return 1000;
                }
                
                // This is a check to verify player has moved, lest it will re-attempt to hearth.
                Vector3 startPos = API.Me.Position;
                while (API.Me.Distance2DTo(startPos) < 50)
                {
                    API.Print("Hearthing to Garrison");
                    API.UseItem(110560);
                    yield return 1000;
                    // This keeps the player from attempting the next action until the Garrison hearthstone is successfully used
                    while(API.Me.IsCasting) 
                    {
                        yield return 100;
                    }
                    yield return 1000;
                    if (API.Me.Distance2DTo(startPos) >= 50)
                    {
				        break;
                    }
                    else
                    {
                        API.Print("Player Failed to Hearth. Trying Again...");
                    }
                }
                // Waiting for loading screen!
                while (!API.IsInGarrison)
                {
                    yield return 1000;
                }
                // Sometimes mesh errors occur by trying to CTM because it tries as soon as loading screen goes away but maybe some assets
                // are not fully loaded in the world.  This gives a slight delay to ensure no error.  Really depends on player PC and connection.
                API.Print("One Moment, Giving Mesh a Chance to Catchup!");
                yield return 5000;
            }
            else 
            {
                // Assumedly, in instances like this, a 2ndary logic route is given as backup, either the mesh or by Flightpath.
                API.Print("Player Wanted to Hearth to Garrison, but it is on Cooldown...");
                // Apply Flight Logic soon...
                yield break;
            }
        }
        yield break;
    }
    
    // Method:          "IsClose(float,float,float,int)"
    // What it does:    Checks the distance of the player at the given time to the Vector3 position and returns a boolean if it is within 
    //                  the given distance(int).
    // Purpose:         The main purpose of this is to enable distance checks to find player positionals.  The advantage of doing this
    //                  is that instead of relying on minimap string names or zone IDs (which can differ on diff. language clients),
    //                  the developer can instead create stuff that is client-friendly based on 3-dimensional positional checks.
    public static bool IsClose(float x, float y, float z, int distance)
    {
        Vector3 location = new Vector3(x,y,z);
        if (API.Me.Distance2DTo(location) < distance)
        {
            return true;
        }
        return false;
    }
    
    // Method:          "IsFlightMapOpen()"
    // What it Does:    Returns true if the player is at a Flightmaster and the flightmap to choose a destination is open
    // Purpose:         Basic boolean gate can be useful at times before attempting to choose flight destination.
    public static bool IsFlightMapOpen()
	{
		return API.ExecuteLua<bool>("if TaxiNodeName(1) ~= \"INVALID\" then return true else return false end");
	}
    
    // Method:          "IsFoodVendorNearby()"
    // What it Does:    Returns true if a player is within 100 yards of a food vendor
    // Purpose:         Could be a useful check if player wanted to search for a vendor if passing through town
    public static bool IsFoodVendorNearby()
	{
		foreach (var unit in API.Units)
		{
			if (!unit.IsDead && unit.HasFlag(UnitNPCFlags.SellsFood))
			{
				return true;
			}
		}
		return false;
	}
    
    // Method:          "IsInCombat()"
    // What it Does:    If the player is in Combat (as in the bot-base is active), then it returns true
    // Purpose:         One example of use could be as simple as while(IsInCombat()) {yield retun 100;} - or something like that... 
    //                  waiting til out of combat, continuing action if in combat and so on.
    public static bool IsInCombat()
    {
        return ReBot.Behaviours.Combat.Base.CombatBase.IsFighting;
    }
    
    // Method:          "IsItemInInventoryAToy(int ID)
    // What it Does:    Returns true if the given item is found in your inventory AND it is a toy
    // Purpose:         This is mainly used to identify items in your bag, as you parse through it, and return true if they are.
    //                  It's a basic boolean check before moving on to more bloated code.
    public static bool IsItemInInventoryAToy(int ID)
	{
		bool result;
		int container = -1;
		int slot = -1;
		// parsing through inventory items to match ID
		foreach (var item in Inventory.Items)
		{
			if (item.ItemId == ID)
			{
				container = item.ContainerId; // Establishing bag position
				slot = item.SlotId;
			}
		}
		
		// Parsing tooltip for Soulbound info
		if (container >= 0)
		{
			result = API.ExecuteLua<bool>("local tooltip; local function create() local tip, leftside = CreateFrame(\"GameTooltip\"), {} for i = 1, 5 do local L,R = tip:CreateFontString(), tip:CreateFontString() L:SetFontObject(GameFontNormal) R:SetFontObject(GameFontNormal) tip:AddFontStrings(L,R) leftside[i] = L end tip.leftside = leftside return tip end; local function Is_Toy(bag, slot) tooltip = tooltip or create() tooltip:SetOwner(UIParent,\"ANCHOR_NONE\") tooltip:ClearLines() tooltip:SetBagItem(bag, slot) local s = tooltip.leftside[2]:GetText() local t = tooltip.leftside[3]:GetText() u = tooltip.leftside[4]:GetText() tooltip:Hide() if (s == TOY or t == TOY or u == TOY) then return true; else return false; end end return Is_Toy(" + container + "," + slot + ")");
		}
		else
		{
			result = false;
		}
		return result;
	}
    
    // Method:          "IsInGordalFortress()"
    // What it Does:    Returns a boolean if the player is located within the specific zone called "Gordal Fortress" located in Talador.
    // Purpose:         Gordal Fortress has a barrier to get to and the mesh system can be problematic navigating it. For all the quests
    //                  located within this area, it can be very important to do a check that if they are OUT of the fortress, then to use 
    //                  a specific navigation routine to get in, or if they are Inside of it, to avoid that routine.
    //                  This is more of a quality of life method for players that maybe hearthed away in the middle of questing in this zone
    //                  or missed something here for whatever reason thus can naviagate back w/o issue.
    public static bool IsInGordalFortress() 
    {
        // This takes the following player coordinates, X and Y, and converts them into a Vector3 position.  Blizzard's API only gives us
        // 2 coordinates, but not the 3-dimensional z-coordinate, so using a simple algorithm we can determine the z coordinate based on
        // the MapAreaID.
        Vector3 location = new Vector3(1666.5f, 1743.6f, 298.6f);      
        if (IsClose(1410f, 1728.5f, 310.3f, 390)) 
        {
            // Z location is important because the Gordal fortress is high, so by determining player IS close to zone AND is above the z coordinate,
            // the height of the player can be determined as likely to represent its position.  I COULD write a 3D area map, but this was significantly
            // less time-consuming and just as effeective.
            if ((API.Me.Position.Z > 302.4) || ((API.Me.Position.Z > 296.0) && (API.Me.Distance2DTo(location) > 47.05))) 
            {
        		return true;
        	}       
        }
        return false;
    }
    
    // Method:          "IsItemSoulbound(int)"
    // What it Does:    Returns whether the given item in a player bags is SoulBound.  If the given item is not, or not owned, it returns false
    // Purpose:         Mainly for vendor filtering.  If item is SoulBound, not equippable, and has a vendor value > 0, it might be worth selling.
    public static bool IsItemSoulbound(int ID)
	{
		bool result;
		int container = -1;
		int slot = -1;
		// parsing through inventory items to match ID
		foreach (var item in Inventory.Items)
		{
			if (item.ItemId == ID)
			{
				container = item.ContainerId; // Establishing bag position
				slot = item.SlotId;
			}
		}
		
		// Parsing tooltip for Soulbound info
		if (container >= 0)
		{
			result = API.ExecuteLua<bool>("local tooltip; local function create() local tip, leftside = CreateFrame(\"GameTooltip\"), {} for i = 1, 3 do local L,R = tip:CreateFontString(), tip:CreateFontString() L:SetFontObject(GameFontNormal) R:SetFontObject(GameFontNormal) tip:AddFontStrings(L,R) leftside[i] = L end tip.leftside = leftside return tip end; local function Is_Soulbound(bag, slot) tooltip = tooltip or create() tooltip:SetOwner(UIParent,\"ANCHOR_NONE\") tooltip:ClearLines() tooltip:SetBagItem(bag, slot) local s = tooltip.leftside[2]:GetText() local t = tooltip.leftside[3]:GetText() tooltip:Hide() if (s == ITEM_SOULBOUND or t == ITEM_SOULBOUND) then return true; else return false; end end return Is_Soulbound(" + container + "," + slot + ")");
		}
		else
		{
			result = false;
		}
		return result;
	}
    
    // Method:          "IsInScenario()"
    // What it Does:    Returns a boolean on if a player is in a scenario or not.  This applies to those major zone story quests
    //                  where the player is phased, are considered "scenarios."
    // Purpose:         Often conditionals exist on navigating to and from a scenario.  While this normally would not apply.  Some people
    //                  maybe got interrupted in the middle of an event, or had to hearth for some reason, or a BG popped, or any number of things
    //                  and may find themselves having to restart.  By checking if a player is In or not, one can control the navigation routines,
    //                  or, more importantly, more efficiently approach problems with less-overhead.
    public static bool IsInScenario()
    {
        return API.ExecuteLua<bool>("local name,_,_ = C_Scenario.GetInfo(); if name ~= nil then return true else return false end;");
    }
    
    // Method:          IsMovingAway(float)
    // What it Does:    Returns a boolean if an NPC is moving away from you quickly (true), or it is stationary or moving closer you (false).
    // Purpose:         This can be useful for say, sending the player to attack a fast-moving flying target that can outrun you.
    //                  Without this, the player may end up chasing an object hundreds of yards, resulting in easy "stucks."
    //                  This helps avoid these situations by determining if the object is moving away from you to abandon what you are doing.
    public static bool IsMovingAway(float initialDistance) 
    {
        bool result = false;
        if (API.Me.Focus != null) {
            if (API.Me.Focus.Distance > initialDistance) 
            {
            result = true;
            }
        }
        else {
            API.Print("Unable to find target...");
        }
        return result;
    }
    
    // Method:          "IsRepairVendorNearby()"
    // What it Does:    Returns true if a player is within 100 yards of a Repair vendor
    // Purpose:         Could be a useful check if player wanted to search for a vendor if passing through town
	public static bool IsRepairVendorNearby()
	{
		foreach (var unit in API.Units)
		{
			if (!unit.IsDead && unit.HasFlag(UnitNPCFlags.CanRepair))
			{
				return true;
			}
		}
		return false;
	}
    
    // Method:		    "IsVendorOpen()"
    // What it Does:    Returns true if the Vendor window is open and you are currently able to buy/selling
    // Purpose:         Many vendors have gossip options on interact.  This could be useful on interaction checking to ensure
    //                  the vendor window is open, and if it is not yet open, parsing through gossip options and selecting the correct one.
	public static bool IsVendorOpen()
	{
		return API.ExecuteLua<bool>("local name = GetMerchantItemInfo(1); if name ~= nil then return true else return false end;");
	}
    
    // Method:          "ItemsNeededForQuest(int,int,int)"
    // What it Does:    Determines how many items for say, a specific quest objective still need to be collected.
    // Purpose:         Some quests require you to collect items and then use them in place or on another objet.
    //                  The objective itself only registers progress on use, so a player could collect more than necessary if it came to it.
    //                  Also, a script might say, "Collect 5" of these objects and THEN use them on something.  However, what if script is stopped
    //                  and restarted but player already has half or all of the items collected, it will still attempt to recollect.
    //                  This just determines how many are still needed to be used, vs how many player has in bags, and then calculates if any more 
    //                  need to be collected.  For max efficiency.
    public static bool ItemsNeededForQuest(int questID, int objective, int itemID) 
    {      
        string luaCall = "local currentProgress = GetQuestObjectiveInfo(" + questID + ", " + objective + " , false); return currentProgress;";
        string progress = API.ExecuteLua<string>(luaCall);
        for (int i = 0; i < progress.Length; i++)
        {
            // verifies this is an "Int" char, not anything else
            if (progress[i] > 47 && progress[i] < 58 )
            {
                progress = progress.Substring(i,3);
                break;
            }
        }
        // Converts parsed string into an int and calculates how many items are still needed.
        int notNeeded = int.Parse(progress.Substring(0,1));
        int total = int.Parse(progress.Substring(2));
        int toLoot = (total - notNeeded);      
        int itemCount = API.ExecuteLua<int>("local itemCount = GetItemCount(" + itemID + ", false, false); return itemCount;");
        int needed = (toLoot - itemCount);
        if (itemCount < toLoot)
        {
           API.Print("Player Needs to Loot " + needed + " More Items");
	       return true;
        }
        return false;
    }
    
    // Method:          "MiniMapZoneEquals(string)"
    // What it does:    Matches the sub-zone of the player to the given one.
    // Purpose:         This can be useful if trying to match "sub-zones" since there is no actual universal ID to do it.  However,
    //                  be warned, that in using this, your "string" you are matching might only work on the client language you choose to match.
    //                  Often the subzones names vary between translations, be it English/German/French... etc.  So, I'd often say it is best to use
    //                  "IsClose()" method in determining positioning to spare client/language issues and make it universally compatible.
    //                  Typically, you should avoid using this unless you have no way to match Vector3 proximity positions.
    public static bool MiniMapZoneEquals(string name)
    {
        string zone = API.ExecuteLua<string>("local zone = GetMinimapZoneText(); return zone;");
        if (zone.Equals(name))
        {
            return true;
        }
        return false;
    }
    
    // Method:          "NeedsFollower(string)"
    // What it Does:    Returns a boolean on Garrison Mission Followers already owned. If you already own the given one, it will return false.
    // Purpose:         This helps to determine if a player should waste time attempting to retrieve said follower or not.
    public static bool NeedsFollower(int followerDisplayID)
    {
        return API.ExecuteLua<bool>("local allFollowers = C_Garrison.GetFollowers(); local toBuy = false; for x, y in pairs(allFollowers) do if (y.displayID == " + followerDisplayID + " and y.isCollected == nil) then toBuy = true; break; end end; return toBuy;");
    }
    
    // Method:          "PlaceGarrisonBuildingAt(int,int)"
    // What it Does:    Exactly as it sounds, sets the given building to the given plot in your garrison when player
    //                  is looking at the architect table in their Garrison.
    // Purpose:         Easier access to tools in C# rather than having to use Lua within code.
    public static void PlaceGarrisonBuildingAt(int plotID, int buildingID)
    {
        API.ExecuteLua("C_Garrison.PlaceBuilding(" + plotID + ", " + buildingID + ");");
    }
    
    // Method:          "PlaceGuildBannerAt(float,float,float)"
    // What it does:    Basic check to first, see if one of the 3 banners is available.  If so,
    //		            it will execute the MoveTo to the given Vector3 location and then use an available banner.
    // Purpose:         The reason why this is separated from the previous function GuildBanners() is because
    //		            it is necessary to do a check before telling the player to moveTo a destination.
    //		            It would be time-wasting of the player to move to the destination to use banner if it
    //		            was not currently available in the first place.
    //		            The MAJOR advantage using a Vector3 as an argument and not explicitly listing the argument is
    //		            that I can call to this whenever I wish to, for whatever reason, and if I wish to pass it an
    //		            explicit pre-determined location I can, or I can implement give it an object location as well.
    public static IEnumerable<int> PlaceGuildBannerAt(float x, float y, float z)
   {
        if (BannerAvailable())
	    {
            Vector3 location = new Vector3(x,y,z);
		     while (!API.MoveTo(location)) 
             {
                 yield return 100;
             }
		     UseGuildBanner();
	    }
	    else
	    {
		     API.Print("Guild Banner Not Currently Available");
	    }
    }
    
    // Method:          "PlaceGuildBannerOnAuraCheck()"
    //      WARNING - Method not yet completed!!!!!
    // What it does:    Uses a Guild Banner at Player Position by, prioritization of best (15% bonus gains) to worse (5%).
    //                  To Be Used recursively, hence it yield breaks if player hits lvl 99, or is player has no banners.
    //                  This also delays the continuous check if all 3 banners are on CD, and it ONLY does a check
    //                  to be used if in combat.
    // Purpose:         If the banner is not on cooldown, it will use it when called upon so player may level faster.
    //	                 Also, it prioritizes the best of the banners to be used so you are always using best one available.
    //		            What makes this different from previous UseGuildBanner() function is...
    //		            ...in effort to conserve from spamming the conditionals, if all 3 banners are on cooldown
    //		            then it determines the shortest cooldown time then "yield returns" or "waits" to carry on
    //		            Ideally, this is a feature to be used with a background aura that checks continuously.
    //		            If you use it in main thread, bot will not continue whilst banners on cooldown.
    //                  It is recommended to run this in a separate thread.
    public static IEnumerable<int> PlaceGuildBannerOnAuraCheck()
    {
        if (!BannerAvailable() && API.Me.Level < 100)
        {
            yield break;  // No Banners in Possession or player is lvl 100, ends thread.
        }
        // Basically the bot ONLY targets an NPC typically if it wants to kill it.  I am not 100% certain of this though.  If this is 
        // Running in a background thread, I should test if it will override combatbase and use the banner.  There does not
        // appear to be ab API to check if player is in combat.
        if (!HasGuildBannerAura() && API.Me.Target != null && API.Me.Target.Distance < 100 && !API.Me.Target.IsFriendly && !API.Me.Target.IsDead)
        {
            if (BannerAvailable())
            {
                UseGuildBanner();
            }
            else
            {
                API.Print("All Available Banners on Cooldown");
                float Banner1 = API.ItemCooldown(64402);
                float Banner2 = API.ItemCooldown(64401);
                float Banner3 = API.ItemCooldown(64400);
                float waitTime = 0.0f;


                // If it DOES = -1 it means it is on cooldown. 
                if (Banner1 != -1)
                {
                    waitTime = Banner1;
                }
                if (Banner2 != -1 && Banner2 < waitTime)
                {
                    waitTime = Banner2;
                    if (Banner3 != -1 && Banner3 < waitTime)
                    {
                        waitTime = Banner3;
                    }
                }
                else if (Banner3 != -1 && Banner3 < waitTime)
                {
                    waitTime = Banner3;
                }
                // Setting up delay timer on recursive method if banners on CD
                int delay = (int)waitTime;
                delay = delay * 1000;
                int minutes = delay / 60000;
                int seconds = (delay % 60000) / 1000;
                API.Print("Next Banner Will Be Available in " + minutes + " minutes and " + seconds + " seconds.");
                yield return delay;
            }
        }
        Random rnd = new Random();
        int wait = rnd.Next(4000, 5000);
        yield return wait;
        // Recursive Repeat
        var check = new Fiber<int>(PlaceGuildBannerOnAuraCheck());
        while (check.Run()) 
        {
            yield return 100;
        }
    }
    
    // Method:          "PlayerHasToy(int toyID)"
    // What it Does:    Returns true if the player has already learned the toy
    // Purpose:         For easy checking to see if toy should be used or not.
    public static bool PlayerHasToy(int toyID)
    {
        return API.ExecuteLua<bool>("local isOwned = PlayerHasToy(" + toyID + "); return isOwned;");
    }
    
    // Method:          "QuestNotDone(int)"
    // What it does:    Verify that the quest is not turned in and completed, but also verifies it is
    //                  not completed yet still in your logs but not yet turned-in.
    // Purpose:         This is an important pre-req check on placing Banners at a specific location.
    //                  Without this, everytime the script restarted, it would place the banner down at the 
    //                  given location, wasting time, even if player had the quest in their logs.
    //                  This is a good boolean check to implement before indicating the need of banners still.
    public static bool QuestNotDone(int QuestID)
    {
        if (!API.IsQuestInLogAndComplete(QuestID) && !API.IsQuestCompleted(QuestID))
        {
            return true;
        }
        return false;
    }

    // Method:          "QuestObjectiveProgress(int,int,string)"
    // What it does:    Returns a boolean TRUE if the given description matches the current quest progress
    // Purpose:         Occasionally when writing quest templates, sometimes you want something to keep
    //                  checking or doing things over and over again until it accomplishes it.  For example,
    //                  imagine a quest that wanted you to destroy 5 targets, but it was all one objective
    //                  so you start off with a description like "0/5"  Often these targets
    //                  are not repeatable as the server stores some private information tied to your character, however,
    //                  given the current interact and collectobject abilities within Rebot, it will often attempt to
    //                  destroy the same target over and over again.  This allows you to break up a single
    //                  quest objective into different pieces, because if say the objective changes to "1/5 targets destroyed"
    //                  then it will carry on to the next sub-part of the objective rather than being stuck
    //                  in often what can occur is an infinite loop.
    public static bool QuestObjectiveProgress(int questID, int objective, string description)
    {
        string luaCall = "local currentProgress = GetQuestObjectiveInfo(" + questID + ", " + objective + " , false); return currentProgress;";
        string progress = API.ExecuteLua<string>(luaCall);
        string result = "";
        string finalResult = "";

        for (int i = 0; i < progress.Length; i++)
        {
            if (progress[i] > 46 && progress[i] < 58)
            {
                result = progress.Substring(i);
                for (int j = 0; j < result.Length; j++)
                {
                    if (result[j] > 46 && result[j] < 58)
                    {
                        finalResult += result[j];
                    }
                }
                break;
            }
        }
        if (finalResult.Equals(description))
        {
            return true;
        }
        return false;
    }

    // Method:          "QuestObjectiveProgress(int,int,int,string)"
    // WARNING          This Method is LEGACY and is largely redundant now.  It has been rescripted
    //                  with one less argument, but due to the vast wide-spread use of it prior to rescripting it,
    //                  this method will remain. It is highly recommended to use the older method.
    // What it does:    Returns a boolean TRUE if the given description matches the current quest progress
    // Purpose:         Occasionally when writing quest templates, sometimes you want something to keep
    //                  checking or doing things over and over again until it accomplishes it.  For example,
    //                  imagine a quest that wanted you to destroy 5 targets, but it was all one objective
    //                  so you start off with a description like "0/5"  Often these targets
    //                  are not repeatable as the server stores some private information tied to your character, however,
    //                  given the current interact and collectobject abilities within Rebot, it will often attempt to
    //                  destroy the same target over and over again.  This allows you to break up a single
    //                  quest objective into different pieces, because if say the objective changes to "1/5 targets destroyed"
    //                  then it will carry on to the next sub-part of the objective rather than being stuck
    //                  in often what can occur is an infinite loop.
    public static bool QuestObjectiveProgress(int questID, int objective, int numberToCompleteObjective, string description)
    {
        string luaCall = "local currentProgress = GetQuestObjectiveInfo(" + questID + ", " + objective + " , false); return currentProgress;";
        string progress = API.ExecuteLua<string>(luaCall);
        string result = "";
        string finalResult = "";
        
        for (int i = 0; i < progress.Length; i++) 
        {
            if (progress[i] > 46 && progress[i] < 58)
            {
                result = progress.Substring(i);
                for (int j = 0; j < result.Length; j++)
                {
                     if (result[j] > 46 && result[j] < 58)
                     {
                         finalResult += result[j];
                     }
                }
                break;
            }
        }
        if (finalResult.Equals(description))
        {
            return true;
        }
        return false;
    }
    
    // Method:          "ReEquipGear()"
    // What it does:    Re-Equips the same Gear you had previously removed. Reactivates "Auto-Equip"
    // Purpose:         This is a companion method to "UnequipGear()" so you can re-equip it easily.
    public static void ReEquipGear()
    {
        int hasGearOn = API.ExecuteLua<int>("return GetInventoryItemID(\"player\", 1);");
        if (hasGearOn == 0)
        {
            // Returning Global Variable from server side -- will not work if you reloaded or relogged.
        	API.ExecuteLua("UseEquipmentSet(\"TempQuesting\")");
            API.Print("Re-Equipping Your Gear");
            API.AutoEquipSettings.EquipItems = true;
            API.ExecuteLua("DeleteEquipmentSet(\"TempQuesting\")"); // Remove quest fingerprint...
        }
    }
    
    // Method:          "RemainingQuests(int[])"
    // What it does:    It returns how many quests are remaining to complete within the profile.
    // Purpose:         In the initialization, this acts as a quality of life check on profile progress so it can skip scanning through it all
    //                  if the profile is already completed, but also gives the user an idea on how much is remaining to do.
    public static int RemainingQuests(int[] questArray)
    {
        int count = 0;

        for (int i = 0; i < questArray.Length; i++)
        {
            if (!API.IsQuestCompleted(questArray[i]))
            {
                count++;
            }
        }
        API.Print("You Have " + count + " quests left to complete in this questpack!");
        return count;
    }
    
    // Method:          "RemainingSpellCD(int)"
    // What it Does:    Returns, in Seconds, how long until the spell is available again
    // Purpose:         Useful for conditional checks on spell casting, prioritization of a spell, etc.    
    public static int RemainingSpellCD(int spellID)
    {
        float start = API.ExecuteLua<float>("local start = GetSpellCooldown(" + spellID + "); return start;");
        float duration = API.ExecuteLua<float>("local _,duration = GetSpellCooldown(" + spellID + "); return duration;");
        float timePassed = API.ExecuteLua<float>("return GetTime();");
        float coolDown = timePassed - start;
        int result = (int)(duration-coolDown);
        if (result < 0) 
        {
            result = 0;
        }
        return result;
    }
    
    // Method:          "ScenarioStageEquals(int)"
    // What it does:    Checks Scenario Stage Progress (or Complex phased Quest Stages)
    // Purpose:         The Built in API for Rebot only had conditional information on current quest objective.
    //                  Some more complex quests have many sub-objectives.  This will open up more
    //                  control on scripting specifically for sub-objectives of the main objective.
    //                  This is particularly important at the large scale scenario quests in Draenor main questchains for each zone.
    public static bool ScenarioStageEquals(int progress)
    {
        int stage = API.ExecuteLua<int>("local _, currentStage = C_Scenario.GetInfo(); return currentStage;");
        if (stage == progress)
        {
        	return true;
        }
        return false;
    }
    
    // Method:          "SetFocusUnit(int)"
    // What it does:    Sets given NPC to the focus target and also targets it.
    // Purpose:         Useful to have a target set as focus as often it is easy to lose the target.
    //                  This also prevents potential crashing by checking empty objects. You can now do a simple
    //                  If Me.Focus != null and know that you are secure.
    public static void SetFocusUnit(int ID)
    {
        API.Me.ClearFocus();
        foreach (var unit in API.Units)
        {
            if (unit.EntryID == ID && !unit.IsDead)
            {
                API.Me.SetFocus(unit);
                API.Me.SetTarget(unit);
                break;
            }
        }
    }
    
    // Method:          "SetGarrisonProfessionBuildings()"
    // What it Does:    Matches your professions to the corresponding profession buildings, then plots them
    //                  Note: It only plots them if the spaces are already vacant, thus not to disrupt player actions
    //                  if they choose to have something else there.
    // Purpose:         To automate profession building and garrison management whilst leveling, thus half the work
    //                  is already completed by the time the player takes back over control at lvl 100.
    public static void SetGarrisonProfessionBuildings()
    {
        int buildingID1 = 93; // Default is Storehouse
        int buildingID2 = 51; // Default is Enchanter's Study
        int plotID1 = 18;  // Default plot positions.
        int plotID2 = 19;
        string pName1 = "Storehouse"; // Default plot names
        string pName2 = "Enchanting";
        // Gathering player professions
        string[] professions = GetPlayerProfessions();
        
        if (!professions[0].Equals("None"))
        {
            buildingID1 = GetProfessionBuildingID(professions[0]);
            if (buildingID1 != 0)
            {
                plotID1 = API.ExecuteLua<int>("local count = 1; local plotID1 = 0; for x, y in pairs(C_Garrison.GetPlotsForBuilding(" + buildingID1 + ")) do if count == 1 then plotID1 = y count = count + 1 end end; return plotID1");
                pName1 = professions[0];
            }
        }
        
        if (!professions[1].Equals("None"))
        {
            buildingID2 = GetProfessionBuildingID(professions[1]);
            if (buildingID2 != 0)
            {
                plotID2 = API.ExecuteLua<int>("local count = 1; local plotID2 = 0; for x, y in pairs(C_Garrison.GetPlotsForBuilding(" + buildingID2 + ")) do if count == 1 then count = count + 1 elseif count == 2 then plotID2 = y end end; return plotID2");
                pName2 = professions[1];
            }
        }
        
        // PlotInfo containes a bool representing if building is built there or not, and building name if there is one.
        List<object> plotInfo =  GetGarrisonBuildingInfo(plotID1);
        List<object> plotInfo2 =  GetGarrisonBuildingInfo(plotID2);
        if ((int)plotInfo[0] == 0)
        {
            if (buildingID1 != (int)plotInfo2[0])
            {
                PlaceGarrisonBuildingAt(plotID1, buildingID1);
                API.Print("Placing Plot For " + pName1 + "!!!");
            }
            else if ((int)plotInfo2[0] != 0)
            {
                PlaceGarrisonBuildingAt(plotID1, buildingID2);
                API.Print("Placing Plot For " + pName2 + "!");
            }
        }
        else
        {
            API.Print("You Already Have the " + (string)plotInfo[1] + " Building There!");
        }
               
        // Second Profession Plotting.
        // Remember, if no 2nd profession is found, the Enchanter's study is placed by default.
        plotInfo =  GetGarrisonBuildingInfo(plotID1);
        plotInfo2 =  GetGarrisonBuildingInfo(plotID2);
        if ((int)plotInfo2[0] == 0)
        {
            if (buildingID2 != (int)plotInfo[0])
            {
                PlaceGarrisonBuildingAt(plotID2, buildingID2);
                API.Print("Placing Plot For " + pName2 + "!");
            }
            else if ((int)plotInfo[0] != 0)
            {
                PlaceGarrisonBuildingAt(plotID2, buildingID1);
                API.Print("Placing Plot For " + pName1 + "!!");
            }
        }
        else
        {
            API.Print("You Already Have the " + (string)plotInfo2[1] + " Building There!");
        }
    }

    // Method:          "SetFocusUnit(int[])"
    // What it does:    Sets Focus to a unit from the given array of units
    // Purpose:         This will likely be used minimally, in favor of "SetNearestFocusUnit()" but
    //                  this allows the player to Set Focus to the first found NPC of an array
    public static void SetFocusUnit(int[] ID)
    {
        API.Me.ClearFocus();
        foreach (var unit in API.Units)
        {
            for (int i = 0; i < ID.Length;  i++)
            {
                if (unit.EntryID == ID[i] && !unit.IsDead)
                {
                    API.Me.SetFocus(unit);
                    API.Me.SetTarget(unit);
                    break;
                }
            }
            if (API.Me.Focus != null)
            {
                break;
            }
        }
    }
    
    // What it does:  Sets focus to target within the givin range limit.
    // Purpise:       Occasionally you only want to target units within a given range.  Typically you could just
    //                target units that are closest, but occasionally none exist at the moment, but you ALSO don't
    //                want to be chasing any down.
    public static void SetFocusUnitMaxDistance(int ID, int yards) 
    {
        foreach (var unit in API.Units)
        {
            if (unit.EntryID == ID && !unit.IsDead && API.Me.Distance2DTo(unit.Position) < yards)
            {
                API.Me.SetFocus(unit);
                API.Me.SetTarget(unit);
                break;
            }
        }
    }
    
    // Method:          "SetNearestFocusUnit(int[])"
    // What it does:    Targets and sets focus to the closest give unit from an Array of units.
    // Purpose:         Sometimes when iterating through the list of "Units," the closest does not always come first.
    //                  Often it is more effective to target closest unit first, rather than seemingly any
    //                  random unit within 100 yrds.
    public static void SetNearestFocusUnit(int[] ID)
    {
        API.Me.ClearFocus();
        var killTarget = API.Me.GUID;
        float closestUnit = 5000f; // Insanely large distance, so first found distance will always be lower.

        // Identifying Closest Desired Unit
        foreach (var unit in API.Units)
        {
            for (int i = 0; i < ID.Length;  i++)
            {
                if (unit.EntryID == ID[i] && !unit.IsDead)
                {
                    if (unit.Distance < closestUnit)
                    {
                        // This stores the distance of the new unit, so when it re-iterates through,
                        // ultimately the unit with the lowest distance, or the one closest to you is stored
                        closestUnit = unit.Distance;
                        // This stores the GUID, which is necessary as ALL units share the same UnitID, but the GUID is a unique identifier.
                        killTarget = unit.GUID;
                    }
                }
            }
        }
        if (closestUnit == 5000)
        {
            API.Print("No Units Found Within Targetable Range.");
        }
        else
        {
            Int32 closest = (Int32)closestUnit; // Easier on the eyes to Print.
            API.Print("Closest target is " + closest + " yards away.");
            // Setting Focus to closest Unit
            foreach (var unit in API.Units)
            {
                if (unit.GUID == killTarget)
                {
                    API.Me.SetFocus(unit);
                    API.Me.SetTarget(unit);
                    break;
                }
            }
        }
    }
    
    // Method:          "SetNearestFocusUnit(int)"
    // What it does:    Targets and sets focus to the closest give unit.
    // Purpose:         Sometimes when iterating through the list of "Units," the closest does not always come first.
    //                  Often it is more effective to target closest unit first, rather than seemingly any
    //                  random unit within 100 yrds.
    public static void SetNearestFocusUnit(int ID)
    {
        API.Me.ClearFocus();
        var killTarget = API.Me.GUID;
        float closestUnit = 5000f; // Insanely large distance, so first found distance will always be lower.

        // Identifying Closest Desired Unit
        foreach (var unit in API.Units)
        {
            if (unit.EntryID == ID && !unit.IsDead)
            {
                if (unit.Distance < closestUnit)
                {
                    // This stores the distance of the new unit, so when it re-iterates through,
                    // ultimately the unit with the lowest distance, or the one closest to you is stored
                    closestUnit = unit.Distance;
                    // This stores the GUID, which is necessary as ALL units share the same UnitID, but the GUID is a unique identifier.
                    killTarget = unit.GUID;
                }
            }
        }
        if (closestUnit == 5000)
        {
            API.Print("No Units Found Within Targetable Range.");
        }
        else
        {
            Int32 closest = (Int32)closestUnit; // Easier on the eyes to Print.
            API.Print("Closest target is " + closest + " yards away.");
            // Setting Focus to closest Unit
            foreach (var unit in API.Units)
            {
                if (unit.GUID == killTarget)
                {
                    API.Me.SetFocus(unit);
                    API.Me.SetTarget(unit);
                    break;
                }
            }
        }
    }
    
    // Method:          "SetNearestFocusUnit(int, Vector3[])"
    // What it does:    Targets and sets focus to the closest give unit.
    // Purpose:         Sometimes when iterating through the list of "Units," the closest does not always come first.
    //                  Often it is more effective to target closest unit first, rather than seemingly any
    //                  random unit within 100 yrds.
    public static void SetNearestFocusUnit(int ID, Vector3[] blacklist)
    {
        API.Me.ClearFocus();
        var killTarget = API.Me.GUID;
        float closestUnit = 5000f; // Insanely large distance, so first found distance will always be lower.
        int count = 0; // Blacklisted nodes
        
        // Identifying Closest Desired Unit
        foreach (var unit in API.Units)
        {
            if (unit.EntryID == ID && !unit.IsDead)
            {
                if (unit.Distance < closestUnit)
                {
                    for (int i = 0; i < blacklist.Length; i++)
                    {
                        if (unit.Distance2DTo(blacklist[i]) <= 10)
                        {
                            count++;
                            break;
                        }
                    }
                    if (count == 0)
                    {
                        // This stores the distance of the new unit, so when it re-iterates through,
                        // ultimately the unit with the lowest distance, or the one closest to you is stored
                        closestUnit = unit.Distance;
                        // This stores the GUID, which is necessary as ALL units share the same UnitID, but the GUID is a unique identifier.
                        killTarget = unit.GUID;
                    }
                }
            }
        }
        if (closestUnit == 5000)
        {
            API.Print("No Units Found Within Targetable Range.");
        }
        else
        {
            Int32 closest = (Int32)closestUnit; // Easier on the eyes to Print.
            API.Print("Closest target is " + closest + " yards away.");
            // Setting Focus to closest Unit
            foreach (var unit in API.Units)
            {
                if (unit.GUID == killTarget)
                {
                    API.Me.SetFocus(unit);
                    API.Me.SetTarget(unit);
                    break;
                }
            }
        }
    }
    
    // Method:          "ShadowElixirNeeded()"
    // What it Does:    Determines if a player even needs the shadow elixir in Spires of Arak anymore.  Returns boolean.
    // Purpose:         Essentially there are these repeatable weekly quests where you can loot shadow elixirs.  These elixirs
    //                  are particularly useful as they allow you to phase into a shadow realm at certain shrines and obtain
    //                  6 different weapons/treasures.  Thus, to prevent the bot from re-colleecting the elixirs needlessly if a player
    //                  revisited that profile at a later date, it checks how many of the treasures remain to be looted, and then checks
    //                  how many elixirs the player possesses.  If it is less than the needed amount, the bot will then go collect a shadow elixir.
    public static bool ShadowElixerNeeded() 
    {
        // Each of theses quest IDs represents one of the treasures to be looted at the shrines in Spires of Arak.
        int[] quests = {36386,36390,36389,36388,36381,36392};
        int count = 0;
        for (int i = 0; i < quests.Length; i++) 
        {
            if (!API.IsQuestCompleted(quests[i])) 
            {
                count++;
            }
        }
        int itemCount = API.ExecuteLua<int>("local itemCount = GetItemCount(115463, false, false); return itemCount;");
        if (itemCount < count) 
        {
            return true;
        }
        return false;
    }
    
    // Method:          "ShredderGarrisonAbility()"
    // What it does:    Moves to Focus Position, then uses zone Shredder ability, prioritizing both attacks.
    // Purpose:         Using the zone ability is critical, and to keep the code from getting too 
    //                  bloated. Calling this method on need will be very efficient and useful to take out powerful enemies.
    //                  Due to 15 min Cooldown, use sparingly.
    public static IEnumerable<int> ShredderGarrisonAbility()
    {
        if (RemainingSpellCD(164050) == 0 || API.Me.IsOnTransport) 
        {
            // Necessary to disable combat because bot will get distracted and activate combat base and not attack the main target.
            API.DisableCombat = true;
            // while player is far away... move to target
            while (API.Me.Focus != null && API.Me.Focus.Distance2D > 14.0)
            {
                API.CTM(API.Me.Focus.Position);
                yield return 100;
            }
            // This is important because often you get turned around for whatever reason right when you target NPC
            // and then your first shot or two is wasted.
            API.SetFacing(API.Me.Focus);
            if (!API.Me.IsOnTransport) 
            {
                API.ExecuteLua("DraenorZoneAbilityFrame:Show(); DraenorZoneAbilityFrame.SpellButton:Click()");
                yield return 2000;
            }
            // The while "Me.IsOnTransport" is critical because what if you did not kill the npc before your time expired.
            // The script needs to stop attempting to use vehicle abilities once you have exited the vehicle.
            while (API.Me.IsOnTransport && API.Me.Focus != null && !API.Me.Focus.IsDead)
            {
                while (API.Me.Focus.Distance2D > 14.0)
                {
                    API.CTM(API.Me.Focus.Position);
                    yield return 100;
                }
                // This extra "CTM" method is placed here because it just one-clicks and forces you to face target again.
                // Without this the npc often will move, and in a vehicle, the shredder, player movement doesn't function normal
                // with the bot.
                API.CTM(API.Me.Focus.Position);
                // The following if/else is to prioritise vehicle abilities...Number 2 is prioritised. 
                if (RemainingSpellCD(165422) > 0) 
                {
                    API.ExecuteLua("OverrideActionBarButton1:Click()");
                    Random rnd = new Random();
                    yield return rnd.Next(1700, 1900);
                }
                else 
                {
                    API.ExecuteLua("OverrideActionBarButton2:Click()");
                }
            }
            // If NPC is killed with time still left, why stand there and wait til it goes away?  Just exit it and keep moving on.
            API.ExecuteLua("OverrideActionBarLeaveFrameLeaveButton:Click()");
            API.DisableCombat = false;
        }
    }
    
    // Method:          "SmugglingRunMacro()"
    // What it Does:    Calls the Smuggler and opens the vendor. Buys useful items if player has need.
    // Purpose:         To streamline the questing process in Spires of Arak, this vendor sells some extraordinarily useful items that are very cheap
    //                  that can greatly increase the player's damage and/or speed and thus clear the objectives faster.
    //                  Also, it will seek out the rare "Follower" sold on the vendor, if not already owned, it will purchase it.
    //                  It is recommended to use this within a recusrive method.
    //                  It is recommended to run this in a separate thread.
    public static IEnumerable<int> SmugglingRunMacro()
    {
         // Smuggler's Run is Not on CoolDown and you are in the right zone, and not on a transport.
        if (RemainingSpellCD(170097) == 0 && API.Me.ZoneId == 6722 && !API.Me.IsOnTransport)
        {
            Inventory.Refresh();
            int item1 = API.ExecuteLua<int>("local itemCount =GetItemCount(113277, false, false); return itemCount;");
            int item2 = API.ExecuteLua<int>("local itemCount =GetItemCount(113276, false, false); return itemCount;");
            int item3 = API.ExecuteLua<int>("local itemCount =GetItemCount(113275, false, false); return itemCount;");
            int item4 = API.ExecuteLua<int>("local itemCount =GetItemCount(113274, false, false); return itemCount;");
            int item5 = API.ExecuteLua<int>("local itemCount =GetItemCount(113273, false, false); return itemCount;");
            if (item1 < 1 || item2 < 1 || item3 < 1 || item4 < 1 || item5 < 1 || (NeedsFollower(58876) && GetPlayerGold() > 400))
            {
                API.ExecuteLua("DraenorZoneAbilityFrame:Show(); DraenorZoneAbilityFrame.SpellButton:Click()");
                yield return 6000;
                // Targeting NPC Smuggler
                int count = 0;
                while (API.Me.Focus == null && count < 3) 
                {
                    API.Print("Targeting the Smuggler...");
                    SetFocusUnit(84243);
                    yield return 2000;
                    if (API.Me.Focus == null)
                    {
                        count ++;
                        if (count == 3)
                        {
                            API.Print("For Some Reason, Player Was Unable to Locate the Smuggler. Continuing On...");
                        }
                    }
                }
                while (API.Me.Focus != null && API.Me.Focus.Distance > 5) 
                {
                    API.MoveTo(API.Me.Focus.Position);
                    yield return 50;
                }
                if (API.Me.Focus != null && API.Me.Focus.EntryID == 84243) 
                {
                    API.Me.Focus.Interact();
                    yield return 1000;
                    API.ExecuteLua("GossipTitleButton1:Click()");
                    yield return 1000;
                    // First check if player needs to buy the rare follower on here.
                    bool toBuy = API.ExecuteLua<bool>("local allFollowers = C_Garrison.GetFollowers(); local toBuy = false; for x, y in pairs(allFollowers) do if (y.displayID == 58876 and y.isCollected == nil) then toBuy = true; break; end end; return toBuy;");
                    if (toBuy && GetPlayerGold() > 400)
                    {
                        var check0 = new Fiber<int>(BuyVendorItemByID(116915,1));
                        while (check0.Run()) {yield return 100;}
                        yield return 1000;
                        Inventory.Refresh();
                        if (API.HasItem(116915))
                        {
                            API.Print("Buying Follower Ziri'ak, Yay! He's Pretty Rare to See on the Vendor Here!");
                            API.UseItem(116915);
                            yield return 2000;
                            API.Print("Yay! Ziri'ak is Now Your Follower!"); 
                        }    
                    }
                    // Let's check for that 500g toy as well, Bloodmane Charm.
                    bool notOwned = PlayerHasToy(113096);
                    if (notOwned && GetPlayerGold() > 400)
                    {
                        var check0 = new Fiber<int>(BuyVendorItemByID(113096,1));
                        while (check0.Run()) {yield return 100;}
                        yield return 1000;
                        Inventory.Refresh();
                        if (API.HasItem(113096))
                        {
                            API.Print("Buying the Rare Toy \"Bloodmane Charm \" Found on the Vendor!");
                            API.UseItem(113096);
                            yield return 2000;
                            API.Print("Toy is now learned! Yay!"); 
                        }    
                    }
                    
                    // If player is not already in posession of an item and doesn't have the aura, he sells it.
                    if (item1 < 1) 
                    {
                        var check1 = new Fiber<int>(BuyVendorItemByID(113277,1));
                        while (check1.Run()) {yield return 100;}
                    }
                    if (item2 < 1) 
                    {
                        var check2 = new Fiber<int>(BuyVendorItemByID(113276,1));
                        while (check2.Run()) {yield return 100;}
                    }
                    if (item3 < 1) 
                    {
                        var check3 = new Fiber<int>(BuyVendorItemByID(113275,1));
                        while (check3.Run()) {yield return 100;}
                    }
                    if (item4 < 1) 
                    {
                        var check4 = new Fiber<int>(BuyVendorItemByID(113274,1));
                        while (check4.Run()) {yield return 100;}
                    }
                    if (item5 < 1) 
                    {
                        var check5 = new Fiber<int>(BuyVendorItemByID(113273,1));
                        while (check5.Run()) {yield return 100;}
                    }
                    yield return 1000;                    
                    API.ExecuteLua("CloseMerchant()");
                    yield return 1000;
                    API.Me.ClearFocus();
                }
            }
        }
        if (API.HasItem(113277) && !API.HasAura(166357))
        {
            API.Print("Using \"Ogreblood Potion\" for 20% Increased Damage and Healing");
            API.UseItem(113277);
        }        
        if (API.HasItem(113276) && !API.HasAura(166361))
        {
            API.Print("Using \"Pridehunter's Fang\" for 20% Bleed Damage");
            API.UseItem(113276);
        }
        if (API.HasItem(113275) && !API.HasAura(166355))
        {
            API.Print("Using \"Ravenlord's Talon\" for 30% Reduced Class Ability Costs");
            API.UseItem(113275);
        }
        if (API.HasItem(113273) && !API.HasAura(166353))
        {
            API.Print("Using \"Orb of the Soulstealer\" for 5% Increased Damage and Healing");
            API.UseItem(113273);
        }
        if (API.HasItem(113274) && !API.HasAura(166354))
        {
            API.Print("Using \"Plume of Celerity\" for 10% Increased Haste and Movement Speed");
            API.UseItem(113274);
        }
    }
    
    // Method:          TakeElevator(int,int,float,float,float)
    //                  WARNING!!! This is 100% working, but the "Catch" I want to add some additional assistance to resolve this, then recursively hit it again. 
    //                  Work in progress until then.
    // What it Does:    Allows the navigation of any elevator
    // Purpose:         At times in the script, transversing an elevator effectively can be a cumbersome to program
    //                  and as such I wrote a scalable method... the only key thing needed is for the developer to
    //                  time how long it takes the elevator to go from the bottom to the top, or the other way around.
    //                  Also, the position you would like the player to exit the elevator and travel to.  The travel time
    //                  was kind of a rough solution because it appears that while on the elevator, the API freezes all return values
    //                  thus I cannot seem to get an accurate positional check, so the timing allows me to enter, then determine exit time.
    public static IEnumerable<int> TakeElevator(int ElevatorID, int elevatorTravelTime, float x, float y, float z) 
    {
        double position;
        double position2;
        bool elevatorFound = false;
        Vector3 destination = new Vector3(x,y,z);
        foreach (var unit in API.GameObjects)
        {
            // This first determines if the elevator is properly identified.
        	if (unit.EntryID == ElevatorID)
        	{
                elevatorFound = true;
                API.SetFacing(unit);
                // The choice to disable combat is because once on the elevator, player should not attempt to leave it
                // or it could mess up the passing as the bot remembers its last spot before combat starts then returns to it
                API.DisableCombat = true;
                API.Print("Waiting For the Elevator...");
                position = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                yield return 100;
                position2 = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                yield return 100;
                
                // Some Added Redundancy to not attempt to take the elevator if it just arrived
                // Lest you try to hop on right before it moves away.
                while (Math.Sqrt(API.Me.DistanceSquaredTo(unit)) <= 20.0) {
                    yield return 100;
                }
                
                // The two positional checks right after each other are to determine movement of the elevator.
                // if they are equal, elevator is not moving, but if they are different, like the second location is further than the first,
                // then it can easily be determined it is moving away from you.
                // This first check holds position until the elevator moves.  This is actually really critical because what if
                // player arrives at the elevator and the elevator is at location already.  The method would recognize this then quickly try to jump on.
                // This could create a problem though because what if the elevator was only going to be there a split second more, then player tries to
                // traverse then ends up missing it.  This just helps avoid that... Long explanation I know.
                while (position == position2)
                {
                    position = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                    yield return 100;
                    position2 = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                    yield return 100;
                }
                // Meaning it is moving away from you or it is at least 10 yrds away.
                if (position != position2 || Math.Sqrt(API.Me.DistanceSquaredTo(unit)) > 10.0) 
                {
                    API.Print("Elevator is Moving...");
                    if (position > position2) 
                    {
                        API.Print("Elevator is Moving Towards Us... Almost Here!");
                    }
                    else 
                    {
                        API.Print("Elevator is Moving Away! Patience!");
                        while(position != position2) 
                        {
                            position = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                            yield return 100;
                            position2 = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                            yield return 100;
                        }
                        API.Print("Elevator Has Stopped at Other Side.  Let's Wait For It To Return!");
                        while(position == position2) 
                        {
                            position = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                            yield return 100;
                            position2 = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                            yield return 100;
                        }
                        API.Print("Alright, It Is Coming Back to us. Get Ready!");
                    }
                    while(position != position2) 
                    {
                        position = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                        yield return 100;
                        position2 = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                        yield return 100;
                    }
                    // Checking position of elevator against your current position.
                    if (API.Me.Position.Z < unit.Position.Z)
                    {
                        while(unit.Position.Z > (API.Me.Position.Z + 1.0)) 
                        {
                            yield return 100;
                        }
                    }
                    else if (API.Me.Position.Z > unit.Position.Z)
                    {
                        while(unit.Position.Z < (API.Me.Position.Z - 1.0)) 
                        {
                            yield return 100;
                        }
                    }
                }
                API.Print("Ah, Excellent! Elevator is Here! Hop On Quick!");
                API.CTM(unit.Position);
                // The 4 seconds is added here to account for the stoppage of when you enter the elevator and it is stationary
                yield return ((elevatorTravelTime + 4) * 1000);
                while(!API.CTM(destination)) 
                {
                    yield return 200;
                }
                API.Print("You Have Successfully Beaten the Elevator Boss... Congratulations!!!");
        	}
        }
        if (!elevatorFound) 
        {
            API.Print("No Elevator Found. Please Be Sure elevator ID is Entered Properly and You are Next to It");
            yield break;
        }
        API.DisableCombat = false;
    }
    
    // Method:          TakeElevator(int,int,float,float,float,float,float,float)
    // What it Does:    Allows the navigation of any elevator! This Elevator method allows the input of a starting position!
    // Purpose:         At times in the script, transversing an elevator effectively can be a cumbersome to program
    //                  and as such I wrote a scalable method... the only key thing needed is for the developer to
    //                  time how long it takes the elevator to go from the bottom to the top, or the other way around.
    //                  Also, the position you would like the player to exit the elevator and travel to.  The travel time
    //                  was kind of a rough solution because it appears that while on the elevator, the API freezes all return values
    //                  thus I cannot seem to get an accurate positional check, so the timing allows me to enter, then determine exit time.
    public static IEnumerable<int> TakeElevator(int ElevatorID, int elevatorTravelTime, float startX, float startY, float startZ, float x, float y, float z) 
    {
        double position;
        double position2;
        bool elevatorFound = false;
        // Starting position to navigate to and wait for elevator (PLACE AT SAME LEVEL AS Elevator)
        Vector3 start = new Vector3(startX,startY,startZ);
        Vector3 destination = new Vector3(x,y,z);
        
        while (!API.MoveTo(start))
        {
            yield return 100;
        }
        foreach (var unit in API.GameObjects)
        {
            // This first determines if the elevator is properly identified.
        	if (unit.EntryID == ElevatorID)
        	{
                elevatorFound = true;
                API.SetFacing(unit);
                // The choice to disable combat is because once on the elevator, player should not attempt to leave it
                // or it could mess up the passing as the bot remembers its last spot before combat starts then returns to it
                API.DisableCombat = true;
                API.Print("Waiting For the Elevator...");
                position = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                yield return 100;
                position2 = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                yield return 100;
                
                // Some Added Redundancy to not attempt to take the elevator if it just arrived
                // Lest you try to hop on right before it moves away.
                while (Math.Sqrt(API.Me.DistanceSquaredTo(unit)) <= 20.0) {
                    yield return 100;
                }
                
                // The two positional checks right after each other are to determine movement of the elevator.
                // if they are equal, elevator is not moving, but if they are different, like the second location is further than the first,
                // then it can easily be determined it is moving away from you.
                // This first check holds position until the elevator moves.  This is actually really critical because what if
                // player arrives at the elevator and the elevator is at location already.  The method would recognize this then quickly try to jump on.
                // This could create a problem though because what if the elevator was only going to be there a split second more, then player tries to
                // traverse then ends up missing it.  This just helps avoid that... Long explanation I know.
                while (position == position2)
                {
                    position = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                    yield return 100;
                    position2 = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                    yield return 100;
                }
                // Meaning it is moving away from you or it is at least 10 yrds away.
                if (position != position2 || Math.Sqrt(API.Me.DistanceSquaredTo(unit)) > 10.0) 
                {
                    API.Print("Elevator is Moving...");
                    if (position > position2) 
                    {
                        API.Print("Elevator is Moving Towards Us... Almost Here!");
                    }
                    else 
                    {
                        API.Print("Elevator is Moving Away! Patience!");
                        while(position != position2) 
                        {
                            position = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                            yield return 100;
                            position2 = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                            yield return 100;
                        }
                        API.Print("Elevator Has Stopped at Other Side.  Let's Wait For It To Return!");
                        while(position == position2) 
                        {
                            position = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                            yield return 100;
                            position2 = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                            yield return 100;
                        }
                        API.Print("Alright, It Is Coming Back to us. Get Ready!");
                    }
                    // Checking position of elevator against your current position.
                    if (API.Me.Position.Z < unit.Position.Z)
                    {
                        while(unit.Position.Z > (startZ + 1.0)) 
                        {
                            yield return 100;
                        }
                    }
                    else if (API.Me.Position.Z > unit.Position.Z)
                    {
                        while(unit.Position.Z < (startZ - 1.0)) 
                        {
                            yield return 100;
                        }
                    }
                }
                API.Print("Ah, Excellent! Elevator is Here! Hop On Quick!");
                API.CTM(unit.Position);
                // The 4 seconds is added here to account for the stoppage of when you enter the elevator and it is stationary
                yield return ((elevatorTravelTime + 4) * 1000);
                while(!API.CTM(destination)) 
                {
                    yield return 200;
                }
                API.Print("You Have Successfully Beaten the Elevator Boss... Congratulations!!!");
        	}
        }
        if (!elevatorFound) 
        {
            API.Print("No Elevator Found. Please Be Sure elevator ID is Entered Properly and You are Next to It");
            yield break;
        }
        API.DisableCombat = false;
    }
    
    // Method:          "UnequipGear(int)"
    // What it does:    Scans through the bags for an open bag slot, and if it finds one, places weapon there.
    // Purpose:         Some quests are hard to complete because you need to use an item on a low-HP NPC.
    //                  Unfortunately, the bot often will kill the NPC off before you can use the item if your gear is high level.
    //                  By unequipping the weapon and other gear, you lose a TON of damage and make these events more unlikely.
    //                  Starts at Index "1" first so it at least keeps weapon equipped for abilities.
    public static void UnequipGear(int numItems)
    {
        int toRemove = API.GetFreeBagSlots();
        // Maximum numItems items to take off.
        if (toRemove >= numItems) 
        {
            toRemove = numItems;
        }
        int temp;
        int temp2 = 0;
        int numFree;
        int inventoryID;
        
        try
        {
            if (toRemove == 0)
            {
                throw new System.Exception("Unable to Unequip Your Weapon Due to No Free Bag Slots.");
            }
            
            // Need to Temporarily Disable AutoEquip so bot won't try to re-equip weapon.
            API.AutoEquipSettings.EquipItems = false;
            // Checking First if gear item is unequipped, if it is, then no need to cycle through this.
            if (API.ExecuteLua<int>("return GetInventoryItemID(\"player\", 1);") != 0) 
            {
                API.ExecuteLua("SaveEquipmentSet(\"TempQuesting\",100)");
                for (int i = 0; i < 5; i++)
                {
                    // Checking how many open slots in a bag.
                    numFree = API.ExecuteLua<int>("return GetContainerNumFreeSlots(" + i + ");");
                    inventoryID = 19 + i;
                    if (numFree > 0)
                    {
                        if (numFree > toRemove) 
                        {
                            numFree = toRemove;
                        }
                        
                        // i = Which Bag
                        // j = Open Slot in Bag
                        for (int j = temp2; j < numFree + temp2; j++) 
                        {
                            temp = j;
                            if (j == 4) 
                            {
                                temp = 15; // Cloak changes to position 15, though it should be 4... not sure why
                            }
                            API.ExecuteLua("PickupInventoryItem(" + temp + ");");
                            if (i == 0)
                            {
                            API.Print("Placing Gear Item Temporarily in Backpack");
                            API.ExecuteLua("PutItemInBackpack();");
                            }
                            else
                            {
                            API.Print("Placing Gear Piece in bag " + i + " to the left of your backpack.");
                            API.ExecuteLua("PutItemInBag(" + inventoryID + ");");
                            }
                        }
                    }
                    // If bag is full, but still gear to remove this will give us a number.
                    temp2 = temp2 + numFree;
                    toRemove = toRemove - numFree;
                    if (toRemove == 0) 
                    {
                        break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            API.Print(e + "Opening Vendor to Cleanup Some Bag Slots.");
            
            // NOT YET FINISHED!!!!!!!!!!!!!!!!!!!
            // Force Vendor - Todo, need to find API to force vendor.
            // To be Implemented Still...
            // Then... try again
            // UnequipGear(numItems);
        }
    }
    
    // Method:          "UnitFoundToLoot(int,int)"
    // What it does:    Determines if a lootable GameObject is within range.
    // Purpose:         Occasionally when destroyed a target, often they will drop many items to loot.
    //                  The way Rebot does the "CollectObject" in the editor is it will loot 1 object only
    //                  This is not good if you have a chain of actions to take, like "Use rocket" then loot items.
    //                  This will allow you to place this block of code within a while loop conditional
    //                  and then identify all detected objects until none are detected.
    public static bool UnitFoundToLoot(int ID, int yards)
    {
        bool found = false;
        foreach (var unit in API.GameObjects)
        {
            if (unit.EntryID == ID && API.Me.Distance2DTo(unit.Position) < yards)
            {
                found = true;
                break;
            }
        }
        if (found == false)
        {
            API.Print("There Are No Lootable Items in Range");
        }
        return found;
    }
    
    // Method:          "UseCannonInMovingVehicle(int[] targetIDs, int vehicleSpell, int spellCooldown)"
    // What it Does:    Targets the closest unit that is IN FRONT of the player and fires the given cannon spell.
    // Purpose:         Some flying quests can crash the bot as the built in cannon tool may try to target the cannon outside its given range.
    //                  This causes not just a bot crash, but can crash the entire WOW game itself.  So, I wrote a custom targeting system when in a "Moving"
    //                  vehicle.  The reason this is important is some moving vehicle quests the player's camera may change swiftly that the bot does not account
    //                  for with current tools.  This resolves that issue/
    // Add. notes:      Note - the "spellCooldown" argument is in SECONDS
    public static IEnumerable<int> UseCannonInMovingVehicle(int[] targetIDs, int vehicleSpell, int spellCooldown)
    {
        if (vehicleSpell < 1 || vehicleSpell > 9)
        {
            API.Print("Error! Vehicle Spell chosen does not exist! Please ensure you chose the correct ability, starting at position 1.\nSetting default vehicle ability to position 1.");
        }
        var killTarget = API.Me.GUID;
        float closestUnit = 5000f; // Insanely large distance, so first found distance will always be lower.
        float position1;
        float position2;
        API.Me.ClearFocus();
        
        // Identifying Closest Desired Unit
        foreach (var unit in API.Units)
        {
            for (int i = 0; i < targetIDs.Length;  i++)
                {
                        if (unit.EntryID == targetIDs[i] && !unit.IsDead)
                        {
                                if (unit.Distance < closestUnit)
                                {
                                    // This stores the distance of the new unit, so when it re-iterates through,
                                    // ultimately the unit with the lowest distance, or the one closest to you is stored
                                    closestUnit = unit.Distance;
                                    // This stores the GUID, which is necessary as ALL units share the same UnitID, but the GUID is a unique identifier.
                                    killTarget = unit.GUID;
                                }
                        }
                }
        }
        if (closestUnit == 5000f)
        {
            API.Print("No Units Found Within Targetable Range.");
        }
        else
        {
            Int32 closest = (Int32)closestUnit; // Easier on the eyes to Print.
                // Setting Focus to closest Unit
        
            foreach (var unit in API.Units)
            {
                if (unit.GUID == killTarget)
                {
                    API.Me.SetTarget(unit);
                    // Verifying Target is in front!!!
                    position1 = API.Me.Distance2DTo(unit.Position);
                    yield return 250;
                    position2 = API.Me.Distance2DTo(unit.Position);
                    if (position1 > position2) 
                    {
                        API.Print("Target Acquired... Firing on " + unit.Name + "!");
                        API.Me.SetFocus(unit);
                        var v = unit.Position - API.Me.Position;
                        v.Normalize();
                        ((UnitObject) API.Me.Transport).AimAt(API.Me.Target.PositionPredicted);
                        API.ExecuteLua(string.Format("local pitch = {0}; local delta = pitch - VehicleAimGetAngle(); VehicleAimIncrement(delta);", Math.Asin(v.Z)));
                        API.ExecuteLua("OverrideActionBarButton" + vehicleSpell + ":Click();");
                    }
                    else
                    {
                        yield return 250;
                        API.Me.ClearFocus();
                    }
                    break;
                }
            }
        }
        yield return (spellCooldown * 1000);
        API.Me.ClearFocus();
    }
    
    // Method:          "UseGuildBanner()"
    // What it does:    Uses a Horde Guild Banner at Player Position by prioritization of best (15% bonus gains) to worst (5%).
    // Purpose:         If the banner is not on cooldown, it will use it when called upon. This is to assist in the Player
    //                  questing and leveling quicker.  5-15% gains may not sound like a lot, but in a 10hr stretch, we are talking about savings
    //                  of on avg. an hour.  It all adds up!
    public static void UseGuildBanner()
    {
        if (API.HasItem(64402) && API.ItemCooldown(64402) == 0)
        {
            API.Print("Using \"Battle Standard of Coordination\"");
            API.UseItem(64402);
        }
        else if (API.HasItem(64401) && API.ItemCooldown(64401) == 0)
        {
            API.Print("Using \"Standard of Unity\"");
            API.UseItem(64401);
        }
        else if (API.HasItem(64400) && API.ItemCooldown(64400) == 0)
        {
            API.Print("Using \"Banner of Cooperation\"");
            API.UseItem(64400);
        }
    }
    
    // Method:          "WaitForSpell(int)"
    // What it Does:    It will cause the player to pause in place until the spell is off cooldown, but also report to the log
    //                  how much time is remaining every 15 seconds.
    // Purpose:         While this is recommended to be used on rare occasions, there are times when it is good to check if something
    //                  is on cooldown, and it would be prudent to wait until it is ready (think zone spell) if the timer
    //                  is lower.  For example, by setting a "wait" for say, your Gorgrond Shredder, rather than skipping a challenging NPC
    //                  completely, if the wait is only a minute or less, just WAIT! :)
    public static IEnumerable<int> WaitForSpell(int spellID)
    {
        int time = RemainingSpellCD(spellID);
        if (time > 0) 
        {
            string name = API.ExecuteLua<string>("local name = GetSpellInfo(" + spellID + "); return name;");
            API.Print("The Spell \"" + name + "\" Is Still on Cooldown. Waiting " + time + " seconds!");
            while (time != 0) 
            {
                if (time <= 15)
                {
                    API.Print("The Spell is Just About Ready! " + time + " seconds...");
                    yield return (time * 1000);
                    yield break;
                }
                else
                {
                    yield return 15000;
                    time = RemainingSpellCD(spellID);
                    API.Print(time + " Seconds Until Spell Is Ready. Patience!!!");
                }
            }
        }
    }
    
    // Method:          "WaitUntilOffTaxi()"
    // What it Does:    This is good in the initialization check of any profile as it will determine if the player is on a 
    //                  taxi/transport and pause all script actions until exiting.
    // Purpose:         The player is unable to do anything on a taxi, so rather than the script trying to execute
    //                  the subsequent objectives and fail, and potentially desync the progress of the player from the script
    //                  it will pause until the flight is over.
    public static IEnumerable<int> WaitUntilOffTaxi()
    {
        if (API.Me.IsOnTaxi)
        {
            API.Print("Player is Currently on a Taxi, Please Be Patient And Enjoy the Scenery!");
            int count = 0;
            while (API.Me.IsOnTaxi)
            {
                yield return 1000;
                count++;
            }
            API.Print("Player Exited the Flightpath after " + count + " seconds!");
        }
        yield break;
    }
    
    // Method:          XPMacro()
    // What it Does:    Simply checks if player has the XP potion aura, and if not, and player owns a potion
    //                  it will now use that ppotion.
    public static IEnumerable<int> XPMacro() 
    {
        int potionCount = API.ExecuteLua<int>("local itemCount = GetItemCount(120182, false, false); return itemCount;");
        if (potionCount > 0 && !API.Me.HasAura(178119) && API.Me.Level < 100) 
        {
            API.UseItem(120182);
            yield return 500;
        }
    }
    
    // Method:          XPMacroRecursive()
    // What it Does:    Recursively checks if the player HAS used an XP potion, thus having the 20% bonus aura.  If not, then if the player has the
    //                  potion in their bags it will use it.  It also checks player level and will exit the recursive method if player hits lvl 100.
    // Purpose:         Rather than have players configure their own macros, this one will spam slightly more intelligently in the background.
    //                  Also, it gives an escape more intelligently if player hits lvl 100.  ALSO, if say, the macro is activated but it turns out the
    //                  Garrison is not yet established, it impliments a 5 min delay before checking again if it is, to prevent unnecessary spam from this thread.
    //                  It is recommended to run this in a separate thread.
    public static IEnumerable<int> XPMacroRecursive() 
    {
        // The initial "If" seems redundant, but what it does is force a bag check, as some API
        // methods do not work until server LOOKS into a player bag.
        if (API.Me.Level > 99) 
        {
            API.Print("Since You Are Level Capped, We Will Not Use the XP Potion!");
            yield break;
        }
        // If Garrison is not yet established!
        if (!API.IsQuestCompleted(34378)) 
        {
            Random rnd = new Random();
            int pause = rnd.Next(300000,302000);
            yield return pause;
        }
        int potionCount = API.ExecuteLua<int>("local itemCount = GetItemCount(120182, false, false); return itemCount;");
        if (potionCount > 0 && !API.Me.HasAura(178119) && API.Me.Level < 100) 
        {
            API.UseItem(120182);
        }
        Random rand = new Random();
        int wait = rand.Next(13000,15200);
        yield return wait;
        // Recursive Return
        var check = new Fiber<int>(XPMacro());
        while (check.Run()) 
        {
            yield return 100;
        }
    }
    
}