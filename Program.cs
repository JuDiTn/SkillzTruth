using Fiddler;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.Threading.Tasks.Sources;

Console.Title = "SkillzTruth";
Console.ForegroundColor = ConsoleColor.DarkBlue;

Console.WriteLine("SkillzTruth"); 
Console.WriteLine("www.skillstruth.com");
Console.WriteLine("Version 1.0 (2/13/2023)");
Console.WriteLine();

Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine("---START OF LEGAL NOTICE---");
Console.WriteLine("This tool was created as a proof of concept demonstrating weaknesses within the Skillz platform and APIS. It has been released for educational and public awareness purposes. Use of this tool could be seen as a violation of various terms of service you have agreed upon. Nobody involved with the creation of this tool takes any responsibility for your actions using it. This tool is provided as-is, without any expressed or implied warranties or guaranties. Please ensure that you use this tool only in a legal way. Don't do anything stupid. Don't cheat other users. Enough said. Thank you.");
Console.WriteLine("---END OF LEGAL NOTICE---");
Console.WriteLine();
Console.WriteLine();


BCCertMaker.BCCertMaker certProvider = new BCCertMaker.BCCertMaker();
CertMaker.oCertProvider = certProvider;

certProvider.ReadRootCertificateAndPrivateKeyFromStream(Assembly.GetEntryAssembly()!.GetManifestResourceStream("SkillzTrust.RootCertificate.p12"), "password");

while (!CertMaker.rootCertIsTrusted())
{
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("This appears to be the first time you've run this tool. You will need to click 'Yes' on the following prompt.");
    Console.WriteLine("Press the 'enter' key when you are ready to be prompted (BE SURE TO CLICK YES).");
    while (Console.ReadKey().Key != ConsoleKey.Enter)
        Console.WriteLine("Press the 'enter' key when you are ready to be prompted (BE SURE TO CLICK YES).");
    CertMaker.trustRootCert();
    Console.WriteLine();
    Console.WriteLine();
}

int score = 40000;

FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;
FiddlerApplication.BeforeResponse += FiddlerApplication_BeforeResponse;

void FiddlerApplication_BeforeResponse(Session oSession)
{
    if (oSession.oRequest.host.EndsWith("skillz.com") && oSession.fullUrl.EndsWith("v1/user") && oSession.HTTPMethodIs("GET"))
    { 
        if (oSession.ResponseBody != null && oSession.ResponseBody.Length > 0)
        {
            var resBody = oSession.GetResponseBodyAsString();
            resBody = resBody.Replace("\"chat_moderator\":false", "\"chat_moderator\":true");
            resBody = resBody.Replace("\"skillz_staff\":false", "\"skillz_staff\":true");
            oSession.utilSetResponseBody(resBody);
        }
    }
}

void FiddlerApplication_BeforeRequest(Session oSession)
{
    oSession["x-OverrideSslProtocols"] = "ssl3;tls1.0;tls1.1;tls1.2";
    if (oSession.oRequest.host.EndsWith("skillz.com") && oSession.fullUrl.Contains("v1/users/") && oSession.fullUrl.Contains("/matches/") && oSession.fullUrl.Contains("/players/") && oSession.HTTPMethodIs("PATCH"))
    {
        if (oSession.RequestBody != null && oSession.RequestBody.Length > 0)
        {
            var reqBody = oSession.GetRequestBodyAsString();
            if (reqBody.Contains("\"state\":\"FINISHED\"}"))
            {
                var rnd = new Random();
                var thisScore = score + rnd.Next(50);
                var newReqBody = "{\"score\":" + thisScore.ToString() + ",\"state\":\"FINISHED\"}";
                oSession.utilSetRequestBody(newReqBody);

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine();
                Console.WriteLine("Game score re-written to: " + thisScore.ToString());
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("When you are finished playing the games, press any key to exit...");
            }
        }
    }

}

Console.ForegroundColor = ConsoleColor.White;
Console.WriteLine("--- Starting Proxy Server ---");

if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
{
    Console.ForegroundColor = ConsoleColor.Black;
    Console.WriteLine("ERROR: No network connection is available.");
    Console.WriteLine("Press any key to exit.");
    Console.ReadKey();
    return;
}

string localIP;
using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
{
    socket.Connect("8.8.8.8", 65530);
    IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
    localIP = endPoint!.Address.ToString();
}

Console.WriteLine("--- Local IP Address: " + localIP);
Console.WriteLine("--- Local Port: 12345");

// Build startup settings:
var settings = new FiddlerCoreStartupSettingsBuilder()
    .ListenOnPort(12345)
    .OptimizeThreadPool()
    .AllowRemoteClients()
    .DecryptSSL()
    .Build();

// Start:
FiddlerApplication.Startup(settings);

Console.WriteLine("--- Proxy Server Running ---");

Console.WriteLine();
Console.WriteLine("Would you like to see the instructions for setting up your iPhone? Yes/No");
if (Console.ReadLine()!.ToLower().StartsWith("y")) {
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine("Setup instructions for iPhone Part 1 (only required one time):");
    Console.WriteLine("---");
    Console.WriteLine("1. Make sure you are connected to the Internet via the same WiFi as this computer.");
    Console.WriteLine("2. Type 'http://" + localIP + ":12345' into the address bar of Safari (only Safari will work) on your iPhone.");
    Console.WriteLine("3. Click the link at the bottom of the page to download the 'FiddlerRoot certificate'.");
    Console.WriteLine("4. Select 'Allow' on the pop-up dialog. If asked to choose a device, select 'iPhone'.");
    Console.WriteLine("5. Open the 'Settings' app on your phone. Click on the 'Profile Downloaded' item located near the top of the list.");
    Console.WriteLine("6. Open the 'Settings' app on your phone. Click on the 'Profile Downloaded' item located near the top of the list.");
    Console.WriteLine("7. Click the 'Install' button in the top right of the screen. Enter your passcode and click 'Install' at the bottom of the screen.");
    Console.WriteLine("8. Click 'Done' and then 'Back' which should take you to the 'General' section of settings. Click the first item labeled 'About'.");
    Console.WriteLine("9. Scroll to the bottom and click 'Certificate Trust Settings. Click on the toggle for 'SkillzTrust' to enable it.");
    Console.WriteLine();
    Console.WriteLine("Setup instructions for iPhone Part 2 (perform when you are ready to play a game):");
    Console.WriteLine("---");
    Console.WriteLine("1. Open the 'Settings' app on your iPhone. Select 'Wi-Fi' from the list.");
    Console.WriteLine("2. Click the blue 'i' icon corresponding to the Wi-Fi network you are connected to.");
    Console.WriteLine("3. Scroll to the bottom and select 'Configure Proxy'. Select 'Manual'.");
    Console.WriteLine("4. Enter '" + localIP + "' for the 'Server' and '12345' for the 'Port'. Click 'Save' in the top right of the screen.");
    Console.WriteLine("You're all ready! Go play Skillz games and see your scores get modified as you play."); 
    Console.WriteLine();
    Console.WriteLine("Removal Instructions:");
    Console.WriteLine("---");
    Console.WriteLine("Follow the instructions from Part 2 above. Select 'Off' on the 'Configure Proxy' screen instead of 'Manual'. Be sure to click 'Save'.");
    Console.WriteLine("");
}

Console.ForegroundColor = ConsoleColor.White;

Console.WriteLine();
Console.WriteLine("What score would you like? (we add some randomness to what you enter each game)");
score = int.Parse(Console.ReadLine()!);

Console.WriteLine();


Console.WriteLine("When you are finished playing the games, press any key to exit...");

Console.ReadKey();

FiddlerApplication.Shutdown();