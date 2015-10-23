WarcraftCustomAPI
=================

**Author: TheGenomeWhisperer. (aka Sklug)**

The QH Class refers to "QuestingHelps" in regards to building a more complete API.  The purpose of its completion is to assist in expanding the avaliable toolset given by the official [Rebot API](http://www.rebot.to/showthread.php?t=1899) so as to have a more efficient process of building questing profiles, scripts, and templates to make a 100% hands-free and efficient questing process for the player.

Also included are full profiles representative of the QH Class in full implemntation...  
**Note:** These profiles are the NUMBER ONE most-popular Draenor questing profiles for World of Warcraft outside of Honorbuddy.

Community Public Release is [HERE ON THE OFFICIAL REBOT FOURMS:](http://www.rebot.to/showthread.php?t=4930)

**The following list contains all Methods and Properties of the QH.cs Class:**

##Constructor Summary
**QH()**  
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Default Constructor Takes No Arguments
    
##Method Summary  

|||
|-----:|:--------------------------|
|**void** | **AbandonQuest(int questID)**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Abandons the given quest from the player's quest-log.|
|**bool** |**BannerAvailable()**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns true if any of the 3 Guild "Bonus XP" banners are available for use.|
|**IEnumerable\<int\>** | **DisableAddOn(string name, bool reload)**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Disables the given addon (name reflects directory folder name), and reloads player UI if 2nd argument is true.|
|**void** |**DoGossipOnMatch(string choice)**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Selects the gossip option on any NPC that matches the given string. Full Unicode Support.|
|**bool** |**HasAddOnEnabled(string name)**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns true only if the given addon is enabled, not just if its installed.|
|**bool** |**HasArchaeology()**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns true if the player has the 2ndary Archaeology profession learned.|
|**bool** |**HasGuildBannerAura()**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns true if the player already is within 100 yrd range of a guild banner, thus having the bonus aura.|
|**bool** |**HasHiddenAura(int spellID)**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns true if player has the given Aura, not based on only buffs/debuffs, but all hidden auras.|
|**bool** |**HasProfession()**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns true if the player has either 1 or 2 professions learned, regardless of skill level.|
|**IEnumerable\<int\>** |**HearthToGarrison()**
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Uses the Garrison hearthstone to return to the Draenor Garrison. This can be useful in future expansions due to the amenities in the Garrison.|
|**bool** |**IsClose(float x, float y, float z, int distance)**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns true if the player distance to the Vector3 coordinate is within the given yards|
|**bool** |**IsInScenario()**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns true of the player is currently phased into a "scenario" quest/instance|
|**bool** |**ItemsNeededForQuest(int questID, int objective, int itemID)**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns true if the player needs to collect more items to fulfill all quest requirements.|
|**bool** |**MiniMapZoneEquals(string name)**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns true if the minimapzone name equals the given name.|
|**IEnumerable\<int\>** |**PlaceGuildBannerAt(float x, float y, float z)**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Places a guild banner at the Vector3 given position by priority of best available banner.|
|**IEnumerable\<int\>** |**PlaceGuildBannerOnAuraCheck()**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;If the player is not within 100 yards of a banner, and the player is in combat, it will place the best available banner at the player position.|
|*bool** |**QuestNotDone(int QuestID)**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns true if the quest not only is NOT completed, but it is also NOT in the log and complete, just not turned in yet.|


||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;|


###Draenor Specific Methods

|||
|-----:|:--------------------------|
|**IEnumerable\<int\>** |**AbandonGarrisonFlightQuests(int questToKeep)**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Abandons all garrison missions that require gossip interaction, except for quest ID to keep.|
|**IEnumerable\<int\>** |**ArsenalGarrisonAbility(int numStrikes)**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Whilst in the Talador Zone or Tanaan Jungle, uses the Outpost ability on a "Focus" target, max 3 strikes.|
|**void** |**BuyExpPotions(int toBuy)**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Buys the given amount of XP potions "toBuy" at the Garrison Vendor|
|**IEnumerable\<int\>** |**CTA_GarrisonAbility()**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;When in Frostfire Ridge or Tanaan Jungle, Uses the Call to Arms Garrison zone ability on a "Focus" target.|
|**int** |**ExpPotionsNeeded()**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns the suggested amount of XP potions to buy based on current player level and proximity to lvl 100.|
|**int** |**GetGarrisonLevel()**|  
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns the current Rank of the player Garrison (1-3) as an int.|
|**int** |**GetGarrisonResources()**|  
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns the current amount of garrison resources player has as an int.|
|**int** |**GetPlayerGold()**|  
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns the amount of Gold the player has as an int.|
|**IEnumerable\<int\>** |**GTownHallExit()**|  
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Custom navigation out of Garrison Town Hall for rank 2 and 3 garrisons.|
|**bool** |**IsInGordalFortress()**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns true if the player is located within the Gordal Fortress in Talador.|
|**bool** |**NeedsFollower(string Name)**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns true if the the player still needs the given follower.
|**int** |**ProfBuildingID(string professionName)**|
||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns the Garrison plotID to match the given profession for a small-size plot.|


||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;|


||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;|


||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;|


||&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;|
