using System;
using System.Diagnostics;
using System.Threading;

namespace FPBooru {
    public static class FileIO {
		public static string GenerateImage(string outpath, string infile, string res, string format, out bool failed) {
			ProcessStartInfo psi;
			Process ps;
			string output = "";
			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				psi = new ProcessStartInfo("mogrify.exe");
				psi.Arguments = "-path " + outpath + " -thumbnail " + res + "^^ -gravity center -extent " + res + " -format " + format + " " + infile;
			} else {
				psi = new ProcessStartInfo("mogrify");
				psi.Arguments = "-path \"" + outpath + "\" -thumbnail " + res + "^ -gravity center -extent " + res + " -format " + format + " \"" + infile + "\"";
			}
			psi.RedirectStandardError = true;
			psi.UseShellExecute = false;
			ps = Process.Start(psi);
			output += ps.StandardError.ReadToEnd();
			while (!ps.HasExited)
				Thread.Sleep(0);
			failed = (ps.ExitCode != 0);
			return output;
        }

		public static string RepairImage(string file, out bool failed) {
			ProcessStartInfo psi;
			Process ps;
			string output = "";
			output += "---> Converting to PNM and to PNG with anytopnm\n";
			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				failed = true;
				return "Repair not supported on Windows based systems.";
			} else {
				psi = new ProcessStartInfo("sh");
				psi.Arguments = "-c \"anytopnm " + file + " | pnmtopng " + System.IO.Path.ChangeExtension(file, "png") + "\"";
			}
			psi.RedirectStandardError = true;
			psi.UseShellExecute = false;
			ps = Process.Start(psi);
			output += ps.StandardError.ReadToEnd();
			while (!ps.HasExited)
				Thread.Sleep(0);
			failed = (ps.ExitCode != 0);
			return output;
		}
    }
}

