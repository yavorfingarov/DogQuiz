using System.ComponentModel.DataAnnotations;

namespace DogQuiz.Common
{
    public sealed class Configuration
    {
        public static string AppInfoHeaderName => "X-App-Info";
        public static string AntiforgeryTokenFormFieldName => "__AntiforgeryToken";
        public static string AntiforgeryTokenCookieName => "__Host-AntiforgeryToken";
        public static string UserSessionIdCookieName => "__Host-UserSessionId";

        [Required, DataType(DataType.Url)]
        public string DogApiBaseUrl { get; set; } = null!;

        [Required]
        public string ImagesPath { get; set; } = null!;

        [Range(300, 900)]
        public int ImageTargetSize { get; set; }

        [Range(1, 100)]
        public uint ImageTargetQuality { get; set; }

        [Range(5_000, 50_000)]
        public long ImageTargetMinFileSize { get; set; }

        [Range(1, 50)]
        public int QuizLength { get; set; }

        [Range(60, 365)]
        public int QuizExpirationInDays { get; set; }

        [Range(1, 6)]
        public int CleanerWaitHours { get; set; }

        [Range(12, 24)]
        public int CleanerPeriodHours { get; set; }
    }
}
