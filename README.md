WarcraftCustomAPI
=================

**Author: TheGenomeWhisperer. (aka Sklug)**

The QH Class refers to "QuestingHelps" in regards to building a more complete API.  The purpose of its completion is to assist in expanding the avaliable toolset given by the official [Rebot API](http://www.rebot.to/showthread.php?t=1899) so as to have a more efficient process of building questing profiles, scripts, and templates to make a 100% hands-free and efficient questing process for the player.

Also included are full profiles representative of the QH Class in full implemntation...  
**Note:** These profiles are the NUMBER ONE most-popular Draenor questing profiles for World of Warcraft outside of Honorbuddy.

Community Public Release is [HERE ON THE OFFICIAL REBOT FOURMS:](http://www.rebot.to/showthread.php?t=4930)

**The following list contains all Methods and Properties of the QH.cs Class:**

###Constructor Summary
**QH()**  
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Default Constructor Takes No Arguments
    
###Method Summary  
***void* AbandonQuest(int questID)** 
>>Abandons the given quest from the player's quest-log.

***bool* BannerAvailable()**
>>Returns true if any of the 3 Guild "Bonus XP" banners are available for use.



####Draenor Specific Methods
**IEnumerable\<int\> AbandonGarrisonFlightQuests(int questToKeep)**
>>Abandons all Draenor garrison missions that require flight gossip interaction, except for the questID the player wishes to keep.

**IEnumerable\<int\> ArsenalGarrisonAbility(int numStrikes)**
>>Whilst in the Talador Zone, using the Outpost ability on a "Focus" target, max 3 strikes.
