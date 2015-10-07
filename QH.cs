    
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
*/  
	
public class QH
{
    public static ReBotAPI API;
    public static Fiber<int> Fib;

    // Empty Constructor
    public QH() { }	
		
	// Method:			"AbandonGarrisonFlightQuests(int)"
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
            string buy = "BuyMerchantItem(22," + toBuy + ")";  // Building LUA script to paste in string form
            API.ExecuteLua(buy);
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
        double x = API.ExecuteLua<double>("local posX, _= GetPlayerMapPosition('player'); return posX;");
        double y = API.ExecuteLua<double>("local _, posY = GetPlayerMapPosition('player'); return posY;");
        int z = API.ExecuteLua<int>("return GetCurrentMapAreaID()");
        var v = API.CoordsToPositionByAreaId(x * 100,y * 100,z);
        Vector3 location = new Vector3(1666.5f, 1743.6f, 298.6f);      
        
        if (IsClose(1410f, 1728.5f, 310.3f, 390)) 
        {
            // Z location is important because the Gordal fortress is high, so by determining player IS close to zone AND is above the z coordinate,
            // the height of the player can be determined as likely to represent its position.  I COULD write a 3D area map, but this was significantly
            // less time-consuming and just as effeective.
            if ((v.Z > 302.4) || ((v.Z > 296.0) && (API.Me.Distance2DTo(location) > 47.05))) 
            {
        		return true;
        	}       
        }
        return false;
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
    public static bool NeedsFollower(string Name)
    {
        string followerName = ("\"" + Name + "\"");
        bool toBuy = false;
        int displayID = API.ExecuteLua<int>("local allFollowers = C_Garrison.GetFollowers(); local followerID = 0; for x, y in pairs(allFollowers) do if (y.name == " + followerName + ") then followerID = y.displayID; break; end end; return followerID;");
        toBuy = API.ExecuteLua<bool>("local allFollowers = C_Garrison.GetFollowers(); local toBuy = false; for x, y in pairs(allFollowers) do if (y.displayID == " + displayID + " and y.isCollected == nil) then toBuy = true; break; end end; return toBuy");
        return toBuy;
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
    
    // Method:          "ProfBuildingID(string)"
    //      WARNING - Method is only compatible with ENGLISH clients and should probably be made redundant.
    // Method:          ProfBuildingID(string)
    // What it Does:    This matches the player's profession to the correct building ID within the garrison
    // Purpose:         This is important because each Garrison Building has a corresponding ID.  This will match the
    //                  given profession to its corresponding ID so that the correct matching building can be placed.
    public static int ProfBuildingID(string professionName)
    {
        int idNumber;
        switch(professionName) 
        {
            case "Alchemy":
                idNumber = 76;
                break;
            case "Enchanting" : 
            	idNumber = 93;
                break;
            case "Engineering":
            	idNumber = 91;
                break;
            case "Jewelcrafting":
            	idNumber = 96;
                break;
            case "Inscription":
            	idNumber = 95;
                break;
            case "Tailoring": 
            	idNumber = 94;
                break;
            case "Blacksmithing": 
            	idNumber = 60;
                break;
            case "Leatherworking":
            	idNumber = 90;
                break;
            case "Herbalism":
            	API.Print("Herbalism Has No Corresponding Building to Place. Using Default!");
                idNumber = 0;
                break;
            case "Mining":
            	API.Print("Mining Has No Corresponding Building to Place. Using Default!");
                idNumber = 0;
                break;
            case "Skinning":
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
    
    // Method:          "QuestObjectiveProgress(int,int,int,string)"
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
    
    // Method:          "SetNearestFocusUnit(int[])"
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
            int item1 = API.ExecuteLua<int>("local itemCount =GetItemCount(113277, false, false); return itemCount;");
            int item2 = API.ExecuteLua<int>("local itemCount =GetItemCount(113276, false, false); return itemCount;");
            int item3 = API.ExecuteLua<int>("local itemCount =GetItemCount(113275, false, false); return itemCount;");
            int item4 = API.ExecuteLua<int>("local itemCount =GetItemCount(113274, false, false); return itemCount;");
            int item5 = API.ExecuteLua<int>("local itemCount =GetItemCount(113273, false, false); return itemCount;");
            if (item1 < 1 || item2 < 1 || item3 < 1 || item4 < 1 || item5 < 1 || (NeedsFollower("Ziri'ak") && GetPlayerGold() > 400))
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
                    API.CTM(API.Me.Focus.Position);
                    yield return 50;
                }
                if (API.Me.Focus != null && API.Me.Focus.EntryID == 84243) 
                {
                    API.Me.Focus.Interact();
                    yield return 1000;
                    API.ExecuteLua("GossipTitleButton1:Click()");
                    yield return 1000;
                    bool toBuy = API.ExecuteLua<bool>("local allFollowers = C_Garrison.GetFollowers(); local toBuy = false; for x, y in pairs(allFollowers) do if (y.displayID == 58876 and y.isCollected == nil) then toBuy = true; break; end end; return toBuy;");
                    if (toBuy && GetPlayerGold() > 400)
                    {
                        API.ExecuteLua("for i = 1, GetMerchantNumItems() do local _, _, price, _, numAvailable, _, _ = GetMerchantItemInfo(i); if (price == 4000000) then BuyMerchantItem(i, 1); end end;");
                        yield return 1000;
                        if (API.HasItem(116915))
                        {
                            API.Print("Buying Follower Ziri'ak, Yay! He's Pretty Rare to See on the Vendor Here!");
                            API.UseItem(116915);
                            yield return 2000;
                            API.Print("Yay! Ziri'ak is Now Your Follower!");
                        }                       
                    }
                    API.ExecuteLua("for i = 1, GetMerchantNumItems() do local _, _, price, _, numAvailable, _, _ = GetMerchantItemInfo(i); if ((price == 20000 and (GetItemCount(113276, false, false) < 1 or GetItemCount(113275, false, false) < 1 or GetItemCount(113273, false, false) < 1 or GetItemCount(113277, false, false) < 1)) or (price == 54595 and GetItemCount(113274, false, false) < 1)) then BuyMerchantItem(i, numAvailable); end end;");
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
    //      WARNING!!! This is 100% working, but the "Catch" I want to add some additional assistance to resolve this, then recursively hit it again. 
    //      Work in progress until then.
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