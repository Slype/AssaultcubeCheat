using System;
using System.Text;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace assaultcube_cheat_v2
{
    class Program
    {
        // Exits program nicely.
        public static void exitProgram(string message)
        {
            Console.WriteLine(message);
            Console.ReadKey();
            Environment.Exit(0);
        }

        static void Main()
        {
            // Init SlypeMemory Class
            AssaultCube assaultcube = new AssaultCube("ac_client", "full");
            if (assaultcube.process == null) // Exit if it was unable to find the process
                exitProgram("Process is unable to be found, Press any key to exit.");

            // Open a handle
            if (!assaultcube.openHandle())
                exitProgram("Unable to open a handle, Press any key to exit.");

            // Calculate addresses
            assaultcube.addAddress("player", assaultcube.readInt32(assaultcube.sumOffsets("game", "playerBase")));
            assaultcube.addAddress("enemyList", assaultcube.readInt32(assaultcube.sumOffsets("game", "enemyList")));    

            // Infinite loop
            while (true)
            {
                
                assaultcube.jumpOnCrouch(); // Auto-Jump when crouching
                assaultcube.applyGodmode(); // Godmode
                assaultcube.applyInfiniteAmmo(); // Infinite Ammo
                assaultcube.giveNoHealth(); // Gives everyone 1 hp
                assaultcube.aimbotOnFire(); // Applies aimbot if mousebutton is held

                //assaultcube.continuousAimbot(); // Continuously applies aimbot
                //assaultcube.aimbotOnScoping(); // Applies aimbot when scooping
            }
            
        }
    }
}
