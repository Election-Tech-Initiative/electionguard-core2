using System.Xml.Linq;

var target = HasArgument("target") ? Argument("target", "Build") : Argument("t", "Build");
var configuration = Argument("configuration", "Release");

var encryptionProj = "../bindings/netstandard/ElectionGuard/ElectionGuard.Encryption/ElectionGuard.Encryption.csproj";
var decryptionProj = "../bindings/netstandard/ElectionGuard/ElectionGuard.Decryption/ElectionGuard.Decryption.csproj";
var setupProj = "../bindings/netstandard/ElectionGuard/ElectionGuard.ElectionSetup/ElectionGuard.ElectionSetup.csproj";

var projects = new[] { encryptionProj, decryptionProj, setupProj };

Task("AssignVersion")
    .Does(() =>
{
    var newVersion = Argument("newVersion", "");
    if (string.IsNullOrEmpty(newVersion))
    {
        throw new InvalidOperationException("Both csproj and newVersion arguments must be provided.");
    }

    foreach (var csprojFile in projects)
    {
        var xdoc = XDocument.Load(csprojFile);
        var ns = xdoc.Root.GetDefaultNamespace();
        var versionElement = xdoc.Descendants(ns + "Version").FirstOrDefault();
        var packageVersionElement = xdoc.Descendants(ns + "PackageVersion").FirstOrDefault();

        if (versionElement == null)
        {
            throw new InvalidOperationException($"No 'Version' element found in '{csprojFile}'.");
        }

        if (packageVersionElement == null)
        {
            throw new InvalidOperationException($"No 'PackageVersion' element found in '{csprojFile}'.");
        }

        versionElement.Value = newVersion;
        packageVersionElement.Value = newVersion;
        xdoc.Save(csprojFile);
    }
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
