/* Author:   	Sklug a.k.a TheGenomeWhisperer
|       	  	The following functions are commonly called to be used as a "help"
|		          For common scripting events as part of questing profile behaviors.
| NOTE:     	"ExecuteLua" API function executes "LUA" code language inserted into the C#
| NOTE:     	ALSO!!! This not a more standardized API with setters and getters, which ultimately would be nice,
| NOTE:	    	but I am writing more focused script functions specifically for questing, so 
| NOTE:	    	please understand if this is lacking common programming practices :D
| Final Note: Class does not need Static Main as it will be injected into the Rebot.exe through the "Editor"
|             Additional Information can be found at Rebot.to
*/


class QuestingHelps
{

    public static ReBotAPI API;
    public static Fiber<int> QH;

    public QuestingHelps() { }
    // Function:      GorgrondGarrisonAbility()
    // What it does:  Moves to Focus Position, then uses zone ability 3 times on target location.
    // Purpose:       To Counteract the Combat Program, as once it begins, it freezes all script functions
    //	              as priority until combat stops, I disable combat ability until after function as completed.
    public static IEnumerable<int> GorgrondGarrisonAbility()
    {
        API.DisableCombat = true;

        while (API.Me.Focus.Distance2D > 5)  // This ensure player paths to the focus target until it is 5 yrds or closer.
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
    
    // Function:      PlayerNeedsExpPotions(int)
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
            int gResources = API.ExecuteLua<int>("local _, amount = GetCurrencyInfo(824); return amount;");
            int canBuy = gResources / 100;
            if (canBuy != 0)
            {
                result = true;
            }
        }
        return result;
    }
    
`   // Function:      BuyExpPotions(int)
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
    
    
    
    
    
// End Class
}
