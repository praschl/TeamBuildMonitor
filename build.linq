<Query Kind="Statements">
  <NuGetReference>DotNetZip</NuGetReference>
  <Namespace>Ionic.Zip</Namespace>
</Query>

/* This script is used to copy the output of a successful Release-build
   into the release directory just beside the script (which is created if it does not exist).
   The dlls are moved to a sub directory called "bin"
   Then the whole directory is zipped as well.
*/

const string source = "MiP.TeamBuilds\\bin\\release";
const string release = "release";
const string bin = release + "\\bin";

// set directory to that of this query, because Environment.CurrentDirectory will point to the linqpad.exe, when linqpad was already open before opening this query.
Environment.CurrentDirectory = Path.GetDirectoryName(Util.CurrentQueryPath);

if (!Directory.Exists(bin))
	Directory.CreateDirectory(bin);

Util.Cmd("copy", source + " " + bin);

Util.Cmd("move", bin + "\\MiP.TeamBuilds.exe " + release);
Util.Cmd("move", bin + "\\MiP.TeamBuilds.pdb " + release);
Util.Cmd("move", bin + "\\MiP.TeamBuilds.exe.config " + release);

using (ZipFile zip = new ZipFile())
{
	zip.AddDirectory(release);
	zip.Save(release + "\\release.zip");
}