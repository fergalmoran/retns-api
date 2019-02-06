using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace retns.api.Services {
    public class DocHelpers {
        public static Task<string> ConvertToDocx(string fileName) {
            return Task.Run(() => {
                var outputFile = $"{fileName}.docx";
                var processArgs = $"-d document --format=docx --output {outputFile} {fileName}";
                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "unoconv",
                        Arguments = processArgs,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                process.Start();
                string result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return outputFile;
            });
        }
        public static Task<string> ConvertToHtml(string fileName, string outputFile) {
            return Task.Run(() => {
                //unoconv -d document --format=docx --output /tmp/test-case-2.doc.docx /tmp/test-case-2.doc
                var processArgs = $"-d document --format=html --output {outputFile} {fileName}";
                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "unoconv",
                        Arguments = processArgs,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                process.Start();
                var result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return outputFile;
            });
        }
    }
}