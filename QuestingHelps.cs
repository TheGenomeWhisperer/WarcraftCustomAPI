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
    
    
    
    
    
// End Class
}
