namespace WebApplication1.Models
{
    #region Lovely
    public class VideoJob
    {
        public string JobId { get; set; }
        public string Status { get; set; } = "Processing";
        public string? OutputPath { get; set; }
        public string? Error { get; set; }
        public int Progress { get; set; } = 0;
    }
    #endregion
}
