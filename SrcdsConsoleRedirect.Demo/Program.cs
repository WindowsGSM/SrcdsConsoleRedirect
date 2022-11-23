using SrcdsConsoleRedirect;
using System.Diagnostics;

SrcdsProcess srcdsProcess = new();
srcdsProcess.OutputDataReceived += (s, e) => Console.WriteLine(e);

string srcdsPath = @"D:\WindowsGSMtest2\servers\28\serverfiles\srcds.exe";
Process process = srcdsProcess.Start(srcdsPath, "-console -game tf +maxplayers 24 +map cp_badlands -port 27010");
process.WaitForExit();
