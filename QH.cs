
/* Author:   	Sklug a.k.a TheGenomeWhisperer
|       	The following functions are commonly called to be used as a "help"
|		For common scripting events as part of questing profile behaviors.
| NOTE:     	"ExecuteLua" API function executes "Lua" code language inserted into the C#
| NOTE:     	ALSO!!! This not a more standardized API with setters and getters, which ultimately would be nice,
| NOTE:	    	but I am writing more focused script functions specifically for questing, so 
| NOTE:	    	please understand if this is lacking common programming practices :D
| Final Note:   Class does not need Static Main as it will be injected into the Rebot.exe through the "Editor"
|               Additional Information can be found at Rebot.to
|               QH = Questing Helps
|
|       WARNING WARNING!!! I have not yet cleaed this and organized it/alphabetized it yet...
*/  

public class QH
{
    public static ReBotAPI API;
    public static Fiber<int> Fib;

    public QH() { 
    }

    // What it does:  It returns how many quests are remaining to complete within the profile.
    // Purpose:       Basically to just keep track of profile progress so the player can know how much is remaining
    //                to Do without having to dig through this.
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

    // What it does:  Scans through the bags for an open bag slot, and if it finds one, places weapon there.
    // Purpose:       Some quests are hard to complete because you need to use an item on a low-HP NPC.
    //                Unfortunately, the bot often will kill the NPC off before you can use the item if your gear is high level.
    //                By unequipping the weapon and other gear, you lose a TON of damage and make these events more unlikely.
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
            // Checking First if Weapon unequipped, if it is, then no need to cycle through this.
            if (API.ExecuteLua<int>("return GetInventoryItemID(\"player\", 16);") != 0) 
            {
                API.ExecuteLua("SaveEquipmentSet(\"Questing\",100)");
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
                            if (j == 0)
                            {
                                // Removing Weapon First
                                temp = 16;
                            }
                            if (j == 4) 
                            {
                                temp = 15; // Cloak changes to position 15, though it should be 4... not sure why
                            }
                            API.ExecuteMacro("/script PickupInventoryItem(" + temp + ");");
                            if (i == 0)
                            {
                            API.Print("Placing Gear Item Temporarily in Backpack");
                            API.ExecuteMacro("/script PutItemInBackpack();");
                            }
                            else
                            {
                            API.Print("Placing Gear Piece in bag " + i + " to the left of your backpack.");
                            API.ExecuteMacro("/script PutItemInBag(" + inventoryID + ");");
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
            // Force Vendor - Todo, need to find API to force vendor.
            // To be Implemented Still...
            // Then... try again
            // UnequipGear(numItems);
        }
    }
    
    // What it does:  Re-Equips the same weapon you had previously removed. Reactivates "Auto-Equip"
    // Purpose:       So you have a weapon again!!!                
    public static void ReEquipGear()
    {
        int hasWeapon = API.ExecuteLua<int>("return GetInventoryItemID(\"player\", 16);");
        if (hasWeapon == 0)
        {
            // Returning Global Variable from server side -- will not work if you reloaded or relogged.
        	API.ExecuteLua("UseEquipmentSet(\"Questing\",100)");
            API.Print("Re-Equipping Your Gear");
            API.AutoEquipSettings.EquipItems = true;
            API.ExecuteLua("DeleteEquipmentSet(\"Questing\",100)");
        }
        else
        {
            API.Print("Weapon Already Equipped");
        }
    }
    

