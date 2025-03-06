namespace DogQuiz.Common
{
    public static class ExtensionMethods
    {
        public static IDbTransaction CreateTransaction(this IDbConnection db)
        {
            if (db.State == ConnectionState.Closed)
            {
                db.Open();
            }

            return db.BeginTransaction();
        }

        public static string? ToPercent<T>(this T value)
        {
            return value != null ? $"{value}%" : null;
        }
    }
}
