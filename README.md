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
**void AbandonQuest(int questID)**  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Abandons the given quest from the player's quest-log.

**bool BannerAvailable()**  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Returns true if any of the 3 Guild "Bonus XP" banners are available for use.

**IEnumerable<int> DisableAddOn(string name, bool reload)**  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Disables the given addon (name reflects directory folder name), and reloads player UI if 2nd argument is true.

**void DoGossipOnMatch(string choice)**  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Selects the gossip option on any NPC that matches the given string. Full Unicode support compatibility



###Draenor Specific Methods
**IEnumerable\<int\> AbandonGarrisonFlightQuests(int questToKeep)**  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Abandons all Draenor garrison missions that require flight gossip interaction, except for the questID the player wishes to keep.

**IEnumerable\<int\> ArsenalGarrisonAbility(int numStrikes)**  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Whilst in the Talador Zone or Tanaan Jungle, uses the Outpost ability on a "Focus" target, max 3 strikes.

**void BuyExpPotions(int toBuy)**  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Buys the given amount of XP potions "toBuy" at the Garrison Vendor

**IEnumerable\<int\> CTA_GarrisonAbility()**  
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;When in Frostfire Ridge or Tanaan Jungle, Uses the Call to Arms Garrison zone ability on a "Focus" target.