    // What it does:  Sets given NPC to the focus target and also targets it.
    // Purpose:       Useful to have a target set as focus as often it is easy to lose the target.
    //                This also prevents potential crashing by checking empty objects. You can now do a simple
    //                if Me.Focus != null and know that you are secure.
    public static void SetFocusUnit(int ID)
    {
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

    // comment incoming
    public static void SetFocusUnitMaxDistance(int ID, int yards) {
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

    // What it does:  Targets and sets focus to the closest give unit.
    // Purpose:       Sometimes when iterating through the list of "Units," the closest does not always come first.
    //                Often it is more effective to target closest unit first, rather than seemingly any
    //                random unit within 100 yrds.
    public static void SetNearestFocusUnit(int ID)
    {
        var killTarget = API.Me.GUID;
        float closestUnit = 5000f; // Insanely large distance, so first found distance will always be lower.

        // Identifying Closest Desired Unit
        foreach (var unit in API.Units)
        {
            if (unit.EntryID == ID && !unit.IsDead)
            {
                if (unit.Distance < closestUnit)
                {
                    closestUnit = unit.Distance;
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
    
    // What it does:  Determines if a lootable GameObject is within range.
    // Purpose:       Occasionally when destroyed a target, often they will drop many items to loot.
    //                The way Rebot does the "CollectObject" in the editor is it will loot 1 object only
    //                This is not good if you have a chain of actions to take, like "Use rocket" then loot items.
    //                This will allow you to place this block of code within a while loop conditional
    //                and then identify all detected objects until none are detected.
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
    
    // Comment Incoming
    public static IEnumerable<int> CTA_GarrisonAbility() {
        if (RemainingSpellCD(161332) == 0) {
            API.DisableCombat = true;
            while (API.Me.Focus != null && API.Me.Focus.Distance2D > 10.0) {
                API.CTM(API.Me.Focus.Position);
                yield return 200;
            }
            API.ExecuteMacro("/use Garrison Ability");
            yield return 500;
            API.DisableCombat = false;
        }
        else {
            API.Print("Player Wanted to Use \"Call to Arms\" Garrison Ability, But it Was on CD");
            yield break;
        }
    }

    // What it does:  Moves to Focus Position, then uses zone ability, prioritizing both attacks.
    // Purpose:       Using the zone ability is critical, and to keep the code from getting too 
    //                bloated, calling this method should be a more sufficient method for repeat use.
    public static IEnumerable<int> ShredderGarrisonAbility()
    {
        if (RemainingSpellCD(164050) == 0 || API.Me.IsOnTransport) {
            API.DisableCombat = true;
            while (API.Me.Focus != null && API.Me.Focus.Distance2D > 14.0)
            {
                API.CTM(API.Me.Focus.Position);
                yield return 100;
            }
            API.SetFacing(API.Me.Focus);
            if (!API.Me.IsOnTransport) {
                API.ExecuteMacro("/use Garrison Ability");
                yield return 2000;
            }
            while (API.Me.IsOnTransport && API.Me.Focus != null && !API.Me.Focus.IsDead)
            {
                while (API.Me.Focus.Distance2D > 14.0)
                {
                    API.CTM(API.Me.Focus.Position);
                    yield return 100;
                }
                API.CTM(API.Me.Focus.Position);
                if (RemainingSpellCD(165422) > 0) {
                    API.ExecuteMacro("/click OverrideActionBarButton1");
                    Random rnd = new Random();
                    yield return rnd.Next(1700, 1900);
                }
                else {
                    API.ExecuteMacro("/click OverrideActionBarButton2");
                }
            }
            API.ExecuteMacro("/click OverrideActionBarLeaveFrameLeaveButton");
            API.DisableCombat = false;
        }
    }

    // What it does:  Moves to Focus Position, then uses zone ability 3 times.
    // Purpose:       Due to the nature of having to call the Garrison ability multiple times when facing
    //                challenging rare monsters in the world, it would be more efficient to
    //                merely require just calling this method. numStrikes should never be more than 3.
    public static IEnumerable<int> ArsenalGarrisonAbility(int numStrikes)
    {
        if (RemainingSpellCD(162075) == 0) {
            API.DisableCombat = true;
            while (API.Me.Focus != null && API.Me.Focus.Distance2D > 10)  // This ensure player paths to the focus target until it is 5 yrds or closer.
            {
                API.MoveTo(API.Me.Focus.Position);
                yield return 100;
            }
            for (int i = 0; i < numStrikes; i++)
            {
                while (API.Me.Focus != null) {
                    API.ExecuteMacro("/use Garrison Ability");
                    yield return 100;
                    API.ClickOnTerrain(API.Me.Focus.Position);
                    yield return 1800;
                }
            }
            API.DisableCombat = false;
        }
        else {
            API.Print("Player Wanted to Use the Arsenal Garrison Ability but it was on Cooldown!");
        }
    }

    // What it does:  Uses a Guild Banner at Player Position by, prioritization of best (15% bonus gains) to worse (5%).
    // Purpose:       If the banner is not on cooldown, it will use it when called upon so player may level faster.
    //	              Also, it prioritizes the best of the banners to be used so you are always using best one available.
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
    
    // Method:          "BannerAvailable"
    // What it Does:    Checks if player has any of the 3 Guild Banners AND the Banner is usuable(not on CD)
    // Purpose:         Help remove code bloat by offering the boolean check here, before cycling through other Banner methods
    public static bool BannerAvailable()
    {
        if ((API.HasItem(64402) && API.ItemCooldown(64402) == 0) || (API.HasItem(64401) && API.ItemCooldown(64401) == 0)
         || (API.HasItem(64400) && API.ItemCooldown(64400) == 0)) 
        {
            return true;
        }
        return false;
    }

    // Method:          "QuestNotDone"
    // What it does:    Verify that the quest is not turned in and completed, but also verifies it is
    //                  not completed yet still in your logs.
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

    // What it does:  Basic check to first, see if one of the 3 banners is available.  If so,
    //		          it will execute the MoveTo to the given location and then use an available banner.
    // Purpose:       The reason why this is separated from the previous function GuildBanners() is because
    //		          it is necessary to do a check before telling the player to moveTo a destination.
    //		          It would be time-wasting of the player to move to the destination to use banner if it
    //		          was not currently available in the first place.
    //		          The MAJOR advantage using a Vector3 as an argument and not explicitly listing the argument is
    //		          that I can call to this whenever I wish to, for whatever reason, and if I wish to pass it an
    //		          explicit pre-determined location I can, or I can implement give it an object location as well.
    public static IEnumerable<int> PlaceGuildBannerAt(float x, float y, float z)
   {
        if (BannerAvailable())
	    {
            Vector3 location = new Vector3(x,y,z);
		     while (!API.MoveTo(location)) {
                 yield return 100;
             }
		     UseGuildBanner();
	    }
	    else
	    {
		     API.Print("Guild Banner Not Currently Available");
	    }
    }

    /*
    |  The Following Banner method is based on the idea of having an aura check whilst in combat
    | and if you are lacking the aura (i.e. no banner nearby), then it drops one, rather than
    |  relying on a specific location pre-determined.  These are to be used as a forked thread
    |  from the main thread to run in parallel to the script to aura check in the background.
    */


    // What it does:  Uses a Guild Banner at Player Position by, prioritization of best (15% bonus gains) to worse (5%).
    //                To Be Used recursively, hence it yield breaks if player hits lvl 99, or is player has no banners.
    //                This also delays the continuous check if all 3 banners are on CD, and it ONLY does a check
    //                to be used if in combat.
    // Purpose:       If the banner is not on cooldown, it will use it when called upon so player may level faster.
    //	              Also, it prioritizes the best of the banners to be used so you are always using best one available.
    //		          What makes this different from previous UseGuildBanner() function is...
    //		          ...in effort to conserve from spamming the conditionals, if all 3 banners are on cooldown
    //		          then it determines the shortest cooldown time then "yield returns" or "waits" to carry on
    //		          Ideally, this is a feature to be used with a background aura that checks continuously.
    //		          WARNING! DO NOT USE unless thread is forked and this is checking in the background in parallel
    //		          If you use it in main thread, bot will not continue whilst banners on cooldown.
    public static IEnumerable<int> PlaceGuildBannerOnAuraCheck()
    {
        if (!API.HasItem(64402) && !API.HasItem(64401) && !API.HasItem(64400))
        {
            yield break;  // No Banners in Possession, ends thread.
        }
        if (API.Me.Level > 99)
        {
            yield break;  // Player hits level 100, ends thread.
        }
        if (API.Me.Target != null && API.Me.Target.Distance < 100 && !API.Me.Target.IsFriendly && !API.Me.Target.IsDead)
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
        var check = new Fiber<int>(PlaceGuildBannerOnAuraCheck());
        while (check.Run()) {
            yield return 100;
        }
    }


    // What it does:  This does a boolean check on quest progress for a given objective and
    //                returns true if the current quest progress matches the one given in the argument.
    // Purpose:       Occasionally when writing quest templates, sometimes you want something to keep
    //                checking or doing things over and over again until it accomplishes it.  For example,
    //                imagine a quest that wanted you to destroy 5 targets, but it was all one objective
    //                so you start off with a description like "0/5"  Often these targets
    //                are not repeatable as the server stores some private information tied to your character, however,
    //                given the current interact and collectobject abilities within Rebot, it will often attempt to
    //                destroy the same target over and over again.  This allows you to break up a single
    //                quest objective into different pieces, because if say the objective changes to "1/5 targets destroyed"
    //                then it will carry on to the next sub-part of the objective rather than being stuck
    //                in often what can occur is an infinite loop.
    public static bool questObjectiveProgress(int questID, int objective, string description)
    {
        string luaCall = "local currentProgress = GetQuestObjectiveInfo(" + questID + ", " + objective + "); return currentProgress;";
        string progress = API.ExecuteLua<string>(luaCall);

        if (progress.Substring(0,3).Equals(description))
        {
            return true;
        }
        return false;
    }
    
    
    // What it does:  Navigates out of a lvl 2 or 3 Garrison Town Hall
    // Purpose:       Rebot has serious Mesh issues when starting a script within a Garrison
    //                But, even worse, starting within a town hall.  This solves this issue
    //                by using Click-To-Move actions to navigate out of the town hall successfully.
    //                Use this as a "initialization" conditional check at start of a script, imo.
    public static IEnumerable<int> GTownHallExit()
    {
        int tier = API.ExecuteLua<int>("local level = C_Garrison.GetGarrisonInfo(); return level;");
        string zone = API.ExecuteLua<string>("local zone = GetMinimapZoneText(); return zone;");
        Vector3 location = new Vector3(5559.2f, 4604.8f, 141.7f);
        if (API.Me.Distance2DTo(location) < 24 || (zone.Equals("Town Hall")) && API.IsInGarrison && (tier == 2 || tier == 3))
        {
            API.GlobalBotSettings.FlightMasterDiscoverRange = 0.0f;
            while(!API.CTM(5562.576f, 4601.484f, 141.7169f))
            {
                yield return 100;
            }
            while(!API.CTM(5576.729f, 4584.367f, 141.0846f))  
            {
                yield return 100;
            }
            while(!API.CTM(5591.181f, 4569.721f, 136.2159f))
                        {
                yield return 100;
            }
            API.GlobalBotSettings.FlightMasterDiscoverRange = 75.0f;
        }
        yield break;
    }
    
    
    // What it does:  Matches the sub-zone of the player to the given one.
    // Purpose:       Occasionally scripting requires "helps" or additional "move-tos"
    //                at times for certain quests, for many reasons.  However, it would be
    //                very annoying to head to those instructions everytime you stopped and restarted
    //                the profile.  This makes it so you can first check the sub-zone/area you are in
    //                before doing so, avoiding a lot of wasted time.
    public static bool MiniMapZoneEquals(string name)
    {
        string zone = API.ExecuteLua<string>("zone = GetMinimapZoneText(); return zone;");
        if (zone.Equals(name))
        {
            return true;
        }
        return false;
    }
    
    
    // What it does:  Checks Scenario Stage Progress (or Complex phased Quest Stages)
    // Purpose:       The Built in API for Rebot only had conditional information on current quest objective.
    //                Some more complex quests have many sub-objectives.  This will open up more
    //                control on scripting specifically for sub-objectives of the main objective.
    public static bool ScenarioStageEquals(int progress)
    {
        int stage = API.ExecuteLua<int>("local _, currentStage = C_Scenario.GetInfo(); return currentStage;");
        if (stage == progress)
        {
        	return true;
        }
        return false;
    }
    
    public static bool IsClose(float x, float y, float z, int distance)
    {
        Vector3 location = new Vector3(x,y,z);
        if (API.Me.Distance2DTo(location) < distance)
        {
            return true;
        }
        return false;
    }
    
    // What it does:  Identifies which Gossip Option is the correct one.
    // Purpose:       If a player has many collected quests, certain NPCs can be loaded with gossip options
    //                thus it is prudent to first check which matches the intended choice before selecting it.
    //                This largely is used with the first quest to each new zone.
    public static void DoGossipOnMatch(string choice)
    {
        // Initializing Function
        string title = "title0";
        string luaCall = ("local title0,_ = GetGossipOptions(); if title0 ~= nil then return title0 else return \"nil\" end");
        string result = API.ExecuteLua<string>(luaCall);
        // Now Ready to Iterate through All Gossip Options!
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
    
    // Method:          "RemainingSpellCD"
    // What it Does:    Returns, in Seconds, how long until the spell is available again
    // Purpose:         Useful for conditional checks on spell casting, prioritization of a spell, etc.    
    public static int RemainingSpellCD(int spellID)
    {
        float start = API.ExecuteLua<float>("local start = GetSpellCooldown(" + spellID + "); return start;");
        float duration = API.ExecuteLua<float>("local _,duration = GetSpellCooldown(" + spellID + "); return duration;");
        float timePassed = API.ExecuteLua<float>("return GetTime();");
        float coolDown = timePassed - start;
        int result = (int)(duration-coolDown);
        if (result < 0) {
            result = 0;
        }
        return result;
    }
    
    // Comment Incoming
    public static bool ShadowElixerNeeded() {
        int[] quests = {36386,36390,36389,36388,36381,36392};
        int count = 0;
        for (int i = 0; i < quests.Length; i++) {
            if (!API.IsQuestCompleted(quests[i])) {
                count++;
            }
        }
        if (API.ItemCount(234735) < count) {
            return true;
        }
        return false;
    }
    
    // Comment Incoming
    public static int ExpPotionsNeeded() {
        // Adding a Quick escape if Player is almost lvl 100... no need to waste resources.
        if (API.Me.Level == 99) {
            float currXP = API.ExecuteLua<float>("return UnitXP(\"player\")");
            float nextLvl = API.ExecuteLua<float>("return UnitXPMax(\"player\")");
            if  (API.Me.HasAura(178119) && currXP/nextLvl > 0.75) {
                API.Print("You are Almost Level 100! No Need to buy any more XP potions... Aura still Active.");
                return 0;
            }
            else if (currXP/nextLvl > 0.85) {
                API.Print("You are less than 15% XP From Level 100. Let's Not Waste Any Garrison Resources on Potions!");
                return 0;
            }
        }
        // Calculating How Many Potions a Player at this lvl should buy.
        double num = (100 - (API.Me.Level))*1.4;
        int maxOwn = (int)num;
        int toBuy;
        int currentPotionCount = API.ExecuteLua<int>("potions = GetItemCount(120182); return potions;");
        API.Print(currentPotionCount + " XP Potions In Your Possession!");
        
        if (currentPotionCount < maxOwn) 
        {
        	int gResources = API.ExecuteLua<int>("_, amount = GetCurrencyInfo(824); return amount;");
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
    
    // Comment Incoming
    public static void BuyExpPotions(int toBuy)
    {
        if (toBuy > 0) {
            string buy = "/run BuyMerchantItem(21," + toBuy + ")";  // Building LUA script to paste in string form
            API.ExecuteMacro(buy);
        }
    }
    
    // Comment Incoming
    // Recursive for live tracking...
    public static IEnumerable<int> XPMacro() {
        // The initial "If" seems redundant, but what it does is force a bag check, as some API
        // methods do not work until server LOOKS into a player bag.
        if (API.HasItem(120182) == true || API.HasItem(120182) == false) {
            if (API.Me.Level > 99) {
                API.Print("Since You Are Level Capped, We Will Not Use the XP Potion!");
                yield break;
            }
            if (!API.IsQuestCompleted(34378)) {
                Random rnd = new Random();
                int pause = rnd.Next(300000,302000);
                yield return pause;
            }
            if (API.HasItem(120182) && !API.Me.HasAura(178119) && API.Me.Level < 100) {
                API.ExecuteMacro("/use Excess Potion of Accelerated Learning");
            }
            Random rand = new Random();
            int wait = rand.Next(15000,17000);
            yield return wait;
            // Recursive Return
            var check = new Fiber<int>(XPMacro());
            while (check.Run()) {
                yield return 100;
            }
        }
    }
    
    // Comment Incoming
    public static bool IsMovingAway(float initialDistance) {
        bool result = false;
        if (API.Me.Focus != null) {
            if (API.Me.Focus.Distance > initialDistance) {
            result = true;
            }
        }
        else {
            API.Print("Unable to find target...");
        }
        return result;
    }
    
    // Comment Incoming
    public static bool HasGuildBannerAura() {
        if (API.HasAura(90633) || API.HasAura(90632) || API.HasAura(90631)) {
            return true;
        }
        return false;
    }
    
    // Comment Incoming
    public static bool HasProfession() {
        int prof1 = API.ExecuteLua<int>("local prof1 = GetProfessions(); return prof1");
        int prof2 = API.ExecuteLua<int>("local _,prof2 = GetProfessions(); return prof2");
        
        if (prof1 != 0 || prof2 != 0) {
            return true;
        }
        return false;
    }
    
    // Comment Incoming
    public static int ProfBuildingID(string professionName) {
        int idNumber;
        switch(professionName) {
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
    
    // Comment Incoming
    public static IEnumerable<int> HearthToGarrison() {
        if (!API.IsInGarrison) {
            if (API.ItemCooldown(110560) == 0) {
                API.Print("Hearthing to Garrison");
                API.UseItem(110560);
                while(API.Me.IsCasting) {
                    yield return 100;
                }
                while (!API.IsInGarrison) {
                    yield return 100;
                }
            }
            else {
                API.Print("Player Wanted to Hearth to Garrison, but it is on Cooldown...");
                yield break;
            }
        }
    }
    
    // Comment Incoming
    public static bool IsInGordalFortress() {
        double x = API.ExecuteLua<double>("local posX, _= GetPlayerMapPosition('player'); return posX;");
        double y = API.ExecuteLua<double>("local _, posY = GetPlayerMapPosition('player'); return posY;");
        int z = API.ExecuteLua<int>("return GetCurrentMapAreaID()");
        var v = API.CoordsToPositionByAreaId(x * 100,y * 100,z);
        Vector3 location = new Vector3(1666.5f, 1743.6f, 298.6f);      
        
        if (IsClose(1410f, 1728.5f, 310.3f, 390)) {
            if ((v.Z > 302.4) || ((v.Z > 296.0) && (API.Me.Distance2DTo(location) > 47.05))) {
        		return true;
        	}       
        }
        return false;
    }
    
    // Only use this if you are standing right in front of Elevator at ground floor
    // Coordinates are for Vector3 position where to go to on Exit
    // Further Comments incoming.
    public static IEnumerable<int> TakeElevator(int ElevatorID, int elevatorTravelTime, float x, float y, float z) {
        double position;
        double position2;
        bool elevatorFound = false;
        Vector3 destination = new Vector3(x,y,z);
        foreach (var unit in API.GameObjects)
        {
        	if (unit.EntryID == ElevatorID)
        	{
                elevatorFound = true;
                API.SetFacing(unit);
                API.DisableCombat = true;
                API.Print("Waiting For the Elevator...");
                position = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                yield return 200;
                position2 = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                yield return 200;
                if (position != position2 || Math.Sqrt(API.Me.DistanceSquaredTo(unit)) > 10.0) {
                    API.Print("Elevator is Moving...");
                    if (position > position2) {
                        API.Print("Elevator is Moving down... Almost Here!");
                    }
                    else {
                        API.Print("Elevator is Moving Up! Patience!");
                        while(position != position2) {
                            position = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                            yield return 200;
                            position2 = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                            yield return 200;
                        }
                        API.Print("Elevator Has Reached the top.  Let's Wait For It To Return!");
                        while(position == position2) {
                            position = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                            yield return 200;
                            position2 = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                            yield return 200;
                        }
                        API.Print("Alright, coming back down. Get Ready!");
                    }
                    while(position != position2) {
                        position = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                        yield return 200;
                        position2 = Math.Sqrt(API.Me.DistanceSquaredTo(unit));
                        yield return 200;
                    }
                }
                API.Print("Ah, Excellent! Elevator is at the Bottom! Hop On Quick!");
                API.CTM(unit.Position);
                yield return ((elevatorTravelTime + 4) * 1000);
                while(!API.CTM(destination)) {
                    yield return 200;
                }
                API.Print("You Have Successfully Beaten the Elevator Boss... Congratulations!!!");
        	}
        }
        if (!elevatorFound) {
            API.Print("No Elevator Found. Please Be Sure elevator ID is Entered Properly and You are Next to It");
            yield break;
        }
        API.DisableCombat = false;
    }

}
    // Comment Incoming
//     public static bool hasMount(string mountName) {
//         int numMounts = ExecuteLua<int>("local numMounts = C_MountJournal.GetNumMounts(); return numMounts");
//         G["azure"] = false;
//         string name = "";
//         
//         for (int i = 0; i < numMounts; i++) {
//             name = ExecuteLua<string>("local name = C_MountJournal.GetMountInfo(" + i + "); return name;");
//             if (name.Equals(mountName)) {
//                 G["azure"] = true;
//                 return true;
//             }
//         }
//         return false;
//     }



//     -- Identifying Primary Professions
// 
// local buildingID1 = 93; -- default is Enchanter's Study
// local buildingID2 = 51; -- default is Storehouse
// local pName1 = "Enchanting"
// local pName2 = "Storehouse"
// local plotID1 = 18
// local plotID2 = 19
// 
// -- Grabbing Profession Index identifies
// local prof1, prof2 = GetProfessions();
// if prof1 == nil and prof2 == nil then
// 	print("No Professions Found, Using Default Professions...")
// else
//     -- function to act like a switch statement since LUA has no Switch Logic
//     
//     function getBuildingToPlot(professionName) 
//     	local idNumber = 0
//     	if professionName == "Alchemy" then 
//     		idNumber = 76
//     	elseif professionName == "Enchanting" then 
//     		idNumber = 93
//     	elseif professionName == "Engineering" then 
//     		idNumber = 91
//     	elseif professionName == "Jewelcrafting" then 
//     		idNumber = 96
//     	elseif professionName == "Inscription" then 
//     		idNumber = 95
//     	elseif professionName == "Tailoring" then 
//     		idNumber = 94
//     	elseif professionName == "Blacksmithing" then 
//     		idNumber = 60
//     	elseif professionName == "Leatherworking" then 
//     		idNumber = 90
//     	else 
//     		print("Missing a Profession")
//     	end
//     	return idNumber;
//     end
//     
//     if (prof1 ~= nil) then
//         local name = GetProfessionInfo(prof1);
//         if name ~=  "Mining" and name ~= "Herbalism" and name ~= "Skinning" then
//         	pName1 = name;
//         	buildingID1 = getBuildingToPlot(pName1);
//         elseif name == "Herbalism" then
//     		print("Herbalism Has No Corresponding Building to Place. Using Default!")
//     	elseif name == "Mining" then
//     		print("Mining Has No Corresponding Building to Place. Using Default!")
//         elseif name == "Skinning" then
//             print("Skinning Has No Corresponding Building to Place. Using Default!")
//         end
//     end
//     
//     if (prof2 ~= nil) then 
//         local name2 = GetProfessionInfo(prof2);
//         if name2 ~=  "Mining" and name2 ~= "Herbalism" and name2 ~= "Skinning" then
//         	pName2 = name2
//         	buildingID2 = getBuildingToPlot(pName2)
//         elseif name2 == "Herbalism" then
//     		print("Herbalism Has No Corresponding Building to Place. Using Default!")
//     	elseif name2 == "Mining" then
//     		print("Mining Has No Corresponding Building to Place. Using Default!")
//         elseif name2 == "Skinning" then
//             print("Skinning Has No Corresponding Building to Place. Using Default!")
//         end
//     end
// end
// 
// local count = 1
// for x, y in pairs(C_Garrison.GetPlotsForBuilding(buildingID1)) do
// 	if count == 1 then
// 		plotID1 = y
// 		count = count + 1
// 	elseif count == 2 then
// 		plotID2 = y
// 		count = count + 1
// 	elseif count == 3 then
// 		print("What!? 3 plots? Do you already have a tier 3 garrison? Only setting for 2 buildings!")
// 	end
// end
// 
// local builtID1, buildingName1 = C_Garrison.GetOwnedBuildingInfo(plotID1)
// local builtID2, buildingName2 = C_Garrison.GetOwnedBuildingInfo(plotID2)
// if builtID1 == nil then
// 	if plotID1 ~= builtID2 then
// 		C_Garrison.PlaceBuilding(plotID1, buildingID1);
// 		print("Placing Plot For " .. pName1 .. "!");
// 	elseif builtID2 ~= nil then
// 		C_Garrison.PlaceBuilding(plotID1, buildingID2);
// 		print("Placing Plot For " .. pName2 .. "!");
// 	end
// else
// 	print("You Already Have the " .. buildingName1 .. " There!")
// end
// 
// local builtID1, buildingName1 = C_Garrison.GetOwnedBuildingInfo(plotID1)
// local builtID2, buildingName2 = C_Garrison.GetOwnedBuildingInfo(plotID2)
// 	
// if builtID2 == nil then
// 	if plotID2 ~= builtID1 then
// 		C_Garrison.PlaceBuilding(plotID2, buildingID2);
// 		print("Placing Plot For " .. pName2 .. "!");
// 	elseif builtID1 ~= nil then
// 		C_Garrison.PlaceBuilding(plotID2, buildingID1);
// 		print("Placing Plot For " .. pName1 .. "!");
// 	end
// else
// 	print("You Already Have the " .. buildingName2 .. " There!")
// end

// LUA Code for Mount Identifier
