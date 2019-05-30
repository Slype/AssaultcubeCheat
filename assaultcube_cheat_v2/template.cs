


public class SlypeCheats {
	
	public SlypeCheats() {}
	
	public class Aimbot {
	
		public Aimbot() {}
		
		public Enemy findNearestEnemy() { return new Enemy(0, 0, 0, 0, 0, 0); }
		
        public double calculateYaw() { return 0; }

        public double calculatePitch() { return 0; }

    }
	
	public class PersonalTraits {

		public PersonalTraits() {}
		
		public void InfiniteAmmo() {} // Has to be called to replenish ammo
		
		public void Godmode() {} // Has to be called to apply high health
		
		public void AutoJump() {} // Has to be called to check if jump is required
		
	}
}

public struct Enemy {
	public long y, x, z;
	public double yaw, pitch;
    public int health;

	public Enemy(long _y, long _x, long _z, double _yaw, double _pitch, int _health) {
        y = _y;
        x = _x;
        z = _z;
        yaw = _yaw;
        pitch = _pitch;
        health = _health;
	}

}