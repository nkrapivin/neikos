namespace neikos
{
    class Program
    {
        public const string NGROK_API_KEY = "1AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

        static void Main()
        {
            NeikosNgrok.Run(NGROK_API_KEY).Wait();
        }
    }
}