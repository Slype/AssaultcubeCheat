using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace assaultcube_cheat_v2
{
    // Use this to represent a player
    public struct Player
    {
        public float x, y, z;
        public int health;
        public bool alive;
        public double yaw, pitch;

        public Player(float _x, float _y, float _z, int _health, bool _alive, double _yaw = 0f, double _pitch = 0f)
        {
            x = _x;
            y = _y;
            z = _z;
            health = _health;
            alive = !_alive;
            yaw = _yaw;
            pitch = _pitch;
        }
    }

    // Use this to represent yaw & float
    public struct YawPitch
    {
        public double yaw, pitch;

        public YawPitch(double _yaw, double _pitch)
        {
            yaw = _yaw;
            pitch = _pitch;
        }
    }

    class AssaultCube : SlypeMemory
    {
        // Offset table
        public Dictionary<string, int> offsets = new Dictionary<string, int>
        {
            { "game", 0x50F4E8 },
            { "playerBase", 0x0C },
            { "enemyList", 0x10 },
            { "enemyListNext", 0x04 },
            { "playerCount", 0x18 },

            { "health", 0xF8 },
            { "dead", 0x82 },
            { "isShooting", 0x0224 },
            { "isScoping", 0x0071 },
            { "shoot", 0x0072 },
            { "jump", 0x6B },
            { "crouching",  0x6C },

            { "posX", 0x34 },
            { "posY", 0x38 },
            { "posZ", 0x3C },
            { "yaw", 0x40 },
            { "pitch", 0x44 },

            { "ammoPrimary",  0x150 },
            { "ammoPrimaryReserve",  0x0128 },
            { "ammoSecondary",  0x013C },
            { "ammoSecondaryReserve",  0x0114 },
            { "ammoAkimbo", 0x015C },
            { "ammoGrenade", 0x0158 },
            
        };

        // Address table
        public Dictionary<string, int> addresses = new Dictionary<string, int>();

        // Enemy table
        public static List<Player> enemies = new List<Player>();

        // Variables
        int playerCount;
        Player player;

        // Emty Constructor, uses base constructor
        public AssaultCube(string _processName, string accesslevel, int numOfRetries = 10, int timeout = 100) : base(_processName, accesslevel, numOfRetries, timeout)
        {
            playerCount = 0;
        }

        // Returns an offsey from offset table
        public int offset(string key)
        {
            if (offsets.ContainsKey(key))
                return offsets[key];
            return 0;
        }

        // Calculate sum of offsets, from offset Table (overloads parent function)
        public int sumOffsets(params string[] addresses)
        {            
            int total = 0;
            for (int i = 0; i < addresses.Length; i++)
            {
                total += offset(addresses[i]);
            }
            return total;
        }

        // Adds an address to address table
        public void addAddress(string name, int address)
        {
            addresses.Add(name, address);
        }

        // Returns an address from address table
        public int address(string key)
        {
            if (addresses.ContainsKey(key))
                return addresses[key];
            return 0;
        }

        // Updates enemy table
        public void updatePlayerAndEnemies()
        {
            player = new Player(
                readFloat(address("player") + offset("posX")),
                readFloat(address("player") + offset("posY")),
                readFloat(address("player") + offset("posZ")),
                readInt32(address("player") + offset("health")),
                readBool(address("player") + offset("dead"))
            );

            enemies.Clear();
            playerCount = readInt32(sumOffsets("game", "playerCount"));
            for(int i = 0;i < playerCount - 1; i++)
            {
                int enemyBase = readInt32(address("enemyList") + offset("enemyListNext") + (i * offset("enemyListNext")));
                enemies.Add(new Player(
                    readFloat(enemyBase + offset("posX")),
                    readFloat(enemyBase + offset("posY")),
                    readFloat(enemyBase + offset("posZ")),
                    readInt32(enemyBase + offset("health")),
                    readBool(enemyBase + offset("dead"))
                ));
            }
        }

        // Calculates nearest enemy, may return null
        public Player findNearestEnemy()
        {
            updatePlayerAndEnemies();
            Player nearestEnemy = player;
            float nearestDistance = float.PositiveInfinity;
            foreach(Player enemy in enemies)
            {
                float xdis = enemy.x - player.x;
                float ydis = enemy.y - player.y;
                float zdis = enemy.z - player.z;
                float distance = (xdis * xdis) + (ydis * ydis) + (zdis * zdis); // No need to sqrt

                if(distance < nearestDistance && enemy.alive)
                {
                    nearestEnemy = enemy;
                    nearestDistance = distance;                    
                }
            }
            return nearestEnemy;
        }

        // Calculates required Yaw to aim at a given enemy
        public YawPitch calculateYawPitch(Player enemy)
        {
            double yaw = Math.Atan2(enemy.y - player.y, enemy.x - player.x) + Math.PI / 2;
            double pitch = Math.Atan2(enemy.z - player.z, Math.Sqrt((enemy.y - player.y) * (enemy.y - player.y) + (enemy.x - player.x) * (enemy.x - player.x)));
            yaw *= (180 / Math.PI);
            pitch *= (180 / Math.PI);
            return new YawPitch(yaw, pitch);
        }

        // Applies Godmode
        public void applyGodmode()
        {
            writeInt32(address("player") + offset("health"), 1337);
        }

        // Applies Infinite Ammo
        public void applyInfiniteAmmo()
        {
            writeInt32(address("player") + offset("ammoPrimary"), 42);
            writeInt32(address("player") + offset("ammoPrimaryReserve"), 42);
            writeInt32(address("player") + offset("ammoSecondary"), 42);
            writeInt32(address("player") + offset("ammoSecondaryReserve"), 42);
            writeInt32(address("player") + offset("ammoAkimbo"), 42);
            writeInt32(address("player") + offset("ammoGrenade"), 1);
        }

        // Only apply aimbot if shooting
        public void aimbotOnFire()
        {
            if (readBool(address("player") + offset("isShooting")))
                aimbot();
        }

        // Only apply aimbot if scooping
        public void aimbotOnScoping()
        {
            if (readBool(address("player") + offset("isScoping")))
                aimbot();
        }

        // Continuously applies aimbot
        public void continuousAimbot()
        {
            aimbot();
            applyFire();
        }

        // Applies aimbot
        public void aimbot()
        {
            Player nearestEnemy = findNearestEnemy();
            if (Player.Equals(nearestEnemy, player)) // No enemy was found
                return;   
            YawPitch newAngle = calculateYawPitch(nearestEnemy); // Calculate required yaw & pitch
            writeFloat(address("player") + offset("yaw"), (float)newAngle.yaw);
            writeFloat(address("player") + offset("pitch"), (float)newAngle.pitch);
        }
        
        // Give everyone 1 hp
        public void giveNoHealth()
        {
            for (int i = 1; i < playerCount; i++)
            {
                int enemyBase = readInt32(address("enemyList") + (i * offset("enemyListNext")));
                writeInt32(enemyBase + offset("health"), 1);
            }
        }

        // Simulates player firing their weapon
        public void applyFire()
        {
            writeBool(address("player") + offset("isShooting"), true);
        }

        // Jumps
        public void jump()
        {
            writeByte(address("player") + offset("jump"), 1);
        }

        // Jumps while crouched
        public void jumpOnCrouch()
        {
            if (readBool(address("player") + offset("crouching")))
                jump();
        }
    }
}

/* Health example
if (!assaultcube.addModule("ac_client.exe"))
    exitProgram("Unable to locate DLL (ac_client.exe)");
// ac_client.exe + 0x10F4F4 > 0xF8 = health
*/
