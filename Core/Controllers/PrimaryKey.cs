namespace Core.Controllers
{
    public static class PrimaryKey
    {
        private static int _lastID = 0;
        
        public static int GetID()
        {
            return _lastID++;
        }
    }
}