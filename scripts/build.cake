using System.Xml.Linq;

var target =  HasArgument("target") ? Argument("target", "Build") : Argument("t", "Build");
var configuration = Argument("configuration", "Release");

var encryptionProj = "../bindings/netstandard/ElectionGuard/ElectionGuard.Encryption/ElectionGuard.Encryption.csproj";

Task("AssignVersion")
    .Does(() =>
{
    var csprojFile =  Argument("csproj", encryptionProj);
    var newVersion = Argument("newVersion", "");

    if (string.IsNullOrEmpty(newVersion))
    {
        throw new InvalidOperationException("Both csproj and newVersion arguments must be provided.");
    }

    var xdoc = XDocument.Load(csprojFile);
    var ns = xdoc.Root.GetDefaultNamespace();
    var versionElement = xdoc.Descendants(ns + "Version").FirstOrDefault();
    var packageVersionElement = xdoc.Descendants(ns + "PackageVersion").FirstOrDefault();

    if (versionElement == null)
    {
        throw new InvalidOperationException($"No 'Version' element found in the '{csprojFile}'.");
    }

    if (packageVersionElement == null)
    {
        throw new InvalidOperationException($"No 'PackageVersion' element found in the '{csprojFile}'.");
    }

    versionElement.Value = newVersion;
    packageVersionElement.Value = newVersion;
    xdoc.Save(csprojFile);
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);