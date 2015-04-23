
/* Author:   	Sklug a.k.a TheGenomeWhisperer
|       	  	The following functions are commonly called to be used as a "help"
|		        For common scripting events as part of questing profile behaviors.
| NOTE:     	"ExecuteLua" API function executes "LUA" code language inserted into the C#
| NOTE:     	ALSO!!! This not a more standardized API with setters and getters, which ultimately would be nice,
| NOTE:	    	but I am writing more focused script functions specifically for questing, so 
| NOTE:	    	please understand if this is lacking common programming practices :D
| Final Note:   Class does not need Static Main as it will be injected into the Rebot.exe through the "Editor"
|               Additional Information can be found at Rebot.to
*/  

public class QuestingHelps
{

    public static ReBotAPI API;
    public static Fiber<int> QH;

    public QuestingHelps() { }


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
            Int32 closest = (Int32)closestUnit; // Easier on the eyes to Print
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
    public static bool UnitFoundToLoot(int ID)
    {
        bool found = false;
        foreach (var unit in API.GameObjects)
        {
            if (unit.EntryID == ID && API.Me.Distance2DTo(unit.Position) < 90)
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
    
    
    // What it does:  Moves to Focus Position, then uses zone ability, prioritizing both attacks.
    // Purpose:       Using the zone ability is critical, and to keep the code from getting too 
    //                bloated, calling this method should be a more sufficient method for repeat use.
    public static IEnumerable<int> GorgrondGarrisonAbility()
    {
        API.DisableCombat = true;
        while (API.Me.Focus.Distance2D > 10.0)
        {
            API.CTM(API.Me.Focus.Position);
            yield return 200;
        }
        API.ExecuteMacro("/use Garrison Ability");
        yield return 3000;
        while (API.Me.IsOnTransport && API.Me.Focus != null && !API.Me.Focus.IsDead)
        {
            API.SetFacing(API.Me.Focus);
            if (API.Me.Focus.Distance2D <= 13)
            {
                API.ExecuteMacro("/click OverrideActionBarButton2");
            }
            else
            {
                while (API.Me.Focus.Distance2D > 13.0)
                {
                    API.CTM(API.Me.Focus.Position);
                    yield return 100;
                }
                API.ExecuteMacro("/click OverrideActionBarButton2");
            }
            API.SetFacing(API.Me.Focus);
            for (int i = 0; i < 3; i++)
            {
                API.ExecuteMacro("/click OverrideActionBarButton1");
                Random rnd = new Random();
                yield return rnd.Next(1700, 1900);
            }
        }
        API.ExecuteMacro("/click OverrideActionBarLeaveFrameLeaveButton");
        API.DisableCombat = false;
    }

    // What it does:  Moves to Focus Position, then uses zone ability 3 times.
    // Purpose:       Due to the nature of having to call the Garrison ability multiple times when facing
    //                challenging rare monsters in the world, it would be more efficient to
    //                merely require just calling this method.
    public static IEnumerable<int> TaladorGarrisonAbility()
    {
        API.DisableCombat = true;
        while (API.Me.Focus != null && API.Me.Focus.Distance2D > 5)  // This ensure player paths to the focus target until it is 5 yrds or closer.
        {
            API.MoveTo(API.Me.Focus.Position);
            yield return 100;
        }
        for (int i = 0; i < 3; i++)
        {
            API.ExecuteMacro("/use Garrison Ability");
            yield return 100;
            API.ClickOnTerrain(API.Me.Focus.Position);
            yield return 1500;
        }
        API.DisableCombat = false;
    }


    // What it does:  Algorithm that Determines Amount of XP potions in possession, if you need more, and if you havunds for more.
    //                then it purchases the indicated amount to buy for you from vendor.
    // Purpose:       To automate XP potion purchasing without wasting Garrison resources.  By automating the purchas of
    //	  	          say, 5 potions, everytime the profile started, it would buy 5 more.  With connectivity issues
    //        	      or other misc. interruptions, you could potentially waste tons of resources needlessly and end up
    //        	      with many remaining useless potions.  This prevents that.
    //		          Note: Initial value "maxOwn" can be adjusted to any desired potion amount count.
    public static void BuyExpPotions(int maxOwn)
    {
        int toBuy = 0;
        int currentPotionCount = API.ExecuteLua<int>("local potions = GetItemCount(120182); return potions;");
        API.Print(currentPotionCount + " XP Potions In Your Possession!");

        if (currentPotionCount < maxOwn)
        {
            int gResources = API.ExecuteLua<int>("local _, amount= GetCurrencyInfo(824); return amount;");
            int canBuy = gResources / 100;

            if (canBuy > maxOwn)
            {
                canBuy = maxOwn; // Player is capable of buying more than default amount, so setting lower amount
            }
            else if (canBuy == 0)
            {
                API.Print("Not Enough Resources to Buy XP Potion");
            }

            // Next step determines if player should buy max amount, or lesser amount if some already in posession

            if ((canBuy + currentPotionCount) > maxOwn)
            {
                toBuy = canBuy - currentPotionCount;
            }
            else
            {
                toBuy = canBuy; // Basically currentPotionCount is zero
            }

            string buy = "/run BuyMerchantItem(21," + toBuy + ")";  // Building LUA script to paste in string form
            API.ExecuteMacro(buy);
        }
        else
        {
            API.Print("You Already Have a Sufficient Supply of XP Potions! YAY!");
        }
    }


    // What it does:  Returns a boolean on if it needs to purchase potions or not.
    // Purpose:       To determine if the player has enough and can buy more XP potions.
    //		          This is a good boolean gate to prevent
    //		  
    public static bool PlayerNeedsExpPotions(int maxOwn)
    {
        bool result = false;
        int currentPotionCount = API.ExecuteLua<int>("local potions = GetItemCount(120182); return potions;");

        if (currentPotionCount < maxOwn)
        {
            int gResources = API.ExecuteLua<int>("local _, amount= GetCurrencyInfo(824); return amount;");
            int canBuy = gResources / 100;
            if (canBuy != 0)
            {
                result = true;
            }
        }
        return result;
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


    // What it does:  Basic check to first, see if one of the 3 banners is available.  If so,
    //		          it will execute the MoveTo to the given location and then use an available banner.
    // Purpose:       The reason why this is separated from the previous function GuildBanners() is because
    //		          it is necessary to do a check before telling the player to moveTo a destination.
    //		          It would be time-wasting of the player to move to the destination to use banner if it
    //		          was not currently available in the first place.
    //		          The MAJOR advantage using a Vector3 as an argument and not explicitly listing the argument is
    //		          that I can call to this whenever I wish to, for whatever reason, and if I wish to pass it an
    //		          explicit pre-determined location I can, or I can implement give it an object location as well.
    public static IEnumerable<int> PlaceGuildBannerAtDestination(Vector3 travel)
   {
        if ((API.HasItem(64402) && API.ItemCooldown(64402) == 0) || (API.HasItem(64401) && API.ItemCooldown(64401) == 0) || (API.HasItem(64400) && API.ItemCooldown(64400) == 0))
	    {
		     while (API.Me.Distance2DTo(travel) > 5)
		     {
			 API.MoveTo(travel);
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
            if ((API.HasItem(64402) && API.ItemCooldown(64402) == 0) || (API.HasItem(64401) && API.ItemCooldown(64401) == 0) || (API.HasItem(64400) && API.ItemCooldown(64400) == 0))
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
                // Determining Cooldown remaining on next banner.
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
        PlaceGuildBannerOnAuraCheck();
    }


    // What it does:  This does a boolean check on quest progress for a given objective and
    //                returns true if the current quest progress matches the one given in the argument.
    // Purpose:       Occasionally when writing quest templates, sometimes you want something to keep
    //                checking or doing things over and over again until it accomplishes it.  For example,
    //                imagine a quest that wanted you to destroy 5 targets, but it was all one objective
    //                so you start off with a description like "0/5 targets destroyed."  Often these targets
    //                are not repeatable as the server stores some private information tied to your character, however,
    //                given the current interact and collectobject abilities within Rebot, it will often attempt to
    //                destroy the same target over and over again.  This allows you to break up a single
    //                quest objective into different pieces, because if say the objective changes to "1/5 targets destroyed"
    //                then it will carry on to the next sub-part of the objective rather than being stuck
    //                in often what can occur is an infinite loop.
    public static bool questObjectiveProgress(int questID, int objective, string description)
    {
        bool result = false;
        string luaCall = "local currentProgress = GetQuestObjectiveInfo(" + questID + ", " + objective + "); return currentProgress;";
        string progress = API.ExecuteLua<string>(luaCall);

        if (progress.Equals(description))
        {
            result = true;
        }
        return result;
    }

}
