namespace Core.Templates
{
    public class Utilities
    {

        public int CalculateETS(int sizeMb, int speedMbs)
        {
            return (sizeMb / speedMbs);
        }
    }
}