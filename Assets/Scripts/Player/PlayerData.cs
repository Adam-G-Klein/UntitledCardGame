namespace global
{
    static class PlayerData
    {
        // using the same prefab for now
        public static string encounterPrefabName = "Player";
        public static string roomPrefabName = "Player";
        // some test vals
        public static int playerHp;
        static int _playerAtk;


        static void Main(string[] args){
            playerHp = 10;
        }

        public static int getsetcounter
        {
            set { _playerAtk = value; }
            get { return _playerAtk; }
        }
    }

}