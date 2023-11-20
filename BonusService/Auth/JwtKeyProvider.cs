namespace BonusService.Auth;

public static class JwtKeyProvider
{
        public static string Key { set; get; } = null!;
        public static string Sole { set; get; } = null!;

        public static void SetJwtSecretKey(string partKey, string sole)
        {
            Sole = sole;
            Key = partKey + Sole;
        }
}