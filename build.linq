<Query Kind="Statements">
  <NuGetReference>DotNetZip</NuGetReference>
  <Namespace>Ionic.Zip</Namespace>
</Query>

const string source = "MiP.TeamBuilds\\bin\\release";
const string release = "release";
const string bin = release + "\\bin";

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