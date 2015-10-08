public class Flight
{
    public static ReBotAPI API;
    public static Fiber<int> Fib;

    // Empty Constructor
    public Flight() { }


    public static void FlyTo(String destinationName)
    {
        getClosestFlight(destinationName);
    }

    // Method:      "GetClosestFlight(string)"
    // Purpose:     Take an Object with all FPs of a given zone, then determine
    //               which one is the closest to the player to take.
    public static List<Object> getClosestFlight(String destinationName)
    {
        List<Object> result = new List<Object>();

        int continentID = API.Me.ContinentID;
        int zoneID = API.Me.ZoneId;
        List<Object> FPs = new List<Object>();
        float closestDistance;
        float tempDistance;
        string closestZone;
        Vector3 closestVector3;
        Vector3 position;

        // initializing instance
        TaxiInfo info = new TaxiInfo(continentID, zoneID);
        FPs = info.zoneFlightInfo;

        // Setting the initial FP to the closest distance
        closestZone = (string)FPs[0];
        closestVector3 = new Vector3((float)FPs[1], (float)FPs[2], (float)FPs[3]);
        closestDistance = API.Me.DistanceTo(closestVector3);

        // Filtering for Closest flight now.
        for (int i = 0; i < FPs.Count - 4; i = i + 4)
        {
            position = new Vector3((float)FPs[i + 1], (float)FPs[i + 2], (float)FPs[i + 3]);
            tempDistance = API.Me.DistanceTo(position);
            if (tempDistance < closestDistance)
            {
                closestDistance = tempDistance;
                closestVector3 = position;
                closestZone = (string)FPs[i];
            }
        }
        result.Add(closestZone);
        result.Add(closestVector3);
        return result;
    }
    
    // Method:      "ToFlightMaster(String)"
    public static IEnumerable<int> ToFlightMaster(String destinationName)
    {
        yield return 100;
    }

    // Method:      "IsKnown(String)"
    public static bool IsKnown(String destinationName)
    {
        bool result = false;
        String playerName = API.Me.Name;
        String serverName = API.ExecuteLua<string>("return GetRealmName");
        String fileName = ("Settings\\" + serverName + "\\" + playerName + "\\Taxibook.json");

        // Add Code

        return result;
    }

    // Method:      "ClosestFM()"
    public static string ClosestFM()
    {
        string result = "";




        return result;
    }
    // END OF CLASS
    //
    //
    //
    //
    // START OF NEW CLASS
    private class TaxiInfo
    {
        public List<Object> zoneFlightInfo;

        public TaxiInfo(int ContinentID, int ZoneID)
        {
            zoneFlightInfo = getFlightMasterInfo(ContinentID, ZoneID);
        }

        // Filters out returns by continent
        private static List<Object> getFlightMasterInfo(int continentID, int zoneID)
        {
            List<Object> result = new List<Object>();
            // Draenor Continent
            if (continentID == 1116)
            {
                return getDraenorInfo(zoneID);
            }
            return result;
        }

        // Filters out returns by zone on the continent
        private static List<Object> getDraenorInfo(int zoneID)
        {
            List<Object> result = new List<Object>();

            // Frostfire Ridge (and caves and phases)
            if (zoneID == 6720 || zoneID == 6868 || zoneID == 6745 || zoneID == 6849 || zoneID == 6861 || zoneID == 6864 || zoneID == 6848 || zoneID == 6875 || zoneID == 6939 || zoneID == 7005 || zoneID == 7209)
            {
                return getFFR();
            }

            // Gorgrond (and caves and phases)
            if (zoneID == 6721 || zoneID == 6885 || zoneID == 7160 || zoneID == 7185)
            {
                return getGorgrond();
            }

            // Talador (and caves and phases)
            if (zoneID == 6662 || zoneID == 6980 || zoneID == 6979 || zoneID == 7089 || zoneID == 7622)
            {
                return getTalador();
            }

            // Spires of Arak
            if (zoneID == 6722)
            {
                return getSpires();
            }

            // Nagrand (and phased caves)
            if (zoneID == 6755 || zoneID == 7124 || zoneID == 7203 || zoneID == 7204 || zoneID == 7267)
            {
                return getNagrand();
            }

            // Shadowmoon Valley (and caves and phases)
            if (zoneID == 6719 || zoneID == 6976 || zoneID == 7460 || zoneID == 7083)
            {
                return getSMV();
            }

            // Tanaan Jungle
            if (zoneID == 6723)
            {
                return getTanaan();
            }

            // Ashran (and mine)
            if (zoneID == 6941 || zoneID == 7548)
            {
                return getAshran();
            }

            // Warspear
            if (zoneID == 7333)
            {
                return getWarspear();
            }

            // Stormshield
            if (zoneID == 7332)
            {
                return getStormshield();
            }

            // Frostwall Garrison (and 3 mine phases)
            if (zoneID == 7004 || zoneID == 7327 || zoneID == 7328 || zoneID == 7329)
            {
                return getHordeGarrison();
            }

            // Lunarfall Garrison (and 3 mine phases)
            if (zoneID == 7078 || zoneID == 7327 || zoneID == 7328 || zoneID == 7329)
            {
                return getHordeGarrison();
            }
            return result;
        }

        // Return for all of these will be in the following format (XYZ = V3 coordinates):  List<Object> zoneFlightInfo = {FPName,X,Y,Z,FPName,X,Y,Z,...)
        private static List<Object> getStormshield()
        {
            throw new NotImplementedException();
        }

        private static List<Object> getWarspear()
        {
            throw new NotImplementedException();
        }

        private static List<Object> getHordeGarrison()
        {
            throw new NotImplementedException();
        }

        private static List<Object> getAshran()
        {
            throw new NotImplementedException();
        }

        private static List<Object> getTanaan()
        {
            throw new NotImplementedException();
        }

        private static List<Object> getSMV()
        {
            throw new NotImplementedException();
        }

        private static List<Object> getNagrand()
        {
            throw new NotImplementedException();
        }

        private static List<Object> getSpires()
        {
            throw new NotImplementedException();
        }

        private static List<Object> getTalador()
        {
            throw new NotImplementedException();
        }

        private static List<Object> getGorgrond()
        {
            throw new NotImplementedException();
        }

        private static List<Object> getFFR()
        {
            throw new NotImplementedException();
        }
    }
}