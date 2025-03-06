namespace DogQuiz.Infrastructure
{
    public sealed class HealthEndpoint
    {
        public static IResult Handle(IDbConnection db, ISql sql, ILogger<HealthEndpoint> logger)
        {
            try
            {
                db.Execute(sql["Db.Check"]);

                return Results.StatusCode(StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Health check failed.");

                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
