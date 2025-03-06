namespace DogQuiz.Common
{
    public interface IIdGenerator
    {
        string NewGuid();
        string NewUlid();
    }

    public sealed class IdGenerator : IIdGenerator
    {
        public string NewGuid()
        {
            var guid = Guid.NewGuid().ToString("N");

            return guid;
        }

        public string NewUlid()
        {
            var ulid = Ulid.NewUlid().ToString().ToLowerInvariant();

            return ulid;
        }
    }
}
