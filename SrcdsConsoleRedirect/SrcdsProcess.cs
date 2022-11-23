using System.Diagnostics;

namespace SrcdsConsoleRedirect
{
    /// <summary>
    /// Srcds Process Simulation using <see cref="SrcdsConsoleRedirect.SrcdsControl"/>, supports output and input
    /// </summary>
    public class SrcdsProcess : IDisposable
    {
        /// <summary>
        /// Provides a output redirection solution on srcds.exe
        /// </summary>
        public SrcdsControl SrcdsControl { get; private set; } = new();

        /// <summary>
        /// Occurs each time srcds.exe writes a line
        /// </summary>
        public EventHandler<string>? OutputDataReceived;

        /// <summary>
        /// Start srcds.exe
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public Process Start(string fileName, string arguments)
        {
            Process process = SrcdsControl.Start(fileName, arguments);

            Task.Run(() =>
            {
                int size = SrcdsControl.GetScreenBufferSize();

                string oldScreenBuffer = string.Empty;
                List<string> oldLines = new();

                while (true)
                {
                    string newScreenBuffer = SrcdsControl.GetScreenBuffer(1, size - 2);

                    if (oldScreenBuffer == newScreenBuffer)
                    {
                        continue;
                    }

                    List<string> newLines = ScreenBufferToLines(newScreenBuffer);

                    for (int i = 0, j = 0; i < newLines.Count;)
                    {
                        if (oldLines.Count <= j)
                        {
                            newLines.Skip(i).ToList().ForEach(x => OutputDataReceived?.Invoke(this, x));
                            break;
                        }

                        if (oldLines[j].Contains(newLines[i]) || newLines[i].Contains(oldLines[j]))
                        {
                            i++;
                            j++;
                        }
                        else
                        {
                            j = j - i + 1;

                            if (oldLines.Count > j)
                            {
                                i = 0;
                            }
                        }
                    }

                    oldScreenBuffer = newScreenBuffer;
                    oldLines = newLines;
                }
            });

            return process;
        }

        /// <summary>
        /// Send command to srcds.exe
        /// </summary>
        /// <param name="data"></param>
        /// <returns><see langword="true"/> if success, else <see langword="false"/></returns>
        public bool Write(string data)
        {
            return SrcdsControl.Write(data);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            SrcdsControl.Dispose();
            GC.SuppressFinalize(this);
        }

        private List<string> ScreenBufferToLines(string screenBuffer)
        {
            screenBuffer = screenBuffer.TrimEnd((char)0).TrimEnd((char)32);

            List<string> lines = new();

            for (int i = 0; i < screenBuffer.Length; i += SrcdsControl.ConsoleLineWidth)
            {
                string line = screenBuffer.Substring(i, (i + SrcdsControl.ConsoleLineWidth > screenBuffer.Length) ? screenBuffer.Length - i : SrcdsControl.ConsoleLineWidth);
                string padding = line.Length >= 3 ? line[..3] : line;
                bool isNewLine = padding == "   " || padding == "\0\0\0";
                line = (isNewLine ? line[3..] : line).TrimEnd((char)32);

                if (isNewLine)
                {
                    lines.Add(line);
                }
                else if (lines.Count > 0)
                {
                    lines[^1] += line;
                }
            }

            return lines;
        }
    }
}
