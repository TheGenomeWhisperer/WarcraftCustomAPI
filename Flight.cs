// This will be the Flight Manager
impot java.util.Scanner;


public class Flight
{
    public static ReBotAPI API;
    public static Fiber<int> Fib;

    // Empty Constructor
    public Flight() { }	
		
        
        
    // Method:      "ToFlightMaster(String)"
    public static IEnumerable<int> ToFlightMaster(String destinationName) {
        
    }
    
    // Method:      "HasDestination(String)"
    public static boolean HasDestination(String destinationName) {
        String playerName = API.Me.Name;
        String serverName = API.ExecuteLua<string>("return GetRealmName");
        String fileName = ("Settings\\" + serverName + "\\" + playerName + "\\Taxibook.json");

    }
    
    // Method:      "