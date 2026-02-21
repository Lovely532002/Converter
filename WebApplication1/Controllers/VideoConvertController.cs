using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoConvertController : ControllerBase
    {
        #region Lovely
        /// <summary>
        /// Created By   : Lovely Prajapati
        /// Created Date : 26-12-2025
        /// Converts a video or audio from a given URL into MP4 or MP3 format.
        /// </summary>
        /// <param name="request">
        /// Request object containing:
        /// - VideoUrl : Public URL of the video or audio file
        /// - Format   : Desired output format (mp4 or mp3)
        /// </param>
        /// <returns>
        /// Returns a JobId and processing status for tracking conversion progress.
        /// </returns>
        [HttpPost("convert-from-url")]
        public IActionResult ConvertFromUrl([FromBody] VideoUrlRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.VideoUrl))
                return BadRequest("Video URL is required");

            var format = request.Format?.ToLower();

            if (format != "mp4" && format != "mp3")
                return BadRequest("Format must be mp4 or mp3");

            var jobId = VideoConvertService.StartConvert(request.VideoUrl, format);

            return Ok(new
            {
                jobId,
                status = "Processing"
            });
        }

        /// <summary>
        /// Created By   : Lovely Prajapati
        /// Created Date : 26-12-2025
        /// Retrieves the current conversion status and progress percentage using JobId.
        /// </summary>
        /// <param name="jobId">
        /// Unique JobId returned during conversion request.
        /// </param>
        /// <returns>
        /// Returns conversion status, progress percentage, error (if any),
        /// and download URL when conversion is completed.
        /// </returns>
        [HttpGet("status/{jobId}")]
        public IActionResult GetStatus(string jobId)
        {
            if (!VideoConvertService.Jobs.TryGetValue(jobId, out var job))
                return NotFound("Invalid JobId");

            return Ok(new
            {
                job.JobId,
                job.Status,
                job.Progress, 
                downloadUrl = job.Status == "Completed"
                    ? Url.Action("Download", new { jobId })
                    : null,
                job.Error
            });
        }

        /// <summary>
        /// Created By   : Lovely Prajapati
        /// Created Date : 26-12-2025
        /// Downloads the converted file once the conversion process is completed.
        /// </summary>
        /// <param name="jobId">
        /// Unique JobId associated with the completed conversion.
        /// </param>
        /// <returns>
        /// Returns the converted MP4 or MP3 file for download.
        /// </returns>
        [HttpGet("download/{jobId}")]
        public IActionResult Download(string jobId)
        {
            if (!VideoConvertService.Jobs.TryGetValue(jobId, out var job))
                return NotFound("Invalid JobId");

            if (job.Status != "Completed")
                return BadRequest("File is not ready");

            var filePath = job.OutputPath;

            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                return NotFound("File not found");

            var contentType = Path.GetExtension(filePath).ToLower() == ".mp3"
                ? "audio/mpeg"
                : "video/mp4";

            return PhysicalFile(filePath, contentType, Path.GetFileName(filePath));
        }
        #endregion
    }
}

