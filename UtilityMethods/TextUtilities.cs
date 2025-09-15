namespace InfernumMode
{
    public static partial class Utilities
    {
        public static string AddOrdinalSuffix(int num)
        {
            if (num <= 0)
                return num.ToString();

            int lastTwoDigits = num % 100;
            if (lastTwoDigits == 11 || lastTwoDigits == 12 || lastTwoDigits == 13)
                return num + "th";
            
            int lastDigit = num % 10;
            switch (lastDigit)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }
        }
    }
}