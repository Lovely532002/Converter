using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public static class VideoConvertService
    {
        public static ConcurrentDictionary<string, VideoJob> Jobs = new();

        public static string StartConvert(string url, string format)
        {
            var jobId = Guid.NewGuid().ToString();

            ClearOutputFolder();

            var job = new VideoJob
            {
                JobId = jobId,
                Status = "Processing",
                Progress = 0
            };

            Jobs[jobId] = job;

            Task.Run(() => ProcessVideo(job, url, format));

            return jobId;
        }

        private static void ProcessVideo(VideoJob job, string url, string format)
        {
            try
            {
                var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "output");
                Directory.CreateDirectory(outputDir);

                string outputFile;
                string ytDlpArgs;

                // ✅ Linux container me ye dono system PATH se milenge
                var ytDlpPath = "yt-dlp";
                var ffmpegPath = "ffmpeg";

                if (format == "mp3")
                {
                    outputFile = Path.Combine(outputDir, $"{job.JobId}.mp3");

                    ytDlpArgs =
                        $"-x --audio-format mp3 --audio-quality 0 " +
                        $"--ffmpeg-location {ffmpegPath} " +
                        $"-o \"{outputFile}\" \"{url}\"";
                }
                else
                {
                    outputFile = Path.Combine(outputDir, $"{job.JobId}.mp4");

                    ytDlpArgs =
                        $"-f bestvideo+bestaudio --merge-output-format mp4 " +
                        $"--ffmpeg-location {ffmpegPath} " +
                        $"-o \"{outputFile}\" \"{url}\"";
                }

                RunProcess(ytDlpPath, ytDlpArgs, job);

                if (!File.Exists(outputFile))
                    throw new Exception("Output file not created");

                job.Progress = 100;
                job.Status = "Completed";
                job.OutputPath = outputFile;
            }
            catch (Exception ex)
            {
                job.Status = "Failed";
                job.Error = ex.Message;
            }
        }

        private static void RunProcess(string exePath, string args, VideoJob job)
        {
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };

            process.OutputDataReceived += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(e.Data)) return;

                var match = Regex.Match(e.Data, @"(\d{1,3}\.\d+)%");

                if (match.Success)
                {
                    if (float.TryParse(match.Groups[1].Value, out float progress))
                        job.Progress = (int)progress;
                }
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                    job.Error = e.Data;
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception("yt-dlp process failed");
        }

        private static void ClearOutputFolder()
        {
            var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "output");

            if (!Directory.Exists(outputDir))
                return;

            foreach (var file in Directory.GetFiles(outputDir))
            {
                try { File.Delete(file); } catch { }
            }
        }
    }
}
